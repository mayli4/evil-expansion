using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class PusClumpItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.PusClump.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 50;
    }
    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        Item.value = Item.sellPrice(0,0,5,0);
        Item.maxStack = Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
}