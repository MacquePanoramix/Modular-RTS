r"""G6 of the guard (.claude/hooks/guard.sh; November 17, 2026): would this git commit or git push carry the path
of the user's own profile folder? That path names the person, and the repository is public. Luis approved the check
that morning (Docs/Correspondence/2026-11-16_FABLE_WRAPS_UP_AND_A_SUBREDDIT_FOR_THE_BUILDING_RESEARCH.md, section 6).

The guard runs it with the PreToolUse payload on stdin, when the command holds git followed by commit or push (the
guard's own pattern, which also lets git's options stand between them) and not WG_GUARD_OK=G6. If a
commit or a push in the command would carry the path, it writes one line on stdout, saying where (the path shown as
<profile>), and exits 2: the guard then refuses. It exits 0 when nothing is found, and on any error of its own (the
guard fails open). What it cannot read in full is refused too (exit 2): more than WG_G6_MAXMB megabytes (256 unless
set), or more than six seconds.

The path is taken from USERPROFILE (or HOME) as it runs, and is written in no file: its last two parts (on Windows,
Users and the name) joined by any run of / \ - (or %5C, %2F), in any case, in UTF-8 and in UTF-16LE. The name alone
is not looked for, since other words hold it.

What it reads:
- for a commit: every file the commit would store (what is staged, and what the same command stages with git add,
  with git commit -a or with a pathspec), the files' names, the message (-m, -F with a file or stdin, the old message
  for --amend, -C and -c), --author and --trailer, and the heredocs and here-strings of the command;
  a $NAME, ${NAME} or $env:NAME in the command's words is filled in (from what the command sets, else the
  environment) unless the shell would leave it as written (single quotes, or escaped), and so is one in an unquoted
  heredoc or a here-string not in single quotes;
- for a push: every object it would send (git rev-list --objects <what is pushed> --not --remotes): the files, the
  names in its folders, and its commits (their messages and names).
With more than 32 staged files, a staged file that is the same as its working-tree copy is read from the working
tree (M1, November 17 evening: a just-written loose object costs about 16 ms to open the first time; see
worktree_copies).
It does not read the rest of the command: a message file's own path, or a cd into the profile folder, is not what a
commit stores. It cannot see a commit or a push made by a script that the command runs, by a git alias, or through
a variable that the command does not set itself, nor a command substitution other than $(cat <file>) in -m.
With WG_G6_TRACE set it writes on stderr what it read ("commit checked: ...", "push checked: ...") or what failed;
the guard throws stderr away, and the cases use it to show that a pass was read and not an error.
Its cases are in guard_test.sh, section G6.
"""
import json
import os
import re
import shutil
import subprocess
import sys
import time

START = time.monotonic()
SECONDS = 6.0
MB = 1 << 20
EMPTY_TREE = '4b825dc642cb6eb9a060e54bf8d69288fbee4904'
QUIET = 0x08000000 if os.name == 'nt' else 0           # CREATE_NO_WINDOW: no console window for git
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS='0', GIT_TERMINAL_PROMPT='0')
SEPS = ';&|()\n'
CD = {'cd', 'pushd', 'chdir', 'set-location', 'sl', 'push-location'}
SHELLS = {'bash': False, 'sh': False, 'zsh': False, 'dash': False, 'powershell': True, 'pwsh': True}


class TooMuch(Exception):
    """More to read than the limits allow: refused, as it could not be checked."""


class Failed(Exception):
    """A git call that failed: that one check is skipped (the guard fails open)."""


def needles():
    prof = os.environ.get('USERPROFILE') or os.environ.get('HOME') or ''
    parts = [p for p in re.split(r'[\\/]+', prof) if p and not p.endswith(':')]
    if len(parts) < 2 or len(parts[-1]) < 2:
        return None, None
    parent, name = parts[-2], parts[-1]

    def u16(s):
        return b''.join(re.escape(c.encode('utf-16-le')) for c in s)
    sep = r'(?:[\\/\-]|%5[cC]|%2[fF])+'
    b = (re.escape(parent.encode()) + sep.encode() + re.escape(name.encode())
         + b'|' + u16(parent) + rb'(?:[\\/\-]\x00)+' + u16(name))
    return re.compile(b, re.I), re.compile(re.escape(parent) + sep + re.escape(name), re.I)


BYTES, TEXT = needles()
CAP = 256 * MB
try:
    CAP = int(float(os.environ['WG_G6_MAXMB']) * MB)
except (KeyError, ValueError):
    pass
SPENT = [0]
TRACE = bool(os.environ.get('WG_G6_TRACE'))


def trace(s):
    """With WG_G6_TRACE set, a line on stderr (the path shown as <profile>): what was read, or what failed."""
    if TRACE:
        sys.stderr.write((TEXT.sub('<profile>', s) if TEXT else s) + '\n')


def spend(n):
    SPENT[0] += n
    if SPENT[0] > CAP:
        raise TooMuch('more than %g MB to read' % (CAP / MB))
    if time.monotonic() - START > SECONDS:
        raise TooMuch('more than %g seconds' % SECONDS)


def has(data):
    if data is None:
        return False
    if isinstance(data, str):
        data = data.encode('utf-8', 'surrogateescape')
    return BYTES.search(data) is not None


def git(cwd, pre, args, data=None):
    left = SECONDS - (time.monotonic() - START)
    if left <= 0:
        raise TooMuch('more than %g seconds' % SECONDS)
    io = {'input': data} if data is not None else {'stdin': subprocess.DEVNULL}
    try:
        r = subprocess.run(['git', '-c', 'core.fsmonitor=false', '-c', 'core.quotepath=false'] + pre + args,
                           cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, timeout=left, env=ENV,
                           creationflags=QUIET, **io)
    except subprocess.TimeoutExpired:
        raise TooMuch('more than %g seconds' % SECONDS)
    except OSError:
        raise Failed(args[0])
    if r.returncode:
        raise Failed(args[0])
    return r.stdout


def native(p, base):
    """A path as the command writes it, made this machine's own and absolute (it need not exist)."""
    if p.startswith('~'):
        p = os.path.expanduser(p)
    if os.name == 'nt' and p.startswith('/'):
        m = re.match(r'/([A-Za-z])(?:/|$)', p)
        if m:
            p = m.group(1) + ':/' + p[3:]
        elif shutil.which('cygpath'):
            try:
                r = subprocess.run(['cygpath', '-w', p], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
                                   stdin=subprocess.DEVNULL, timeout=2, creationflags=QUIET)
                p = r.stdout.decode('utf-8', 'surrogateescape').strip() or p
            except (OSError, subprocess.SubprocessError):
                pass
    return os.path.normpath(os.path.join(base, p))


def inside(p, top):
    a, b = os.path.normcase(os.path.normpath(p)), os.path.normcase(os.path.normpath(top))
    return a == b or a.startswith(b.rstrip('\\/') + os.sep)


def readfile(path):
    try:
        spend(os.path.getsize(path))
        with open(path, 'rb') as f:
            return f.read()
    except OSError:
        return None


def split0(b):
    return [x.decode('utf-8', 'surrogateescape') for x in b.split(b'\0') if x]


def base(word):
    b = re.split(r'[\\/]', word)[-1].lower()
    return b[:-4] if b.endswith('.exe') else b


# -- reading the command ------------------------------------------------------------------------------------------

HEREDOC = re.compile(r"""(?<!<)<<(?!<)(-?)[ \t]*(?:'([^'\n]+)'|"([^"\n]+)"|\\?([A-Za-z0-9_.@-]+))""")
HERESTR = re.compile(r"""(?<!<)<<<[ \t]*("[^"]*"|'[^']*'|\S+)""")
REDIR = re.compile(r'[<>]+&?[0-9-]*')
VAR = re.compile(r'\$(?:env:([A-Za-z_][A-Za-z0-9_]*)|\{([A-Za-z_][A-Za-z0-9_]*)\}|([A-Za-z_][A-Za-z0-9_]*))')


def heredocs(cmd):
    """The command without its heredoc bodies, and the texts of its heredocs and here-strings: an unquoted one with
    its $NAME and ${NAME} filled in from the environment, as the shell would (a quoted one is left as written)."""
    texts = [m.group(1).strip('\'') if m.group(1).startswith("'") else expand(m.group(1).strip('"'), {})
             for m in HERESTR.finditer(cmd)]
    keep, lines, i = [], cmd.split('\n'), 0
    while i < len(lines):
        line = lines[i]
        keep.append(line)
        i += 1
        for m in HEREDOC.finditer(line):
            end, body = m.group(2) or m.group(3) or m.group(4), []
            while i < len(lines):
                row = lines[i]
                i += 1
                if (row.lstrip('\t') if m.group(1) else row).rstrip('\r') == end:
                    break
                body.append(row)
            body = '\n'.join(body)
            quoted = m.group(2) or m.group(3) or '\\' in m.group(0)
            texts.append(body if quoted else expand(body, {}))
    return '\n'.join(keep), texts


LIT = '\ue000'     # a $ the shell leaves as it is (in single quotes, or escaped): kept apart until the words are expanded


def tokens(s, ps):
    """Words, separators ('|', text) and redirections ('<', text) of a command line, quotes taken off. A $ in single
    quotes or escaped comes out as LIT, so that expand() leaves it; analyse() turns it back into $."""
    out, cur, have = [], [], False
    esc = '`' if ps else '\\'
    i, n = 0, len(s)
    while i < n:
        c = s[i]
        if c == "'":
            j = s.find("'", i + 1)
            j = n if j < 0 else j
            cur.append(s[i + 1:j].replace('$', LIT))
            have, i = True, j + 1
        elif c == '"':
            i += 1
            while i < n and s[i] != '"':
                if s[i] == esc and i + 1 < n and (ps or s[i + 1] in '"\\$`\n'):
                    cur.append(LIT if s[i + 1] == '$' else s[i + 1])
                    i += 2
                else:
                    cur.append(s[i])
                    i += 1
            have, i = True, i + 1
        elif c == esc and i + 1 < n:
            if s[i + 1] != '\n':
                cur.append(LIT if s[i + 1] == '$' else s[i + 1])
                have = True
            i += 2
        elif c == '#' and not have:
            j = s.find('\n', i)
            i = n if j < 0 else j
        elif c in ' \t\r' or c in SEPS or c in '<>':
            word = ''.join(cur)
            if have and not (c in '<>' and re.fullmatch(r'[0-9*]+', word)):    # 2>&1: the 2 is no word
                out.append(word)
            cur, have = [], False
            if c in ' \t\r':
                i += 1
            elif c in SEPS:
                j = i
                while j < n and s[j] in SEPS:
                    j += 1
                out.append(('|', s[i:j]))
                i = j
            else:
                m = REDIR.match(s, i)
                out.append(('<', m.group(0)))
                i = m.end()
        else:
            cur.append(c)
            have = True
            i += 1
    if have:
        out.append(''.join(cur))
    return out


def segments(toks):
    """Simple commands: (their tokens, whether a pipe feeds them)."""
    segs, cur, piped = [], [], False
    for t in toks:
        if isinstance(t, tuple) and t[0] == '|':
            if cur:
                segs.append((cur, piped))
            cur, piped = [], t[1].strip('\n') in ('|', '|&')
        else:
            cur.append(t)
    if cur:
        segs.append((cur, piped))
    return segs


def words_of(seg):
    """A command's words, without its redirections; and the file a < redirection reads, if any."""
    words, infile, pending = [], None, None
    for t in seg:
        if isinstance(t, tuple):
            pending = ('in' if t[1] == '<' else 'skip') if t[1][-1] in '<>' else None
            continue
        if pending:
            if pending == 'in':
                infile = t
            pending = None
            continue
        words.append(t)
    return words, infile


def expand(w, var):
    def one(m):
        if m.group(1):
            return os.environ.get(m.group(1), m.group(0))
        k = m.group(2) or m.group(3)
        return var.get(k.lower(), os.environ.get(k, m.group(0)))
    return VAR.sub(one, w) if '$' in w else w


def cd_target(args):
    k = 0
    while k < len(args):
        a = args[k]
        k += 1
        if a.lower() in ('-path', '-literalpath') and k < len(args):
            return args[k]
        if a.startswith('-') and len(a) > 1:
            continue
        return a
    return '~'


def inner(words):
    """The command a shell is given to run (bash -c "...", powershell -Command ..., cmd /c ...), and its kind."""
    name = base(words[0])
    for k, w in enumerate(words[1:], 1):
        lw = w.lower()
        if k + 1 >= len(words):
            break
        if name in SHELLS and not SHELLS[name] and re.fullmatch(r'-[a-z]*c', lw):
            return words[k + 1], False
        if name in SHELLS and SHELLS[name] and lw in ('-c', '-command'):
            return ' '.join(words[k + 1:]), True
        if name == 'cmd' and lw == '/c':
            return ' '.join(words[k + 1:]), False
    return None


def git_call(words):
    """A git command among the words: (its -C folders, its --git-dir/--work-tree, the subcommand, the rest)."""
    for k, w in enumerate(words):
        if base(w) == 'git':
            break
    else:
        return None
    dirs, where, j = [], [], k + 1
    while j < len(words):
        w = words[j]
        if w == '-C' and j + 1 < len(words):
            dirs.append(words[j + 1])
            j += 2
        elif w in ('-c', '--namespace', '--super-prefix', '--config-env') and j + 1 < len(words):
            j += 2
        elif w in ('--git-dir', '--work-tree') and j + 1 < len(words):
            where.append((w, words[j + 1]))
            j += 2
        elif w.startswith('--git-dir=') or w.startswith('--work-tree='):
            where.append(tuple(w.split('=', 1)))
            j += 1
        elif w.startswith('-'):
            j += 1
        else:
            return dirs, where, w, words[j + 1:]
    return None


class Ctx:
    """Where a git command runs: its folder after cd and -C, and its --git-dir/--work-tree."""

    def __init__(self, cwd, pre):
        self.cwd, self.pre, self._top = cwd, pre, None

    def git(self, args, data=None):
        return git(self.cwd, self.pre, args, data)

    def top(self):
        if self._top is None:
            out = self.git(['rev-parse', '--show-toplevel']).decode('utf-8', 'surrogateescape').strip()
            self._top = os.path.normpath(out)
        return self._top


# -- what a commit or a push would carry ---------------------------------------------------------------------------

def cat(ctx, oids):
    """The objects named, read through git cat-file within the read limit: {oid: (type, bytes)}."""
    oids = list(dict.fromkeys(oids))
    if not oids:
        return {}
    listing = ctx.git(['cat-file', '--batch-check=%(objectname) %(objecttype) %(objectsize)'],
                      ('\n'.join(oids) + '\n').encode())
    want, total = [], 0
    for line in listing.decode('ascii', 'replace').splitlines():
        f = line.split()
        if len(f) == 3 and f[1] in ('blob', 'tree', 'commit', 'tag'):
            want.append(f[0])
            total += int(f[2])
    spend(total)
    raw = ctx.git(['cat-file', '--batch'], ('\n'.join(want) + '\n').encode()) if want else b''
    out, i = {}, 0
    while i < len(raw):
        nl = raw.find(b'\n', i)
        if nl < 0:
            break
        f = raw[i:nl].split()
        if len(f) < 3:
            i = nl + 1
            continue
        size = int(f[2])
        out[f[0].decode()] = (f[1].decode(), raw[nl + 1:nl + 1 + size])
        i = nl + 2 + size
    return out


def parse_raw(raw):
    """git diff --raw -z: the new blobs [(oid, name)] and every name that is added or changed."""
    parts = raw.split(b'\0')
    blobs, names, i = [], [], 0
    while i < len(parts):
        meta = parts[i]
        if not meta.startswith(b':'):
            i += 1
            continue
        f = meta[1:].split()
        n = 3 if f[4][:1] in (b'R', b'C') else 2
        path = parts[i + n - 1].decode('utf-8', 'surrogateescape') if i + n - 1 < len(parts) else ''
        if f[4][:1] != b'D':
            names.append(path)
            if f[1] != b'160000' and f[3].strip(b'0'):
                blobs.append((f[3].decode(), path, f[1].decode()))
        i += n
    return blobs, names


MANY = 32


def worktree_copies(ctx, blobs):
    """M1 (November 17, evening). A blob that git add has just written is a new loose object, and on this machine the
    first read of each such file costs about 16 ms, whoever reads it (git cat-file with any format, --batch, or
    Python opening the file: measured; the second read is fast). The breaker's 500, 1500 and 3000 files were that
    cost, not the --batch-check format: a commit of 3000 new files needed about 50 s just to be read. So, for more
    than MANY staged blobs, a staged file that is the same as its copy in the working tree (not in git diff
    --name-only), is a plain file (mode 100644 or 100755) and has no filter, working-tree-encoding or ident attribute
    is read from the working tree,
    which holds the same text up to line endings and was read moments ago by git add. The rest go to cat-file.
    Returns {oid: bytes} for the copies read."""
    if len(blobs) <= MANY:
        return {}
    top = ctx.top()
    tctx = Ctx(top, ctx.pre)                          # paths from git diff are from the top
    dirty = set(split0(tctx.git(['diff', '--name-only', '-z', '--no-renames', '--'])))
    cand = [(o, p) for o, p, m in blobs if m in ('100644', '100755') and p not in dirty]
    if not cand:
        return {}
    # filter, working-tree-encoding and ident each make the staged bytes differ from the working copy's (mend,
    # November 17-18: the latter two added); a path with any of them set goes to cat-file
    attrs = split0(tctx.git(['check-attr', '-z', '--stdin', 'filter', 'working-tree-encoding', 'ident'],
                            ('\0'.join(p for _, p in cand) + '\0').encode('utf-8', 'surrogateescape')))
    filtered = {attrs[i] for i in range(0, len(attrs) - 2, 3) if attrs[i + 2] not in ('unspecified', 'unset')}
    out = {}
    for o, p in cand:
        if p not in filtered and o not in out:
            data = readfile(os.path.join(top, p))
            if data is not None:
                out[o] = data
    return out


def parse_add(args):
    """git add: (its pathspecs, whether untracked files are taken, whether ignored ones are)."""
    specs, every, update, force, dd = [], False, False, False, False
    k = 0
    while k < len(args):
        a = args[k]
        k += 1
        if dd or not a.startswith('-') or a == '-':
            specs.append(a)
        elif a == '--':
            dd = True
        elif a in ('--all', '--no-ignore-removal'):
            every = True
        elif a == '--update':
            update = True
        elif a == '--force':
            force = True
        elif a.startswith('--pathspec-from-file='):
            b = readfile(a.split('=', 1)[1])
            specs += b.decode('utf-8', 'surrogateescape').splitlines() if b else []
        elif not a.startswith('--'):
            every, update, force = every or 'A' in a, update or 'u' in a, force or 'f' in a
    if (every or update) and not specs:
        specs = [':/']
    return specs, not update, force


def staged_later(ctx, specs, untracked, force):
    """Names (from the top) of the files that this command would stage with these pathspecs."""
    top, ps = ctx.top(), []
    for s in specs:
        if s.startswith(':'):
            ps.append(s)
        else:
            p = native(s, ctx.cwd)
            if inside(p, top):
                ps.append(os.path.relpath(p, ctx.cwd).replace('\\', '/'))
    if not ps:
        return []
    names = split0(ctx.git(['diff', '--name-only', '-z', '--no-renames', '--'] + ps))
    if untracked:
        names += split0(ctx.git(['ls-files', '-o', '--full-name', '-z'] + ([] if force else ['--exclude-standard'])
                                + ['--'] + ps))
    return names


CVAL = {'-m': 'm', '--message': 'm', '-F': 'F', '--file': 'F', '-C': 'C', '--reuse-message': 'C', '-c': 'C',
        '--reedit-message': 'C', '-t': 'T', '--template': 'T', '--author': 'v', '--trailer': 'v',
        '--pathspec-from-file': 'P', '--date': '', '--cleanup': '', '--fixup': '', '--squash': ''}


def check_commit(ctx, args, stdin, adds, hits):
    top = ctx.top()
    msgs, specs, reuse = [], [], []
    state = {'every': False, 'amend': False, 'given': False}

    def take(kind, v):
        if kind in ('m', 'F', 'C'):
            state['given'] = True
        if kind == 'm':
            msgs.append(('the commit message', v))
            for f in re.findall(r'\$\(\s*(?:cat|type|get-content|gc)\s+([^)]*)\)', v, re.I):
                for w in f.split():
                    if not w.startswith(('-', '<')):
                        msgs.append(('the commit message', readfile(native(w.strip('"\''), ctx.cwd))))
        elif kind in ('F', 'T'):
            msgs.append(('the commit message', stdin() if v == '-' else readfile(native(v, ctx.cwd))))
        elif kind == 'C':
            reuse.append(v)
        elif kind == 'v':
            msgs.append(('--author or --trailer', v))
        elif kind == 'P':
            b = readfile(native(v, ctx.cwd))
            specs.extend(x for x in (b.decode('utf-8', 'surrogateescape').splitlines() if b else []) if x)

    k, dd = 0, False
    while k < len(args):
        a = args[k]
        k += 1
        if dd or not a.startswith('-') or a == '-':
            specs.append(a)
        elif a == '--':
            dd = True
        elif a.startswith('--'):
            key, eq, val = a.partition('=')
            if CVAL.get(key):
                if not eq:
                    if k >= len(args):
                        break
                    val, k = args[k], k + 1
                take(CVAL[key], val)
            elif key in CVAL and not eq:
                k += 1
            elif key == '--all':
                state['every'] = True
            elif key == '--amend':
                state['amend'] = True
        else:
            j = 1
            while j < len(a):
                ch = a[j]
                j += 1
                if ch in 'mFCct':
                    val = a[j:]
                    if not val:
                        if k >= len(args):
                            break
                        val, k = args[k], k + 1
                    take(CVAL['-' + ch], val)
                    break
                if ch in 'Su':
                    break
                if ch == 'a':
                    state['every'] = True
    if state['amend'] and not state['given']:
        reuse.append('HEAD')
    for r in reuse:
        try:
            msgs.append(('the message taken from ' + r, ctx.git(['log', '-1', '--format=%B', r, '--'])))
        except Failed:
            pass

    basis = []
    if state['amend']:
        try:
            ctx.git(['rev-parse', '-q', '--verify', 'HEAD~1^{commit}'])
            basis = ['HEAD~1']
        except Failed:
            basis = [EMPTY_TREE]
    blobs, names = parse_raw(ctx.git(['diff', '--cached', '--raw', '-z', '--no-abbrev', '--no-renames']
                                     + basis + ['--']))
    later = []
    for actx, (aspecs, untracked, force) in adds:
        try:
            if os.path.normcase(actx.top()) == os.path.normcase(top):
                later += staged_later(actx, aspecs, untracked, force)
        except Failed:
            pass
    if state['every']:
        later += staged_later(ctx, [':/'], False, False)
    if specs:
        later += staged_later(ctx, specs, False, False)
    later = list(dict.fromkeys(later))

    for where, text in msgs:
        if has(text):
            hits.append(('commit', where))
    for name in names + later:
        if has(name):
            hits.append(('commit', 'a file name, ' + name))
    copies = worktree_copies(ctx, blobs)
    objs = cat(ctx, [o for o, _, _ in blobs if o not in copies])
    for o, name, _ in blobs:
        data = copies[o] if o in copies else objs[o][1] if o in objs else None
        if data is not None and has(data):
            hits.append(('commit', name + ' (staged)'))
    for name in later:
        if has(readfile(os.path.join(top, name))):
            hits.append(('commit', name + ' (staged by this same command)'))
    trace('commit checked: %d messages, %d staged files (%d read from the working tree), %d staged by the command'
          % (len(msgs), len(blobs), len(copies), len(later)))


def check_push(ctx, args, hits):
    pos, every, mirror, tags, dd = [], False, False, False, False
    k = 0
    while k < len(args):
        a = args[k]
        k += 1
        if dd or not a.startswith('-'):
            pos.append(a)
        elif a == '--':
            dd = True
        elif a in ('--delete', '-d'):
            return
        elif a in ('--all', '--branches'):
            every = True
        elif a == '--mirror':
            mirror = True
        elif a in ('--tags', '--follow-tags'):
            tags = True
        elif a in ('--repo', '--receive-pack', '--exec', '--push-option', '-o'):
            k += 1
        elif not a.startswith('--') and 'd' in a[1:]:
            return
    specs, revs, i = pos[1:], [], 0
    while i < len(specs):
        s = specs[i]
        i += 1
        if s == 'tag' and i < len(specs):
            revs.append('refs/tags/' + specs[i])
            i += 1
            continue
        src = s.lstrip('+').split(':')[0]
        if src:
            revs.append(src)
    if mirror:
        revs = ['--all']
    elif every:
        revs.append('--branches')
    elif not specs:
        revs = ['HEAD']
    if tags:
        revs.append('--tags')
    if not revs:
        return
    listing = ctx.git(['rev-list', '--objects'] + revs + ['--not', '--remotes', '--'])
    where = {}
    for line in listing.decode('utf-8', 'surrogateescape').splitlines():
        oid, _, path = line.partition(' ')
        if oid:
            where[oid] = path
    trace('push checked: %d objects not yet on a remote' % len(where))
    for oid, (kind, data) in cat(ctx, list(where)).items():
        if has(data):
            if kind == 'commit':
                hits.append(('push', 'commit %s (its message, or its author or committer)' % oid[:9]))
            elif kind == 'tree':
                hits.append(('push', 'a file name in %s, in a commit not yet pushed' % (where[oid] or 'the top folder')))
            else:
                hits.append(('push', '%s, in a commit not yet pushed' % (where[oid] or kind + ' ' + oid[:9])))


def analyse(cmd, ps, cwd, depth, hits):
    text, extra = heredocs(cmd)
    var, adds, prev, commits = {}, [], [], 0
    for seg, piped in segments(tokens(text, ps)):
        words, infile = words_of(seg)
        words = [expand(w, var).replace(LIT, '$') for w in words]
        infile = infile.replace(LIT, '$') if infile else infile
        while words and re.match(r'[A-Za-z_][A-Za-z0-9_]*=', words[0]):
            key, _, val = words[0].partition('=')
            var[key.lower()] = val
            words = words[1:]
        if ps and len(words) >= 3 and words[0].startswith('$') and words[1] == '=':
            var[words[0][1:].lower()] = words[2]
            words = []
        if not words:
            prev = []
            continue
        if base(words[0]) in CD:
            d = native(cd_target(words[1:]), cwd)
            if os.path.isdir(d):
                cwd = d
        elif inner(words) and depth < 3:
            sub, sub_ps = inner(words)
            analyse(sub, sub_ps, cwd, depth + 1, hits)
        else:
            g = git_call(words)
            if g:
                dirs, gwhere, cmdname, args = g
                eff = cwd
                for d in dirs:
                    eff = native(d, eff)
                ctx = Ctx(eff, ['%s=%s' % (key, native(val, eff)) for key, val in gwhere])

                def stdin(infile=infile, piped=piped, prev=prev, eff=eff):
                    texts = []
                    if infile:
                        texts.append(readfile(native(infile, eff)))
                    if piped and prev:
                        if base(prev[0]) in ('cat', 'type', 'get-content', 'gc'):
                            texts += [readfile(native(w, eff)) for w in prev[1:] if not w.startswith('-')]
                        else:
                            texts.append(' '.join(prev[1:]).encode('utf-8', 'surrogateescape'))
                    return b'\n'.join(t for t in texts if t)
                try:
                    if cmdname in ('add', 'stage'):
                        adds.append((ctx, parse_add(args)))
                    elif cmdname == 'commit':
                        commits += 1
                        check_commit(ctx, args, stdin, adds, hits)
                    elif cmdname == 'push':
                        check_push(ctx, args, hits)
                except Failed as e:
                    trace('skipped: a git call failed (git %s)' % e)
        prev = words
    if commits and any(has(t) for t in extra):
        hits.append(('commit', 'a heredoc or here-string of this command (a message, or a file written for it)'))


def say(s):
    sys.stdout.buffer.write(' '.join(s.split()).encode('utf-8', 'replace'))


def main():
    if BYTES is None:
        return 0
    hits = []
    try:
        p = json.loads(sys.stdin.buffer.read().decode('utf-8', 'surrogateescape'))
        cmd = p['tool_input']['command']
        if not isinstance(cmd, str):
            return 0
        ps = str(p.get('tool_name', '')).lower() == 'powershell'
        cwd = p.get('cwd')
        cwd = cwd if isinstance(cwd, str) and os.path.isdir(cwd) else os.getcwd()
        analyse(cmd, ps, cwd, 0, hits)
    except TooMuch as e:
        say("this is too large to check for the path of the user's own profile folder (%s). Check it by hand "
            "(git grep -i for a commit, git log -p for a push) before going on." % e)
        return 2
    except Exception as e:
        trace('error: %r' % e)
        return 0
    if not hits:
        return 0
    kind, where = hits[0]
    more = ' (%d places; the first is shown)' % len(hits) if len(hits) > 1 else ''
    say("this %s would carry the path of the user's own profile folder, which names the person, into a public "
        "repository: %s%s. Take it out (write ~ or %%USERPROFILE%% for it), or leave that file out; only Luis can "
        "approve carrying it." % (kind, TEXT.sub('<profile>', where), more))
    return 2


if __name__ == '__main__':
    sys.exit(main())
