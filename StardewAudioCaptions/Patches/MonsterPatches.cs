using HarmonyLib;
using StardewAudioCaptions.Captions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace StardewAudioCaptions.Patches;

public class MonsterPatches : ICaptionPatch
{
    public void Patch(Harmony harmony, IMonitor monitor)
    {
        var takeDamageParams = new[]
            { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) };
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.spawnFlyingMonsterOffScreen)),
            new Caption("serpentDie", "monsters.serpentSpawn"),
            new Caption("rockGolemHit", "monsters.hauntedSkull"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Serpent), "localDeathAnimation"),
            new Caption("serpentDie", "monsters.serpentDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Serpent), nameof(Serpent.takeDamage), takeDamageParams),
            new Caption("serpentHit", "monsters.serpentHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BigSlime), nameof(BigSlime.takeDamage), takeDamageParams),
            new Caption("hitEnemy", "monsters.bigSlimeHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BigSlime), "localDeathAnimation"),
            new Caption("slimedead", "monsters.bigSlimeDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BlueSquid), nameof(BlueSquid.takeDamage), takeDamageParams),
            new Caption("slimeHit", "monsters.blueSquidHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BlueSquid), "sharedDeathAnimation"),
            new Caption("slimedead", "monsters.blueSquidDie"));

        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(BlueSquid), nameof(BlueSquid.behaviorAtGameTick)),
            new Caption("debuffSpell", "monsters.debuffFire"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bat), nameof(Bat.takeDamage), takeDamageParams),
            new Caption("magma_sprite_hit", "monsters.magmaSpriteHurt"),
            new Caption("hitEnemy", "monsters.genericHurt"),
            new Caption("magma_sprite_die", "monsters.magmaSpriteDie"),
            new Caption("rockGolemHit", "monsters.genericDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bat), nameof(Bat.behaviorAtGameTick)),
            new Caption("magma_sprite_spot", "monsters.genericSpot"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bug), nameof(Bug.takeDamage), takeDamageParams),
            new Caption("crafting", "weapons.weaponClank"),
            new Caption("hitEnemy", "monsters.bugHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Bug), "localDeathAnimation"),
            new Caption("slimedead", "monsters.bugDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DinoMonster), "sharedDeathAnimation"),
            new Caption("skeletonDie", "monsters.dinoDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DinoMonster), nameof(DinoMonster.behaviorAtGameTick)),
            new Caption("furnace", "monsters.dinoFire"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Duggy), nameof(Duggy.takeDamage), takeDamageParams),
            new Caption("hitEnemy", "monsters.duggyHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Duggy), "localDeathAnimation"),
            new Caption("monsterdead", "monsters.duggyDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DustSpirit), "localDeathAnimation"),
            new Caption("dustMeep", "monsters.dustSpriteDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DustSpirit), "updateAnimation"),
            new Caption("dustMeep", "monsters.dustSpriteMeep", shouldLog: false));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DwarvishSentry), nameof(DwarvishSentry.takeDamage), takeDamageParams),
            new Caption("clank", "monsters.sentryHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(DwarvishSentry), "localDeathAnimation"),
            new Caption("fireball", "monsters.sentryDie"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Fly), nameof(Fly.takeDamage), takeDamageParams),
            new Caption("hitEnemy", "monsters.flyHurt"),
            new Caption("monsterdead", "monsters.flyDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Ghost), "localDeathAnimation"),
            new Caption("ghost", "monsters.ghostDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Ghost), nameof(Ghost.behaviorAtGameTick)),
            new Caption("fishSlap", "monsters.debuffFire"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Grub), nameof(Grub.takeDamage), takeDamageParams),
            new Caption("slimeHit", "monsters.grubHurt"),
            new Caption("slimedead", "monsters.grubDie"),
            new Caption("crafting", "weapons.weaponClank"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(HotHead), nameof(HotHead.takeDamage), takeDamageParams),
            new Caption("fuse", "monsters.hotheadFuse"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(HotHead), nameof(HotHead.DropBomb)),
            new Caption("fuse", "monsters.hotheadFuse"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(LavaLurk), "sharedDeathAnimation"),
            new Caption("skeletonDie", "monsters.lavalurkDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(LavaLurk), "updateAnimation"),
            new Caption("waterSlosh", "world.lavaSlosh"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(LavaLurk), nameof(LavaLurk.behaviorAtGameTick)),
            new Caption("fireball", "monsters.lavalurkFire"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Leaper), "localDeathAnimation"),
            new Caption("monsterdead", "monsters.spiderDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Leaper), nameof(Leaper.behaviorAtGameTick)),
            new Caption("batFlap", "monsters.spiderJump"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(HotHead), nameof(HotHead.takeDamage), takeDamageParams),
            new Caption("clank", "monsters.hotheadHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MetalHead), nameof(MetalHead.takeDamage), takeDamageParams),
            new Caption("clank", "monsters.metalheadHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(MetalHead), "localDeathAnimation"),
            new Caption("monsterdead", "monsters.genericDie"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(Mummy), nameof(Mummy.takeDamage), takeDamageParams),
            new Caption("shadowHit", "monsters.mummyHurt"),
            new Caption("ghost", "monsters.mummyDie"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(RockCrab), nameof(RockCrab.hitWithTool)),
            new Caption("hammer", "tools.pickaxe"),
            new Caption("stoneCrack", "monsters.rockCrabCrack"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(RockCrab), nameof(RockCrab.takeDamage), takeDamageParams),
            new Caption("crafting", "weapons.weaponClank"),
            new Caption("hitEnemy", "monsters.genericHurt"),
            new Caption("monsterdead", "monsters.genericDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(RockGolem), nameof(RockGolem.takeDamage), takeDamageParams),
            new Caption("rockGolemHit", "monsters.golemHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(RockGolem), "localDeathAnimation"),
            new Caption("rockGolemDie", "monsters.golemDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(RockGolem), nameof(RockGolem.behaviorAtGameTick)),
            new Caption("rockGolemSpawn", "monsters.golemSpawn"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShadowBrute), nameof(ShadowBrute.takeDamage), takeDamageParams),
            new Caption("shadowHit", "monsters.shadowBruteHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShadowBrute), "localDeathAnimation"),
            new Caption("shadowDie", "monsters.shadowBruteDie"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.takeDamage), takeDamageParams),
            new Caption("shadowHit", "monsters.shamanHurt"),
            new Caption("shadowDie", "monsters.shamanDie"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick)),
            new Caption("shadowpeep", "monsters.shamanPeep"),
            new Caption("healSound", "monsters.monsterHeal"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Shooter), nameof(Shooter.behaviorAtGameTick)),
            new Caption("Cowboy_gunshot", "monsters.shooterFire"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Shooter), nameof(Shooter.takeDamage), takeDamageParams),
            new Caption("shadowHit", "monsters.shooterHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Shooter), "localDeathAnimation"),
            new Caption("shadowDie", "monsters.shooterDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Skeleton), nameof(Skeleton.takeDamage), takeDamageParams),
            new Caption("skeletonHit", "monsters.skeletonHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Skeleton), "sharedDeathAnimation"),
            new Caption("skeletonDie", "monsters.skeletonDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(SquidKid), nameof(SquidKid.takeDamage), takeDamageParams),
            new Caption("hitEnemy", "monsters.squidkidHurt"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(SquidKid), "localDeathAnimation"),
            new Caption("fireball", "monsters.squidkidDie"));
        
        PatchGenerator.GeneratePatchPair(
            harmony,
            monitor,
            AccessTools.Method(typeof(Shooter), nameof(Shooter.behaviorAtGameTick)),
            new Caption("fireball", "monsters.squidkidFire"));
        
        PatchGenerator.GeneratePatchPairs(
            harmony,
            monitor,
            AccessTools.Method(typeof(GreenSlime), nameof(GreenSlime.takeDamage), takeDamageParams),
            new Caption("slimeHit", "monsters.slimeHurt"),
            new Caption("slimedead", "monsters.slimeDie"));
    }
}