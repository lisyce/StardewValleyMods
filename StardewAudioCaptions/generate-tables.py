import json
from collections import defaultdict

result = defaultdict(list)
total = 0

with open("i18n/default.json", 'r') as i18n:
    data = json.load(i18n)
    
    for k, v in data.items():
        if not k.endswith(".caption"):
            continue
        
        parts = k.split(".")
        category = data[parts[0] + '.category']
        result[category].append((".".join(parts[:2]), v))
        total += 1


markdown = f"# List of Captions\n\nStardew Audio Captions currently features {total} captions."
for k, v in result.items():
    markdown += f"\n\n## {k}\n\n{len(v)} captions in this category\n\n| Internal Id | English Caption |\n|-|-|"
    for _id, eng in sorted(v, key=lambda x: x[1]):
        markdown += f"\n| `{_id}` | {eng} |"
    
with open("docs/caption-list.md", "w") as out:
    out.write(markdown)