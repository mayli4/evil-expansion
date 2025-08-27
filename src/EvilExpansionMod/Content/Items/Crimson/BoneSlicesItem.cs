using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public class BoneSlicesItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.KEY_BoneSlices;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);

        Item.maxStack = Terraria.Item.CommonMaxStack;
    }
}