using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class PlanetoidProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Planetoids.SmallPlanetoid.KEY;

    private static readonly string[] _texturePaths = {
        Assets.Images.Corruption.Items.Planetoids.SmallPlanetoid.KEY,
        Assets.Images.Corruption.Items.Planetoids.MediumPlanetoid.KEY,
        Assets.Images.Corruption.Items.Planetoids.BigPlanetoid.KEY,
        Assets.Images.Corruption.Items.Planetoids.HugePlanetoid.KEY
    };

    private static readonly float[] _growthThresholds = {
        0.25f,
        0.50f,
        0.75f,
        1.00f
    };

    public ref float GrowthTimer => ref Projectile.ai[0];
    public ref float State => ref Projectile.ai[1];

    private ref float CurrentTextureIndex => ref Projectile.localAI[0];
    private ref float HasHitGround => ref Projectile.localAI[1];
    private ref float PreExplosionDelayTimer => ref Projectile.localAI[2];

    private bool _canExplode;
    private float _shake;

    private const float GROWTH_TIME = 60 * 5;

    private float _rot;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override bool ShouldUpdatePosition() => true;

    public override bool? CanCutTiles() => true;

    public override void OnSpawn(IEntitySource source) {
        GrowthTimer = 0;
        Projectile.scale = 0.0f;
        CurrentTextureIndex = 0;
        _rot = Main.rand.NextFloat(-0.03f, 0.03f);
        State = 0f;
        HasHitGround = 0f;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        var shake = 0.1f;
        var directionToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);

        if(State == 0f) {
            if(!player.channel || !player.active || player.dead) {
                State = 1f;
                Projectile.netUpdate = true;
                Projectile.tileCollide = true;

                Projectile.damage *= 3;

                Projectile.timeLeft = 3600;
                return;
            }
            else {
                Projectile.timeLeft = 2;

                Projectile.velocity += directionToMouse * 0.8f;
                Projectile.velocity *= 0.89f;

                //shake = MathHelper.Lerp(0f, 0f, Projectile.scale);
            }

            Projectile.rotation += _rot;

            GrowthTimer++;

            Projectile.scale = MathHelper.Clamp(GrowthTimer / GROWTH_TIME, 0f, 1f);

            float powerFactor = Projectile.scale;
            Projectile.damage = (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(50 * powerFactor);
            Projectile.knockBack = player.GetTotalKnockback(DamageClass.Magic).ApplyTo(10f * powerFactor);

            if(GrowthTimer >= GROWTH_TIME) {
                _canExplode = true;
                State = 2f;
                Projectile.netUpdate = true;
                PreExplosionDelayTimer = 0;
            }
        }
        else if(State == 1f) {
            Projectile.velocity.Y += 0.2f;
            if(Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            if(HasHitGround == 1f) {
                Projectile.velocity.X *= 0.98f;

                if(Math.Abs(Projectile.velocity.X) < 0.3f) {
                    Projectile.velocity.X = 0f;
                    Projectile.Kill();
                }
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;
        }
        else if(State == 2f) {
            Projectile.rotation += _rot;

            Projectile.velocity += directionToMouse * 0.8f;
            Projectile.velocity *= 0.89f;

            Projectile.timeLeft = (int)(20f - PreExplosionDelayTimer + 5);
            shake = 6;
            PreExplosionDelayTimer++;

            if(PreExplosionDelayTimer >= 20f) {
                Projectile.Kill();
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            }
        }

        if(State != 1f) {
            Vector2 randomOffset = Main.rand.NextVector2Circular(shake, shake);
            Projectile.Center += randomOffset;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        var player = Main.player[Projectile.owner];

        if(State == 1f) {
            if(HasHitGround == 0f && oldVelocity.Y > 0) {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash, Projectile.Center);

                float initialRollSpeed = 8f;
                Projectile.velocity.X = player.direction * initialRollSpeed;
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;

                for(int i = 0; i < 8; i++) {
                    var randomDirection = Main.rand.NextVector2Unit();
                    var dustPos = Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                    var newDustData = new Smoke.Data()
                    {
                        InitialLifetime = 40,
                        ElapsedFrames = 0,
                        InitialOpacity = 0.5f,
                        ColorStart = Color.Black,
                        ColorFade = new Color(69, 69, 113),
                        Spin = 0f,
                        InitialScale = Main.rand.NextFloat(0.5f, 2f)
                    };

                    var newDust = Dust.NewDustPerfect(
                        dustPos,
                        ModContent.DustType<Smoke>(),
                        null,
                        0,
                        newColor: Color.White,
                        newDustData.InitialScale
                    );

                    newDust.customData = newDustData;

                    Dust.NewDustPerfect(dustPos, DustID.Corruption);
                    Dust.NewDustPerfect(dustPos, DustID.Dirt);
                }
                HasHitGround = 1f;
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60 * 5);
            }
            else if(HasHitGround == 1f) {
                if(Projectile.velocity.X != oldVelocity.X) {
                    Projectile.velocity.X = -oldVelocity.X * 0.7f;
                }
                if(Projectile.velocity.Y != oldVelocity.Y) {
                    if(oldVelocity.Y > 0) {
                        Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
                    }
                    else {
                        Projectile.velocity.Y = 0f;
                    }
                }
            }

            return false;
        }
        return true;
    }

    public override void OnKill(int timeLeft) {
        if(Main.netMode == NetmodeID.Server)
            return;

        if(_canExplode) {
            ExplosionProjectile.New(
                Projectile.GetSource_Death(),
                Projectile.Center,
                (int)Main.player[Projectile.owner].GetTotalDamage(DamageClass.Magic).ApplyTo(500),
                new Color(136, 150, 37),
                Color.LightGoldenrodYellow,
                size: 500,
                timeLeft: 35
            );

            var rotation = Main.rand.NextFloat();
            for(var i = 0; i < 7; i++) {
                var direction = rotation.ToRotationVector2();

                var gore = Mod.Find<ModGore>("PlanetoidGore" + i);
                var size = TextureAssets.Gore[gore.Type].Size();

                Gore.NewGorePerfect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * (Main.rand.NextFloat(0.5f) + 0.5f) * 42 - size / 2f,
                    direction * Main.rand.NextFloat(5f, 10f),
                    gore.Type
                );

                rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
            }

            for(var i = 0; i < 8; i++) {
                var additionalSize = 30;
                Dust.NewDust(Projectile.position - Vector2.One * additionalSize / 2f, Projectile.width + additionalSize, Projectile.height + additionalSize, DustID.Corruption);
            }
        }


        if(HasHitGround == 1f) {
            for(int i = 0; i < 8; i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos = Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.5f,
                    ColorStart = Color.Black,
                    ColorFade = new Color(69, 69, 113),
                    Spin = 0f,
                    InitialScale = Main.rand.NextFloat(0.5f, 2f)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    null,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;

                Dust.NewDustPerfect(dustPos, DustID.Corruption);
                Dust.NewDustPerfect(dustPos, DustID.Dirt);
            }

            var gorePrefix = CurrentTextureIndex switch
            {
                0 => "PlanetoidGoreSmall",
                1 => "PlanetoidGoreMedium",
                2 => "PlanetoidGoreBig",
                3 => "PlanetoidGore",
                _ => throw new ArgumentOutOfRangeException(CurrentTextureIndex.ToString()),
            };

            var maxRadius = CurrentTextureIndex switch
            {
                0 => 12,
                1 => 24,
                2 => 32,
                3 => 42,
                _ => throw new ArgumentOutOfRangeException(CurrentTextureIndex.ToString()),
            };

            var goreCount = CurrentTextureIndex switch
            {
                0 => 3,
                1 => 4,
                2 => 4,
                3 => 5,
                _ => throw new ArgumentOutOfRangeException(CurrentTextureIndex.ToString()),
            };

            var rotation = Main.rand.NextFloat();
            for(var i = 1; i < goreCount + 1; i++) {
                var direction = rotation.ToRotationVector2();

                var gore = Mod.Find<ModGore>(gorePrefix + i);
                var size = TextureAssets.Gore[gore.Type].Size();

                Gore.NewGorePerfect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * (Main.rand.NextFloat(0.5f) + 0.5f) * maxRadius - size / 2f,
                    direction * Main.rand.NextFloat(2f, 3f),
                    gore.Type
                );

                rotation += MathF.PI * 2f / goreCount + Main.rand.NextFloatDirection() * 0.2f;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D currentTexture = null;
        float drawProgress = 0f;
        float previousThreshold = 0f;
        int newTextureIndex = 0;

        for(int i = 0; i < _growthThresholds.Length; i++) {
            if(Projectile.scale <= _growthThresholds[i]) {
                currentTexture = ModContent.Request<Texture2D>(_texturePaths[i]).Value;
                newTextureIndex = i;

                var currentStageRange = _growthThresholds[i] - previousThreshold;
                drawProgress = currentStageRange > 0 ? (Projectile.scale - previousThreshold) / currentStageRange : 0f;
                break;
            }
            previousThreshold = _growthThresholds[i];
        }

        if(currentTexture == null) {
            currentTexture = ModContent.Request<Texture2D>(_texturePaths[_texturePaths.Length - 1]).Value;
            newTextureIndex = _texturePaths.Length - 1;
            drawProgress = 1f;
        }

        //this shouldnt be here but idc
        if(newTextureIndex != CurrentTextureIndex && GrowthTimer > 1) {
            for(int i = 0; i < 8; i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos =
                    Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.5f,
                    ColorStart = Color.Black,
                    ColorFade = new Color(69, 69, 113),
                    Spin = 0f,
                    InitialScale = Main.rand.NextFloat(0.5f, 2f)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    null,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;

                Dust.NewDustPerfect(dustPos, DustID.Corruption);
                Dust.NewDustPerfect(dustPos, DustID.Dirt);
            }

            CurrentTextureIndex = newTextureIndex;
            _shake = 1f;

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Projectile.Center);
        }

        _shake *= 0.95f;

        float startingScale = 0.7f;
        float easedDrawProgress = MathF.Pow(drawProgress, 0.5f);
        float finalDrawScale = MathHelper.Lerp(startingScale, 1.0f, easedDrawProgress);

        var width = (int)(currentTexture.Width * finalDrawScale);
        var height = (int)(currentTexture.Height * finalDrawScale);

        Projectile.Resize(Math.Max(1, width - 12), Math.Max(1, height - 12));
        var drawPosition = Projectile.Center + _shake * Main.rand.NextVector2Unit() * 2f;

        Main.EntitySpriteDraw(
            currentTexture,
            drawPosition - Main.screenPosition,
            null,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            currentTexture.Size() / 2f,
            finalDrawScale,
            SpriteEffects.None
        );

        float crackProgress = PreExplosionDelayTimer / 20f;
        if(State == 2f && crackProgress > 0f) {
            float easedCrackProgress = MathF.Pow(crackProgress, 2f);
            var crackShader = Assets.Shaders.Pixel.PlanetoidCracks.Asset.Value;

            Graphics.Begin(Graphics.WorldTransformMatrix)
                .SetEffectParams(
                    crackShader,
                    ("sampleTexture2", Assets.Images.Sample.CrackMap.Asset.Value),
                    ("sampleTexture3", Assets.Images.Corruption.Items.Planetoids.HugePlanetoidCrackMappng.Asset.Value),
                    ("uTime", easedCrackProgress),
                    ("drawColor", Projectile.GetAlpha(lightColor).ToVector4()),
                    ("sourceFrame", new Vector4(0, 0, currentTexture.Width, currentTexture.Height)),
                    ("texSize", currentTexture.Size()))
                .SetBlendState(BlendState.NonPremultiplied)
                .DrawTexture(new()
                {
                    Texture = currentTexture,
                    Position = drawPosition,
                    Color = Projectile.GetAlpha(lightColor),
                    Rotation = Projectile.rotation,
                    Origin = currentTexture.Size() / 2f,
                    Scale = new Vector2(finalDrawScale, finalDrawScale),
                    Effect = crackShader,
                })
                .End();
        }

        return false;
    }
}