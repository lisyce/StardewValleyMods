# Changelog

## v2.0.0

Released 11 June 2025 for SMAPI 4.2.1 or later.

> Previous versions of this mod are now *deprecated*. The web server may go out of support and the hosted CSS/JS files for existing
> mod lists may be removed in favor of this new version, which is more stable and provides new features. This means your existing mod lists
> from previous versions of this mod may stop working at some point in the future, even if you have them saved locally. You may download
> the `script.js` and `style.css` files at the root of the repository if you wish to copy them into your existing mod lists for any reason,
> but it is highly recommended that you just regenerate your lists with this version.

- This mod now generates a JSON mod list before it becomes HTML.
  - See https://modlists.barleyzp.com/schemas/modlist-v2.json for the JSON schema, which is automatically added to all generated mod lists.
- The HTML is now templated on the web server, not in this mod.
  - Made improvements to formatting on mobile.
- Changed the remote web server to one with MUCH faster response time. Visit https://modlists.barleyzp.com to check it out!
  - JSON can also be uploaded on this website instead of using the `mod_list_share` command.
- Implemented color theme support; current themes are `default`, `sakura`, `stardrop`, `coffee`, and `sage`.
- Added support for GitHub update keys. If there are no Nexus update keys, the mod will try to find a GitHub one to use.
- Mod lists can now be renewed so they last longer than 4 weeks; see the link in the footer of the HTML mod list to renew one.
- Changed command names to `mod_list_json` and `mod_list_share`. The former generates a JSON-formatted mod list and the latter sends the JSON to the web server to be turned into an HTML-formatted page.
  - Running `mod_list_share` still saves an HTML-formatted mod list to your computer for safekeeping.