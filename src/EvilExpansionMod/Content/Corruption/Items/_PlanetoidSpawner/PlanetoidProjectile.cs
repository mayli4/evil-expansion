using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Corruption.Items._PlanetoidSpawner;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Particles;
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

    private static int postDeathTimer = 0;

    private bool _canExplode;
    private float _shake;

    private float GROWTH_TIME = 60 * 7; // Time it takes for the planet to grow to maximum size

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
        _rot = Main.rand.NextFloat(-0.02f, 0.02f) * 1.5f;
        State = 0f;
        HasHitGround = 0f;
    }

    public override void AI() {
        //Main.NewText("Projectile.scale" + Projectile.scale, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("CurrentTextureIndex" + CurrentTextureIndex, Microsoft.Xna.Framework.Color.White);

        Player player = Main.player[Projectile.owner];

        var shake = 0.1f;
        var directionToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);

        if(State == 0f) { // Channeling state
            if(!player.channel || !player.active || player.dead) {
                State = 1f;
                Projectile.netUpdate = true;
                Projectile.tileCollide = true;

                Projectile.damage *= 3;

                Projectile.timeLeft = (int)( 60 * 10 * (GrowthTimer / GROWTH_TIME) ); // Not sure if this amounts to anything... smaller planetoids have a hard upper limit to how long they can roll for
                return;
            }
            else {
                Projectile.timeLeft = 2;

                Vector2 toMouse = (Main.MouseWorld - Projectile.Center);
                float distanceToMouse = toMouse.Length();

                if(distanceToMouse > Main.rand.NextFloat(1f, 3f)) Projectile.velocity += directionToMouse * 1.0f * (1 - 0.3f * Projectile.scale);
                Projectile.velocity *= 0.89f;


                //shake = MathHelper.Lerp(0f, 0f, Projectile.scale);
            }

            Projectile.rotation += _rot;

            GrowthTimer++;

            Projectile.scale = MathHelper.Clamp(GrowthTimer / GROWTH_TIME, 0f, 1f);

            float powerFactor = Projectile.scale;
            Projectile.damage = (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(50 * (powerFactor)); // Deals 0-50 damage
            Projectile.knockBack = player.GetTotalKnockback(DamageClass.Magic).ApplyTo(10f * (powerFactor)); // Applies 0-10f KB

            if(GrowthTimer >= GROWTH_TIME) {
                _canExplode = true;
                State = 2f;
                Projectile.netUpdate = true;
                PreExplosionDelayTimer = 0;
            }

            //if(postDeathTimer > 0) {
            //    postDeathTimer--;
            //    Projectile.Kill();
            //}
        }
        else if(State == 1f) { // Dropped state
            //postDeathTimer = 1;
            Projectile.velocity.Y += 0.35f;
            if(Projectile.velocity.Y > 30f * (1 - Projectile.scale / 4) ) Projectile.velocity.Y = 30f * (1 - Projectile.scale / 4);

            if(HasHitGround == 1f) {
                Projectile.velocity.X *= 0.98f;

                if(Math.Abs(Projectile.velocity.X) < 0.3f) {
                    Projectile.velocity.X = 0f;
                    Projectile.Kill();
                }
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;

            if(player.channel) Projectile.Kill();
        }
        else if(State == 2f) { // Pre-explosion state
            //postDeathTimer = 1;
            Projectile.rotation += _rot;

            Projectile.velocity += directionToMouse * 0.5f;
            Projectile.velocity *= 0.89f;

            Projectile.timeLeft = (int)(20f - PreExplosionDelayTimer + 5);
            shake = 6;
            PreExplosionDelayTimer++;

            if(PreExplosionDelayTimer >= 20f) {
                Projectile.Kill();
                //SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center); // Put all relevant on-explode vfx/ sfx here
                int soundNum = Main.rand.Next(1, 3);
                SoundStyle bigBoomSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/PlanetoidSpawner/Isaacexplosion_strong{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(0.3f, 0.5f),
                    Volume = Main.rand.NextFloat(0.3f, 0.5f),
                };
                SoundEngine.PlaySound(bigBoomSound, Projectile.position);
                SoundStyle groundHitSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/PlanetoidSpawner/Isaacexplosion_weak{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(-1.5f, -1f),
                    Volume = Main.rand.NextFloat(1f, 1f),
                };
                SoundEngine.PlaySound(groundHitSound, Projectile.position);
            }
        }

        if(State != 1f) {
            Vector2 randomOffset = Main.rand.NextVector2Circular(shake, shake);
            Projectile.Center += randomOffset;
        }

        if(State == 0f && Projectile.scale < 0.9) {
            if(Main.rand.NextBool((int)(1 + 7 * (1 - Projectile.scale)))) {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2(MathF.Cos(randomAngle), MathF.Sin(randomAngle)) * (float)(16f * 15 * (Projectile.scale + 0.1));
                Projectile.NewProjectile(
                    Main.player[Projectile.owner].GetSource_FromAI(),
                    Projectile.Center + offset,
                    new Vector2(0f, 0f),
                    ModContent.ProjectileType<PlanetoidDebris>(),
                    10,
                    0.5f,
                    Main.myPlayer,
                    ai0: Projectile.whoAmI,
                    ai1: 0f,
                    ai2: CurrentTextureIndex
                );
            }
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
        var player = Main.player[Projectile.owner];
        if(State == 1f) {
            if(oldVelocity.Y > 3) { // Whenever planetoid hits the ground
                int soundNum = Main.rand.Next(1, 3);
                SoundStyle groundHitSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/PlanetoidSpawner/Isaacexplosion_weak{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(1f, 1.2f) - (CurrentTextureIndex / 2),
                    Volume = Main.rand.NextFloat(0.25f, 0.3f) * (1 + CurrentTextureIndex) * Math.Max( 0, (oldVelocity.Y / 40)),
                };
                SoundEngine.PlaySound(groundHitSound, Projectile.position);

                Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(1 + CurrentTextureIndex * 3 * (oldVelocity.Y / 16), 0.85f));

                for(int i = 0; i < CurrentTextureIndex; i++) {
                    var randomDirection = Main.rand.NextVector2Unit();
                    var dustPos = Projectile.Bottom + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                    var newDustData = new Smoke.Data()
                    {
                        InitialLifetime = 40,
                        ElapsedFrames = 0,
                        InitialOpacity = 0.5f,
                        ColorStart = Color.Black,
                        ColorFade = new Color(69, 69, 113),
                        Spin = 0f,
                        InitialScale = Main.rand.NextFloat(0.4f, 1.2f)
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
            if(HasHitGround == 0f && oldVelocity.Y > 0) { // First time hitting the ground
                float initialRollSpeed = 5f;
                //player.direction was used below prior... not sure
                //Projectile.velocity.X = player.direction * initialRollSpeed * Math.Abs(oldVelocity.X / 2);
                Projectile.velocity.X = initialRollSpeed * oldVelocity.X / 2f;
                Projectile.velocity.Y = -oldVelocity.Y * (CurrentTextureIndex > 2 ? 0.4f : CurrentTextureIndex > 1? 0.5f: 0.6f);

                for(int i = 0; i < 8; i++) {
                    var randomDirection = Main.rand.NextVector2Unit();
                    var dustPos = Projectile.Bottom + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                    var newDustData = new Smoke.Data()
                    {
                        InitialLifetime = 40,
                        ElapsedFrames = 0,
                        InitialOpacity = 0.5f,
                        ColorStart = Color.Black,
                        ColorFade = new Color(69, 69, 113),
                        Spin = 0f,
                        InitialScale = Main.rand.NextFloat(0.4f, 1.2f)
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

            for(int i = 0; i < 20; i++) {
                var ember = GlowEmberParticle.NewParticle(Projectile.Center + Main.rand.NextVector2Circular(15, 15), Main.rand.NextVector2Circular(15, 15), Main.rand.NextFloat(0.5f, 2f), new Color(214, 237, 5) with { A = 0 }, Color.White with { A = 0 });
                ember.Randomness *= 2f;
                ember.LossPerSecond *= Main.rand.NextFloat(1f, 3f);
                ParticleEngine.PARTICLES.Add(ember);
            }

            var rotation = Main.rand.NextFloat();
            for(var i = 0; i < 7; i++) {
                var direction = rotation.ToRotationVector2();

                var gore = Mod.Find<ModGore>("PlanetoidGore" + i);
                var size = TextureAssets.Gore[gore.Type].Size();

                Gore.NewGorePerfect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * (Main.rand.NextFloat(0.5f) + 0.5f) * 42 - size / 2f,
                    direction * Main.rand.NextFloat(5f, 10f) * (CurrentTextureIndex > 2 ? 3f : CurrentTextureIndex > 1 ? 2f : 1f),
                    gore.Type
                );

                rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
            }

            for(var i = 0; i < 8 + CurrentTextureIndex; i++) {
                var additionalSize = 130;
                Dust.NewDust(Projectile.position - Vector2.One * additionalSize / 2f, Projectile.width + additionalSize, Projectile.height + additionalSize, DustID.Corruption);
            }
        }


        if(HasHitGround == 1f || State == 1f) {
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
            if (CurrentTextureIndex > 1) SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Pitch = -0.5f }, Projectile.Center);

            for(int i = 0; i < (CurrentTextureIndex > 1 ? 10 : 5); i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos = Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                var newDustData = new Smoke.Data() // On-death dust
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = Main.rand.NextFloat(0.3f, 0.7f),
                    ColorStart = Color.Black,
                    ColorFade = new Color(69, 69, 113),
                    Spin = 0f,
                    InitialScale = Main.rand.NextFloat(0.4f, 1.2f)
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

            var rotation = Main.rand.NextFloat(); // NOTE TO SELF: SCALE OF THE GORE SPRITES SHOULD CORRESPOND TO THE SCALE OF THE PLANETOID PROJ. WHEN SPAWNING
            for(var i = 1; i < goreCount + 1; i++) {
                var direction = rotation.ToRotationVector2();

                var gore = Mod.Find<ModGore>(gorePrefix + i);
                var size = TextureAssets.Gore[gore.Type].Size();

                Gore.NewGorePerfect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * (Main.rand.NextFloat(0.5f) + 0.5f) * maxRadius - size / 2f,
                    direction * Main.rand.NextFloat(2f, 3f),
                    gore.Type,
                    0.85f // Honestly... good enough? A placeholder used until I figure out how to grab finalDrawScale from Predraw, but honestly this works really well
                );

                rotation += MathF.PI * 2f / goreCount + Main.rand.NextFloatDirection() * 0.2f;
            }
        }
        if(CurrentTextureIndex > 1) {
            for(int i = 0; i < 30 * Projectile.scale; i++) {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2(MathF.Cos(randomAngle) * (float)(16f * 50) * (1 + Projectile.scale / 2), MathF.Sin(randomAngle)) * (float)(16f * 50);
                Projectile.NewProjectile(
                    Main.player[Projectile.owner].GetSource_FromAI(),
                    Projectile.Center,
                    offset * (2 + Projectile.scale),
                    ModContent.ProjectileType<PlanetoidDebris>(),
                    10,
                    0.5f,
                    Main.myPlayer,
                    ai0: Projectile.whoAmI,
                    ai1: 1f,
                    ai2: CurrentTextureIndex
                );
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D currentTexture = null;
        float drawProgress = 0f;
        float previousThreshold = 0f;
        int newTextureIndex = 0;

        for(int i = 0; i < _growthThresholds.Length; i++) {
            if(Projectile.scale <= _growthThresholds[i]) { // Determines when to switch sprites
                currentTexture = ModContent.Request<Texture2D>(_texturePaths[i]).Value;
                newTextureIndex = i;

                var currentStageRange = _growthThresholds[i] - previousThreshold;
                drawProgress = currentStageRange > 0 ? (Projectile.scale - previousThreshold) / currentStageRange : 0f;
                break;

                Main.NewText("currentStageRange" + currentStageRange, Microsoft.Xna.Framework.Color.White);
                Main.NewText("previousThreshold" + previousThreshold, Microsoft.Xna.Framework.Color.White);
                Main.NewText("drawProgress" + drawProgress, Microsoft.Xna.Framework.Color.White);

                // Making sense of this:

                // currentStageRange: current _growthThresholds (0.25, 0.5, 0.75, 1) MINUS previousThreshold (updated to reflect... the previous _growthThresholds...)
                // currentStageRange ranges from 0.25 (newly transitions) to approaching 0.5 (on the brink of the next stage)

                //  drawProgress, if currentStageRange is larger than 0 (AKA planetoid is at least threshold 2, because @ threshold 1, previousThreshold is 0),
                // drawProgress: ( Projectile.scale (0-1) - previousThreshold (0.25,0.5,0.75,1) ) / currentStageRange (0.25-0.5)

                // hypothetical 1: currentStageRange approaches 0.5 (on the brink), previousThreshold is 0 (stage 1), drawProgress = (0.5-0.25) / 0.25 = 1
                // hypothetical 2: currentStageRange is 0.25 (just transitioned), previousThreshold is 0.25 (stage 2), drawProgress = (0.25-0.25) / 0.25 = 0
            }
            previousThreshold = _growthThresholds[i];
        }

        if(currentTexture == null) { // Uhh
            currentTexture = ModContent.Request<Texture2D>(_texturePaths[_texturePaths.Length - 1]).Value;
            newTextureIndex = _texturePaths.Length - 1;
            drawProgress = 1f;
        }

        //this shouldnt be here but idc
        if(newTextureIndex != CurrentTextureIndex && GrowthTimer > 0) { // Dust made during each stage transition
            for(int i = 0; i < (CurrentTextureIndex > 1 ? 10 : 5); i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos =
                    Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 1f);

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.5f,
                    ColorStart = Color.Black,
                    ColorFade = new Color(69, 69, 113),
                    Spin = 0f,
                    InitialScale = Main.rand.NextFloat(0.5f, 1.2f)
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

            if(Projectile.owner == Main.myPlayer) { // Mana drain with stage transition
                Player player = Main.player[Projectile.owner];
                int manaCost = player.HeldItem.mana;
                if(player.statMana >= manaCost) {
                    player.statMana -= manaCost;
                    //player.ManaEffect(-manaCost);
                }
                else Projectile.Kill();
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = Main.rand.NextFloat(0.4f, 0.7f) }, Projectile.Center);
        }

        _shake *= 0.95f;

        float startingScale = 0.7f; // Ahh! Found you.
        float easedDrawProgress = MathF.Pow(drawProgress, 0.5f);
        float finalDrawScale = MathHelper.Lerp(startingScale, 1.0f, easedDrawProgress); // Use this float in gore drawing for consistent pixel sizes between the projectile and gore

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