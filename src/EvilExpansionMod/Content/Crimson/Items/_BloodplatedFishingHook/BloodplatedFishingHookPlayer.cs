using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class BloodplateHookPlayer : ModPlayer {
    public bool HasBloodplatedFishingAccessory = false; // UpdateAccessory uses this to tell Modplayer if equipped

    public override void ResetEffects(){
        HasBloodplatedFishingAccessory = false;
    }
    public int _plateTier = 3; // How broken the shield is (descending from 3 to 0)
    public bool IsShieldActive(Player player) { //Check if the shield exists
        for(int i = 0; i < Main.maxProjectiles; i++) {
            Projectile proj = Main.projectile[i];

            if(proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<BloodplateHookShield>()) {
                return true;
            }

        }
        return false;
    }
    public bool IsActivelyFishingWithBloodplated(Player player){ // Check if the player is actively fishing
        // Check if accessory is equipped
        if (!HasBloodplatedFishingAccessory)
            return false;

        // Check if the currently held item is a fishing rod
        Item heldItem = player.HeldItem;
        if (heldItem == null || heldItem.fishingPole <= 0)
            return false;

        // Check if the player has an active bobber projectile spawned
        for (int i = 0; i < Main.maxProjectiles; i++){
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.bobber && proj.wet){
                int buffIndex = Player.FindBuffIndex(ModContent.BuffType<BloodPlatedBroken>()); // Find cooldown debuff
                if(buffIndex == -1) { // If cooldown debuff isn't present
                    if(_plateTier == 0) //If shield is somehow broken and hasn't been reset by the debuff running out (nurse) (nurse) (nurse), Reset it manually
                        _plateTier = 3;
                    if (!IsShieldActive(player)){ //If there isn't a shield projectile already
                        Projectile.NewProjectile(
                            player.GetSource_Accessory(heldItem),
                            player.Center,
                            new Microsoft.Xna.Framework.Vector2(0f, 0f),
                            ModContent.ProjectileType<BloodplateHookShield>(),
                            66,
                            14,
                            Main.myPlayer
                            );

                        var buffType = _plateTier switch
                        {
                            1 =>
                                ModContent.BuffType<BloodPlatedDamaged2>(),
                            2 =>
                                ModContent.BuffType<BloodPlatedDamaged1>(),
                            _ =>
                                ModContent.BuffType<BloodPlatedHealthy>(),
                        };
                        Player.AddBuff(buffType, int.MaxValue);

                        SoundEngine.PlaySound(SoundID.NPCDeath19 with { Volume = 0.8f, PitchRange = (-2f, -1.5f) }, player.Center);
                        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 1f, PitchRange = (-0.5f, 0f) }, player.Center);
                    }
                }
                return true; // Found
            }
        }
        return false;
    }

    public override void OnHurt(Player.HurtInfo hurtInfo) { // Break the shield progressively and then shatter it
        if(HasBloodplatedFishingAccessory && IsShieldActive(Player)) {
            //YES. I AM MAKING MORE DUST. THIS IS A GOOD IDEA, TRUST ME
            for(int i = 0; i < Main.rand.NextFloat(40f, 50f) * (4 - _plateTier); i++) {
                Dust.NewDust(
                    Player.position + Main.rand.NextVector2Circular(45, 45) + Main.rand.NextVector2Circular(45, 45),
                    2,
                    2,
                    DustID.Blood,
                    Main.rand.NextFloat(-6f, 6f) * (4 - _plateTier),
                    Main.rand.NextFloat(-6f, 6f) * (4 - _plateTier),
                    default,
                    default,
                    Main.rand.NextFloat(1f, 1.4f * (float)Math.Cbrt(4 - _plateTier))
                );
                Dust.NewDust(
                    Player.position + Main.rand.NextVector2Circular(45, 45) + Main.rand.NextVector2Circular(45, 45),
                    2,
                    2,
                    DustID.Grubby,
                    Main.rand.NextFloat(-6f, 6f) * (4 - _plateTier),
                    Main.rand.NextFloat(-6f, 6f) * (4 - _plateTier),
                    default,
                    default,
                    Main.rand.NextFloat(0.8f, 1f)
                );
            }
            Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        new Microsoft.Xna.Framework.Vector2(0f, 0f),
                        ModContent.ProjectileType<BloodExplosion>(),
                        0,
                        0.5f,
                        Main.myPlayer,
                        ai0: 0,
                        ai1: 0,
                        ai2: 1
                        );
            if(_plateTier == 3) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.8f } with { PitchRange = (-0.8f, -0.6f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath22 with { Volume = 0.4f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                Player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged1>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 2) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.8f } with { PitchRange = (-0.6f, -0.4f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath22 with { Volume = 0.6f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged2>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 1) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.8f } with { PitchRange = (1.6f, 1.8f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath22 with { Volume = 0.9f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.9f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);

                Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged2>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedBroken>(), 30 * 60);
                _plateTier--;
            }
        }
        else{
            Player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
            Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
            Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged2>());
        }
    }
}
public class BloodExplosion : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodExplosion.KEY;
    public override void SetDefaults() {
        Projectile.width = 98;
        Projectile.height = 98;
        Projectile.DamageType = DamageClass.Default;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 30;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 7;
    }
    public override void AI() {
        if(Projectile.ai[0] == 1) {
            Projectile.hostile = true;
        }
        else {
            Projectile.hostile = false;
        }
        if(Projectile.ai[1] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }
        Projectile.scale = Projectile.ai[2];
        // Visuals: Create dust explosion effects here
        for(int i = 0; i < 5; i++) {
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Blood,
                Main.rand.NextFloat(-3f, 3f),
                Main.rand.NextFloat(-3f, 3f),
                default,
                default,
                Main.rand.NextFloat(0.8f, 1.2f));
        }
        Lighting.AddLight(Projectile.Center, 0.1f, 0.0f, 0.0f);
        AnimateProjectile();
    }
    private const int FrameCount = 7;
    private void AnimateProjectile() {
        int frameDuration = 2;
        Projectile.frameCounter++;
        if(Projectile.frameCounter >= frameDuration) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
            if(Projectile.frame >= FrameCount) {
                Projectile.active = false;
            }
        }
    }
}