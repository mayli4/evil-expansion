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

    private int attackCooldownTimer;
    public int StingerProjectileId = -1;

    public Player Target => Main.player[NPC.target];

    private const int FOLLOW_RANGE = 16 * 64;
    public const int CHARGING_RADIUS = 26 * 10;
    private const int ATTACK_COOLDOWN = 60 * 1;
    public const int STINGER_DURATION_MAX = 60 * 30;
    public const int MAX_DRAIN_FOR_LEVEL = 160;
    private const int TENATCLE_SEGMENT_COUNT = 8;

    public Vector2 DrawScale = Vector2.One;
    public float Pulsation;

    private int totalLifeDrained;

    private Vector2[] stingerTrailPositions;
    private Vector2[][] tentacleTrailPositions;
    private float[] tentacleWaveDirections;

    public int TotalLifeDrained { get; set; }
    
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
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
        if(Main.expertMode) {
            NPC.knockBackResist = 0f;
        }
    }
    public override void Load() {
        for(int j = 1; j <= 5; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/DevilOWarGore" + j);
    }

    public override void OnSpawn(IEntitySource source) {
        tentacleTrailPositions = new Vector2[4][];
        for(int i = 0; i < tentacleTrailPositions.Length; i++) {
            tentacleTrailPositions[i] = new Vector2[TENATCLE_SEGMENT_COUNT];
            for(int j = 0; j < TENATCLE_SEGMENT_COUNT; j++) {
                tentacleTrailPositions[i][j] = NPC.Center;
            }
        }
        stingerTrailPositions = new Vector2[TENATCLE_SEGMENT_COUNT];
        for(int i = 0; i < TENATCLE_SEGMENT_COUNT; i++) {
            stingerTrailPositions[i] = NPC.Center;
        }
        tentacleWaveDirections = new float[tentacleTrailPositions.Length];
        for(int i = 0; i < tentacleWaveDirections.Length; i++) {
            tentacleWaveDirections[i] = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }
    
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.DevilOWarNPCBestiary.KEY),
        });
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) =>
        spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.2f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 3, 6));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<InflatableDevilOWarItem>(), 50, 1, 1));
    }

    public override void AI() {
        NPC.TargetClosest();
        if(Target is { active: true, dead: false }) {
            if(Target.Center.X < NPC.Center.X) {
                NPC.direction = -1;
            }
            else {
                NPC.direction = 1;
            }
        }

        NPC.rotation = NPC.velocity.X * 0.05f;

        if(StingerProjectileId != -1) {
            Projectile stingerProj = Main.projectile[StingerProjectileId];
            if(!stingerProj.active || stingerProj.type != ModContent.ProjectileType<DevilOWarStingerProjectile>()) {
                StingerProjectileId = -1;
            }
            else {
                if(stingerProj is { active: true, ModProjectile: DevilOWarStingerProjectile { IsRetracting: false } }) {
                    var activeStingerStart = NPC.Center;
                    stingerTrailPositions[0] = activeStingerStart;
                    GenerateWavyTentaclePoints(
                        stingerTrailPositions,
                        activeStingerStart,
                        stingerProj.Center,
                        TENATCLE_SEGMENT_COUNT,
                        0.5f,
                        0.1f,
                        15f
                    );
                }
                else if(stingerProj is { active: true, ModProjectile: DevilOWarStingerProjectile { IsRetracting: true } retractingStinger }) {
                    var retractingStingerStart = NPC.Center;
                    stingerTrailPositions[0] = retractingStingerStart;
                    GenerateWavyTentaclePoints(
                        stingerTrailPositions,
                        retractingStingerStart,
                        retractingStinger.Projectile.Center,
                        TENATCLE_SEGMENT_COUNT,
                        0.5f,
                        0.1f,
                        15f
                    );
                }
            }

            if(stingerProj is { active: true, ModProjectile: DevilOWarStingerProjectile { AttachedToPlayer: true } }) {
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
        float difficultyScaler = Main.expertMode ? 2f : 1f; //aggro range, move speed multiplied on Expert and higher
        switch(CurrentState) {
            case State.Idle:
                if(NPC.Center.Distance(Target!.Center) < FOLLOW_RANGE * difficultyScaler) {
                    NPC.velocity += 0.05f * NPC.Center.DirectionTo(Target.Center) * difficultyScaler;
                    if(NPC.velocity.Length() > 2f * difficultyScaler) {
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * 2f * difficultyScaler;
                    }
                }
                else {
                    NPC.velocity *= 0.98f;
                }

                float bobbingFrequency = 0.05f;
                float bobbingAmplitude = 0.02f;
                NPC.velocity.Y += MathF.Sin(Main.GameUpdateCount * bobbingFrequency + NPC.whoAmI * 0.2f) * bobbingAmplitude;

                if(StingerProjectileId == -1 && NPC.Center.Distance(Target.Center) < CHARGING_RADIUS) {
                    FireStinger();
                    CurrentState = State.Charging;
                }
                break;

            case State.Charging:
                if(StingerProjectileId != -1 && Main.projectile[StingerProjectileId].active && Main.projectile[StingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
                    if(!stinger.IsRetracting) {
                        NPC.velocity += 0.1f * NPC.Center.DirectionTo(Target!.Center);
                        if(NPC.velocity.Length() > 1.5f) {
                            NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.5f * difficultyScaler;
                        }

                        if(NPC.Center.Distance(Target.Center) >= CHARGING_RADIUS + 16 * 2) {
                            RetractStinger();
                        }
                    }
                    else {
                        CurrentState = State.AttackCooldown;
                        attackCooldownTimer = ATTACK_COOLDOWN;
                    }
                }
                else {
                    CurrentState = State.AttackCooldown;
                    attackCooldownTimer = ATTACK_COOLDOWN;
                }
                break;

            case State.AttackCooldown:
                NPC.velocity *= 0.90f;
                attackCooldownTimer--;
                if(attackCooldownTimer <= 0) {
                    CurrentState = State.Idle;
                }
                break;
        }
    }

    private void FireStinger() {
        if(StingerProjectileId == -1) {
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
                StingerProjectileId = proj;
            }
            else {
                StingerProjectileId = -1;
                CurrentState = State.AttackCooldown;
                attackCooldownTimer = ATTACK_COOLDOWN;
            }
        }
    }

    public void RetractStinger() {
        if(StingerProjectileId != -1 && Main.projectile[StingerProjectileId].active && Main.projectile[StingerProjectileId].ModProjectile is DevilOWarStingerProjectile { IsRetracting: false } stinger) {
            stinger.StartRetraction();
        }
    }

    public override void OnKill() {
        if(StingerProjectileId != -1 && Main.projectile[StingerProjectileId].active && Main.projectile[StingerProjectileId].ModProjectile is DevilOWarStingerProjectile stinger) {
            stinger.StartRetraction();
            StingerProjectileId = -1;
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

        if(tentacleTrailPositions != null) {
            foreach(var tentaclePositions in tentacleTrailPositions) {
                if(tentaclePositions is { Length: > 0 }) {
                    for(int i = 0; i < tentaclePositions.Length - 1; i += 2) {
                        var gorePosition = tentaclePositions[i];
                        var goreVelocity = Main.rand.NextVector2Circular(3, 3);
                        Gore.NewGoreDirect(NPC.GetSource_Death(), gorePosition, goreVelocity, Mod.Find<ModGore>("DevilOWarGore4").Type);
                    }

                    var tipPosition = tentaclePositions[^1];
                    var tipVelocity = Main.rand.NextVector2Circular(2, 2);

                    Gore.NewGoreDirect(NPC.GetSource_Death(), tipPosition, tipVelocity, Mod.Find<ModGore>("DevilOWarGore5").Type);
                }
            }
        }
    }

    private void PopulateTrails(RenderPipeline pipeline, Vector2 bodyWorldPosition, Color drawColor) {
        float equation(float x) {
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
            var positions = tentacleTrailPositions[i];
            var currentTentacleBase = bodyWorldPosition + initialRelativePositions[i] * 16f;

            positions[0] = currentTentacleBase;
            var moveDirection = initialRelativePositions[i].SafeNormalize(Vector2.Zero);

            var perpendicular = new Vector2(-moveDirection.Y, moveDirection.X);
            perpendicular = perpendicular.RotatedBy(tentacleWaveDirections[i]);

            float phaseOffsetMainTentacles = NPC.whoAmI * 0.123f;

            for(int j = 1; j < TENATCLE_SEGMENT_COUNT; j++) {
                float factor = j / (TENATCLE_SEGMENT_COUNT - 1f);
                positions[j] = currentTentacleBase
                               + moveDirection
                               * MathHelper.Lerp(110, 130, MathF.Sin(Main.GameUpdateCount * (0.02f + i * 0.003f) + i * 0.6f + phaseOffsetMainTentacles))
                               * factor
                               + perpendicular
                               * equation(Main.GameUpdateCount * (0.04f + i * 0.005f) + factor * 4f + factor + i * 0.4f + phaseOffsetMainTentacles * 0.5f)
                               * 20f;
            }

            pipeline.SetTexture(tentacleTexture);
            pipeline.DrawTrail(positions, static _ => 10, _ => drawColor);
        }

        if(StingerProjectileId != -1) {
            var stingerProj = Main.projectile[StingerProjectileId];
            if(stingerProj.active && stingerProj.ModProjectile is DevilOWarStingerProjectile stinger) {
                var activeStingerStart = NPC.Center;
                if(!stinger.IsRetracting) {
                    GenerateWavyTentaclePoints(stingerTrailPositions, activeStingerStart, stingerProj.Center, TENATCLE_SEGMENT_COUNT, 0.5f, 0.1f, 15f, NPC.whoAmI * 0.234f);
                }
                else {
                    GenerateWavyTentaclePoints(stingerTrailPositions, activeStingerStart, stinger.Projectile.Center, TENATCLE_SEGMENT_COUNT, 0.5f, 0.1f, 15f, NPC.whoAmI * 0.234f);
                }

                var stingerColor = Color.Lerp(drawColor, Color.Yellow, 0.5f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.2f);

                pipeline.SetTexture(tentacleTexture);
                pipeline.DrawTrail(stingerTrailPositions, static _ => 10, _ => stingerColor);
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

        var pipeline = Graphics.Begin(Graphics.WorldTransformMatrix);

        var offsetForTrails = flipped ? new Vector2(-5, 30) : new Vector2(5, 30);
        Vector2 bodyWorldPositionForTrails = NPC.Center + offsetForTrails;

        PopulateTrails(pipeline, bodyWorldPositionForTrails, drawColor);
        pipeline.End();

        var fluidEffect = Assets.Shaders.Pixel.DevilOWarFluid.Asset.Value;

        int clampedLifeDrained = Math.Clamp(TotalLifeDrained, 0, MAX_DRAIN_FOR_LEVEL);
        float mappedLevel = MathHelper.Lerp(0.07f, 0.5f, (float)clampedLifeDrained / MAX_DRAIN_FOR_LEVEL);

        Main.spriteBatch.Draw(insidesTexture, NPC.Center + new Vector2(0, 19) - screenPos, null, drawColor, NPC.rotation, insidesTexture.Size() / 2, 1f, effects, 0f);

        Graphics.BeginPixelated()
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
        direction = Vector2.DistanceSquared(start, end) > 0.001f ? Vector2.Normalize(end - start) : Vector2.UnitY;

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