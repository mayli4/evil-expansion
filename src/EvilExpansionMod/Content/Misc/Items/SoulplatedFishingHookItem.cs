using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Crimson;
using EvilExpansionMod.Content.Corruption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Items;

public class SoulplatedHookItem : ModItem {
    public override string Texture => Assets.Images.Misc.Items.SoulplatedFishingHook.KEY;

    private int _projectileID = -1; 

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 7);
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<WailingHookItem>(), 1)
            .AddIngredient(ModContent.ItemType<BloodplateHookItem>(), 1)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();
        modPlayer.HasBloodplatedFishingAccessory = true;
        if (!modPlayer.IsActivelyFishingWithBloodplated(player)){
            player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
            player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
            player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged2>());
        }

        var modPlayer2 = player.GetModPlayer<WailingHookPlayer>(); // Uhm... surely this works
        modPlayer2.HasSoulsuckingFishingAccessory = true;

        // Max range limit
        float maxRange = 40 * 16f;

        // Recalculate every 10 frames
        if(Main.GameUpdateCount % 10 == 0) {
            int count = 0;
            float rangeSquared = maxRange * maxRange;

            for(int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];

                // Active, hostile, capable of dealing damage
                if(npc.active && !npc.friendly && npc.damage > 0 && !npc.dontTakeDamage) {
                    // Distance check using squared vectors
                    if(Vector2.DistanceSquared(player.Center, npc.Center) <= rangeSquared) {
                        count++;
                    }
                }
            }
            player.GetModPlayer<WailingHookPlayer>().nearbyEnemyCount = count;
        }
    }
}