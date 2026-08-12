using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class PlanetoidProjectile : ModProjectile {
    public override string Texture => Assets.Textures.Items.Corruption.Planetoids.SmallPlanetoid.KEY;

    private static readonly string[] _texturePaths = {
        Assets.Textures.Items.Corruption.Planetoids.SmallPlanetoid.KEY,
        Assets.Textures.Items.Corruption.Planetoids.MediumPlanetoid.KEY,
        Assets.Textures.Items.Corruption.Planetoids.BigPlanetoid.KEY,
        Assets.Textures.Items.Corruption.Planetoids.HugePlanetoid.KEY
    };

    private static readonly float[] _growthThresholds = {
        0.25f,
        0.50f,
        0.75f,
        1.00f
    };

    public ref float GrowthTimer => ref Projectile.ai[0];
    public ref float State => ref Projectile.ai[1];

    private ref float _currentTextureIndex => ref Projectile.localAI[0];
    private ref float _hasHitGround => ref Projectile.localAI[1];
    private ref float _preExplosionDelayTimer => ref Projectile.localAI[2];

    private bool _canExplode;

    private const float growth_time = 60 * 5;

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

    public override bool? CanCutTiles() => false;

    public override void OnSpawn(IEntitySource source) {
        GrowthTimer = 0;
        Projectile.scale = 0.0f;
        _currentTextureIndex = 0;
        _rot = Main.rand.NextFloat(-0.03f, 0.03f);
        State = 0f;
        _hasHitGround = 0f;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        var shake = 0.1f;

        if(State == 0f) {
            if(!player.channel || !player.active || player.dead) {
                State = 1f;
                Projectile.netUpdate = true;
                Projectile.tileCollide = true;

                Projectile.damage *= 10;

                Projectile.timeLeft = 3600;
                return;
            }
            else {
                Projectile.timeLeft = 2;

                Vector2 targetPos = Main.MouseWorld;
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.05f + 0.1f * Projectile.scale);

                //shake = MathHelper.Lerp(0f, 0f, Projectile.scale);
            }

            Projectile.rotation += _rot;

            GrowthTimer++;

            Projectile.scale = MathHelper.Clamp(GrowthTimer / growth_time, 0f, 1f);

            float powerFactor = Projectile.scale;
            Projectile.damage = (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(5 * powerFactor);
            Projectile.knockBack = player.GetTotalKnockback(DamageClass.Magic).ApplyTo(1f * powerFactor);

            if(GrowthTimer >= growth_time) {
                _canExplode = true;
                State = 2f;
                Projectile.netUpdate = true;
                _preExplosionDelayTimer = 0;
            }
        }
        else if(State == 1f) {
            Projectile.velocity.Y += 0.2f;
            if(Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            if(_hasHitGround == 1f) {
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
            Vector2 targetPos = Main.MouseWorld;
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.05f);

            Projectile.timeLeft = (int)(20f - _preExplosionDelayTimer + 5);
            shake = 6;
            _preExplosionDelayTimer++;

            if(_preExplosionDelayTimer >= 20f) {
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
            if(_hasHitGround == 0f && oldVelocity.Y > 0) {
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
                _hasHitGround = 1f;
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60 * 5);
            }
            else if(_hasHitGround == 1f) {
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
                (int)Main.player[Projectile.owner].GetTotalDamage(DamageClass.Magic).ApplyTo(90),
                new Color(136, 150, 37),
                Color.LightGoldenrodYellow,
                size: 500,
                timeLeft: 35
            );

            var rotation = Main.rand.NextFloat();
            for(var i = 0; i < 7; i++) {
                var direction = rotation.ToRotationVector2();
                Gore.NewGoreDirect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * 10f - new Vector2(8, 8),
                    direction * Main.rand.NextFloat(3f, 5f),
                    Mod.Find<ModGore>("PlanetoidGore" + i).Type
                );

                rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
            }

            for(var i = 0; i < 8; i++) {
                var additionalSize = 30;
                Dust.NewDust(Projectile.position - Vector2.One * additionalSize / 2f, Projectile.width + additionalSize, Projectile.height + additionalSize, DustID.Corruption);
            }
        }

        if(_hasHitGround == 1f) {
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
        if(newTextureIndex != _currentTextureIndex && GrowthTimer > 1) {
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
            _currentTextureIndex = newTextureIndex;

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Projectile.Center);
        }

        float startingScale = 0.7f;
        float easedDrawProgress = MathF.Pow(drawProgress, 0.5f);
        float finalDrawScale = MathHelper.Lerp(startingScale, 1.0f, easedDrawProgress);

        var width = (int)(currentTexture.Width * finalDrawScale);
        var height = (int)(currentTexture.Height * finalDrawScale);

        Projectile.Resize(Math.Max(1, width - 12), Math.Max(1, height - 12));

        Main.EntitySpriteDraw(
            currentTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            currentTexture.Size() / 2f,
            finalDrawScale,
            SpriteEffects.None
        );

        float crackProgress = _preExplosionDelayTimer / 20f;
        if(State == 2f && crackProgress > 0f) {
            float easedCrackProgress = MathF.Pow(crackProgress, 2f);
            var crackShader = Assets.Effects.Pixel.PlanetoidCracks.Asset.Value;

            Graphics.BeginPipeline(1.0f)
                .EffectParams(
                    crackShader,
                    ("sampleTexture2", Assets.Textures.Sample.CrackMap.Asset.Value),
                    ("sampleTexture3", Assets.Textures.Items.Corruption.Planetoids.HugePlanetoidCrackMappng.Asset.Value),
                    ("uTime", easedCrackProgress),
                    ("drawColor", Projectile.GetAlpha(lightColor).ToVector4()),
                    ("sourceFrame", new Vector4(0, 0, currentTexture.Width, currentTexture.Height)),
                    ("texSize", currentTexture.Size()))
                .SetBlendState(BlendState.NonPremultiplied)
                .DrawSprite(
                    currentTexture,
                    Projectile.Center - Main.screenPosition,
                    Projectile.GetAlpha(lightColor),
                    null,
                    Projectile.rotation,
                    currentTexture.Size() / 2f,
                    new Vector2(finalDrawScale, finalDrawScale),
                    effect: crackShader
                )
                .Flush();
        }

        return false;
    }
}