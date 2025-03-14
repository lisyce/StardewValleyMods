# BarleyZP's Allergies

**BarleyZP's Allergy Mod** is a [Stardew Valley](https://www.stardewvalley.net/) mod which allows players to configure food allergies for an extra challenge. It is compatible with version 1.6+.

![Stardew Valley player is prompted to choose whether or not to eat cheese, which they are allergic to.](docs/screenshots/CheeseAllergenPopup.png)

# Installation

1. Install [SMAPI](https://smapi.io/)
1. Install [this mod](https://www.nexusmods.com/stardewvalley/mods/21238)
1. Install [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915)
1. Install [SpaceCore](https://www.nexusmods.com/stardewvalley/mods/1348)
1. Optionally, install [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) to easily configure the mod within the game.
1. Optionally, install [Special Power Utilities](https://www.nexusmods.com/stardewvalley/mods/22300) for the skill books to show up in your powers tab.
1. Launch Stardew Valley through SMAPI
1. Configure some allergies and enjoy the challenge!

# For Mod Authors

## Creating Custom Allergens and Integrating Modded Items

See [the content pack docs](docs/content_packs.md).

## Adding NPC Reaction Dialogue

Many of the base game NPCs have special dialogue if you speak to them while having an allergic reaction. If you'd like to have your custom NPCs react, you'll need to add a dialogue option with the key `"BarleyZP.BzpAllergies_farmer_allergic_reaction"`. You may also included dialogue in the `"Characters/Dialogue/MarriageDialogue{Name}"` assets, and the mod will try to use the married dialogue instead if you are married or roommates with that character. Here is an example using Content Patcher (that the base mod uses!) for Alex's non-marriage reaction dialogue.

```json
{
  "Format": "2.0.0",
  "Changes": [
    {
      "LogName": "Alex Dialogue",
      "Action": "EditData",
      "Target": "Characters/Dialogue/Alex",
      "Entries": {
        "BarleyZP.BzpAllergies_farmer_allergic_reaction": "Yikes! You don't look so good...$7"
      }
    }
  ]
}
```

Adding this dialogue is not currently supported through the BarleyZP's Allergies content pack framework. You will need to edit the asset in C# or use another framework like Content Patcher.

# Bug Reports and Feature Requests

You may leave a comment on the linked Nexus mod page or create an issue on this GitHub repository's issues page. There is no guarantee that feature requests will be implemented, but I will take a look at your suggestions!
