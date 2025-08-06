using EvilExpansionMod.Content.NPCs.Corruption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;
public class HeadPounderHeldProjectile : ModProjectile {
    Player Owner => Main.player[Projectile.owner];

    static float HitboxLength = 100f;
    static int PostChargeFrames = 15;
    static int MaxCharge = 30;
    int _charge;
    bool _hitCheck;
    bool _hit;
    float ChargeProgress => MathF.Min((float)_charge / MaxCharge, 1f);

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
            Projectile.rotation = -3f * MathF.PI / 4f;

            _charge += 1;
            if(_charge >= MaxCharge) {
                if(_charge == MaxCharge) OnChargedUp();
            }
        }
        else {
            var progress = 1f - (float)Projectile.timeLeft / PostChargeFrames;
            var progressHit = 0.4f;

            Projectile.rotation = -3f * MathF.PI / 4f * (1f - MathF.Pow(progress / progressHit, 2));
            if(progress >= progressHit) {
                if(!_hitCheck) {
                    _hitCheck = true;

                    Vector2 rotationVector = Projectile.rotation.ToRotationVector2() * new Vector2(Owner.direction, 1f);
                    var hitCenter = Projectile.position + rotationVector * 90f;
                    var hitSize = 15;

                    var hitPosition = hitCenter - Vector2.One * hitSize / 2f;
                    if(Collision.SolidTiles(hitPosition, hitSize, hitSize)) {
                        SoundEngine.PlaySound(
                            Assets.Assets.Sounds.Cursehound.MaceSlam,
                            Projectile.position
                        );

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            hitCenter - Vector2.UnitY * 45,
                            Vector2.Zero,
                            ModContent.ProjectileType<MaceCrack>(),
                            0,
                            0
                        );
                        _hit = true;
                    }
                }

                Projectile.rotation = _hit ? 0
                    : (1f - MathF.Pow(progress - (1f + progressHit), 2)) * MathF.PI / 2f;
            }
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.direction * (Projectile.rotation - MathHelper.PiOver2));
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Owner.channel) return false;

        Vector2 rotationVector = Projectile.rotation.ToRotationVector2() * new Vector2(Owner.direction, 1f);
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.position,
            Projectile.position + rotationVector * 80f
        );
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        var modifier = MathF.Pow(ChargeProgress, 2);
        modifiers.Knockback *= modifier;
        modifiers.SourceDamage *= modifier;
    }

    void OnChargedUp() {
        SoundEngine.PlaySound(SoundID.Tink);
        // SoundEngine.PlaySound(SoundID.Research);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var offset = -6;
        Vector2 origin = new(offset, texture.Height - offset);

        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            null,
            lightColor,
            (Projectile.rotation + MathF.PI / 4f) * Owner.direction,
            Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Projectile.scale,
            Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0
        );

        return false;
    }
}
