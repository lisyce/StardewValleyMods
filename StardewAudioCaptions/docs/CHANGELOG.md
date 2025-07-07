# Changelog

## v1.0.2

Released 7 July 2025 for SMAPI 4.2.1 or later.

- Fixed a bug where changing translations for captions outside of default.json did not reflect in-game.
- Fixed a bug where tokenized captions did not work properly.
- Fixed NRE spam due to null sounds being played.

## v1.0.1

Released 19 June 2025 for SMAPI 4.2.1 or later.

- Stopped concatenating i18n keys on the GMCM screen.
- Allowed for reloading caption translations in-game via `reload_i18n` followed by `patch invalidate BarleyZP.Captions/Definitions`.

## v1.0.0

Released 18 June 2025 for SMAPI 4.2.1 or later.

- Initial upload