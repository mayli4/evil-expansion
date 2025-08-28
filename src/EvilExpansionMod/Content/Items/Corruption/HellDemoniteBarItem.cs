using EvilExpansionMod.Content.Items.Corruption;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class HellDemoniteBarItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.KEY_HellDemoniteBar;
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
            .AddIngredient<Tiles.Corruption.UnderworldDemoniteItem>(3)
            .AddIngredient<RawShadowScalesItem>()
            .AddTile(TileID.Furnaces)
            .Register();
    }
}
