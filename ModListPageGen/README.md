# Shareable Mod List Generator

**Shareable Mod List Generator** is a Stardew Valley mod which provides console commands to generate a shareable mod list
based on the mods SMAPI currently has loaded. You can download it from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/32609).

## Usage

This tool has two parts: generating a mod list in JSON format, and sharing it as nicely-formatted HTML.
Generating the JSON list is done via console command in the SMAPI console.
You can generate a shareable HTML version through another console command or you can upload the JSON to the web interface's [upload](https://modlists.barleyzp.com/upload) page.

### Generating the JSON

1. Install this mod.
2. Launch the game with SMAPI.
3. Run the command `mod_list_json "title" "author"`.
   - You can optionally append `true` to the end to skip calling the Nexus Mods API. If you do this, category information and other stats may be unavailable. For example: `mod_list_json "title" "author" true`
4. The JSON mod list will be saved to the `GeneratedModListsJson` folder inside this mod's folder.

Make sure to back up the mod list elsewhere for safekeeping! At this point, if you wish to edit your mod list (say, to include links that could be automatically added),
you may do so. It is recommended that you use an editor that has support for JSON schemas so you can validate that your format is correct.

### Generating and Sharing the HTML

There are two ways to generate the HTML mod list and get a shareable link. Option 1 (console command) will also automatically save the HTML to your computer.
Shareable links will be valid for 4 weeks. Anyone with this link will be able to access your mod list.

#### Option 1 (Console Command)

Run the command `mod_list_share "title" "theme"`.

- The list will be saved in the `GeneratedModListsHtml` folder inside this mod's folder.
- A shareable link will be printed to the SMAPI console.
- Valid color themes are: `default`, `sakura`, `stardrop`, `coffee`, and `sage`. For example: `mod_list_share "title" sakura`.
- The `title` *must* match the title you provided to the `mod_list_json` command.

#### Option 2 (Web UI)

Head to https://modlists.barleyzp.com/upload and upload the JSON file that was generated earlier.

- If you want to have the HTML saved to your computer so you can always have it, save the HTML from your web browser. It will function without being hosted on the web server.