using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace EvilExpansionMod.Utilities;

internal static class RenderPipelineExtensions {
    extension(RenderPipeline @this) {
        public RenderPipeline ApplyTint(Color color) {
            return @this.ApplyEffect(Assets.Shaders.Pixel.Tint.Asset.Value, ("uColor", color));
        }

        public RenderPipeline ApplyOutline(Color color, float threshold = 0.001f) {
            return @this.ApplyEffect(
                Assets.Shaders.Pixel.Outline.Asset.Value,
                ("uColor", color),
                ("uThreshold", threshold),
                ("uSize", Main.ScreenSize.ToVector2() * 2f));
        }

        public RenderPipeline ApplyBloom(float intensity = 1.5f, float threshold = 0.5f) {
            Vector2 screenSize = Main.ScreenSize.ToVector2() * 2f;
            Vector2 texelSize = new Vector2(1f / screenSize.X, 1f / screenSize.Y);

            return @this
                .SetSamplerState(0, SamplerState.LinearClamp)
                .ApplyEffect(
                    Assets.Shaders.Pixel.Bloom.Asset.Value,
                    ("uThreshold", threshold),
                    ("uIntensity", intensity),
                    ("uTexelSize", texelSize)
                );
        }
    }

}
