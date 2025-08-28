using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson._MeatAxe;
public class CutProjectile : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;

    public List<Vector2> TrailPositions = [];

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.DamageType = DamageClass.Melee;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override bool ShouldUpdatePosition() => false;

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

    public override bool PreDraw(ref Color lightColor) {
        var effect = Assets.Assets.Effects.Trail.AxeCut.Value;
        var positions = CollectionsMarshal.AsSpan(TrailPositions);
        var texture = Assets.Assets.Textures.Gores.PlanetoidGore0.Value;
        Graphics.BeginPipeline(0.5f)
            .DrawTrail(
                positions,
                t => MathF.Sin(t * MathHelper.Pi) * 20f
                    * MathF.Pow(MathF.Sin(MathHelper.PiOver2 * Projectile.timeLeft / 120f), 2),
                static _ => Color.Red,
                effect,
                ("uImage0Texture", texture),
                ("uImage0Size", texture.Size()),
                ("uTransformMatrix", Graphics.WorldTransformMatrix)
            )
            .Flush();

        return false;
    }
}
