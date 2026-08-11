using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public class CharredBloodwoodItem : ModItem {
    public override string Texture => Assets.Textures.Items.Crimson.KEY_CharredBloodwoodItem;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.DefaultToPlaceableTile(TileID.Shadewood);
    }
}