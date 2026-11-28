using Microsoft.Xna.Framework;
using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Food;

public class CowHeart : ModItem {
    public override string Texture => Assets.Images.Foods.CowHeart.KEY;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        ItemID.Sets.IsFood[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3) { NotActuallyAnimating = true });

        ItemID.Sets.FoodParticleColors[Type] = [
                new Color(199, 169, 159),
                new Color(167, 71, 80),
                new Color(106, 45, 51)
        ];

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
        Recipes.AddToGroup(RecipeGroupID.Fruit, Type);
    }

    public override void SetDefaults() {
        Item.DefaultToFood(26, 32, BuffID.WellFed2, 5 * 60 * 60);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 5, 0);

        Item.width = 26;
        Item.height = 32;
        Item.rare = ItemRarityID.Green;
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
        Item.buffType = BuffID.WellFed2;
    }
}