using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class ImputedFlameItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.ImputedFlame.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 20;
    }
    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        Item.value = 3500;
        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
    public override Color? GetAlpha(Color lightColor) {
        return (new Color (255,255,255));
    }

    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.YellowGreen.ToVector3() * 0.55f * Main.essScale);
    }
}
