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
}

public class CultistPortal : ModProjectile {
    private const int AnimationSpeed = 4;
    public override string Texture => Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPortal.KEY;

    PortalType PortalType => (PortalType)Projectile.ai[0];
    bool _spawnedEye;
    bool _playedSpearSound;

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

    public override void AI() {
        var t = Projectile.timeLeft / Projectile.ai[1];
        switch(PortalType) {
            case PortalType.Blood:
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
                    SoundEngine.PlaySound(SoundID.Item117 with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 0.8f,
                    }, Projectile.Center);
                }

                if(Main.rand.NextBool(2)) {
                    Dust.NewDustPerfect(
                        Projectile.Center + Projectile.velocity * 20f
                        + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                        DustID.Blood,
                        Velocity: Projectile.velocity * 5f
                    );
                }

                if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft % 20 == 0) {
                    var position = Projectile.Center + Projectile.velocity * 15f;
                    var velocity = Projectile.velocity.RotatedByRandom(MathF.PI / 4f) * Main.rand.NextFloat(5f, 10f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        position,
                        velocity,
                        ModContent.ProjectileType<PortalGore>(),
                        20,
                        4f
                    );
                    SoundEngine.PlaySound(SoundID.Drown with
                    {
                        Pitch = Main.rand.NextFloatDirection() * 0.1f,
                        Volume = 0.8f,
                    }, Projectile.Center);
                }
                break;
            case PortalType.Spear:
                if(t < 0.6f && !_playedSpearSound) {
                    SoundEngine.PlaySound(SoundID.Item71 with {
                    Pitch = Main.rand.NextFloatDirection() * 0.1f,
                    Volume = 0.8f,
                    }, Projectile.Center);
                    _playedSpearSound = true;
                }
                break;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var t = Projectile.timeLeft / Projectile.ai[1];

        if(PortalType != PortalType.Spear || t < 0.3f || t > 0.6f ) return false;

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
        var portalSource = new Rectangle(0, frame * frameHeight, portalTexture.Width, frameHeight);

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
        }

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });

        var scale = t < 0.1f ? t / 0.1f : (t > 0.9f ? MathF.Max(0f, (0.1f - (t - 0.9f)) / 0.1f) : 1f);
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.Red * 0.2f,
            rotation,
            glowTexture.Size() * 0.5f,
            0.55f * scale * new Vector2(1.25f, 2.2f),
            SpriteEffects.None,
            0
        );

        Main.spriteBatch.Restart(ss);
        return false;
    }
}