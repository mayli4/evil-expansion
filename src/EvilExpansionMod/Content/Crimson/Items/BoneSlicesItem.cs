using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class BoneSlicesItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.BoneSlices.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 35;
    }
    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        Item.value = 3500;
        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
    public override void PostUpdate() {
        // Optional: Use a random check so dust doesn't spawn *every single frame* (which can lag the game)
        if(Main.rand.NextBool(4)) {
            // Spawn the dust at the item's current position in the world
            int dustIndex = Dust.NewDust(
                Item.position,   // Position X, Y
                Item.width,      // Width of spawn area
                Item.height,     // Height of spawn area
                DustID.BloodWater,    // Change this to whatever DustID or ModContent.DustType you want
                0f, 0f,          // Speed X, Speed Y
                100,             // Alpha transparency (0-255)
                default,  // Custom color override
                1.5f             // Scale/Size of the dust particle
            );

            // Optional: Customize the behavior of the newly created dust
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= 0.5f;
        }
    }
}