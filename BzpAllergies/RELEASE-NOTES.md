# BarleyZP's Allergies Release Notes

## 1.4.2

Release 14 July 2024

- Ingredients in fridges are no longer double-consumed when cooking

## 1.4.1

Released 13 July 2024

- Object Ids in the Allergen Data asset are now case-insensitive
- Squid and Octopus have been moved to the shellfish allergy
- Replaced "BarleyZP.BzpAllergies_FlourIngredient" context tag with more general "flour_item"
- Changed how processed goods receive their allergies to allow for fuels and extra consumed items to count for allergy contributions
- Books now use Special Power Utilities' `spu_book_no_message` context tag to avoid double-messages and remove a transpiler
- Calico eggs can cause reactions in those with egg allergies (cause why not? it's an egg)
- Added `ExceptObjectIds` and `ExceptContextTags` to the allergen data asset (see content pack docs)
- Objects with the `allergen_{allergenid}` tag automatically get allergens assigned
- When passing out due to allergic reaction energy loss, a global message is shown to explain what's happening

## 1.4.0

Released 31 May 2024

- Fixed UI crash when crafting field snacks
- Full i18n support; please reach out if you'd like to help translate!
- Thanks to my friend Seb for the Spanish translation
- Fixed bug where the telephone had Fish and Gluten allergens

## 1.3.2

Released 17 May 2024

- Compatibility with SpaceCore custom skills
- New option for a standalone allergy menu

## 1.3.1

Released 15 May 2024

- Crafting is no longer broken

## 1.3.0

Released 13 May 2024

- Implemented randomizable farmer allergies
- Changed asset loading for compatibility packs
- Cooking alternatives for dairy and gluten
- Renamed wheat allergy to gluten
- 1.6 skill books
- Lots more difficulty configuration
- And a ton of code cleanup

## 1.2.1

Released 11 April 2024

- Fixed a bug where custom allergen Ids in content packs were allowed special characters that caused errors
- Fixed a bug with asset edit order for `Data/Objects` and items added by other mods
- Correctly categorized Beer under the "wheat" allergy
- Fixed a bug where custom content packs could not add context-tag allergies to existing allergens

## 1.2.0

leased 9 April 2024

- Cooked and crafted items now keep track of what they were made with! This is handy for foods like sashimi that can be made with either fish or shellfish.
- Updated content pack documentation
- Improved the config menu to sort allergens alphabetically and by content pack

## 1.1.0

Released 2 April 2024

- Added custom content pack support! See the README for documentation!

## 1.0.1

Released 31 March 2024

- Fixed a bug where configurations became out of sync and did not save between sessions upon resetting them to default values

## 1.0.0

Released 29 March 2024

- Players can configure allergens and face debuffs when eating foods containing allergens
- Allergy Medicine and Lactase Pills as remedies to allergic reactions
- Mail from Harvey
- Custom dialogue when interacting with NPCs while having an allergic reaction