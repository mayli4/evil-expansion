using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class CharredBloodwoodItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodItem.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.DefaultToPlaceableTile(TileID.Shadewood);
    }
}