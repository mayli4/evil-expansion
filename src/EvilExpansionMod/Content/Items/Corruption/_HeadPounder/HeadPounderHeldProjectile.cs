using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.NPCs.Corruption;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;
public class HeadPounderHeldProjectile : ModProjectile {
    Player Owner => Main.player[Projectile.owner];

    static int PostChargeFrames = 15;
    static int MaxCharge = 30;

    int _charge;
    bool _hitCheck;
    bool _hit;
    float _outlineAlpha;

    float ChargeProgress => MathF.Min((float)_charge / MaxCharge, 1f);
    Vector2 RotationVector => (Projectile.rotation - MathF.PI / 4f).ToRotationVector2() * new Vector2(Owner.direction, 1f);

    Vector2[] _trailPositions;

    public override string Texture => Assets.Assets.Textures.Items.Corruption.HeadPounder.KEY_HeadPounderItem;
    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = PostChargeFrames;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 999;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreAI() {
        Owner.itemAnimation = 1;
        if(Owner.HeldItem.type != ModContent.ItemType<HeadPounderItem>()) {
            Projectile.Kill();
            return false;
        }

        return true;
    }

    public override void AI() {
        Owner.heldProj = Projectile.whoAmI;
        if(Owner.channel) {
            Projectile.timeLeft = PostChargeFrames;
            Projectile.rotation = -3f * MathF.PI / 4f - ChargeProgress * MathF.PI / 8f;

            _charge += 1;
            _outlineAlpha = MathF.Pow(ChargeProgress, 2);

            if(_charge >= MaxCharge) {
                if(_charge == MaxCharge) {
                    SoundEngine.PlaySound(SoundID.Tink);
                }
            }
        }
        else {
            var charged = ChargeProgress >= 1f;
            if(charged) _outlineAlpha = 1f;

            var progress = 1f - (float)Projectile.timeLeft / PostChargeFrames;
            var progressHit = 0.4f;

            Projectile.rotation = -3f * MathF.PI / 4f * (1f - MathF.Pow(progress / progressHit, 2));
            if(progress >= progressHit) {
                if(charged && !_hitCheck) {
                    _hitCheck = true;

                    var hitCenter = Projectile.position
                        + RotationVector * 65f
                        + Owner.direction * RotationVector.RotatedBy(MathF.PI / 2f) * 35f;
                    var hitSize = 40;

                    var hitPosition = hitCenter - Vector2.One * hitSize / 2f;
                    if(Collision.SolidTiles(hitPosition, hitSize, hitSize)) {
                        SoundEngine.PlaySound(
                            Assets.Assets.Sounds.Cursehound.MaceSlam,
                            Projectile.position
                        );

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            hitCenter - Vector2.UnitY * 15,
                            Vector2.Zero,
                            ModContent.ProjectileType<MaceCrack>(),
                            0,
                            0
                        );

                        for(int i = 0; i < 5; i++) {
                            var dustPos = hitCenter + Main.rand.NextVector2Circular(120f, 20f)
                                + Vector2.UnitY * Projectile.height * 1.5f;
                            var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f)
                                + hitCenter.DirectionTo(dustPos) * 5f;

                            var dustColorStart = new Color(133, 122, 94);
                            var dustColorFade = dustColorStart * 0.4f;

                            var newDustData = new Smoke.Data()
                            {
                                InitialLifetime = 40,
                                ElapsedFrames = 0,
                                InitialOpacity = 0.8f,
                                ColorStart = dustColorStart,
                                ColorFade = dustColorFade,
                                Spin = 0.03f,
                                InitialScale = Main.rand.NextFloat(1f, 2f)
                            };

                            var newDust = Dust.NewDustPerfect(
                                dustPos,
                                ModContent.DustType<Smoke>(),
                                dustVelocity,
                                0,
                                newColor: Color.White,
                                newDustData.InitialScale
                            );

                            newDust.customData = newDustData;
                        }

                        for(int i = 0; i < 5; i++) {
                            Dust.NewDust(hitPosition, hitSize, hitSize, DustID.Stone);
                        }

                        MathUtilities.ForEachNPCInRange(hitCenter, 80f, npc =>
                        {
                            Owner.StrikeNPCDirect(npc, new()
                            {
                                Damage = Projectile.damage * 2,
                                Knockback = Projectile.knockBack * 5f,
                            });
                        });

                        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(13f, 0.6f));
                        _hit = true;
                    }
                }

                Projectile.rotation = _hit ? 0
                    : (1f - MathF.Pow(progress - (1f + progressHit), 2)) * MathF.PI / 2f;
            }
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.direction * (Projectile.rotation - MathHelper.PiOver2));
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        var spriteRotationVector = Projectile.rotation.ToRotationVector2() * new Vector2(Owner.direction, 1f);
        var trailLastPosition = spriteRotationVector * 65f;
        _trailPositions ??= [.. Enumerable.Repeat(trailLastPosition, 4)];

        for(var i = _trailPositions.Length - 1; i > 0; i--) {
            _trailPositions[i] = _trailPositions[i - 1]
                + new Vector2(MathF.Sin(i * 0.35f + Main.GameUpdateCount * 0.1f) * 3.2f, -4f);
        }
        _trailPositions[0] = trailLastPosition;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Owner.channel) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.position,
            Projectile.position + RotationVector * 80f,
            40,
            ref _
        );
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        var modifier = MathF.Pow(ChargeProgress, 2);
        modifiers.Knockback *= modifier;
        modifiers.SourceDamage *= modifier;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        var offset = -6;
        var rotation = (Projectile.rotation + MathF.PI / 4f) * Owner.direction;
        Vector2 origin = new(offset - 5f, texture.Height - offset);

        var trailColor = new Color(96, 91, 206) * _outlineAlpha * 0.4f;
        Renderer.BeginPipeline(0.5f)
            .DrawBasicTrail(
                _trailPositions.Select(p => p + Projectile.position).ToArray(),
                static t => (1.25f - t) * 20f,
                TextureAssets.MagicPixel.Value,
                trailColor
            )
            .DrawSprite(
                 texture,
                 Projectile.position - Main.screenPosition,
                 lightColor,
                 rotation: rotation,
                 origin: Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
                 scale: Vector2.One * Projectile.scale,
                 spriteEffects: Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            )
            .ApplyOutline(trailColor)
            .ApplyOutline(new Color(230, 255, 5) * _outlineAlpha)
            .Flush();

        var tintFlashFrames = 15;
        var tintColor = Color.Transparent;
        if(Owner.channel) {
            tintColor = Color.White
                * MathF.Max(1f - MathF.Pow(2f * MathF.Max(_charge - MaxCharge + tintFlashFrames / 2, 0) / tintFlashFrames - 1f, 2), 0f);
        }

        Renderer.BeginPipeline()
            .DrawSprite(
                texture,
                Projectile.position - Main.screenPosition,
                lightColor,
                rotation: (Projectile.rotation + MathF.PI / 4f) * Owner.direction,
                origin: Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
                scale: Vector2.One * Projectile.scale,
                spriteEffects: Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            )
            .ApplyTint(tintColor)
            .ApplyTint(tintColor)
            .Flush();

        return false;
    }
}
