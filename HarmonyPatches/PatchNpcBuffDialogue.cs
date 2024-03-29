using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
    internal class PatchNpcBuffDialogue : Initializable
    {
        [HarmonyPrefix]
        static bool CheckAction_Prefix(ref NPC __instance, ref Farmer __who)
        {
            try
            {
                if (__instance.IsInvisible || __instance.isSleeping.Value || !__who.CanMove)
                {
                    return true;  // run the original logic here
                }

                // is the farmer having a reaction?
                if (__who.hasBuff(AllergenManager.ALLERIC_REACTION_DEBUFF))
                {
                    Monitor.Log("Buff!", LogLevel.Debug);
                    Dialogue? reactionDialogue = GetNpcAllergicReactionDialogue(__instance);
                    if (reactionDialogue != null)
                    {
                        __instance.CurrentDialogue.Push(reactionDialogue);
                    }
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CheckAction_Prefix)}:\n{ex}", LogLevel.Error);
            }

            return true;  // more dialogue can stack here; let the original method do its thing
        }

        private static Dialogue? GetNpcAllergicReactionDialogue(NPC npc)
        {
            Dialogue? marriageDialogue = npc.tryToGetMarriageSpecificDialogue(AllergenManager.REACTION_DIALOGUE_KEY);
            if (marriageDialogue != null)
            {
                return marriageDialogue;
            }

            return npc.TryGetDialogue(AllergenManager.REACTION_DIALOGUE_KEY);
        }
    }
}
