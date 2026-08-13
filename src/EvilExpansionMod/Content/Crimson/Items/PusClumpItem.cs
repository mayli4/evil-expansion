using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class PusClumpItem : ModItem {
    public override string Texture => Assets.Textures.Items.Crimson.PusClump.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
}