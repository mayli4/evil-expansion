using Daybreak.Common.Features.ModPanel;
using Daybreak.Common.Rendering;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;

namespace EvilExpansionMod.Content.Panel;

[UsedImplicitly]
internal sealed class PanelStyle : ModPanelStyleExt {
    private sealed class ModIcon() : UIImage(TextureAssets.MagicPixel) {
        protected override void DrawSelf(SpriteBatch spriteBatch) { }
    }

    public override IEnumerable<PanelInfo> GetInfos(Mod mod) => Array.Empty<PanelInfo>();

    //public override UIImage ModifyModIcon(UIModItem element, UIImage modIcon, ref int modIconAdjust) => new ModIcon();

    public override bool PreDrawPanel(UIModItem element, SpriteBatch sb, ref bool drawDivider) {
        if (element._needsTextureLoading) {
            element._needsTextureLoading = false;
            element.LoadTextures();
        }

        drawDivider = true;

        var dims = element.GetDimensions();

        int width = (int)Math.Floor(dims.Width);
        int height = (int)Math.Floor(dims.Height);
        Vector2 drawPosition = new((float)Math.Floor(dims.X), (float)Math.Floor(dims.Y));

        sb.End(out var ss);
        var scissor = sb.GraphicsDevice.ScissorRectangle;

        using var panelTarget = RenderTargetPool.Shared.Rent(
            sb.GraphicsDevice, width, height
        );

        var oldX = element._dimensions.X;
        var oldY = element._dimensions.Y;

        using (panelTarget.Scope(clearColor: Color.Transparent)) {
            element._dimensions.X = 0;
            element._dimensions.Y = 0;

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            var borderColor = element.BorderColor * 1.5f;

            element.DrawPanel(sb, element._borderTexture.Value, borderColor);
            element.DrawPanel(sb, element._backgroundTexture.Value, element.BackgroundColor);
            sb.End();
        }

        element._dimensions.X = oldX;
        element._dimensions.Y = oldY;
        sb.GraphicsDevice.ScissorRectangle = scissor;

        sb.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, ss.RasterizerState, null, Main.UIScaleMatrix); {
            var shader = Assets.Shaders.Panel.PanelBackgroundShader.CreatePanelShader();
            shader.Parameters.source = Transform(new Vector4(width, height, drawPosition.X, drawPosition.Y));
            shader.Parameters.colorLeft = Color.DarkRed.ToVector4();
            shader.Apply();

            sb.Draw(panelTarget.Target, drawPosition, Color.White);
        }
        sb.Restart(in ss);

        return false;
    }

    private static Vector4 Transform(Vector4 vector) {
        var vec1 = Vector2.Transform(new Vector2(vector.X, vector.Y), Main.UIScaleMatrix);
        var vec2 = Vector2.Transform(new Vector2(vector.Z, vector.W), Main.UIScaleMatrix);
        return new Vector4(vec1, vec2.X, vec2.Y);
    }
}