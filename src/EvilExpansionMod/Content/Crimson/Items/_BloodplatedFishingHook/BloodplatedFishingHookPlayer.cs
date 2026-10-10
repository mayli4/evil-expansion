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
    public bool HasActiveShield = false; // True if the player is actively fishing and the shield is not broken

    public override void ResetEffects(){
        HasBloodplatedFishingAccessory = false;
    }
    public int _plateTier = 3; // How broken the shield is (descending from 3 to 0)
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
            if (proj.active && proj.owner == player.whoAmI && proj.bobber){
                int buffIndex = Player.FindBuffIndex(ModContent.BuffType<BloodPlatedBroken>());
                if(buffIndex == -1) {
                    if (HasActiveShield == false){
                        Projectile.NewProjectile(
                            player.GetSource_Accessory(heldItem),
                            player.Center,
                            new Microsoft.Xna.Framework.Vector2(0f, 0f),
                            ModContent.ProjectileType<BloodplateHookShield>(),
                            50,
                            0.5f,
                            Main.myPlayer
                            );
                        if (_plateTier == 3) Player.AddBuff(ModContent.BuffType<BloodPlatedHealthy>(), int.MaxValue);
                        else if (_plateTier == 2) Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged1>(), int.MaxValue);
                        else if (_plateTier == 1) Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged2>(), int.MaxValue);
                        SoundEngine.PlaySound(SoundID.NPCDeath58 with { Volume = 1f }, player.Center);
                    }
                }
                return true; // Found an active fishing bobber!
            }
        }
        return false;
    }
    public override void OnHurt(Player.HurtInfo hurtInfo) { // Break the shield progressively and then shatter it
        if(HasBloodplatedFishingAccessory && HasActiveShield) {
            //YES. I AM MAKING MORE DUST. THIS IS A GOOD IDEA, TRUST ME
            for(int i = 0; i < Main.rand.NextFloat(8f, 17f) * (4 - _plateTier); i++) {
                Dust.NewDust(
                    Player.position,
                    Player.width,
                    Player.height,
                    DustID.Blood,
                    Main.rand.NextFloat(-6f,6f) * (4 - _plateTier),
                    Main.rand.NextFloat(-6f,6f) *(4 - _plateTier),
                    125,
                    default,
                    Main.rand.NextFloat(0.4f, 1.2f)
                ); 
            }
            if(_plateTier == 3) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.8f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath23 with { Volume = 0.2f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                Player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged1>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 2) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.6f } with { PitchRange = (-1.0f, -0.6f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath23 with { Volume = 0.4f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged2>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 1) {
                SoundEngine.PlaySound(SoundID.Item178 with { Volume = 0.4f } with { PitchRange = (-1.0f, -0.7f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath23 with { Volume = 0.6f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Volume = 0.8f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                
                Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged2>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedBroken>(), 45 * 60);
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