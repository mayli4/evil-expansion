using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class RawShadowScalesItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.RawShadowScales.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
}