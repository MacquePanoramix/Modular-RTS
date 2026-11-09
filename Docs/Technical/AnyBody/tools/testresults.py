import re, sys
s = open(sys.argv[1], encoding="utf-8", errors="ignore").read()
m = re.search(r'<test-run[^>]*total="(\d+)"[^>]*passed="(\d+)"[^>]*failed="(\d+)"[^>]*skipped="(\d+)"', s)
if m: print("total", m.group(1), "passed", m.group(2), "failed", m.group(3), "skipped", m.group(4))
for c in re.finditer(r'<test-case [^>]*?fullname="([^"]*)"[^>]*?result="([A-Za-z]*)"[^>]*?(?:duration="([0-9.,]*)")?', s):
    print(f"  {c.group(2):8s} {c.group(1).split('.')[-1]}")
for c in re.finditer(r'<test-case [^>]*?fullname="([^"]*)"[^>]*?result="Failed".*?<message><!\[CDATA\[(.*?)\]\]>', s, re.S):
    print("  FAILED", c.group(1).split('.')[-1], ":", c.group(2).strip()[:300])
