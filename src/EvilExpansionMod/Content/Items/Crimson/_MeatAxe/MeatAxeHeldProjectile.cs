using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Items.Crimson._MeatAxe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class MeatAxeHeldProjectile : ModProjectile {
    Player Owner => Main.player[Projectile.owner];
    float Progress => 1f - (float)Owner.itemAnimation / Owner.itemAnimationMax;

    Vector2[] _trailPositions;
    Vector2 _rotationVector;

    ref float TargetRotation => ref Projectile.ai[0];
    int CutProjectile { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
    int _cutUpdateTimer;

    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatAxe.KEY_MeatAxeItem;
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
        Projectile.timeLeft = 999;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 999;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        TargetRotation = Owner.Center.DirectionTo(Main.MouseWorld).ToRotation();

        CutProjectile = -1;
        Projectile.netUpdate = true;
    }

    public override bool PreAI() {
        if(Owner.HeldItem.type != ModContent.ItemType<MeatAxeItem>() || Owner.ItemAnimationEndingOrEnded) {
            Projectile.Kill();
            return false;
        }

        Owner.heldProj = Projectile.whoAmI;
        return true;
    }

    public override void AI() {
        var arc = 1.5f * MathF.PI * Owner.direction;

        var t = 3f * MathF.Pow(Progress, 2) - 2f * MathF.Pow(Progress, 3) - 0.3f * (1f - Progress) * MathF.Sin(MathHelper.Pi * Progress);
        Projectile.rotation = TargetRotation - arc / 2f + arc * t;

        _rotationVector = Projectile.rotation.ToRotationVector2();

        if(Progress > 0.3f && Progress < 0.7f) {
            if(CutProjectile == -1) {
                if(Main.netMode != NetmodeID.MultiplayerClient) {
                    CutProjectile = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.position,
                        Vector2.Zero,
                        ModContent.ProjectileType<CutProjectile>(),
                        30,
                        0f
                    );
                    Projectile.netUpdate = true;
                }
            }
            else {
                var cutPosition = Projectile.position
                    + _rotationVector * 75f
                    + _rotationVector.RotatedBy(MathHelper.PiOver2) * 30f;

                var cut = Main.projectile[CutProjectile].ModProjectile as CutProjectile;
                cut.TrailPositions.Add(cutPosition);

                for(var i = 0; i < 4; i++) {
                    Dust.NewDustPerfect(
                        cutPosition + Main.rand.NextVector2Unit() * Main.rand.NextFloat(30f),
                        DustID.Blood,
                        _rotationVector.RotatedBy((-Main.rand.NextFloat(MathHelper.PiOver2) - MathHelper.PiOver2) * Owner.direction) * 8f
                    );
                }
            }
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        var trailLastPosition = _rotationVector;
        _trailPositions ??= [.. Enumerable.Repeat(trailLastPosition, 5)];

        for(var i = _trailPositions.Length - 1; i > 0; i--) {
            _trailPositions[i] = _trailPositions[i - 1];
        }
        _trailPositions[0] = trailLastPosition;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.position,
            Projectile.position + _rotationVector * 90f,
            40,
            ref _
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        if(Progress > 0.15f) {
            Graphics.BeginPipeline(0.5f)
                .DrawBasicTrail(
                    _trailPositions.Select(
                        p => Projectile.position + p * 85f + p.RotatedBy(MathF.PI / 2f) * 10f
                    ).ToArray(),
                    static t => (1f - t) * 3f,
                    TextureAssets.MagicPixel.Value,
                    static t => Color.Lerp(Color.White, Color.Transparent, t)
                )
                .Flush();

            Graphics.BeginPipeline(0.5f)
                .DrawBasicTrail(
                    _trailPositions.Select(
                        p => Projectile.position + p * 65f - p.RotatedBy(MathF.PI / 2f) * 18f
                    ).ToArray(),
                    static t => (1f - t) * 5f,
                    TextureAssets.MagicPixel.Value,
                    static t => Color.Lerp(Color.White, Color.Transparent, t)
                )
                .Flush();
        }

        var texture = TextureAssets.Projectile[Type].Value;
        var origin = new Vector2(0, 64);

        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation + (Owner.direction > 0 ? MathHelper.PiOver4 : MathHelper.PiOver4 * 3),
            Owner.direction > 0 ? origin : new Vector2(texture.Width - origin.X, origin.Y),
            Projectile.scale,
            Owner.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        return false;
    }
}
