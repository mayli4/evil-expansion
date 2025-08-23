using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class LamethrowerHeldProjectile : ModProjectile {
    static int FullFlameFrames = 60;

    static float FlameWidth = 60;
    static float FlameLength = 140;

    float FlameScale => MathF.Sin(MathF.PI * Projectile.timeLeft / FullFlameFrames / 2);

    Player Owner => Main.player[Projectile.owner];
    Vector2 _rotationVector;

    Vector2[] _trailPositions;

    public override string Texture => Assets.Assets.Textures.Items.Corruption.Lamethrower.KEY_LamethrowerItem;
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
        Projectile.localNPCHitCooldown = 999;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        _rotationVector = Projectile.Center.DirectionTo(Main.MouseWorld);
        Projectile.rotation = _rotationVector.ToRotation();
    }

    public override bool PreAI() {
        Owner.itemAnimation = 1;
        if(Owner.HeldItem.type != ModContent.ItemType<LamethrowerItem>()) {
            Projectile.Kill();
            return false;
        }

        if(Owner.channel) Projectile.timeLeft = Math.Max(Projectile.timeLeft, FullFlameFrames);
        return true;
    }

    public override void AI() {
        Owner.heldProj = Projectile.whoAmI;

        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        _rotationVector = Vector2.Lerp(mouseDirection, _rotationVector, 0.2f);
        Projectile.rotation = _rotationVector.ToRotation();

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.direction * (Projectile.rotation - MathHelper.PiOver2));
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        _trailPositions ??= [.. Enumerable.Repeat(Projectile.position, 8)];
        for(var i = 0; i < _trailPositions.Length; i++) {
            var targetPosition = Projectile.position + (FlameLength * i / _trailPositions.Length) * _rotationVector;
            _trailPositions[i] = Vector2.Lerp(_trailPositions[i], targetPosition, 1f - MathF.Min(1.25f * i / _trailPositions.Length, 0.95f));
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Owner.channel) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.position,
            Projectile.position + _rotationVector * FlameLength * FlameScale,
            FlameWidth,
            ref _
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        Graphics.BeginPipeline(0.5f)
            .DrawBasicTrail(_trailPositions, static _ => FlameWidth, texture, lightColor)
            .Flush();

        return false;
    }
}
