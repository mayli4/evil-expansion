using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Crimson.Tiles;

namespace EvilExpansionMod.Content.Crimson;

public class CharredBloodwoodItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 100;
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(ModContent.ItemType<CharredBloodwoodItem>());
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CharredBloodwoodTile>());
        Item.width = 20;
        Item.height = 20;
        Item.value = Item.sellPrice();

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes() {
        CreateRecipe(1)
            .AddIngredient<CharredBloodwoodWallItem>(4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
internal sealed class CharredBloodwoodTile : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.CharredBloodwoodTile.KEY;
    public override void SetStaticDefaults() {
                
        Main.tileSolid[Type] = true;
        Main.tileBrick[Type] = true;

        DustType = DustID.Blood;

        AddMapEntry(new Color(118, 18, 15));
    }
}