import json
from collections import defaultdict

result = defaultdict(list)

with open("i18n/default.json", 'r') as i18n:
   data = json.load(i18n)
   
   for k, v in data.items():
     if not k.endswith(".caption"):
       continue
     
     parts = k.split(".")
     category = data[parts[0] + '.category']
     result[category].append((".".join(parts[:2]), v))
     

markdown = "# List of Captions"
for k, v in result.items():
  markdown += f"\n\n## {k}\n\n| Internal Id | English Caption |\n|-|-|"
  for id, eng in v:
    markdown += f"\n| `{id}` | {eng} |"
    
with open("docs/caption-list.md", "w") as out:
  out.write(markdown)