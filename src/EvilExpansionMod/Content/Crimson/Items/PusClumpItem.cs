using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class PusClumpItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.PusClump.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
    }
    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        Item.value = 2500;
        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
}