using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class ImputedFlameItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.ImputedFlame.KEY;
    
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 35;
        
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));

        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }
    
    public override void SetDefaults() {
        (Item.width, Item.height) = (20, 20);
        Item.value = 3500;
        Item.maxStack = Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
    
    public override Color? GetAlpha(Color lightColor) {
        return (new Color (255,255,255));
    }

    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.YellowGreen.ToVector3() * 0.55f * Main.essScale);
    }
}
