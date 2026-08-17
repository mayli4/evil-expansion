using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class CartilageBarItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CartilageBarItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
        ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
    }

    public override void SetDefaults() {
        // Item.DefaultToPlaceableTile(ModContent.TileType<>());
        Item.width = 20;
        Item.height = 20;
        Item.value = 750;

        Item.maxStack = Terraria.Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CartilageOreItem>(3)
            .AddIngredient<PusClumpItem>()
            .AddTile(TileID.Furnaces)
            .Register();
    }
}
