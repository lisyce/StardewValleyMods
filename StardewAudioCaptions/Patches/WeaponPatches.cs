using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Tools;

namespace StardewAudioCaptions.Patches;

public class WeaponPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.weaponsTypeUpdate)),
            new Caption("objectiveComplete", "weapons.recharge", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.triggerClubFunction)),
            new Caption("clubSmash", "weapons.clubSmash"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MeleeWeapon), "quickStab"),
            new Caption("daggerswipe", "weapons.daggerStab"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.doSwipe)),
            new Caption("swordswipe", "weapons.swipe"),
            new Caption("clubswipe", "weapons.swipe"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"),
            new Caption("batFlap", "weapons.block"),
            new Caption("clubswipe", "weapons.swipe"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
            new Caption("parry", "weapons.parry"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Projectile), nameof(Projectile.update)),
            new Caption(CaptionManager.AnyCue, "weapons.projectileHitsWall", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Constructor(typeof(BasicProjectile)),
            new Caption(CaptionManager.AnyCue, "weapons.projectileFire"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithPlayer)),
            new Caption(CaptionManager.AnyCue, "weapons.playerDebuff"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BlueSquid), nameof(BlueSquid.behaviorAtGameTick)),
            new Caption("debuffSpell", "weapons.debuffFire", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Constructor(typeof(DebuffingProjectile)),
            new Caption("debuffSpell", "weapons.debuffFire"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(DebuffingProjectile), nameof(DebuffingProjectile.behaviorOnCollisionWithPlayer)),
            new Caption("frozen", "weapons.playerFreeze"),
            new Caption("debuffHit", "weapons.playerDebuff"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DebuffingProjectile), nameof(DebuffingProjectile.behaviorOnCollisionWithMonster)),
            new Caption("frozen", "weapons.monsterFreeze"));
    }
}