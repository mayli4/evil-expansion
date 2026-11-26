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

public class CorruptAshwoodChandelierTile : ChandelierTile {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodChandelierTile.KEY;
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        g += GlowColor.G / 255f;
        r += GlowColor.B / 500f;
        return;
    }
};

public class CorruptAshwoodChandelier : ChandelierItem<CorruptAshwoodChandelierTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodChandelierItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(4)
            .AddIngredient(ItemID.Torch, 4)
            .AddIngredient(ItemID.Chain, 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
};