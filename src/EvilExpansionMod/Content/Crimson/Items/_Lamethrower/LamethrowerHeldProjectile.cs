using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerHeldProjectile : ModProjectile {
    readonly static int FullFlameFrames = 60;

    readonly static float FlameWidth = 90;
    readonly static float FlameLength = 450;

    float FlameScale => MathF.Sin(MathF.PI * Projectile.timeLeft / FullFlameFrames / 2);

    Player Owner => Main.player[Projectile.owner];
    Vector2 _rotationVector;
    Vector2 _trailOrigin;

    Vector2[] _trailPositions;

    public override string Texture => Assets.Images.Corruption.Items.Lamethrower.LamethrowerItem.KEY;
    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = FullFlameFrames * 2;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        _rotationVector = Projectile.Center.DirectionTo(Main.MouseWorld);
        Projectile.rotation = _rotationVector.ToRotation();
    }

    public override bool PreAI() {
        if(Owner.HeldItem.type != ModContent.ItemType<LamethrowerItem>()) {
            Projectile.Kill();
            return false;
        }

        if(Owner.channel) {
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, FullFlameFrames);
            Owner.itemTime = Projectile.timeLeft;
            Owner.itemAnimation = Projectile.timeLeft;
        }

        return true;
    }

    public override void AI() {
        Owner.heldProj = Projectile.whoAmI;

        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        _rotationVector = Vector2.Lerp(mouseDirection, _rotationVector, 0.2f);

        _trailOrigin = Projectile.position + _rotationVector * 80f + _rotationVector.RotatedBy(MathHelper.PiOver2 * Owner.direction) * 8f;
        _trailOrigin += Owner.velocity;

        Projectile.rotation = _rotationVector.ToRotation();

        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full,
            Projectile.rotation - MathHelper.PiOver2
        );
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        _trailPositions ??= [.. Enumerable.Repeat(Vector2.Zero, 8)];
        for(var i = 0; i < _trailPositions.Length; i++) {
            var targetPosition = FlameScale * FlameLength * i / _trailPositions.Length * _rotationVector;
            _trailPositions[i] = Vector2.Lerp(_trailPositions[i], targetPosition, 0.1f);
        }

        Span<Point> tiles = new Point[128];
        var count = Helper.TilesLineCollision(
            tiles,
            _trailOrigin + _trailPositions[0],
            _trailOrigin + _trailPositions[^1],
            FlameWidth * FlameScale
        );

        foreach(var tile in tiles[..count]) {
            if(
                Main.tile[tile.X, tile.Y].HasTile
                && Main.tile[tile.X, tile.Y].BlockType == BlockType.Solid
                && !Main.tile[tile.X, tile.Y - 1].HasTile
                && Main.rand.NextFloat() < 0.05f
            ) {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    tile.ToVector2() * 16f + Vector2.UnitX * 8f,
                    Vector2.Zero,
                    ModContent.ProjectileType<LingeringIchorProjectile>(),
                    Projectile.damage,
                    0f
                );
            }
        }

        Lighting.AddLight(
            _trailOrigin + _trailPositions[0],
            2.41f * FlameScale,
            1.94f * FlameScale,
            0.92f * FlameScale
        );
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        for(var i = 0; i < _trailPositions.Length - 1; i++) {
            var _ = 0f;
            if(Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                _trailOrigin + _trailPositions[i],
                _trailOrigin + _trailPositions[i + 1],
                50f,
                ref _
            )) {
                return true;
            }
        }

        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var outlineColor = new Color(223, 116, 40);
        var flameColor = new Color(241, 194, 92);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var glowPosition = _trailOrigin + _rotationVector * 2f;
        Main.spriteBatch.Draw(
            glowTexture,
            glowPosition - Main.screenPosition,
            null,
            flameColor * 0.35f,
            Projectile.rotation,
            glowTexture.Size() / 2f,
            new Vector2(FlameScale * 0.3f, 0.15f + Main.rand.NextFloat(0.02f)),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowTexture,
            (_trailPositions[0] + _trailPositions[^1]) * 0.5f - Main.screenPosition + _trailOrigin,
            null,
            flameColor * 0.35f * FlameScale,
            (_trailPositions[0] - _trailPositions[^1]).ToRotation(),
            glowTexture.Size() / 2f,
            new Vector2(FlameScale * 1.3f, 0.45f + Main.rand.NextFloat(0.02f)),
            SpriteEffects.None,
            0f
        );
        Main.spriteBatch.EndBegin(snapshot);

        var flameShader = Assets.Shaders.Trail.LingeringFlame.Asset.Value;
        var noiseTexture1 = Assets.Images.Sample.Pebbles.Asset.Value;
        var circleTexture = Assets.Images.Misc.Circle.Asset.Value;

        Renderer.BeginPipeline(0.5f)
            .SetTexture(0, circleTexture)
            .SetEffectParams(
                flameShader,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI + 10),
                ("size", new Vector2(1, 1)),
                ("coreColor", Color.Lerp(flameColor, Color.White, 0.4f).ToVector4()),
                ("outerCoreColor", flameColor.ToVector4()),
                ("flameColor", outlineColor.ToVector4()),
                ("noiseScale", 0.5f),
                ("flameSize", FlameScale),
                ("tex1", noiseTexture1),
                ("uTransformMatrix", Graphics.WorldTransformMatrix))
            .DrawTrail(
                _trailPositions.Select(p => p + _trailOrigin).ToArray(),
                static _ => FlameWidth,
                t => Color.Lerp(flameColor, outlineColor, t),
                flameShader
            )
            .ApplyOutline(flameColor)
            .End();

        var texture = TextureAssets.Projectile[Type].Value;
        var origin = new Vector2(-8, 18);
        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation - (Owner.direction == -1 ? MathF.PI : 0f),
            Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Vector2.One * Projectile.scale,
            Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        Main.spriteBatch.EndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            glowPosition - Main.screenPosition,
            null,
            flameColor,
            0f,
            glowTexture.Size() / 2f,
            FlameScale * 0.1f + Main.rand.NextFloat(0.02f),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}
