using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZP_Allergies.AssetPatches
{
    internal class PatchLetters : Initializable
    {
        public static void PatchMail(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();

                    editor.Data[ModEntry.MOD_ID + "_harvey_ad"] =
                        "@,^I heard that you experienced an allergic reaction the other day." +
                        "^You should come by the clinic sometime and get some medicine so it doesn't happen again!" +
                        "^I've enclosed some Allergy Medicine to get you by until you can stop in." +
                        "^^-Dr. Harvey%item object " + AllergenManager.ALLERGY_RELIEF_ID + " 1%%[#]Harvey's Allergy Medicine";
                });
            }
        }
    }
}
