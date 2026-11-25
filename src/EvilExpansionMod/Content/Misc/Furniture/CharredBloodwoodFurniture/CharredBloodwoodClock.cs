using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodClockTile : ClockTile<CharredBloodwoodClock> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodClockTile.KEY;
};

public class CharredBloodwoodClock : ClockItem<CharredBloodwoodClockTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodClockItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(10)
            .AddIngredient(ItemID.IronBar, 3)
            .AddIngredient(ItemID.Glass, 6)
            .AddTile(TileID.Sawmill)
            .Register();
    }
};