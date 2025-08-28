using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Food;

public class Lime : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Foods.KEY_Lime;
    
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        ItemID.Sets.IsFood[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(2, 3) { NotActuallyAnimating = true });

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
        Recipes.AddToGroup(RecipeGroupID.Fruit, Type);
    }
    
    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 32;
        Item.rare = ItemRarityID.Blue;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(0, 0, 5, 0);
        Item.noUseGraphic = false;
        Item.useStyle = ItemUseStyleID.EatFood;
        Item.useTime = Item.useAnimation = 20;
        Item.noMelee = true;
        Item.consumable = true;
        Item.autoReuse = false;
        Item.UseSound = SoundID.Item2;
        Item.buffTime = 5 * 60 * 60;
        Item.buffType = BuffID.WellFed;
    }
}