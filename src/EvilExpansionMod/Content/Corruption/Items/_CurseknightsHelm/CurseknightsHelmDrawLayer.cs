using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
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

        var fireParticlePlayer = drawPlayer.GetModPlayer<CurseknightFireParticlePlayer>();

        var fireTexture = Assets.Images.Misc.Circle.Asset.Value;
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;

        var fireColor = new Color(0.901f, 1f, 0);

        using(var scope = _targetLease.Scope(true, Color.Transparent)) {
            using var pipeline = Graphics.BeginPixelated();
            pipeline.SetBlendState(BlendState.Additive);

            foreach(var particle in fireParticlePlayer.Particles) {
                var progress = (float)particle.TimeLeft / CurseknightFireParticle.MaxTimeLeft;
                var color = Color.Lerp(fireColor, new Color(0.2f, 0.2f, 0.2f, 0f), MathF.Max(0f, 1f - particle.Alpha * progress * immuneAlphaMultiplier));

                pipeline.DrawTexture(new()
                {
                    Texture = fireTexture,
                    Position = particle.Position - Main.screenPosition,
                    Scale = new Vector2(1f, 0.65f) * (0.2f + progress * 0.25f) * particle.Scale,
                    Origin = fireTexture.Size() / 2f,
                    Rotation = particle.Rotation,
                    Color = color,
                });
            }

            // foreach(var particle in fireParticlePlayer.Particles) {
            //     var progress = (float)particle.TimeLeft / CurseknightFireParticle.MaxTimeLeft;
            //     pipeline.DrawTexture(new()
            //     {
            //         Texture = glowTexture,
            //         Position = particle.Position - Main.screenPosition,
            //         Scale = Vector2.One * progress * 0.25f,
            //         Origin = glowTexture.Size() / 2f,
            //         Color = fireColor * 0.1f * immuneAlphaMultiplier,
            //     });
            // }

            pipeline.ApplyBloom(0.5f);
        }

        drawInfo.DrawDataCache.Add(new(
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
