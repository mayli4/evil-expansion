using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class RawShadowScalesItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.KEY_RawShadowScales;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        
        Item.maxStack = Terraria.Item.CommonMaxStack;
    }
}