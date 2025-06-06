import json

with open("assets/captions.json") as i, open("assets/caption-definitions.json", "w") as o:
    original = json.load(i)
    result = {}

    for _id, priority in original.items():
        obj = {}
        
        if priority != 0:
            obj["Priority"] = priority
        result[_id] = obj
        
    
    json.dump(result, o, indent=4)