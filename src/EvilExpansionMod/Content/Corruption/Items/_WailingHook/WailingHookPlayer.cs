using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class WailingHookPlayer : ModPlayer {
    public bool HasSoulsuckingFishingAccessory = false; // UpdateAccessory uses this to tell Modplayer if equipped
    public int nearbyEnemyCount = 0; // UpdateAccessory uses this to track the # of nearby enemies
    private float _spawnTimer = 0; // Internal timer for spawning ghosts

    public override void ResetEffects() {
        HasSoulsuckingFishingAccessory = false;
        nearbyEnemyCount = 0;
    }
    public bool IsActivelyFishingWithSoulsucker(Player player){ // Check if the player is actively fishing
        // Check if accessory is equipped
        if (!HasSoulsuckingFishingAccessory)
            return false;

        // Check if the currently held item is a fishing rod
        Item heldItem = player.HeldItem;
        if (heldItem == null || heldItem.fishingPole <= 0)
            return false;

        // Check if the player has an active bobber projectile spawned
        for (int i = 0; i < Main.maxProjectiles; i++){
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.bobber){
                return true; // Found an active fishing bobber!
            }
        }
        return false;
    }
    public override void PostUpdate(){
            if (Player.whoAmI != Main.myPlayer)
                return;
            if (!IsActivelyFishingWithSoulsucker(Player)){
                _spawnTimer = 0; // Reset timer when not fishing
                return;
            }

            // Increment the timer, with the amount scaling with enemy count
            float enemyBonus = (float)Math.Log(1 + nearbyEnemyCount) * 10.0f;
            _spawnTimer += 1 + enemyBonus;
            if (_spawnTimer < 120)
                return;
            
            _spawnTimer = 0; // Reset loop interval

            // Loop backwards through projectiles. Projectiles spawned last are at the end of the array.
            for (int i = Main.maxProjectiles - 1; i >= 0; i--){
                Projectile proj = Main.projectile[i];

                // Combined check order: check activity/owner first (fast integers), then check bobber flag
                if (proj.active && proj.owner == Player.whoAmI && proj.bobber){
                    // Spawn the projectile directly at the cached vector center
                    Projectile.NewProjectile(
                        Player.GetSource_FromThis(), 
                        proj.Center, 
                        Vector2.Zero,
                        ModContent.ProjectileType<LavaTelegraphProjectile>(),
                        0,
                        0f,
                        Main.myPlayer,
                        ai0: 45,
                        ai2: 50 / 2
                    );

                    break;
                }
            }
        }
}
