using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodLampTile : LampTile<CharredBloodwoodLamp> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodLampTile.KEY;

    public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Glow");
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        r += GlowColor.R / 255f;
        g += GlowColor.R / 400f;
        return;
    }
};

    public class CharredBloodwoodLamp : LampItem<CharredBloodwoodLampTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodLampItem.KEY;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(3)
            .AddIngredient(ItemID.Torch, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
};