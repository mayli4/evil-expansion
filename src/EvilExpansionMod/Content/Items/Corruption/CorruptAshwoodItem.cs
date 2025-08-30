using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class CorruptAshwoodItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.KEY_CorruptAshwoodItem;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        
        Item.DefaultToPlaceableTile(TileID.Ebonwood);
    }
}