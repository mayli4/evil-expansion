using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class CutProjectile : ModProjectile, IPreDrawEverythingBulk<CutProjectile> {
    public override string Texture => Helper.PlaceholderTextureKey;

    public List<Vector2> TrailPositions = [];

    static int MaxTimeLeft = 120;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.DamageType = DamageClass.Melee;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        for(var i = 0; i < TrailPositions.Count - 1; i++) {
            TrailPositions[i] += Vector2.UnitY * 0.25f * i / TrailPositions.Count;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        for(var i = 0; i < TrailPositions.Count - 1; i++) {
            var _ = 0f;
            if(Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                TrailPositions[i],
                TrailPositions[i + 1],
                30f,
                ref _
            )) {
                return true;
            }
        }

        return false;
    }

    public void PreDrawEverythingBulk(ReadOnlySpan<CutProjectile> entities) {
        using var pipeline = new RenderPipeline(Graphics.PreDrawNPCsQueue, 0.5f, Graphics.WorldTransformMatrix);
        foreach(var entity in entities) {
            entity.Draw(pipeline);
        }

        pipeline.ApplyOutline(Color.Lerp(Color.DarkRed, Color.Black, 0.5f));
    }

    private void Draw(RenderPipeline pipeline) {
        var progress = 1f - (float)Projectile.timeLeft / MaxTimeLeft;

        pipeline.SetTexture(TextureAssets.MagicPixel.Value);

        var buf = new Vector2[2];
        for(var i = 1; i < TrailPositions.Count - 2; i++) {
            buf[0] = TrailPositions[i];
            buf[1] = buf[0]
                + Vector2.UnitY * 20f
                * MathF.Sin(progress * MathHelper.Pi)
                * ((MathF.Sin(4f * MathHelper.Pi * i / TrailPositions.Count) + 1f) / 2f);

            pipeline.DrawTrail(
                buf,
                t => MathF.Max((1f - t) * 3f, 2.1f),
                static _ => Color.DarkRed
            );
        }

        var axeCutEffect = Assets.Shaders.Trail.AxeCut.Asset.Value;

        var positions = CollectionsMarshal.AsSpan(TrailPositions);
        var texture = Assets.Images.Crimson.Items.MeatAxe.CutTexture.Asset.Value;

        pipeline
            .SetEffectParams(
                axeCutEffect,
                ("uImage0Texture", texture),
                ("uImage0Size", texture.Size()),
                ("uTransformMatrix", Graphics.WorldTransformMatrix))
            .DrawTrail(
                positions,
                t => MathF.Sin(t * MathHelper.Pi) * 30f * (1f + 0.2f * MathF.Sin(t * MathHelper.Pi * 6f))
                    * MathF.Pow(MathF.Sin(MathHelper.PiOver2 * (1f - progress)), 2),
                static _ => Color.Red,
                axeCutEffect);
    }
}
