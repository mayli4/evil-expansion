using EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;

public class CorruptAshwoodBedTile : BedTile<CorruptAshwoodBed> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodBedTile.KEY;
};

public class CorruptAshwoodBed : BedItem<CorruptAshwoodBedTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodBedItem.KEY;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(15)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Sawmill)
            .Register();
    }
};