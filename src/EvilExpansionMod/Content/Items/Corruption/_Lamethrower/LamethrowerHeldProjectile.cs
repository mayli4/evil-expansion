using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

using Textures = Assets.Assets.Textures;

public class LamethrowerHeldProjectile : ModProjectile {
    static int FullFlameFrames = 60;

    static float FlameWidth = 90;
    static float FlameLength = 450;

    float FlameScale => MathF.Sin(MathF.PI * Projectile.timeLeft / FullFlameFrames / 2);

    Player Owner => Main.player[Projectile.owner];
    Vector2 _rotationVector;

    Vector2[] _trailPositions;

    public override string Texture => Textures.Items.Corruption.Lamethrower.KEY_LamethrowerItem;
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
        }

        return true;
    }

    public override void AI() {
        Owner.heldProj = Projectile.whoAmI;

        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        _rotationVector = Vector2.Lerp(mouseDirection, _rotationVector, 0.2f);
        Projectile.rotation = _rotationVector.ToRotation();

        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full,
            Projectile.rotation - MathHelper.PiOver2
        );
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        _trailPositions ??= [.. Enumerable.Repeat(Vector2.Zero, 8)];
        for(var i = 0; i < _trailPositions.Length; i++) {
            var targetPosition =
                FlameScale * FlameLength * i / _trailPositions.Length * _rotationVector + Main.rand.NextVector2Unit() * 10f;
            _trailPositions[i] = Vector2.Lerp(_trailPositions[i], targetPosition, 0.1f);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float _ = 0;
        var start = Projectile.position + _rotationVector * 80f;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            start,
            start + _rotationVector * FlameLength * FlameScale,
            FlameWidth,
            ref _
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var outlineColor = new Color(223, 116, 40);
        var flameColor = new Color(241, 194, 92);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        var glowTexture = Textures.Sample.Glow1.Value;
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position - Main.screenPosition + _rotationVector * 80f,
            null,
            flameColor * 0.35f,
            Projectile.rotation,
            glowTexture.Size() / 2f,
            new Vector2(FlameScale * 0.3f, 0.45f + Main.rand.NextFloat(0.02f)),
            SpriteEffects.None,
            0f
        );
        Main.spriteBatch.EndBegin(snapshot);

        var effect = Assets.Assets.Effects.Trail.Lamethrower.Value;
        for(var i = 0; i < 3; i++) {
            Main.graphics.GraphicsDevice.SamplerStates[i].AddressU =
                Main.graphics.GraphicsDevice.SamplerStates[i].AddressV = TextureAddressMode.Wrap;
        }

        var textures = Main.graphics.GraphicsDevice.Textures;
        textures[0] = Textures.Sample.PerlinNoise.Value;
        textures[1] = Textures.Sample.PlasmaNoise.Value;
        textures[2] = Textures.Sample.PortalNoise.Value;

        Graphics.BeginPipeline(0.5f)
            .EffectParams(
                effect,
                ("uTransformMatrix", Graphics.WorldTransformMatrix),
                ("uTime", Main.GameUpdateCount * 0.025f),
                ("uColor", outlineColor.ToVector4())
            )
            .DrawTrail(
                _trailPositions.Select(
                    p => p + Projectile.position + _rotationVector * 85f
                ).ToArray(),
                static _ => FlameWidth,
                t => Color.Lerp(flameColor, outlineColor, t),
                effect
            )
            .ApplyOutline(outlineColor)
            .ApplyOutline(flameColor)
            .Flush();

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
            Projectile.position - Main.screenPosition + _rotationVector * 80f,
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
