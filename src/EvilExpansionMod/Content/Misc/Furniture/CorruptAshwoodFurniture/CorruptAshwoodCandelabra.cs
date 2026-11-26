using EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture;
using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;

public class CorruptAshwoodCandelabraTile : CandelabraTile {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodCandelabraTile.KEY;
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        g += GlowColor.G / 255f;
        r += GlowColor.B / 500f;
        return;
    }

};

public class CorruptAshwoodCandelabra : CandelabraItem<CorruptAshwoodCandelabraTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodCandelabraItem.KEY;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(5)
            .AddIngredient(ItemID.Torch, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
};