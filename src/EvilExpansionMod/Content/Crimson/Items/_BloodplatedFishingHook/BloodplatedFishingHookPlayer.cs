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

public class BloodplatedFishingHookPlayer : ModPlayer {
    public bool HasBloodplatedFishingAccessory = false; // UpdateAccessory uses this to tell Modplayer if the helmet is in the accessory slot
    public bool HasActiveShield = false; // True if the player is actively fishing and the shield is not broken

    public override void ResetEffects(){
        HasBloodplatedFishingAccessory = false;
        HasActiveShield = false;
    }
    public int _plateTier = 3; // How broken the shield is (descending from 3 to 0)
    public bool IsActivelyFishing(Player player){ // Check if the player is actively fishing
        // Check if accessory is equipped
        if (!HasBloodplatedFishingAccessory)
            return false;

        // Check if the currently held item is a fishing rod
        Item heldItem = player.HeldItem;
        if (heldItem == null || heldItem.fishingPole <= 0)
            return false;

        // Check if the player has an active bobber projectile spawned
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.bobber){
                int buffIndex = Player.FindBuffIndex(ModContent.BuffType<BloodPlatedBroken>());
                if(buffIndex == -1) {
                    if (HasActiveShield == false){
                        Projectile.NewProjectile(
                            player.GetSource_Accessory(heldItem),
                            player.Center,
                            new Microsoft.Xna.Framework.Vector2(0f, 0f),
                            ModContent.ProjectileType<BloodplatedFishingHookShield>(),
                            50,
                            0.5f,
                            Main.myPlayer
                            );
                        if (_plateTier == 3) Player.AddBuff(ModContent.BuffType<BloodPlatedHealthy>(), int.MaxValue);
                        else if (_plateTier == 2) Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged1>(), int.MaxValue);
                        else if (_plateTier == 1) Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged2>(), int.MaxValue);
                        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 1f }, player.Center);
                    }
                }
                return true; // Found an active fishing bobber!
            }
        }
        return false;
    }
    public override void OnHurt(Player.HurtInfo hurtInfo) { // Break the shield progressively and then shatter it
        if(HasBloodplatedFishingAccessory && HasActiveShield) {
            if(_plateTier == 3) {
                Player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged1>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 2) {
                Player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
                Player.AddBuff(ModContent.BuffType<BloodPlatedDamaged2>(), int.MaxValue);
                _plateTier--;
            }
            else if(_plateTier == 1) {
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