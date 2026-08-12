using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class RawShadowScalesItem : ModItem {
    public override string Texture => Assets.Textures.Items.Corruption.RawShadowScales.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
}