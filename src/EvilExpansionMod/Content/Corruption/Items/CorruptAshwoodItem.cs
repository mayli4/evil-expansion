using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CorruptAshwoodItem : ModItem {
    public override string Texture => Assets.Textures.Items.Corruption.CorruptAshwoodItem.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.DefaultToPlaceableTile(TileID.Ebonwood);
    }
}