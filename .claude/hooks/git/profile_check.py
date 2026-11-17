r"""Git's own side of G6 (November 17, 2026): would this commit or this push carry the path of the user's own profile
folder into the repository? That path holds the person's user name, and the repository is public. Luis approved the
check for this repository only. The PreToolUse guard (.claude/hooks/guard.sh, rule G6) reads a command before it
runs; this file is run by git itself, from the hooks pre-commit, commit-msg and pre-push beside it, and so sees the
commit and the push as they really are, whatever program asked for them (a terminal, a script, GitHub Desktop).

It is self-contained: install.sh copies it into the repository's common git folder (git rev-parse --git-common-dir,
then hooks/), and the hooks run it with python -I, so nothing from the working tree is imported.

The needles. The path is taken from USERPROFILE (or HOME) as it runs, and is written in no file: its last two parts
(on Windows, Users and the name) joined by any run of / \ - (or %5C, %2F), in any case, in UTF-8 and in UTF-16LE. So
C:\Users\<name>, C:/Users/<name>, /c/Users/<name>, the JSON-escaped form, C--Users-<name>, upper case and the
URL-encoded form all match. The name alone is not looked for, since other words hold it.

What it reads:
- pre-commit: the staged changes against HEAD (against the empty tree on a first commit), git diff --cached --raw:
  every added or changed path name, and the whole content of every added or changed blob (symlinks too; not
  submodules), streamed through one git cat-file --batch process, in chunks, with no size limit.
- commit-msg <file>: the whole message file as git hands it over (comment lines and a verbose diff included).
- pre-push <remote> <url>, git's ref lines on stdin: for each line that is not a delete, the objects the push would
  send: git rev-list --objects <local oid> --not <the remote-tracking refs of that remote> <the remote oid, when that
  object exists here>. With no remote-tracking refs (a push by URL) only the remote oid is left out; with none, the
  whole history of that ref is read. Of those objects it reads every blob's content, every path name rev-list prints
  (file and folder names), every tree raw (the names in it), and every commit and tag object raw (messages, names,
  authors, committers, taggers).
What it cannot see: a commit or push made with --no-verify, or by a program that runs no hooks; plumbing commits
(commit-tree, update-ref) until they are pushed; a merge's content at merge time (pre-merge-commit is not installed;
the push reads it); the GitHub website and API; anything once pushed; compressed content (zip, gzip, Unity packages);
other forms of the path (UTF-16BE, base64, \u005c escapes, Users/./<name>).

Exit 1 with a short message on stderr when it finds the path (the path shown as <profile>, the name as <name>); exit 0
when it finds nothing, and on any error of its own, with one line on stderr: "profile check skipped: <why>".
WG_PROFILE_TRACE=1 in the environment prints one line on success: what was read, and 0 hits.
Its cases: .claude/hooks/git/test_git_hooks.sh.
"""
import os
import re
import subprocess
import sys
import threading

QUIET = 0x08000000 if os.name == 'nt' else 0           # CREATE_NO_WINDOW: no console window for git (GitHub Desktop)
CHUNK = 16 << 20                                       # read a blob in pieces of 16 MB
OVERLAP = 64 << 10                                     # and search each piece with the end of the one before
SHOWN = 5                                              # places named in a refusal


class Failed(Exception):
    """An error of the check's own: it passes, with a warning line."""


def needles():
    prof = os.environ.get('USERPROFILE') or os.environ.get('HOME') or ''
    parts = [p for p in re.split(r'[\\/]+', prof) if p and not p.endswith(':')]
    if len(parts) < 2 or len(parts[-1]) < 2:
        return None, None, None
    parent, name = parts[-2], parts[-1]

    def u16(s):
        return b''.join(re.escape(c.encode('utf-16-le')) for c in s)
    sep = r'(?:[\\/\-]|%5[cC]|%2[fF])+'
    b = (re.escape(parent.encode()) + sep.encode() + re.escape(name.encode())
         + b'|' + u16(parent) + rb'(?:[\\/\-]\x00)+' + u16(name))
    return (re.compile(b, re.I), re.compile(re.escape(parent) + sep + re.escape(name), re.I),
            re.compile(re.escape(name), re.I))


BYTES, TEXT, NAME = needles()
TRACE = os.environ.get('WG_PROFILE_TRACE') == '1'


def masked(s):
    """A text as it may be shown: the path as <profile>, the name alone as <name>."""
    if TEXT is not None:
        s = NAME.sub('<name>', TEXT.sub('<profile>', s))
    return s


def say(s):
    sys.stderr.buffer.write((masked(s).rstrip('\n') + '\n').encode('utf-8', 'replace'))
    sys.stderr.buffer.flush()


def has(data):
    return BYTES.search(data) is not None


def text(b):
    return b.decode('utf-8', 'replace')


def git(args, data=None):
    io = {'input': data} if data is not None else {'stdin': subprocess.DEVNULL}
    try:
        r = subprocess.run(['git', '-c', 'core.fsmonitor=false', '-c', 'core.quotepath=false'] + args,
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE, creationflags=QUIET, **io)
    except OSError as e:
        raise Failed('git could not be run (%s)' % e)
    if r.returncode:
        first = text(r.stderr).strip().splitlines()
        raise Failed('git %s failed: %s' % (args[0], first[0] if first else 'exit %d' % r.returncode))
    return r.stdout


class Read:
    """What was read, for the trace line."""

    def __init__(self):
        self.names = self.bytes = 0
        self.kinds = {}


def scan(out, size):
    """Read exactly size bytes of one object from cat-file's output, in chunks; whether the path is in them."""
    found, tail, left = False, b'', size
    while left > 0:
        piece = out.read(min(CHUNK, left))
        if not piece:
            raise Failed('git cat-file ended in the middle of an object')
        left -= len(piece)
        if not found:
            found = has(tail + piece)
            tail = (tail + piece)[-OVERLAP:]
    return found


def stream(oids, seen):
    """Every object named, through one git cat-file --batch process: yields (oid, type, whether it holds the path)."""
    if not oids:
        return
    try:
        p = subprocess.Popen(['git', 'cat-file', '--batch', '--buffer'], stdin=subprocess.PIPE,
                             stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, creationflags=QUIET)
    except OSError as e:
        raise Failed('git cat-file could not be run (%s)' % e)

    def feed():                                        # in its own thread, so that neither pipe can fill and stall
        try:
            for o in oids:
                p.stdin.write(o.encode('ascii') + b'\n')
        except OSError:
            pass
        finally:
            try:
                p.stdin.close()
            except OSError:
                pass
    t = threading.Thread(target=feed, daemon=True)
    t.start()
    try:
        for _ in oids:
            head = p.stdout.readline()
            if not head:
                raise Failed('git cat-file ended early')
            f = head.split()
            if len(f) == 2 and f[1] in (b'missing', b'ambiguous'):
                continue
            if len(f) != 3:
                raise Failed('git cat-file wrote an unexpected line')
            oid, kind, size = text(f[0]), text(f[1]), int(f[2])
            hit = scan(p.stdout, size)
            p.stdout.read(1)                           # the line end after the content
            seen.bytes += size
            seen.kinds[kind] = seen.kinds.get(kind, 0) + 1
            yield oid, kind, hit
    finally:
        t.join(5)
        try:
            p.stdout.close()
        except OSError:
            pass
        p.wait()


def empty_tree():
    return text(git(['hash-object', '-t', 'tree', '--stdin'], b'')).strip()


def pre_commit(seen, hits):
    try:
        against = text(git(['rev-parse', '-q', '--verify', 'HEAD^{commit}'])).strip()
    except Failed:
        against = empty_tree()                         # a first commit
    raw = git(['diff', '--cached', '--raw', '-z', '--no-abbrev', '--no-renames', '--ignore-submodules=none',
               against, '--'])
    parts, blobs, i = raw.split(b'\0'), [], 0
    while i + 1 < len(parts):
        meta, path = parts[i], parts[i + 1]
        i += 2
        if not meta.startswith(b':'):
            i -= 1
            continue
        f = meta[1:].split()
        if len(f) < 5 or f[4][:1] == b'D':
            continue
        seen.names += 1
        if has(path):
            hits.append('the name of the staged file ' + text(path))
        if f[1] != b'160000' and f[3].strip(b'0'):
            blobs.append((text(f[3]), path))
    where = {}
    for oid, path in blobs:
        where.setdefault(oid, []).append(path)
    for oid, kind, hit in stream(list(where), seen):
        if hit:
            hits.extend('the staged file ' + text(p) for p in where[oid])
    return 'commit'


def commit_msg(path, seen, hits):
    try:
        with open(path, 'rb') as f:
            data = f.read()
    except OSError as e:
        raise Failed('the message file could not be read (%s)' % e.strerror)
    seen.bytes += len(data)
    seen.kinds['message'] = 1
    if has(data):
        hits.append('the commit message')
    return 'commit'


def pre_push(remote, lines, seen, hits):
    tips, olds = [], []
    for line in lines:
        f = line.split()
        if len(f) != 4:
            continue
        local, remote_oid = f[1], f[3]
        if not local.strip('0') or local == remote_oid:  # a delete, or nothing to send
            continue
        tips.append(local)
        if remote_oid.strip('0'):
            olds.append(remote_oid)
    if not tips:
        return 'push'
    nots = []
    if remote and not re.search(r'[*?\[\\]', remote):
        refs = git(['for-each-ref', '--format=%(objectname)', 'refs/remotes/%s/' % remote.rstrip('/')])
        nots += text(refs).split()
    if olds:                                           # the remote's own oids, when those objects are here
        check = git(['cat-file', '--batch-check=%(objectname) %(objecttype)'], ('\n'.join(olds) + '\n').encode())
        nots += [l.split()[0] for l in text(check).splitlines() if len(l.split()) == 2 and l.split()[1] != 'missing']
    nots = list(dict.fromkeys(nots))
    feed = '\n'.join(tips + ['^' + o for o in nots]) + '\n'
    listing = git(['rev-list', '--objects', '--stdin'], feed.encode())
    where = {}
    for line in listing.split(b'\n'):
        oid, _, path = line.partition(b' ')
        if not oid:
            continue
        where.setdefault(text(oid), path)
        if path:
            seen.names += 1
            if has(path):
                hits.append('the file or folder name ' + text(path))
    for t in tips:                                     # an annotated tag pushed: its own object too
        where.setdefault(t, b'')
    for oid, kind, hit in stream(list(where), seen):
        if not hit:
            continue
        name = text(where.get(oid, b''))
        if kind == 'blob':
            hits.append('the file ' + (name or oid[:10]))
        elif kind == 'tree':
            hits.append('a name in the folder ' + (name or '(the top folder)') + ' of a commit')
        elif kind == 'commit':
            hits.append('commit %s (its message, author or committer)' % oid[:10])
        else:
            hits.append('%s object %s (its message or name)' % (kind, oid[:10]))
    return 'push'


def main():
    mode = sys.argv[1] if len(sys.argv) > 1 else ''
    if BYTES is None:
        say('profile check skipped: USERPROFILE and HOME name no profile folder')
        return 0
    seen, hits = Read(), []
    try:
        if mode == 'pre-commit':
            kind = pre_commit(seen, hits)
        elif mode == 'commit-msg' and len(sys.argv) > 2:
            kind = commit_msg(sys.argv[2], seen, hits)
        elif mode == 'pre-push':
            lines = text(sys.stdin.buffer.read()).splitlines()
            kind = pre_push(sys.argv[2] if len(sys.argv) > 2 else '', lines, seen, hits)
        else:
            raise Failed('unknown mode %r' % mode)
    except Failed as e:
        say('profile check skipped: %s' % e)
        return 0
    except Exception as e:                             # any error of its own passes, with the warning line
        say('profile check skipped: %s' % e.__class__.__name__ + (': %s' % e if str(e) else ''))
        return 0
    if not hits:
        if TRACE:
            k = seen.kinds
            say('profile check (%s): read %d names, %d objects (%s), %d bytes; 0 hits'
                % (mode, seen.names, sum(k.values()), ', '.join('%d %s' % (v, n) for n, v in sorted(k.items()))
                   or 'none', seen.bytes))
        return 0
    hits = list(dict.fromkeys(hits))
    more = '' if len(hits) <= SHOWN else '\n  ... and %d more' % (len(hits) - SHOWN)
    say('profile check: this %s would carry the path of your profile folder (<profile>), which holds your user name, '
        'into the repository:\n  %s%s\nTake it out (write ~ or %%USERPROFILE%% for it), then try again.\n'
        'To skip this check for one command: git %s --no-verify. Only the user decides to skip it.'
        % (kind, '\n  '.join(hits[:SHOWN]), more, kind))
    return 1


if __name__ == '__main__':
    try:
        code = main()
    except BaseException as e:                         # even an error while reporting passes
        try:
            sys.stderr.write('profile check skipped: %s\n' % e.__class__.__name__)
        except Exception:
            pass
        code = 0
    sys.exit(code)
