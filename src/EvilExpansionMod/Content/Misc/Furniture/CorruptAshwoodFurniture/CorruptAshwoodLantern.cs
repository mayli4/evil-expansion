using EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture;
using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;

public class CorruptAshwoodLanternTile : LanternTile {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodLanternTile.KEY;
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        g += GlowColor.G / 255f;
        r += GlowColor.B / 500f;
        return;
    }
};

public class CorruptAshwoodLantern : LanternItem<CorruptAshwoodLanternTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodLanternItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(6)
            .AddIngredient(ItemID.Torch, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

};