using EvilExpansionMod.Common.Bestiary;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class DevilOWarNPC : ModNPC {
    public enum State {
        Idle,
        Charging,
        AttackCooldown,
    }

    public override string Texture => Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarHead.KEY;

    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private int _attackCooldownTimer;
    public int _stingerProjectileId = -1;

    public Player Target => Main.player[NPC.target];

    private const int follow_range = 16 * 30;
    public const int charging_radius = 26 * 10;
    private const int attack_cooldown_time = 60 * 1;
    public const int stinger_duration_max = 60 * 30;

    private const int tentacle_segment_count = 8;

    public Vector2 DrawScale = Vector2.One;
    public float Pulsation;

    private int _totalLifeDrained;

    private Vector2[] _stingerTrailPositions;
    private Vector2[][] _tentacleTrailPositions;
    private float[] _tentacleWaveDirections;

    public int TotalLifeDrained { get; set; }
    public const int MAX_DRAIN_FOR_LEVEL = 160;
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        NPCID.Sets.NeedsExpertScaling[Type] = true;
    }
    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 36;
        NPC.lifeMax = 780;
        NPC.value = 255;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit13;
        NPC.DeathSound = SoundID.NPCDeath64;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<DevilOWarBannerItem>();
    }

    public override void Load() {
        for(int j = 1; j <= 5; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/DevilOWarGore" + j);
    }

    public override void OnSpawn(IEntitySource source) {
        _tentacleTrailPositions = new Vector2[4][];
        for(int i = 0; i < _tentacleTrailPositions.Length; i++) {
            _tentacleTrailPositions[i] = new Vector2[tentacle_segment_count];
            for(int j = 0; j < tentacle_segment_count; j++) {
                _tentacleTrailPositions[i][j] = NPC.Center;
            }
        }
        _stingerTrailPositions = new Vector2[tentacle_segment_count];
        for(int i = 0; i < tentacle_segment_count; i++) {
            _stingerTrailPositions[i] = NPC.Center;
        }
        _tentacleWaveDirections = new float[_tentacleTrailPositions.Length];
        for(int i = 0; i < _tentacleWaveDirections.Length; i++) {
            _tentacleWaveDirections[i] = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) =>
        bestiaryEntry.AddInfo(this, "");

    public override float SpawnChance(NPCSpawnInfo spawnInfo) =>
        spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.2f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 3, 6));
    }

    public override void AI() {
        NPC.TargetClosest();
        if(Target != null && Target.active && !Target.dead) {
            if(Target.Center.X < NPC.Center.X) {
                NPC.direction = -1;
            }
            else {
                NPC.direction = 1;
            }
        }

        NPC.rotation = NPC.velocity.X * 0.1f;

        if(_stingerProjectileId != -1) {
            Projectile stingerProj = Main.projectile[_stingerProjectileId];
            if(!stingerProj.active || stingerProj.type != ModContent.ProjectileType<DevilOWarStingerProjectile>()) {
                _stingerProjectileId = -1;
            }
            else {
                if(stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile astinger && !astinger.IsRetracting) {
                    var activeStingerStart = NPC.Center;
                    _stingerTrailPositions[0] = activeStingerStart;
                    GenerateWavyTentaclePoints(
                        _stingerTrailPositions,
                        activeStingerStart,
                        stingerProj.Center,
                        tentacle_segment_count,
                        0.5f,
                        0.1f,
                        15f
                    );
                }
                else if(stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile retractingStinger && retractingStinger.IsRetracting) {
                    var retractingStingerStart = NPC.Center;
                    _stingerTrailPositions[0] = retractingStingerStart;
                    GenerateWavyTentaclePoints(
                        _stingerTrailPositions,
                        retractingStingerStart,
                        retractingStinger.Projectile.Center,
                        tentacle_segment_count,
                        0.5f,
                        0.1f,
                        15f
                    );
                }
            }

            if(stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile stinger && stinger.AttachedToPlayer) {
                Pulsation = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f;

                float minScale = 0.95f;
                float maxScale = 1.05f;

                float scaleX = MathHelper.Lerp(minScale, maxScale, Pulsation);
                float scaleY = MathHelper.Lerp(maxScale, minScale, Pulsation);

                DrawScale = new Vector2(scaleX, scaleY);
            }
            else {
                Pulsation = 0f;
                DrawScale = Vector2.One;
            }
        }
        else {
            Pulsation = 0f;
            DrawScale = Vector2.One;
        }

        switch(CurrentState) {
            case State.Idle:
                if(NPC.Center.Distance(Target!.Center) < follow_range) {
                    NPC.velocity += 0.05f * NPC.Center.DirectionTo(Target.Center);
                    if(NPC.velocity.Length() > 2f) {
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * 2f;
                    }
                }
                else {
                    NPC.velocity *= 0.98f;
                }

                float bobbingFrequency = 0.05f;
                float bobbingAmplitude = 0.02f;
                NPC.velocity.Y += MathF.Sin(Main.GameUpdateCount * bobbingFrequency + NPC.whoAmI * 0.2f) * bobbingAmplitude;

                if(_stingerProjectileId == -1 && NPC.Center.Distance(Target.Center) < charging_radius) {
                    FireStinger();
                    CurrentState = State.Charging;
                }
                break;

            case State.Charging:
                if(_stingerProjectileId != -1 && Main.projectile[_stingerProjectileId].active && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
                    if(!stinger.IsRetracting) {
                        NPC.velocity += 0.02f * NPC.Center.DirectionTo(Target!.Center);
                        if(NPC.velocity.Length() > 1.5f) {
                            NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.5f;
                        }

                        if(NPC.Center.Distance(Target.Center) >= charging_radius + 16 * 2) {
                            RetractStinger();
                        }
                    }
                    else {
                        CurrentState = State.AttackCooldown;
                        _attackCooldownTimer = attack_cooldown_time;
                    }
                }
                else {
                    CurrentState = State.AttackCooldown;
                    _attackCooldownTimer = attack_cooldown_time;
                }
                break;

            case State.AttackCooldown:
                NPC.velocity *= 0.95f;
                _attackCooldownTimer--;
                if(_attackCooldownTimer <= 0) {
                    CurrentState = State.Idle;
                }
                break;
        }
    }

    private void FireStinger() {
        if(_stingerProjectileId == -1) {
            var proj = Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                NPC.Center.DirectionTo(Target.Center) * 10f,
                ModContent.ProjectileType<DevilOWarStingerProjectile>(),
                NPC.damage,
                0,
                Main.myPlayer,
                Target.whoAmI,
                NPC.whoAmI
            );

            if(proj != -1 && Main.projectile[proj].active) {
                _stingerProjectileId = proj;
            }
            else {
                _stingerProjectileId = -1;
                CurrentState = State.AttackCooldown;
                _attackCooldownTimer = attack_cooldown_time;
            }
        }
    }

    public void RetractStinger() {
        if(_stingerProjectileId != -1 && Main.projectile[_stingerProjectileId].active && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
            if(!stinger.IsRetracting) {
                stinger.StartRetraction();
            }
        }
    }

    public override void OnKill() {
        if(_stingerProjectileId != -1 && Main.projectile[_stingerProjectileId].active && Main.projectile[_stingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
            stinger.StartRetraction();
            _stingerProjectileId = -1;
        }
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 1; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("DevilOWarGore" + i).Type);
        }

        for(int i = 0; i < 4; i++) {
            Dust.NewDustDirect(NPC.Center, 5, 5, ModContent.DustType<Gas>(), 0, 0, 1, new Color(61, 54, 138, 255));
        }

        if(_tentacleTrailPositions != null) {
            foreach(var tentaclePositions in _tentacleTrailPositions) {
                if(tentaclePositions != null && tentaclePositions.Length > 0) {
                    for(int i = 0; i < tentaclePositions.Length - 1; i += 2) {
                        var gorePosition = tentaclePositions[i];
                        var goreVelocity = Main.rand.NextVector2Circular(3, 3);
                        Gore.NewGoreDirect(NPC.GetSource_Death(), gorePosition, goreVelocity, Mod.Find<ModGore>("DevilOWarGore4").Type);
                    }

                    var tipPosition = tentaclePositions[tentaclePositions.Length - 1];
                    var tipVelocity = Main.rand.NextVector2Circular(2, 2);

                    Gore.NewGoreDirect(NPC.GetSource_Death(), tipPosition, tipVelocity, Mod.Find<ModGore>("DevilOWarGore5").Type);
                }
            }
        }
    }

    private void PopulateTrails(RenderPipeline pipeline, Vector2 bodyWorldPosition, Color drawColor) {
        float Equation(float x) {
            return 0.2f * MathF.Sin(x) + 0.8f * MathF.Cos(x + MathHelper.PiOver4);
        }

        var initialRelativePositions = new[] {
            new Vector2(-0.3f, 0.3f),
            new Vector2(0.3f, 0.2f),
            new Vector2(0.4f, 0.1f),
            new Vector2(-0.2f, 0.4f)
        };

        var tentacleTexture = Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarTentacle.Asset.Value;
        var defaultTrailEffect = Assets.Shaders.Trail.Default.Asset.Value;

        for(int i = 0; i < 4; i++) {
            var positions = _tentacleTrailPositions[i];
            var currentTentacleBase = bodyWorldPosition + initialRelativePositions[i] * 16f;

            positions[0] = currentTentacleBase;
            var moveDirection = initialRelativePositions[i].SafeNormalize(Vector2.Zero);

            var perpendicular = new Vector2(-moveDirection.Y, moveDirection.X);
            perpendicular = perpendicular.RotatedBy(_tentacleWaveDirections[i]);

            float phaseOffsetMainTentacles = NPC.whoAmI * 0.123f;

            for(int j = 1; j < tentacle_segment_count; j++) {
                float factor = j / (tentacle_segment_count - 1f);
                positions[j] = currentTentacleBase
                               + moveDirection
                               * MathHelper.Lerp(110, 130, MathF.Sin(Main.GameUpdateCount * (0.02f + i * 0.003f) + i * 0.6f + phaseOffsetMainTentacles))
                               * factor
                               + perpendicular
                               * Equation(Main.GameUpdateCount * (0.04f + i * 0.005f) + factor * 4f + factor + i * 0.4f + phaseOffsetMainTentacles * 0.5f)
                               * 20f;
            }

            pipeline.SetTexture(tentacleTexture);
            pipeline.DrawTrail(positions, static _ => 10, _ => drawColor);
        }

        if(_stingerProjectileId != -1) {
            var stingerProj = Main.projectile[_stingerProjectileId];
            if(stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile stinger) {
                var activeStingerStart = NPC.Center;
                if(!stinger.IsRetracting) {
                    GenerateWavyTentaclePoints(_stingerTrailPositions, activeStingerStart, stingerProj.Center, tentacle_segment_count, 0.5f, 0.1f, 15f, NPC.whoAmI * 0.234f);
                }
                else {
                    GenerateWavyTentaclePoints(_stingerTrailPositions, activeStingerStart, stinger.Projectile.Center, tentacle_segment_count, 0.5f, 0.1f, 15f, NPC.whoAmI * 0.234f);
                }

                var stingerColor = Color.Lerp(drawColor, Color.Yellow, 0.5f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.2f);

                pipeline.SetTexture(tentacleTexture);
                pipeline.DrawTrail(_stingerTrailPositions, static _ => 10, _ => stingerColor);
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var headTexture = Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarHead.Asset.Value;
        var insidesTexture = Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarInsides.Asset.Value;
        var headSpikesTexture = Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarHeadSpikes.Asset.Value;
        var headUnderTexture = Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarHead_Under.Asset.Value;

        var glowColor = Color.Lerp(drawColor, new Color(114, 109, 27, 200), Pulsation);

        var flipped = NPC.direction != -1;
        var effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Vector2 origin = new Vector2(headTexture.Width, headTexture.Height) / 2;
        origin.X = flipped ? headTexture.Width - origin.X : origin.X;
        float squishWaveFrequency = 0.08f;
        float squishMaxMagnitude = 0.106f;
        float squishActivationSpeed = 0.005f;

        float squishAmount =
            (MathF.Abs((float)Math.Sin(Main.GameUpdateCount * squishWaveFrequency + NPC.whoAmI * 0.345f)) * 0.5f + 0.5f)
            * squishMaxMagnitude
            * Math.Max(0, (float)Math.Sin(Main.GameUpdateCount * squishActivationSpeed + NPC.whoAmI * 0.123f));

        Vector2 finalDrawScale = DrawScale * new Vector2(1f - squishAmount, 1f + squishAmount);

        if(NPC.IsABestiaryIconDummy) {
            Main.spriteBatch.Draw(
                Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarBestiary.Asset.Value,
                NPC.Center - new Vector2(0, 10),
                null,
                Color.White,
                0,
                origin,
                1f,
                effects,
                0f
            );
            return false;
        }

        var pipeline = Renderer.BeginPipeline(1f, Graphics.WorldTransformMatrix);

        var offsetForTrails = flipped ? new Vector2(-5, 30) : new Vector2(5, 30);
        Vector2 bodyWorldPositionForTrails = NPC.Center + offsetForTrails;

        PopulateTrails(pipeline, bodyWorldPositionForTrails, drawColor);
        pipeline.End();

        var fluidEffect = Assets.Shaders.Pixel.DevilOWarFluid.Asset.Value;

        int clampedLifeDrained = Math.Clamp(TotalLifeDrained, 0, MAX_DRAIN_FOR_LEVEL);
        float mappedLevel = MathHelper.Lerp(0.07f, 0.5f, (float)clampedLifeDrained / MAX_DRAIN_FOR_LEVEL);

        Main.spriteBatch.Draw(insidesTexture, NPC.Center + new Vector2(0, 19) - screenPos, null, drawColor, NPC.rotation, insidesTexture.Size() / 2, 1f, effects, 0f);

        Renderer.BeginPipeline(0.5f)
            .SetEffectParams(
                fluidEffect,
                ("level", mappedLevel),
                ("smooth", 0.95f),
                ("liquidColor", CursedSpiritNPC.GhostColor1.ToVector4()),
                ("noisetex", Assets.Images.Sample.BubblyNoise.Asset.Value),
                ("noisetex2", Assets.Images.Sample.SpottyNoise.Asset.Value),
                ("uNoiseStrength", 3.0f),
                ("uNoise1ScrollSpeedX", 0.09f),
                ("uDarkenStrength", 0.3f),
                ("uNoise2ScrollVector", new Vector2(0.1f, 0.1f)),
                ("uNoise2Scale", 1.0f),
                ("uTime", Main.GameUpdateCount * 0.05f)
            )
            .DrawTexture(new()
            {
                Texture = headUnderTexture,
                Position = NPC.Center - new Vector2(0, 4) - screenPos,
                Color = Color.White,
                Rotation = NPC.rotation,
                Origin = origin,
                Scale = new Vector2(finalDrawScale.X - 0.05f, finalDrawScale.Y - 0.05f),
                SpriteEffects = effects,
                Effect = fluidEffect,
            })
            .End();

        Lighting.AddLight(NPC.position, CursedSpiritNPC.GhostColor1.ToVector3() * 0.4f);

        Main.spriteBatch.Draw(headTexture, NPC.Center - new Vector2(0, 4) - screenPos, null, glowColor * 0.8f, NPC.rotation, origin, finalDrawScale, effects, 0f);
        Main.spriteBatch.Draw(headSpikesTexture, NPC.Center - new Vector2(0, 4) - screenPos, null, glowColor * 0.8f, NPC.rotation, origin, finalDrawScale, effects, 0f);

        return false;
    }

    private void GenerateWavyTentaclePoints(
        Vector2[] pointsArray,
        Vector2 start,
        Vector2 end,
        int segments,
        float waveFrequency,
        float waveSpeed,
        float waveAmplitude,
        float phaseOffset = 0f
    ) {
        pointsArray[0] = start;

        var direction = Vector2.Zero;
        if(Vector2.DistanceSquared(start, end) > 0.001f) {
            direction = Vector2.Normalize(end - start);
        }
        else {
            direction = Vector2.UnitY;
        }

        var perpendicular = new Vector2(-direction.Y, direction.X);
        float instancePhaseOffset = Main.GameUpdateCount * waveSpeed + phaseOffset;

        for(int i = 1; i < segments; i++) {
            float t = (float)i / (segments - 1);
            var basePoint = Vector2.Lerp(start, end, t);

            float waveDisplacement =
                (float)Math.Sin(t * MathHelper.TwoPi * waveFrequency + instancePhaseOffset)
                * waveAmplitude
                * (1f - t);

            pointsArray[i] = basePoint + perpendicular * waveDisplacement;
        }

        pointsArray[segments - 1] = end;
    }
}