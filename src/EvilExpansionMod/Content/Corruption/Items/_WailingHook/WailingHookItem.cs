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

public class WailingHookItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.WailingHookItem.KEY;

    private int _projectileID = -1; 

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<WailingHookPlayer>();
        modPlayer.HasSoulsuckingFishingAccessory = true;

        // Max range limit here (8 pixels = 1 tiles)
        float maxRange = 40 * 8f; 

        // Recalculate every 10 frames (~6 times a second)
        if (Main.GameUpdateCount % 10 == 0){
            int count = 0;
            float rangeSquared = maxRange * maxRange;

            for (int i = 0; i < Main.maxNPCs; i++){
                NPC npc = Main.npc[i];

                // 1. Quick filter: Must be active, hostile, and capable of dealing damage
                if (npc.active && !npc.friendly && npc.damage > 0 && !npc.dontTakeDamage){
                    // 2. Efficient distance check using squared vectors
                    if (Vector2.DistanceSquared(player.Center, npc.Center) <= rangeSquared){
                        count++;
                    }
                }
            }
            player.GetModPlayer<WailingHookPlayer>().nearbyEnemyCount = count;
        }
    }
}