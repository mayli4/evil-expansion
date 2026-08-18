using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Terraria;

namespace EvilExpansionMod.Utilities;

internal static class RenderPipelineExtensions {
    public static RenderPipeline ApplyTint(this RenderPipeline @this, Color color) {
        return @this.ApplyEffect(Assets.Shaders.Pixel.Tint.Asset.Value, ("uColor", color));
    }

    public static RenderPipeline ApplyOutline(this RenderPipeline @this, Color color, float threshold = 0.001f) {
        return @this.ApplyEffect(
            Assets.Shaders.Pixel.Outline.Asset.Value,
            ("uColor", color),
            ("uThreshold", threshold),
            ("uSize", Main.ScreenSize.ToVector2()));
    }

    public static RenderPipeline ApplyBloom(this RenderPipeline @this, float intensity = 1.5f, float threshold = 0.5f) {
        return @this.ApplyEffect(
            Assets.Shaders.Pixel.Bloom.Asset.Value,
            ("uThreshold", threshold),
            ("uIntensity", intensity),
            ("uSize", Main.ScreenSize.ToVector2()));
    }
}
