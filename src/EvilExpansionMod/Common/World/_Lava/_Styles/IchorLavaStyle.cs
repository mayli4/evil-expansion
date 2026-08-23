using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.World;

public class IchorLavafall : ModWaterfallStyle {
    public override string Texture => Assets.Images.Lavas.IchorLavafall.KEY;
}

public class IchorLavaStyle : ModLavaStyle {
    public override string LavaTexturePath => Assets.Images.Lavas.IchorLava.KEY;

    public override string BlockTexturePath => LavaTexturePath + "_Block";

    public override string SlopeTexturePath => LavaTexturePath + "_Slope";

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("EvilExpansionMod/IchorLavafall").Slot;

    public override int GetSplashDust() => DustID.Ichor;

    public override int GetDropletGore() => 0;
    
    public override int DebuffType() => BuffID.Ichor;

    public override void SelectLightColor(ref Color initialLightColor) {
        initialLightColor = Color.Yellow;
        initialLightColor.A = 255;
    }

    public override void ModifyVertexColors(int x, int y, ref VertexColors colors) {
        var colorA = Color.Yellow;
        var colorB = new Color(250, 195, 0, 255);

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