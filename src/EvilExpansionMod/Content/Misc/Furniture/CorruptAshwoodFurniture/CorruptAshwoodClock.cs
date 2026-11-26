using EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;

public class CorruptAshwoodClockTile : ClockTile<CorruptAshwoodClock> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodClockTile.KEY;
};

public class CorruptAshwoodClock : ClockItem<CorruptAshwoodClockTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodClockItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(10)
            .AddIngredient(ItemID.IronBar, 3)
            .AddIngredient(ItemID.Glass, 6)
            .AddTile(TileID.Sawmill)
            .Register();
    }
};