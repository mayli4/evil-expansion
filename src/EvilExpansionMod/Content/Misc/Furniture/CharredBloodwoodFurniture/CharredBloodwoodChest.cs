using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodChestTile : ChestTile<CharredBloodwoodChest> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodChestTile.KEY;
};

public class CharredBloodwoodChest : ChestItem<CharredBloodwoodChestTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodChestItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(4)
            .AddIngredient(ItemID.IronBar, 2)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
};