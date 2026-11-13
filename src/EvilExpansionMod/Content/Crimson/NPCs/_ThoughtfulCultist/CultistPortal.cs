using Daybreak.Common.Rendering;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

internal enum PortalType {
    Spear,
    Blood,
    Eyes,
    Knives,
}

public class CultistPortal : ModProjectile {
    private const int AnimationSpeed = 4;
    public override string Texture => Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPortal.KEY;
    public int portalVariant = 0; // 0 = Spear, 1 = Eyes, 2 = Blood
    PortalType PortalType => (PortalType)Projectile.ai[0];
    bool _spawnedEye;

    public override void SetDefaults() {
        Projectile.width = 45;
        Projectile.height = 140;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 10;
        Projectile.hide = true;

        Projectile.aiStyle = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 999;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = (int)Projectile.ai[1];
        Projectile.netUpdate = true;
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.Bleeding, 60 * 10);
    }
    private float SoftenVolume = 1;
    public override void AI() {
        if(Projectile.timeLeft == (int)Projectile.ai[1]) {
            switch(PortalType) {
                case PortalType.Spear:
                    SoftenVolume = 0.6f;
                return;
            }
            SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistPortalOpenLayer1.Asset with
            {
                Pitch = Main.rand.NextFloatDirection() * 0.4f,
                Volume = 0.75f * SoftenVolume,
                MaxInstances = 50,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            }, Projectile.Center);

            SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistPortalOpenLayer2.Asset with
            {
                Pitch = Main.rand.NextFloatDirection() * 0.4f,
                Volume = 0.75f * SoftenVolume,
                MaxInstances = 50,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            }, Projectile.Center);
        }
        else if(Projectile.timeLeft == (int)Projectile.ai[1] - 16) {
            if(PortalType == PortalType.Spear) {
                SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistSpearThrustMaybe.Asset with
                {
                    Pitch = Main.rand.NextFloatDirection() * 0.2f,
                    Volume = 0.55f,
                    MaxInstances = 50,
                    SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
                }, Projectile.Center);
            }
        }

        var t = Projectile.timeLeft / Projectile.ai[1];

        Dust.NewDustPerfect(
            Projectile.Center + Projectile.velocity * 20f
            + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
            DustID.Blood,
            Velocity: Projectile.velocity * 6f,
            Main.rand.Next(0, 50),
            default,
            Main.rand.NextFloat(1f, 2f) * t
        );
        if(Main.rand.NextBool(2)) {
            Dust d = Dust.NewDustPerfect(
                Projectile.Center + Projectile.velocity * 10f
                + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                DustID.Torch,
                Velocity: Projectile.velocity * 6f,
                Main.rand.Next(0, 150),
                default,
                Main.rand.NextFloat(1f, 3f) * t
            );
            var spread = 2f;
            d.velocity = d.velocity + new Vector2(Main.rand.NextFloat(-spread * t, spread * t), Main.rand.NextFloat(-spread * t, spread * t));
            d.noGravity = true;
        }
        switch(PortalType) {
            case PortalType.Blood:
                portalVariant = 2;
                if(Main.rand.NextBool(2)) {
                    Dust.NewDustPerfect(
                        Projectile.Center + Projectile.velocity * 20f
                        + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                        DustID.Blood,
                        Velocity: Projectile.velocity * 5f,
                        Main.rand.Next(0, 50),
                        default,
                        Main.rand.NextFloat(1f, 2f)
                    );
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Projectile.velocity * 10f
                        + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                        DustID.Torch,
                        Velocity: Projectile.velocity * 10f * (0.5f + t),
                        Main.rand.Next(0, 255),
                        default,
                        Main.rand.NextFloat(1f, 2f) * (0.5f + t)
                    );
                    var spread = 1f;
                    d.velocity = d.velocity + new Vector2(Main.rand.NextFloat(-spread * t, spread * t), Main.rand.NextFloat(-spread * t, spread * t));
                    d.noGravity = true;
                }
                Dust.NewDustPerfect(
                    Projectile.Center + Projectile.velocity * 20f
                    + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                    DustID.BloodWater,
                    Velocity: Projectile.velocity * 10f,
                    Main.rand.Next(0, 50),
                    default,
                    Main.rand.NextFloat(1f, 2f) * (0.5f + t)
                );

                if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft % 20 == 0) {
                    var position = Projectile.Center + Projectile.velocity * 15f;
                    var velocity = Projectile.velocity.RotatedByRandom(MathF.PI / 4f) * Main.rand.NextFloat(5f, 10f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        position,
                        velocity,
                        ModContent.ProjectileType<PortalGore>(),
                        25, //50, 100, 150
                        4f
                    );

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistGoreFlingLowLayer.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 0.85f,
                        MaxInstances = 40,
                        SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                    }, Projectile.Center);

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistGoreFlingSquelch.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 1.2f,
                        MaxInstances = 40,
                        SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                    }, Projectile.Center);
                }
                break;

            case PortalType.Spear:
                portalVariant = 0;
                break;

            case PortalType.Eyes:
                portalVariant = 1;
                if(Main.rand.NextBool(4)) {
                    Dust.NewDustPerfect(
                        Projectile.Center + Projectile.velocity * 20f
                        + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                        DustID.Shadowflame,
                        Velocity: Projectile.velocity * Main.rand.NextFloat(1f, 6f),
                        Main.rand.Next(0, 50),
                        default,
                        Main.rand.NextFloat(0.75f, 2f) * t
                    );
                }
                break;

            case PortalType.Knives:
                portalVariant = 2;
                if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft % 20 == 0 && Projectile.timeLeft > 40) {
                    float _portalRotation = 0;
                    for(int i = 0; i < (int)Main.rand.Next(3, 5); i++) {
                        var position = Projectile.Center - Projectile.velocity * 8f;
                        var velocity = Projectile.velocity.RotatedBy(_portalRotation) * 6f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromAI(),
                            position,
                            velocity,
                            ModContent.ProjectileType<PortalKnives>(),
                            Projectile.damage,
                            4f
                        );
                        _portalRotation += MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f));
                    }

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistSpearThrustMaybe.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.2f + 1.0f,
                        Volume = 1.2f,
                        MaxInstances = 50,
                        SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
                    }, Projectile.Center);

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistGoreFlingSquelch.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 1.2f,
                        MaxInstances = 40,
                        SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                    }, Projectile.Center);
                }
                break;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var t = Projectile.timeLeft / Projectile.ai[1];

        if(PortalType != PortalType.Spear || t < 0.3f || t > 0.6f) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            Projectile.Center + Projectile.velocity * 190f,
            15,
            ref _
        );
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCs.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var portalTexture = TextureAssets.Projectile[Type].Value;
        var spearTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.PortalSpear.Asset.Value;
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;

        var maxTimeLeft = (int)Projectile.ai[1];

        var t = (float)Projectile.timeLeft / maxTimeLeft;
        var rotation = Projectile.velocity.ToRotation();

        var timer = (maxTimeLeft - Projectile.timeLeft) / AnimationSpeed;
        var frame = timer < 8 ?
            timer :
            (timer >= maxTimeLeft / AnimationSpeed - 8 ? 23 + timer - maxTimeLeft / AnimationSpeed : 8 + (timer - 8) % 5);

        var frameHeight = 86;
        var frameWidth = 48;
        var frameOffset = 0;
        frameOffset = frameWidth * portalVariant;
        var portalSource = new Rectangle(frameOffset, frame * frameHeight, frameWidth, frameHeight);

        Main.spriteBatch.Draw(
            portalTexture,
            Projectile.Center - Main.screenPosition,
            portalSource,
            Color.Lerp(lightColor, Color.White, 0.5f),
            rotation,
            portalSource.Size() / 2f,
            1f,
            rotation > MathHelper.Pi ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally,
            0f);

        switch(PortalType) {
            case PortalType.Blood:
                break;
            case PortalType.Spear:

                float spearX = 0f;

                if(t is < 0.6f and >= 0.57f) {
                    var progress = (0.6f - t) / 0.1f;
                    var x = progress - 1f;

                    spearX = x * x * x + 1f;
                }
                else if(t is < 0.57f and >= 0.56f) {
                    spearX = 2f;
                }
                else if(t is < 0.56f and >= 0.54f) {
                    spearX = 1.7f;
                }
                else if(t < 0.54f) {
                    var progress = MathF.Max(0f, (t - 0.2f) / 0.2f);
                    var x = progress - 1.7f;
                    spearX = -x * x + 1.7f;
                }

                Main.spriteBatch.Draw(
                    spearTexture,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, 0, (int)(spearX * spearTexture.Width), spearTexture.Height),
                    lightColor,
                    rotation,
                    Vector2.UnitY * 18,
                    1f,
                    SpriteEffects.FlipHorizontally,
                    0f
                );
                break;
            case PortalType.Eyes:
                break;
            case PortalType.Knives:
                break;
        }

        switch(PortalType) {
            case PortalType.Eyes:
                if(Main.netMode != NetmodeID.MultiplayerClient && !_spawnedEye && t < 0.6f) {
                    _spawnedEye = true;

                    var spawnPosition = Projectile.Center + Projectile.velocity * 20f;
                    var npc = NPC.NewNPCDirect(
                        Projectile.GetSource_FromAI(),
                        (int)spawnPosition.X,
                        (int)spawnPosition.Y,
                        ModContent.NPCType<CultistEye>()
                    );
                    npc.velocity = Projectile.velocity * 12f;

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistGoreFlingLowLayer.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 0.85f,
                        MaxInstances = 3,
                    }, Projectile.Center);

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistGoreFlingSquelch.Asset with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 1.5f,
                        MaxInstances = 3,
                    }, Projectile.Center);
                }
                break;
            case PortalType.Spear:
                break;
            case PortalType.Blood:
                break;
            case PortalType.Knives:
                break;
        }


        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });

        var scale = t < 0.1f ? t / 0.1f : (t > 0.9f ? MathF.Max(0f, (0.1f - (t - 0.9f)) / 0.1f) : 1f);
        switch(PortalType) {
            case PortalType.Spear:
                Main.spriteBatch.Draw(
                glowTexture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Red * 0.15f,
                rotation,
                glowTexture.Size() * 0.5f,
                0.55f * scale * new Vector2(1.25f, 2.2f),
                SpriteEffects.None,
                0
                );
                break;
            case PortalType.Eyes:
                Main.spriteBatch.Draw(
                glowTexture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Purple * 0.35f,
                rotation,
                glowTexture.Size() * 0.5f,
                0.55f * scale * new Vector2(1.25f, 2.2f),
                SpriteEffects.None,
                0
                );
                break;
            case PortalType.Blood:
                Main.spriteBatch.Draw(
                glowTexture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Red * 0.30f,
                rotation,
                glowTexture.Size() * 0.5f,
                0.55f * scale * new Vector2(1.25f, 2.2f),
                SpriteEffects.None,
                0
                );
                break;
            case PortalType.Knives:
                Main.spriteBatch.Draw(
                glowTexture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Red * 0.15f,
                rotation,
                glowTexture.Size() * 0.5f,
                0.55f * scale * new Vector2(1.25f, 2.2f),
                SpriteEffects.None,
                0
                );
                break;
        }
        Main.spriteBatch.Restart(ss);
        return false;
    }
}