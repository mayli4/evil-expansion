using EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;

public class CorruptAshwoodDresserTile : DresserTile<CorruptAshwoodDresser> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodDresserTile.KEY;
};

public class CorruptAshwoodDresser : DresserItem<CorruptAshwoodDresserTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodDresserItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(16)
            .AddTile(TileID.Sawmill)
            .Register();
    }
};