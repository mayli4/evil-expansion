using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodWorkBenchTile : WorkBenchTile {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodWorkBenchTile.KEY;
};

public class CharredBloodwoodWorkBench : WorkBenchItem<CharredBloodwoodWorkBenchTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodWorkBenchItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(10)
            .Register();
    }
};