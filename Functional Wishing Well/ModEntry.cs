using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace Functional_Wishing_Well;

public class ModEntry : Mod
{
    private static ITranslationHelper StaticTranslation;
    public override void Entry(IModHelper helper)
    {
        StaticTranslation = helper.Translation;

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanRefillWateringCanOnTile)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(CanRefillWateringCanOnTile_Postfix)));
        
        harmony.Patch(
            original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(CheckForAction_Postfix)));

        helper.Events.GameLoop.DayStarted += OnDayStarted;

    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        Game1.player.modData["BarleyZP.FunctionalWishingWell_TossCoinToday"] = "0";
    }

    private static void CanRefillWateringCanOnTile_Postfix(GameLocation __instance, int tileX, int tileY, ref bool __result)
    {
        if (__result) return;
        
        SObject? obj = __instance.getObjectAtTile(tileX, tileY);
        __result = obj is { QualifiedItemId: "(BC)BarleyZP.FunctionalWishingWell_WishingWell", isTemporarilyInvisible: false };
    }

    private static void CheckForAction_Postfix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
    {
        if (__result || justCheckingForActivity ||
            __instance is not { QualifiedItemId: "(BC)BarleyZP.FunctionalWishingWell_WishingWell", isTemporarilyInvisible: false }) return;

        if (who.modData["BarleyZP.FunctionalWishingWell_TossCoinToday"] == "1")
        {
            Game1.drawObjectDialogue(StaticTranslation.Get("AlreadyTossedToday"));
        }
        else
        {
            var responses = new Response[2];
            responses[0] = new Response("yes", StaticTranslation.Get("TossCoinYes"));
            responses[1] = new Response("no", StaticTranslation.Get("TossCoinNo"));
        
            __instance.Location.createQuestionDialogue(StaticTranslation.Get("TossCoin"), responses, AfterTossCoin);
        }
        
        __result = true;
    }

    private static void AfterTossCoin(Farmer who, string whichAnswer)
    {
        switch (whichAnswer)
        {
            case "yes":
                if (who.Money >= 1)
                {
                    who.Money -= 1;
                    Game1.playSound("dropItemInWater");
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 250;

                    var msgNum = Game1.random.Next(1, 6);
                    Game1.showGlobalMessage(StaticTranslation.Get($"AfterTossCoin{msgNum}"));
                    
                    who.stats.Increment("BarleyZP.FunctionalWishingWell_CoinsTossed", 1);
                    who.modData["BarleyZP.FunctionalWishingWell_TossCoinToday"] = "1";
                }
                else
                {
                    Game1.playSound("cancel");
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                }
                break;
            case "no":
                Game1.playSound("cancel");
                break;
        }
    }
}