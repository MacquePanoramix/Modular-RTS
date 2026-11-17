r"""Git's own side of G6 (November 17, 2026): would this commit or this push carry the path of the user's own profile
folder into the repository? That path holds the person's user name, and the repository is public. Luis approved the
check for this repository only. The PreToolUse guard (.claude/hooks/guard.sh, rule G6) reads a command before it
runs; this file is run by git itself, from the hooks pre-commit, commit-msg and pre-push beside it, and so sees the
commit and the push as they really are, whatever program asked for them (a terminal, a script, GitHub Desktop).
Mended November 17-18 after two reviews (Docs/Research/2026-11-16_HarnessGuideReview/g6_git_report.md, "Mend").

It is self-contained: install.sh copies it into the repository's common git folder (git rev-parse --git-common-dir,
then hooks/), and the hooks run it with python -I, so nothing from the working tree is imported.

The needles. The path is taken both from USERPROFILE (or HOME, when USERPROFILE is empty) as it runs and from the
profile folder the system reports for the user running it (on Windows the known folder FOLDERID_Profile, which does
not read the environment; elsewhere the password database), so a changed USERPROFILE does not remove the real one.
It is written in no file. Of each: its last two parts (on Windows, Users and the name) joined by any run of
/ \ - _ whitespace or line breaks (or %5C, %2F), also with dot segments (Users\.\<name>), in any case, in UTF-8,
UTF-16LE, UTF-16BE and UTF-32LE; and the name must end there: a letter or digit right after it is another user
(Users/<name>x is not this profile). So C:\Users\<name>, C:/Users/<name>, /c/Users/<name>, the JSON-escaped form,
C--Users-<name>, upper case and the URL-encoded form all match. The name alone is not looked for, since other words
hold it.

Every git call runs with GIT_NO_REPLACE_OBJECTS=1, so what is read is what git stores and sends (a replace ref
cannot show the check a clean object in place of the real one).

What it reads:
- pre-commit: the staged changes against HEAD (against the empty tree on a first commit), git diff --cached --raw:
  every added or changed path name, and the whole content of every added or changed blob (symlinks too; not
  submodules), streamed through one git cat-file --batch process, in chunks, with no size limit. With more than 32
  staged files, a plain file whose working-tree copy is that very blob (its git hash, as it is or with CRLF made LF,
  is the staged oid) and that no filter, working-tree-encoding or ident attribute touches is read from the working
  tree: the first read of a just-written loose object costs about 16 ms here. With more than 300 it says first
  "profile check: reading N files".
- commit-msg <file>: the message as git will store it. Git cleans the message after this hook, so: it is cut at a
  scissors line only when it is git's own (followed by git's two lines "Do not modify or remove the line above." and
  "Everything below it will be ignored."); comment lines are left out only when git will drop them (the file holds the
  template git writes when it opens the editor, whose text says lines starting with the comment character will be
  ignored, or commit.cleanup is strip); otherwise every line is read.
- pre-push <remote> <url>, git's ref lines on stdin: for each line that is not a delete, the objects the push would
  send: git rev-list --objects <local oid> --not <the remote-tracking refs of that remote> <the remote oid, when that
  object exists here>. With no remote-tracking refs (a push by URL) only the remote oid is left out; with none, the
  whole history of that ref is read. Of those objects it reads every blob's content, every path name rev-list prints
  (file and folder names), every tree raw (the names in it), and every commit and tag object raw (messages, names,
  authors, committers, taggers); and, for every commit pushed, every full path it adds or changes (one git diff-tree
  --stdin -r -m --root process), since rev-list names an object only once.
What it cannot see: a commit or push made with --no-verify, or by a program that runs no hooks; plumbing commits
(commit-tree, update-ref) until they are pushed; a merge's content at merge time (pre-merge-commit is not installed;
the push reads it); the GitHub website and API; anything once pushed; a remote-tracking ref made by hand or fetched
from elsewhere (its commits count as already on the remote); a --cleanup given on the command line, or git's template
in another language (the push still reads every stored message); compressed or otherwise encoded content (zip, gzip,
Unity packages, base64, \u005c escapes, %255C).

Exit 1 with a short message on stderr when it finds the path (the path shown as <profile>, the name as <name>); exit 0
when it finds nothing, and on any error of its own, with one line on stderr: "profile check skipped: <why>".
WG_PROFILE_TRACE=1 in the environment prints one line on success: what was read, and 0 hits.
Its cases: .claude/hooks/git/test_git_hooks.sh.
"""
import hashlib
import os
import re
import subprocess
import sys
import threading

os.environ['GIT_NO_REPLACE_OBJECTS'] = '1'             # every git below reads the real objects, not replacements

QUIET = 0x08000000 if os.name == 'nt' else 0           # CREATE_NO_WINDOW: no console window for git (GitHub Desktop)
CHUNK = 16 << 20                                       # read a blob in pieces of 16 MB
OVERLAP = 64 << 10                                     # and search each piece with the end of the one before
SHOWN = 5                                              # places named in a refusal
MANY = 32                                              # more staged files than this: read the working-tree copies
LOUD = 300                                             # more than this: say first how many files are read
NOTES = []                                             # for the trace line


class Failed(Exception):
    """An error of the check's own: it passes, with a warning line."""


def system_profile():
    """The profile folder the system reports for the user running this, not read from the environment."""
    try:
        if os.name == 'nt':
            import ctypes
            from ctypes import wintypes

            class GUID(ctypes.Structure):
                _fields_ = [('a', wintypes.DWORD), ('b', wintypes.WORD), ('c', wintypes.WORD),
                            ('d', ctypes.c_ubyte * 8)]
            profile = GUID(0x5E6C858F, 0x0E22, 0x4760,       # FOLDERID_Profile
                           (ctypes.c_ubyte * 8)(0x9A, 0xFE, 0xEA, 0x33, 0x17, 0xB6, 0x71, 0x73))
            out = ctypes.c_wchar_p()
            f = ctypes.windll.shell32.SHGetKnownFolderPath
            f.argtypes = [ctypes.POINTER(GUID), wintypes.DWORD, wintypes.HANDLE, ctypes.POINTER(ctypes.c_wchar_p)]
            f.restype = ctypes.c_long
            hr = f(ctypes.byref(profile), 0, None, ctypes.byref(out))
            if hr:
                return None, 'SHGetKnownFolderPath returned 0x%08x' % (hr & 0xffffffff)
            value = out.value
            ctypes.windll.ole32.CoTaskMemFree(out)
            return value, None
        import pwd
        return pwd.getpwuid(os.getuid()).pw_dir, None
    except Exception as e:
        return None, e.__class__.__name__


def pair(prof):
    parts = [p for p in re.split(r'[\\/]+', prof or '') if p and not p.endswith(':')]
    if len(parts) < 2 or len(parts[-1]) < 2:
        return None
    return parts[-2], parts[-1]


SEPS = ['\\', '/', '-', '_', ' ', '\t', '\r', '\n', '\f', '\v', '%5C', '%2F']
CODECS = ('utf-8', 'utf-16-le', 'utf-16-be', 'utf-32-le')


def lit(s, codec):
    return b''.join(re.escape(c.encode(codec)) for c in s)


def form(parent, name, codec):
    """The path's pattern in one encoding: parent, a run of separators (dot segments allowed), the name, its end."""
    w = len('a'.encode(codec))
    atom = b'(?:' + b'|'.join(lit(s, codec) for s in SEPS) + b')'
    sep = atom + b'+(?:' + lit('.', codec) + atom + b'+)*'
    z = b'\\x00' * (w - 1)
    after = b'[a-z0-9]' + z if codec.endswith('le') or w == 1 else z + b'[a-z0-9]'
    return lit(parent, codec) + sep + lit(name, codec) + b'(?!' + after + b')'


def needles():
    env = os.environ.get('USERPROFILE') or os.environ.get('HOME') or ''
    sysp, why = system_profile()
    if why:
        NOTES.append('the system profile folder was not read (%s), the environment\'s alone is used' % why)
    pairs = []
    for p in (pair(env), pair(sysp)):
        if p and p[0].lower() + '/' + p[1].lower() not in [a.lower() + '/' + b.lower() for a, b in pairs]:
            pairs.append(p)
    if not pairs:
        return None, None, None, ()
    if len(pairs) > 1:
        NOTES.append('the environment\'s and the system\'s profile folders differ: both looked for')
    tsep = r'(?:[\\/\-_\s]|%5C|%2F)'
    tsep = tsep + r'+(?:\.' + tsep + r'+)*'
    b = b'|'.join(form(p, n, c) for p, n in pairs for c in CODECS)
    t = '|'.join(re.escape(p) + tsep + re.escape(n) + r'(?![a-z0-9])' for p, n in pairs)
    names = '|'.join(re.escape(n) for _, n in pairs)
    # A quick first test: the name itself, in each encoding, lowered as bytes.lower lowers (ASCII only, as re.I on
    # bytes folds): wherever BYTES matches, the lowered bytes hold one of these. Four encodings and the separator runs
    # made the pattern alone about five times slower on long files (the mend's 200 MB case, 15 s).
    pre = tuple(dict.fromkeys(n.encode(c).lower() for _, n in pairs for c in CODECS))
    return re.compile(b, re.I), re.compile(t, re.I), re.compile(names, re.I), pre


BYTES, TEXT, NAME, PRE = needles()
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
    low = data.lower()
    if not any(p in low for p in PRE):
        return False
    return BYTES.search(data) is not None


def text(b):
    return b.decode('utf-8', 'replace')


def git(args, data=None):
    io = {'input': data} if data is not None else {'stdin': subprocess.DEVNULL}
    try:
        r = subprocess.run(['git', '--no-replace-objects', '-c', 'core.fsmonitor=false', '-c', 'core.quotepath=false']
                           + args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, creationflags=QUIET, **io)
    except OSError as e:
        raise Failed('git could not be run (%s)' % e)
    if r.returncode:
        first = text(r.stderr).strip().splitlines()
        raise Failed('git %s failed: %s' % (args[0], first[0] if first else 'exit %d' % r.returncode))
    return r.stdout


def config(key):
    try:
        return text(git(['config', '--get', key])).strip()
    except Failed:
        return ''


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
        p = subprocess.Popen(['git', '--no-replace-objects', 'cat-file', '--batch', '--buffer'], stdin=subprocess.PIPE,
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


def raw_entries(raw):
    """The (status, mode, oid, path) of git's --raw -z output, with --no-renames (one path per entry)."""
    parts, out, i = raw.split(b'\0'), [], 0
    while i + 1 < len(parts):
        meta, path = parts[i], parts[i + 1]
        if not meta.startswith(b':'):                  # a commit id line or an empty piece
            i += 1
            continue
        i += 2
        f = meta[1:].split()
        if len(f) >= 5:
            out.append((f[4][:1], f[1], f[3], path))
    return out


def worktree_copies(cand):
    """For many staged files: {oid: bytes} of those whose working-tree copy is the staged blob itself. A just-written
    loose object costs about 16 ms on its first read here, whoever reads it; the working-tree file, just read by git
    add, does not. A copy counts only when its git hash, as it is or with CRLF made LF (text, eol, core.autocrlf), is
    the staged oid, so the bytes read are the blob's bytes; a path that a filter, working-tree-encoding or ident
    attribute touches goes to cat-file without being tried."""
    try:
        top = text(git(['rev-parse', '--show-toplevel'])).strip()
        algo = text(git(['rev-parse', '--show-object-format'])).strip() or 'sha1'
        hashlib.new(algo)
    except (Failed, ValueError):
        return {}
    first = {}
    for oid, path, mode in cand:
        if mode in (b'100644', b'100755'):
            first.setdefault(oid, path)
    if not first:
        return {}
    paths = list(first.values())
    attrs = git(['check-attr', '-z', '--stdin', 'filter', 'working-tree-encoding', 'ident'], b'\0'.join(paths) + b'\0')
    a = attrs.split(b'\0')
    touched = {a[i] for i in range(0, len(a) - 2, 3) if a[i + 2] not in (b'unspecified', b'unset')}
    out = {}
    for oid, path in first.items():
        if path in touched:
            continue
        try:
            full = os.path.join(top, path.decode('utf-8'))
            if os.path.islink(full) or os.path.getsize(full) > CHUNK:
                continue
            with open(full, 'rb') as f:
                data = f.read()
        except (OSError, UnicodeDecodeError):
            continue
        for d in (data, data.replace(b'\r\n', b'\n') if b'\r\n' in data else None):
            if d is not None and hashlib.new(algo, b'blob %d\0' % len(d) + d).hexdigest() == oid:
                out[oid] = d
                break
    return out


def pre_commit(seen, hits):
    try:
        against = text(git(['rev-parse', '-q', '--verify', 'HEAD^{commit}'])).strip()
    except Failed:
        against = empty_tree()                         # a first commit
    raw = git(['diff', '--cached', '--raw', '-z', '--no-abbrev', '--no-renames', '--ignore-submodules=none',
               against, '--'])
    blobs = []
    for status, mode, oid, path in raw_entries(raw):
        if status == b'D':
            continue
        seen.names += 1
        if has(path):
            hits.append('the name of the staged file ' + text(path))
        if mode != b'160000' and oid.strip(b'0'):
            blobs.append((text(oid), path, mode))
    where = {}
    for oid, path, _ in blobs:
        where.setdefault(oid, []).append(path)
    if len(where) > LOUD:
        say('profile check: reading %d files' % len(where))
    copies = worktree_copies(blobs) if len(where) > MANY else {}
    for oid, data in copies.items():
        seen.bytes += len(data)
        seen.kinds['blob'] = seen.kinds.get('blob', 0) + 1
        if has(data):
            hits.extend('the staged file ' + text(p) for p in where[oid])
    if len(where) > MANY:
        NOTES.append('%d read from the working tree' % len(copies))
    for oid, kind, hit in stream([o for o in where if o not in copies], seen):
        if hit:
            hits.extend('the staged file ' + text(p) for p in where[oid])
    return 'commit'


SCISSORS = b' ------------------------ >8 ------------------------'
SCISSORS_1 = b' Do not modify or remove the line above.'
SCISSORS_2 = b' Everything below it will be ignored.'
TEMPLATE = re.compile(rb'^(.{1,8}?) Please enter (?:the|a) commit message')


def kept_message(data):
    """The message as git will store it, as far as the file shows: (the bytes, what was left out)."""
    lines = data.split(b'\n')
    plain = [l[:-1] if l.endswith(b'\r') else l for l in lines]
    left = []
    for i, l in enumerate(plain):                      # git's own scissors line: the line and git's two below it
        if l.endswith(SCISSORS) and len(l) > len(SCISSORS) and i + 2 < len(plain):
            c = l[:-len(SCISSORS)]
            if plain[i + 1] == c + SCISSORS_1 and plain[i + 2] == c + SCISSORS_2:
                lines, plain = lines[:i], plain[:i]
                left.append('a scissors cut')
                break
    drop = None
    for i, l in enumerate(plain):                      # git's editor template says comment lines will be ignored
        m = TEMPLATE.match(l)
        if not m:
            continue
        c = m.group(1)
        block = []
        for k in plain[i:]:
            if not k.startswith(c):
                break
            block.append(k[len(c):].strip())
        if (b"Lines starting with '" + c + b"' will be ignored") in b' '.join(block):
            drop = c
            break
    if drop is None and config('commit.cleanup').lower() == 'strip':
        c = (config('core.commentString') or config('core.commentChar') or '#').encode('utf-8')
        if c != b'auto':
            drop = c
    if drop is not None:
        n = len(lines)
        lines = [l for l in lines if not l.startswith(drop)]
        left.append('%d comment lines' % (n - len(lines)))
    return b'\n'.join(lines), left


def commit_msg(path, seen, hits):
    try:
        with open(path, 'rb') as f:
            data = f.read()
    except OSError as e:
        raise Failed('the message file could not be read (%s)' % e.strerror)
    kept, left = kept_message(data)
    if left:
        NOTES.append('message: left out as git will, ' + ' and '.join(left))
    seen.bytes += len(kept)
    seen.kinds['message'] = 1
    if has(kept):
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
    commits = []
    for oid, kind, hit in stream(list(where), seen):
        if kind == 'commit':
            commits.append(oid)
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
    if commits:                                        # every full path each pushed commit adds or changes
        raw = git(['diff-tree', '--stdin', '-r', '-z', '--root', '-m', '--no-renames', '--no-commit-id',
                   '--ignore-submodules=none'], ('\n'.join(commits) + '\n').encode())
        n = 0
        for status, _, _, path in raw_entries(raw):
            if status == b'D':
                continue
            n += 1
            if has(path):
                hits.append('the file or folder name ' + text(path))
        NOTES.append('%d paths of %d commits' % (n, len(commits)))
    return 'push'


def main():
    mode = sys.argv[1] if len(sys.argv) > 1 else ''
    if BYTES is None:
        say('profile check skipped: neither USERPROFILE and HOME nor the system name a profile folder')
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
            say('profile check (%s): read %d names, %d objects (%s), %d bytes%s; 0 hits'
                % (mode, seen.names, sum(k.values()), ', '.join('%d %s' % (v, n) for n, v in sorted(k.items()))
                   or 'none', seen.bytes, ''.join('; ' + x for x in NOTES)))
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
