using StardewModdingAPI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using StardewValley;

namespace WikiLinks
{
    public partial class ModEntry
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void OpenPage(string page)
        {
            SMonitor.Log($"Opening wiki page for {page}");

            if (Config.SendToBack)
                SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);

            //ShowWindow(Process.GetCurrentProcess().MainWindowHandle, SW_MINIMIZE);

            string prefix = "";
            if (ModEntry.Config.WikiLang == "Auto-Detect")  // get the right wiki
            {
                LocalizedContentManager.LanguageCode code = Game1.content.GetCurrentLanguage();
                prefix = code switch
                {
                    LocalizedContentManager.LanguageCode.de => "de.",
                    LocalizedContentManager.LanguageCode.es => "es.",
                    LocalizedContentManager.LanguageCode.fr => "fr.",
                    LocalizedContentManager.LanguageCode.it => "it.",
                    LocalizedContentManager.LanguageCode.ja => "ja.",
                    LocalizedContentManager.LanguageCode.ko => "ko.",
                    LocalizedContentManager.LanguageCode.hu => "hu.",
                    LocalizedContentManager.LanguageCode.pt => "pt.",
                    LocalizedContentManager.LanguageCode.ru => "ru.",
                    LocalizedContentManager.LanguageCode.zh => "zh."
                    _ => ""
                };
            }

            var ps = new ProcessStartInfo($"https://{prefix}stardewvalleywiki.com/{page}")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        public static string GetWikiPageForItem(Item obj, ITranslationHelper helper)
        {
            bool englishGameEnglishWiki = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en &&
                                          ModEntry.Config.WikiLang == "English";
            if (ModEntry.Config.WikiLang == "Auto-Detect" || englishGameEnglishWiki)
            {
                // is there a key in i18n json for qualified id?
                string translated = helper.Get(obj.QualifiedItemId).UsePlaceholder(false);
                return translated ?? obj.DisplayName;
            }

            // we use english wiki but we arent playing in english; search with internal name
            // is there a key in i18n default for internal name?
            var translatedDict = helper.GetInAllLocales(obj.QualifiedItemId);
            if (translatedDict.ContainsKey("default")) return translatedDict["default"] ?? obj.Name;

            return obj.Name;
        }

    }
}