using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class PusBottleNPC : ModNPC {
    public override string Texture => Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_PusBottle;

    public int ParentNPCID => (int)NPC.ai[0];
    public ref float IsDetached => ref NPC.ai[1];

    public ref float SquishTimer => ref NPC.localAI[0];

    private const int spew_interval = 60 * 2;
    private const int detached_lifetime = 60 * 10;

    private float _maxSquishTime = 30f;

    public override void SetDefaults() {
        NPC.width = 80;
        NPC.height = 80;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.damage = 0;
        NPC.lifeMax = 400;
        NPC.knockBackResist = 0f;
        NPC.value = 0f;

        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit3;
        NPC.DeathSound = SoundID.Shatter;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        database.Entries.Remove(bestiaryEntry);
    }

    public override void AI() {
        NPC parentNPC = Main.npc[ParentNPCID];

        if(SquishTimer > 0)
            SquishTimer--;

        if(IsDetached == 0) {
            if(parentNPC.active && parentNPC.type == ModContent.NPCType<StinkgrubNPC>()) {
                StinkgrubNPC grub = (StinkgrubNPC)parentNPC.ModNPC;
                if(grub.IsPusCarrier) {
                    NPC.Center = parentNPC.Center + new Vector2(
                        parentNPC.spriteDirection,
                        -parentNPC.height / 2 * parentNPC.scale - 50
                    );
                    NPC.velocity = parentNPC.velocity;
                    NPC.gfxOffY = parentNPC.gfxOffY;

                    NPC.direction = parentNPC.direction;
                    NPC.spriteDirection = parentNPC.spriteDirection;

                    float tiltAngle = 0.2f;
                    NPC.rotation = (NPC.direction == 1) ? -tiltAngle : tiltAngle;

                    NPC.ai[2]++;
                    if(NPC.ai[2] >= spew_interval) {
                        FirePus();
                        NPC.ai[2] = 0;
                    }
                    NPC.timeLeft = 2;
                }
                else {
                    IsDetached = 1;
                    NPC.timeLeft = detached_lifetime;
                    NPC.netUpdate = true;
                }
            }
            else {
                IsDetached = 1;
                NPC.timeLeft = detached_lifetime;
                NPC.netUpdate = true;
            }
        }
        else {
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.velocity.Y += 0.2f;
            if(NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;

            NPC.velocity.X *= 0.98f;

            NPC.rotation += 0.1f;

            if(NPC.collideX || NPC.collideY) {
                NPC.StrikeInstantKill();
            }
        }
    }

    private void FirePus() {
        var amount = Main.rand.Next(2, 4);

        SquishTimer = _maxSquishTime;

        for(int i = 0; i < amount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed;

            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(20, 100),
                velocity,
                ModContent.ProjectileType<PusGlob>(),
                (int)(ParentNPCID != -1 && Main.npc[ParentNPCID].active ? Main.npc[ParentNPCID].damage * 0.75f : 10),
                0.5f,
                Main.myPlayer
            );

            Dust.NewDustPerfect(
                NPC.Center - new Vector2(20, 100),
                ModContent.DustType<PusGas>(),
                Vector2.Zero,
                100,
                new Color(98, 90, 40)
            );
        }

        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = Main.rand.NextFloat(0.5f, 0.8f) }, NPC.Center);
    }

    public override void OnKill() {
        if(Main.netMode == NetmodeID.Server) return;

        int pusImpCount = Main.rand.Next(2, 4);
        for(int i = 0; i < pusImpCount; i++) {
            NPC.NewNPC(
                NPC.GetSource_FromThis(),
                (int)NPC.Center.X + Main.rand.Next(-10, 10),
                (int)NPC.Center.Y + Main.rand.Next(-10, 10),
                ModContent.NPCType<PusImpNPC>()
            );
        }
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;

        var rotation = NPC.rotation;
        for(var i = 0; i < 3; i++) {
            var direction = rotation.ToRotationVector2();
            var gore = Gore.NewGoreDirect(
                NPC.GetSource_Death(),
                NPC.Center + direction * 30f,
                direction * Main.rand.NextFloat(3f, 5f),
                Mod.Find<ModGore>("BottleGore" + i).Type
            );
            gore.position -= new Vector2(gore.Width, gore.Height) / 2f;

            rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
        }

        for(int i = 0; i < 15; i++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Glass, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 0, default, 1.2f);
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ichor, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 0, default, 0.8f);
        }
        SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = Assets.Textures.NPCs.Crimson.Stinkgrub.PusBottle;
        var textureInside = Assets.Textures.NPCs.Crimson.Stinkgrub.PusBottle_Inside;

        var origin = new Vector2(texture.Width / 2, 94);

        float intensityFactor = Math.Clamp(SquishTimer / _maxSquishTime, 0f, 1f);
        float easedIntensity = MathF.Pow(intensityFactor, 0.5f);

        var shakeOffset = Main.rand.NextVector2Circular(1 * easedIntensity, 1 * easedIntensity);

        float additionalRotation = MathF.Sin(Main.GameUpdateCount * 0.8f) * 0.05f * easedIntensity;

        float finalRotation = NPC.rotation + additionalRotation;

        Vector2 finalScale = Vector2.One * NPC.scale;

        var fluidEffect = Assets.Effects.Pixel.DevilOWarFluid;

        if(!NPC.IsABestiaryIconDummy) {
            Graphics.BeginPipeline(0.5f)
                .EffectParams(
                    fluidEffect,
                    ("level", 0.3f),
                    ("smooth", 1.0f),
                    ("liquidColor", new Color(98, 90, 40).ToVector4()),
                    ("noisetex", Assets.Textures.Sample.BubblyNoise),
                    ("noisetex2", Assets.Textures.Sample.SpottyNoise),
                    ("uNoiseStrength", 1.0f),
                    ("uNoise1ScrollSpeedX", 0.09f),
                    ("uDarkenStrength", 0.3f),
                    ("uNoise2ScrollVector", new Vector2(0.1f, 0.1f)),
                    ("uNoise2Scale", 1.0f),
                    ("uTime", Main.GameUpdateCount * 0.05f))
                .DrawSprite(
                    textureInside,
                    NPC.Center - screenPos + shakeOffset,
                    drawColor,
                    null,
                    finalRotation,
                    origin,
                    finalScale,
                    NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    effect: fluidEffect
                )
                .ApplyOutline(new Color(132, 122, 61))
                .Flush();
        }

        spriteBatch.Draw(
            texture,
            NPC.Center - screenPos + shakeOffset,
            null,
            drawColor,
            finalRotation,
            origin,
            finalScale,
            NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        return false;
    }
}