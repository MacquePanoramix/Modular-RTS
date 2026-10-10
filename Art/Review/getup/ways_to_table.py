"""The fall's own table of ways, written again from a file of ways: table.py <ways file>. Run from Assets/_WonderGather."""
import sys
from pathlib import Path

ways, way = [], None
for raw in Path(sys.argv[1]).read_text(encoding="utf8").splitlines():
    line = raw.strip()
    if not line or line.startswith("#"): continue
    if line.startswith("way "):
        w = line.split()
        way = dict(name=w[1], least=w[2], most=w[3], gives=w[4] == "gives", stages=[])
        ways.append(way); continue
    f = [x.strip() for x in line.split("|")]
    times = f[1].split(); trunk = f[2].split()
    pose = [trunk[0], trunk[1]] + f[3].split() + f[4].split() + f[5].split() + f[6].split() + [trunk[2] if len(trunk) > 2 else "0"]
    assert len(pose) == 15, line
    way["stages"].append(dict(name=f[0], over=times[0], kept=times[1], pose=pose))

def number(text):
    return text + "f" if "." in text else text

table = []
for w in ways:
    rows = []
    for n, st in enumerate(w["stages"]):
        last = n == len(w["stages"]) - 1
        rows.append("                    Pose(\"%s\", %s, %s, %s%s)," % (st["name"], number(st["over"]), number(st["kept"]), ", ".join(number(x) for x in st["pose"]),
                                                                   ", true" if last and w["gives"] else ""))
    table.append("                new Way { name = \"%s\", frontLeast = %s, frontMost = %s, gives = %s, stages = new[]\n                {\n%s\n                } }," % (
        w["name"], number(w["least"]), number(w["most"]), "true" if w["gives"] else "false", "\n".join(rows)))

p = Path("Scripts/Units/PhysicalFall.cs"); s = p.read_text(encoding="utf8")
head = "        private static Way[] Usual() => new[]\n        {\n"
a = s.index(head) + len(head)
b = s.index("\n        };\n", a)
s = s[:a] + "\n".join(table) + s[b:]
p.write_text(s, encoding="utf8"); print("table written:", ", ".join(w["name"] + " (" + str(len(w["stages"])) + ")" for w in ways))
