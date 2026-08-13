using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.World;

public class UnderworldCorruptLavafall : ModWaterfallStyle {
    public override string Texture => Assets.Textures.Lavas.UnderworldCorruptLavafall.KEY;
}

public class UnderworldCorruptLavaStyle : ModLavaStyle {
    public override string LavaTexturePath => Assets.Textures.Lavas.UnderworldCorruptLava.KEY;

    public override string BlockTexturePath => LavaTexturePath + "_Block";

    public override string SlopeTexturePath => LavaTexturePath + "_Slope";

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("EvilExpansionMod/UnderworldCorruptLavafall").Slot;

    public override int GetSplashDust() => DustID.CursedTorch;

    public override int GetDropletGore() => 0;

    public override int DebuffType() => BuffID.CursedInferno;

    public override void SelectLightColor(ref Color initialLightColor) {
        initialLightColor = Color.Yellow;
        initialLightColor.A = 200;
    }

    public override void ModifyVertexColors(int x, int y, ref VertexColors colors) {
        var colorA = new Color(177, 199, 13, 255);
        var colorB = Color.Yellow;

        var distanceFromBottom = Math.Max(Main.maxTilesY - y - 88f, 0f);
        var range = 35f;

        var topColor = Color.Lerp(colorA, colorB, distanceFromBottom / range);
        var bottomColor = Color.Lerp(colorA, colorB, (distanceFromBottom - 1) / range);

        colors.TopLeftColor = new Color(topColor.R, topColor.G, topColor.B, colors.TopLeftColor.A);
        colors.TopRightColor = new Color(topColor.R, topColor.G, topColor.B, colors.TopRightColor.A);

        colors.BottomLeftColor = new Color(bottomColor.R, bottomColor.G, bottomColor.B, colors.BottomLeftColor.A);
        colors.BottomRightColor = new Color(bottomColor.R, bottomColor.G, bottomColor.B, colors.BottomRightColor.A);
    }
}