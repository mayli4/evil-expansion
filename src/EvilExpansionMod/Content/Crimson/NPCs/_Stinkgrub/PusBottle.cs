using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class PusBottleNPC : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.Stinkgrub.PusBottle.KEY;

    public int ParentNPCID => (int)NPC.ai[0];
    public ref float IsDetached => ref NPC.ai[1];

    public ref float SquishTimer => ref NPC.localAI[0];

    private const int spew_interval = 60 * 2;
    private const int detached_lifetime = 60 * 10;

    private float _maxSquishTime = 30f;
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        NPCID.Sets.NeedsExpertScaling[Type] = true;
    }
    public override void SetDefaults() {
        NPC.width = 80;
        NPC.height = 80;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.damage = 0;
        NPC.lifeMax = 320;
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
        float difficultyScaler = Main.expertMode ? (Main.masterMode ? 2f : 1.5f) : 1f;
        var amount = Main.rand.Next(3, 6) * difficultyScaler;

        SquishTimer = _maxSquishTime;

        for(int i = 0; i < amount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed;

            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(20, 100),
                velocity * Main.rand.NextFloat(0.75f, 1.25f * difficultyScaler),
                ModContent.ProjectileType<PusGlob>(),
                (int)(ParentNPCID != -1 && Main.npc[ParentNPCID].active ? Main.npc[ParentNPCID].damage * 0.75f : 10),
                0.5f,
                Main.myPlayer
            );
        }
        for(int i = 0; i < Main.rand.NextFloat(2f, 4f); i++) {
            Dust.NewDustPerfect(
                NPC.Center - new Vector2(20, 100) + Main.rand.NextVector2Circular(20f, 20f),
                ModContent.DustType<PusGas>(),
                Vector2.Zero,
                100,
                new Color(98, 90, 40)
            );
            SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.7f, Pitch = Main.rand.NextFloat(0.0f, 0.2f) }, NPC.Center);
        }
    }

    public override void OnKill() {
        if(Main.netMode == NetmodeID.Server) return;

        int pusImpCount = Main.rand.Next(2, 4);
        for(int i = 0; i < pusImpCount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed;

            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(20, 100),
                velocity,
                ModContent.ProjectileType<PusGlob>(),
                (int)(ParentNPCID != -1 && Main.npc[ParentNPCID].active ? Main.npc[ParentNPCID].damage * 0.75f : 10),
                0.5f,
                0,
                0f,
                1f
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
        var texture = Assets.Images.Crimson.NPCs.Stinkgrub.PusBottle.Asset.Value;
        var textureBack = Assets.Images.Crimson.NPCs.Stinkgrub.PusBottle_Back.Asset.Value;
        var textureInside = Assets.Images.Crimson.NPCs.Stinkgrub.PusBottle_Inside.Asset.Value;

        var origin = new Vector2(texture.Width / 2, 94);

        float intensityFactor = Math.Clamp(SquishTimer / _maxSquishTime, 0f, 1f);
        float easedIntensity = MathF.Pow(intensityFactor, 0.5f);

        var shakeOffset = Main.rand.NextVector2Circular(1 * easedIntensity, 1 * easedIntensity);
        var drawPosition = NPC.Center - screenPos + shakeOffset;

        var spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        float additionalRotation = MathF.Sin(Main.GameUpdateCount * 0.8f) * 0.05f * easedIntensity;

        float finalRotation = NPC.rotation + additionalRotation;

        Vector2 finalScale = Vector2.One * NPC.scale;

        spriteBatch.Draw(
            textureBack,
            drawPosition,
            null,
            drawColor,
            finalRotation,
            origin,
            finalScale,
            spriteEffects,
            0f);

        if(!NPC.IsABestiaryIconDummy) {
            var fluidEffect = Assets.Shaders.Pixel.DevilOWarFluid.Asset.Value;
            Renderer.BeginPixelated(Graphics.WorldTransformMatrix)
                .SetEffectParams(
                    fluidEffect,
                    ("level", 0.3f),
                    ("smooth", 1.0f),
                    ("liquidColor", new Color(98, 90, 40).ToVector4()),
                    ("noisetex", Assets.Images.Sample.BubblyNoise.Asset.Value),
                    ("noisetex2", Assets.Images.Sample.SpottyNoise.Asset.Value),
                    ("uNoiseStrength", 1.0f),
                    ("uNoise1ScrollSpeedX", 0.09f),
                    ("uDarkenStrength", 0.3f),
                    ("uNoise2ScrollVector", new Vector2(0.1f, 0.1f)),
                    ("uNoise2Scale", 1.0f),
                    ("uTime", Main.GameUpdateCount * 0.05f))
                .DrawTexture(new()
                {
                    Texture = textureInside,
                    Position = NPC.Center + shakeOffset,
                    Color = drawColor,
                    Rotation = finalRotation,
                    Origin = origin,
                    Scale = finalScale,
                    SpriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Effect = fluidEffect,
                })
                .ApplyOutline(new Color(132, 122, 61))
                .End();
        }

        spriteBatch.Draw(
            texture,
            drawPosition,
            null,
            drawColor,
            finalRotation,
            origin,
            finalScale,
            spriteEffects,
            0f);

        return false;
    }
}