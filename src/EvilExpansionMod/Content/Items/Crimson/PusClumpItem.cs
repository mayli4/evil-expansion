using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public class PusClumpItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.KEY_PusClump;

    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        
        Item.maxStack = Terraria.Item.CommonMaxStack;
    }
}