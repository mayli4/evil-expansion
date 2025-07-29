using EvilExpansionMod.Common.Bestiary;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

public sealed class DevilOWarNPC : ModNPC {
    public enum State {
        Idle,
        Charging,
        AttackCooldown,
    }

    public override string Texture =>
        Assets.Assets.Textures.NPCs.Corruption.DevilOWar.KEY_DevilOWarHead;

    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private int _attackCooldownTimer;
    public int _stingerProjectileId = -1;

    public Player Target => Main.player[NPC.target];

    private const int follow_range = 16 * 30;
    public const int charging_radius = 26 * 10;
    private const int attack_cooldown_time = 60 * 1; // 1 second
    public const int stinger_duration_max = 60 * 5; // 5 seconds

    private const int tentacle_segment_count = 8;

    public Vector2 DrawScale = Vector2.One;
    public float Pulsation;

    private int _totalLifeDrained;

    private Vector2[] _stingerTrailPositions;
    private Vector2[][] _tentacleTrailPositions;
    private float[] _tentacleWaveDirections;

    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 36;
        NPC.lifeMax = 320;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<DevilOWarBannerItem>();
    }

    public override void Load() {
        for(int j = 1; j <= 5; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Textures/Gores/DevilOWarGore" + j);
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

    private void PopulateTrails(Renderer.Pipeline pipeline, Vector2 bodyWorldPosition, Color drawColor) {
        float Equation(float x) {
            return 0.2f * MathF.Sin(x) + 0.8f * MathF.Cos(x + MathHelper.PiOver4);
        }

        var initialRelativePositions = new[] {
            new Vector2(-0.3f, 0.3f),
            new Vector2(0.3f, 0.2f),
            new Vector2(0.4f, 0.1f),
            new Vector2(-0.2f, 0.4f)
        };

        var tentacleTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarTentacle.Value;
        var defaultTrailEffect = Assets.Assets.Effects.Compiled.Trail.Default.Value;

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

            pipeline.DrawTrail(
                positions,
                static _ => 10,
                _ => drawColor,
                defaultTrailEffect,
                ("sampleTexture", tentacleTexture),
                ("color", drawColor.ToVector4()),
                ("transformationMatrix", MathUtilities.WorldTransformationMatrix)
            );
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

                pipeline.DrawTrail(
                    _stingerTrailPositions,
                    static _ => 10,
                    _ => stingerColor,
                    defaultTrailEffect,
                    ("sampleTexture", tentacleTexture),
                    ("color", stingerColor.ToVector4()),
                    ("transformationMatrix", MathUtilities.WorldTransformationMatrix)
                );
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var headTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarHead.Value;
        var insidesTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarInsides.Value;
        var headSpikesTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarHeadSpikes.Value;
        var headUnderTexture = Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarHead_Under.Value;

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
                Assets.Assets.Textures.NPCs.Corruption.DevilOWar.DevilOWarBestiary.Value,
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
        var pipeline = new Renderer.Pipeline();

        var offsetForTrails = flipped ? new Vector2(-5, 30) : new Vector2(5, 30);
        Vector2 bodyWorldPositionForTrails = NPC.Center + offsetForTrails;

        PopulateTrails(pipeline, bodyWorldPositionForTrails, drawColor);
        pipeline.Flush();

        var fluidEffect = Assets.Assets.Effects.Compiled.Pixel.DevilOWarFluid.Value;

        fluidEffect.Parameters["liquidColor"].SetValue(CursedSpiritNPC.GhostColor1.ToVector4());
        fluidEffect.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.05f);
        fluidEffect.Parameters["level"].SetValue(0.3f);
        fluidEffect.Parameters["noisetex"].SetValue(Assets.Assets.Textures.Sample.BubblyNoise.Value);
        fluidEffect.Parameters["noisetex2"].SetValue(Assets.Assets.Textures.Sample.SpottyNoise.Value);
        fluidEffect.Parameters["uNoiseStrength"].SetValue(3.0f);
        fluidEffect.Parameters["uNoise1ScrollSpeedX"].SetValue(0.09f);
        fluidEffect.Parameters["uDarkenStrength"].SetValue(0.5f);
        fluidEffect.Parameters["uNoise2ScrollVector"].SetValue(new Vector2(0.1f, 0.1f));
        fluidEffect.Parameters["uNoise2Scale"].SetValue(1.0f);

        Main.spriteBatch.Draw(insidesTexture, NPC.Center + new Vector2(0, 19) - screenPos, null, drawColor, NPC.rotation, insidesTexture.Size() / 2, 1f, effects, 0f);
        var snapshot = Main.spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with { CustomEffect = fluidEffect});
        Main.spriteBatch.Draw(headUnderTexture, NPC.Center - new Vector2(0, 4) - screenPos, null, glowColor * 0.8f, NPC.rotation, origin, finalDrawScale, effects, 0f);
        Main.spriteBatch.EndBegin(snapshot);

        Main.spriteBatch.Draw(headTexture, NPC.Center - new Vector2(0, 4) - screenPos, null, glowColor * 0.8f, NPC.rotation, origin, finalDrawScale, effects, 0f );
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