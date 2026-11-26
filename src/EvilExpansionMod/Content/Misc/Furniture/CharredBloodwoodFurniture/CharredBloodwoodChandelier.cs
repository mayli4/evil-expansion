using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodChandelierTile : ChandelierTile {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodChandelierTile.KEY;
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = GlowColor.R / 255f;
            g = GlowColor.G / 255f;
            b = GlowColor.B / 255f;
        return;
    }
};

public class CharredBloodwoodChandelier : ChandelierItem<CharredBloodwoodChandelierTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodChandelierItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(4)
            .AddIngredient(ItemID.Torch, 4)
            .AddIngredient(ItemID.Chain, 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
};