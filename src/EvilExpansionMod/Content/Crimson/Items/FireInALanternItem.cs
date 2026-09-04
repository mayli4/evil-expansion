using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class FireInALanternItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.FlameInALanternItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToAccessory(20, 26);
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetJumpState<FireJump>().Enable();
    }

    public class FireJump : ExtraJump {
        public override Position GetDefaultPosition() => new After(CloudInABottle);

        public override float GetDurationMultiplier(Player player) {
            return 2f;
        }

        public override void UpdateHorizontalSpeeds(Player player) {
            player.runAcceleration *= 1.75f;
            player.maxRunSpeed *= 2f;
        }

        public override void OnStarted(Player player, ref bool playSound) {
            int offsetY = player.height;
            if(player.gravDir == -1f)
                offsetY = 0;

            offsetY -= 16;

            SpawnSmoke(player, player.Top + new Vector2(-16f, offsetY));
            SpawnSmoke(player, player.position + new Vector2(-36f, offsetY));
            SpawnSmoke(player, player.TopRight + new Vector2(4f, offsetY));
        }

        private static void SpawnSmoke(Player player, Vector2 position) {
            var newDustData = new Smoke.Data()
            {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.5f,
                ColorStart = Color.Black,
                ColorFade = new Color(69, 69, 113),
                Spin = 0f,
                InitialScale = 1
            };

            var newDust = Dust.NewDustPerfect(
                position,
                ModContent.DustType<Smoke>(),
                player.velocity,
                0,
                newColor: Color.White,
                newDustData.InitialScale
            );

            newDust.customData = newDustData;

            for(int i = 0; i < 5; i++) {
                Dust.NewDust(
                    position,
                    20,
                    20,
                    DustID.Firefly,
                    player.velocity.X / 2,
                    player.velocity.Y / 2,
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );

                Dust.NewDust(
                    position,
                    20,
                    20,
                    DustID.Torch,
                    player.velocity.X / 2,
                    player.velocity.Y / 2,
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
            }

            var flameSpawnPosition = player.Bottom + new Vector2(Main.rand.NextFloat(-player.width / 4f, player.width / 4f), 0f);

            var initialFlameVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 0.5f));

            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                flameSpawnPosition,
                initialFlameVelocity,
                ModContent.ProjectileType<FireInALanternFlame>(),
                20,
                0f,
                player.whoAmI
            );
        }

        public override void ShowVisuals(Player player) {
            int offsetY = player.height - 6;
            if(player.gravDir == -1f)
                offsetY = 6;

            Vector2 spawnPos = new Vector2(player.position.X, player.position.Y + offsetY);

            for(int i = 0; i < 1; i++) {
                if(Main.rand.NextBool(5)) {
                    SpawnBlizzardDust(player, spawnPos, 0.1f, i == 0 ? -0.07f : -0.13f);
                }
            }
        }

        private static void SpawnBlizzardDust(Player player, Vector2 spawnPos, float dustVelocityMultiplier, float playerVelocityMultiplier) {
            var newDustData = new Smoke.Data()
            {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.5f,
                ColorStart = Color.Black,
                ColorFade = new Color(69, 69, 113),
                Spin = 0f,
                InitialScale = 1
            };

            Dust dust = Dust.NewDustDirect(spawnPos, player.width, 12, ModContent.DustType<Smoke>(), player.velocity.X * 0.3f, player.velocity.Y * 0.3f, newColor: Color.Gray);
            dust.customData = newDustData;
            dust.fadeIn = 1.5f;
            dust.velocity *= dustVelocityMultiplier;
            dust.velocity += player.velocity * playerVelocityMultiplier;
            dust.noGravity = true;
            dust.noLight = true;
        }
    }
}

public class FireInALanternFlame : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;

    private const int lifetime = 60 * 3;
    private const float radius = 30f;

    public override void SetDefaults() {
        Projectile.width = (int)(radius * 2);
        Projectile.height = (int)(radius * 2);
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = lifetime;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.alpha = 255;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        if(Projectile.alpha > 0) {
            Projectile.alpha -= 30;
            if(Projectile.alpha < 0) Projectile.alpha = 0;
        }

        if(Projectile.timeLeft < 60) {
            Projectile.alpha += 4;
        }

        Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3());
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire, 60);
    }

    public override bool PreDraw(ref Color lightColor) {
        var flameShader = Assets.Shaders.Pixel.LingeringFlame.Asset.Value;
        var noiseTexture1 = Assets.Images.Sample.Pebbles.Asset.Value;
        var circleTexture = Assets.Images.Misc.Circle.Asset.Value;
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;

        float flameScaleFactor = 1f;
        if(Projectile.timeLeft < lifetime / 2) {
            flameScaleFactor = Projectile.timeLeft / (lifetime / 2f);
        }
        flameScaleFactor = MathHelper.Clamp(flameScaleFactor, 0.1f, 1f);
        float currentFlameSize = 1.2f * flameScaleFactor;

        float alphaFactor = (1f - Projectile.alpha / 255f);
        currentFlameSize *= alphaFactor;
        currentFlameSize = Math.Max(currentFlameSize, 0.0f);

        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetEffectParams(
                flameShader,
                ("time", 0.01f * Main.GameUpdateCount + Projectile.whoAmI + 10),
                ("size", new Vector2(1, 1)),
                ("flameColor", Color.Black.ToVector4() * 0.5f),
                ("coreColor", Color.Black.ToVector4() * 0.5f),
                ("outerCoreColor", Color.Black.ToVector4() * 0.5f),
                ("noiseScale", 0.5f),
                ("flameSize", currentFlameSize),
                ("tex1", noiseTexture1)
            )
            .SetBlendState(BlendState.Additive)
            .DrawTexture(new()
            {
                Texture = circleTexture,
                Position = Projectile.position - new Vector2(10, 20),
                Size = new(70, 70),
                Color = Projectile.GetAlpha(lightColor),
                Rotation = Projectile.rotation,
                Effect = flameShader
            })
            .End();

        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetEffectParams(
                flameShader,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI + 10),
                ("size", new Vector2(1, 1)),
                ("flameColor", new Color(255, 106, 0).ToVector4()),
                ("coreColor", new Color(234, 255, 0).ToVector4()),
                ("outerCoreColor", new Color(255, 150, 0).ToVector4()),
                ("noiseScale", 0.5f),
                ("flameSize", currentFlameSize),
                ("tex1", noiseTexture1)
            )
            .SetBlendState(BlendState.Additive)
            .DrawTexture(new()
            {
                Texture = circleTexture,
                Position = Projectile.position,
                Size = new(50, 50),
                Color = Projectile.GetAlpha(lightColor),
                Rotation = Projectile.rotation,
                Effect = flameShader
            })
            .ApplyOutline(new Color(255, 150, 0))
            .End();

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - new Vector2(),
            null,
            new Color(255, 106, 0) * 0.25f * (1f - Projectile.alpha / 255f),
            0f,
            glowTexture.Size() / 2f,
            currentFlameSize / 2,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);

        return false;
    }
}