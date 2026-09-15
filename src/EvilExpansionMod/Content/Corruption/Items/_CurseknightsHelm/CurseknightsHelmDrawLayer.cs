using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption.Items._CurseknightsHelm;

internal class CurseknightsHelmDrawLayer : PlayerDrawLayer {
    private static RenderTargetLease _targetLease = null!;

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            _targetLease = ScreenspaceTargetPool.Shared.Rent(Graphics.Device);
        });
    }

    public override void Unload() {
        _targetLease.Dispose();
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        if(drawInfo.drawPlayer.dead)
            return false;

        var modPlayer = drawInfo.drawPlayer.GetModPlayer<CurseknightsHelmPlayer>();
        return modPlayer.IsWearingHelm && !modPlayer.HideVisual;
    }
    
    protected override void Draw(ref PlayerDrawSet drawInfo) {
        var drawPlayer = drawInfo.drawPlayer;

        var headPosition = new Vector2(
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)
        ) + drawPlayer.headPosition + drawInfo.headVect;

        var immuneAlphaMultiplier = ((255f - drawInfo.drawPlayer.immuneAlpha) / 255f);

        var circleTexture = Assets.Images.Misc.Circle.Asset.Value;
        
        const int flamePoints = 9;
        const float flameSpacing = 7f;
        const float flameWidth = 22f;

        using(var scope = _targetLease.Scope(true, Color.Transparent)) {
            using var pipeline = Graphics.Begin(0.5f * Main.GameViewMatrix.Zoom.X, Matrix.Identity);
            pipeline.SetBlendState(BlendState.Additive);
            
            var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
            
            if(CurseknightsHelm.HelmExploded) {
                var anchor = headPosition - drawInfo.headVect + new Vector2(drawPlayer.bodyFrame.Width * 0.5f, 18f) + Main.screenPosition;
                anchor += Main.OffsetsPlayerHeadgear[drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height] * drawPlayer.gravDir;
                var wobble = 0.1f * Main.GameUpdateCount + drawPlayer.whoAmI * 0.7f;
                
                var flow = new Vector2(-drawPlayer.direction * 0.6f, -1f).SafeNormalize(Vector2.UnitY);
                var sideways = new Vector2(-flow.Y, flow.X);

                Span<Vector2> flame = new Vector2[flamePoints];
                for(var i = 0; i < flamePoints; i++) {
                    var t = i / (float)(flamePoints - 1);
                    flame[i] = anchor
                               + flow * (i * flameSpacing)
                               + sideways * MathF.Sin(wobble - i * 0.7f) * (1.5f + 4.5f * t)
                               - drawPlayer.velocity * (i * 0.45f)
                               - Main.screenPosition;
                }
                
                const float anchorBoundsRadius = 50f;

                var fire = Assets.Shaders.Pixel.CursedSpiritFire.Asset.Value;
                pipeline
                    .SetTexture(0, Assets.Images.Sample.Noise2.Asset.Value)
                    .SetTexture(1, Assets.Images.Sample.Noise6.Asset.Value)
                    .SetEffectParams(
                        fire,
                        ("uTime", 0.025f * Main.GameUpdateCount + drawPlayer.whoAmI * 3.432f),
                        ("uStepY", 0.095f),
                        ("uColor1", CursedSpiritNPC.GhostColor2),
                        ("uColor2", new Color(230, 255, 0)),
                        ("uStepColor", 0.05f),
                        ("uScale", 0.65f))
                    .DrawTrail(
                        flame,
                        static t => flameWidth * (1f - t) + 10f,
                        _ => Color.White * immuneAlphaMultiplier,
                        fire)
                    // need this so the circle doesnt jitter? assuming due to aggressive viewport crop matrix
                    .DrawTexture(new DrawTextureOptions
                    {
                        Texture = TextureAssets.MagicPixel.Value,
                        Position = anchor - new Vector2(anchorBoundsRadius) - Main.screenPosition,
                        Size = new Vector2(anchorBoundsRadius * 2f),
                        Color = Color.Transparent,
                    })
                    .DrawTexture(new DrawTextureOptions
                    {
                        Texture = circleTexture,
                        Position = anchor - Main.screenPosition,
                        Color = CursedSpiritNPC.GhostColor2 * immuneAlphaMultiplier,
                        Origin = 16f * Vector2.One,
                        Scale = Vector2.One * 0.70f,
                    })
                    .ApplyOutline(CursedSpiritNPC.GhostColor1 * 0.5f)
                    .DrawTexture(new DrawTextureOptions
                    {
                        Texture = glowTexture,
                        Position = anchor - Main.screenPosition,
                        Color = CursedSpiritNPC.GhostColor1 * 0.45f * immuneAlphaMultiplier,
                        Origin = glowTexture.Size() / 2f,
                        Scale = Vector2.One * 0.15f,
                    })
                    .ApplyBloom(0.2f);
            }
        }

        drawInfo.DrawDataCache.Add(new DrawData(
            _targetLease.Target,
            Vector2.Zero,
            Color.White));

        var helmTexture = CurseknightsHelm.HelmExploded
            ? Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_HeadGlow.Asset
            : Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_HeadGlow.Asset;

        var drawData = new DrawData(
            helmTexture.Value,
            headPosition,
            drawPlayer.bodyFrame,
            Color.White * immuneAlphaMultiplier,
            drawPlayer.headRotation,
            drawInfo.headVect,
            1f,
            drawInfo.playerEffect)
        {
            shader = drawInfo.cHead,
        };

        drawInfo.DrawDataCache.Add(drawData);
    }
}
