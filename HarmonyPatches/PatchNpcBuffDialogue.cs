using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace BZP_Allergies.HarmonyPatches
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
    internal class PatchNpcBuffDialogue : Initializable
    {
        [HarmonyPrefix]
        static void CheckAction_Prefix(ref NPC __instance, ref Farmer who)
        {
            try
            {
                if (__instance.IsInvisible || __instance.isSleeping.Value || !who.CanMove)
                {
                    return;  // run the original logic here
                }

                // is the farmer having a reaction?
                if (who.hasBuff(AllergenManager.ALLERIC_REACTION_DEBUFF))
                {
                    Dialogue? reactionDialogue = GetNpcAllergicReactionDialogue(__instance, who);
                    if (reactionDialogue != null && !ModEntry.NpcsThatReactedToday.Contains(__instance.Name))
                    {
                        __instance.CurrentDialogue.Push(reactionDialogue);
                        ModEntry.NpcsThatReactedToday.Add(__instance.Name);
                    }
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(CheckAction_Prefix)}:\n{ex}", LogLevel.Error);
            }

        }

        private static Dialogue? GetNpcAllergicReactionDialogue(NPC npc, Farmer who)
        {
            if (who.isMarriedOrRoommates() && who.spouse == npc.Name)
            {
                Dialogue? marriageDialogue = npc.tryToGetMarriageSpecificDialogue(AllergenManager.REACTION_DIALOGUE_KEY);
                if (marriageDialogue != null)
                {
                    return marriageDialogue;
                }
            }
            
            return npc.TryGetDialogue(AllergenManager.REACTION_DIALOGUE_KEY);
        }
    }
}
