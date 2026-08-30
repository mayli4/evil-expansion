using EvilExpansionMod.Common;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class CursehoundNPC : ModNPC {
    public enum State {
        Idle,
        Walking,
        Running,
        MaceSpinning,
        MaceAttacking,
        MaceRetracting,
        RoarTelegraph,
        Roaring,
        RoarDowntime
    }

    public State CurrentState {
        get => (State)NPC.ai[0];
        set {
            if(NPC.ai[0] != (int)value) {
                NPC.ai[0] = (int)value;
                NPC.ai[1] = 0;
                NPC.netUpdate = true; // sync, i think
            }
        }
    }

    public ref float Timer => ref NPC.ai[1];
    public ref float MaceAttackCooldown => ref NPC.ai[2];
    public ref float RoarAttackCooldown => ref NPC.ai[3];

    private const int ROAR_TELEGRAPH_DURATION = (int)(0.5 * 60);
    private const int ROAR_DURATION = 2 * 60;
    private const int ROAR_DOWNTIME_DURATION = 1 * 60;

    private const int MACE_SPIN_DURATION = 1 * 60;
    private const int MACE_DURATION = (int)(2.5f * 60);
    private const int MACE_RETRACT_DURATION = 1 * 60;

    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.CursehoundNPC.KEY;

    public Player Target => Main.player[NPC.target];

    private Projectile _roarProjectile = null!;
    private float _timeGrounded;
    private const int GROUND_TIME_FOR_ATTACK = 1 * 60;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 29;
    }

    public override void SetDefaults() {
        (NPC.width, NPC.height) = (150, 150);

        NPC.lifeMax = 1800;
        NPC.damage = 30;
        NPC.defense = 10;
        NPC.value = Item.buyPrice(gold: 5, silver: 50);
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0.01f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<CursehoundBannerItem>();
    }

    public override void Load() {
        for(int j = 1; j <= 8; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/CursehoundGore" + j);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.1f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RawShadowScalesItem>(), 1, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ImputedFlameItem>(), 1, 2, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CurseknightsHelm>(), 10, 1, 1));
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 1; i <= 8; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("CursehoundGore" + i).Type);
        }
    }


    public override void AI() {
        NPC.TargetClosest(false);
        var target = Target;

        if(target.dead || !target.active) {
            NPC.velocity.X = 0;
            CurrentState = State.Idle;
            return;
        }

        MaceAttackCooldown -= 1f;
        RoarAttackCooldown -= 1f;

        if(NPC.velocity.Y == 0) {
            _timeGrounded++;
        }
        else {
            _timeGrounded = 0;
        }

        var los = Collision.CanHitLine(target.position, target.width, target.height, NPC.Top, 1, 1);
        var broadLos = los;

        if(!broadLos && Collision.CanHitLine(NPC.Top, 1, 1, NPC.Center - Vector2.UnitY * 100f, 1, 1)) {
            broadLos = Collision.CanHitLine(target.position, target.width, target.height, NPC.Center - Vector2.UnitY * 100f, 1, 1);
        }

        float distanceToTarget = NPC.Distance(target.Center);
        float distanceToPlayerX = Math.Abs(target.Center.X - NPC.Center.X);

        switch(CurrentState) {
            case State.Idle:
                if(distanceToTarget < 1000f && broadLos) {
                    CurrentState = State.Walking;
                }

                NPC.velocity.X *= 0.9f;

                NPC.direction = (Target.Center.X < NPC.Center.X) ? -1 : 1;
                NPC.spriteDirection = NPC.direction;

                break;
            case State.Walking:
            case State.Running:
                Movement(distanceToTarget, distanceToPlayerX, broadLos);
                break;

            case State.MaceSpinning:
                MaceSpinning();
                break;
            case State.MaceAttacking:
                MaceAttack(ref target);
                break;
            case State.MaceRetracting:
                MaceRetracting();
                break;
            case State.RoarTelegraph:
                RoarTelegraph(ref target);
                break;
            case State.Roaring:
                Roar(ref target);
                break;
            case State.RoarDowntime:
                HandleRoarDowntime();
                break;
        }
    }

    private void Movement(float distanceToTarget, float distanceToPlayerX, bool broadLineOfSight) {
        float maceAttackRange = 550f;
        float roarAttackMinRange = 200f;
        float roarAttackMaxRange = 500f;
        float runThreshold = 300f;

        float baseJumpPower = 5f;
        float jumpScaleFactor = 0.05f;
        float maxJumpPower = 12f;

        float verticalDifference = NPC.Center.Y - Target.Center.Y;
        float dynamicJumpVelocity = MathHelper.Clamp(
            -(baseJumpPower + Math.Max(0, verticalDifference) * jumpScaleFactor), 
            -maxJumpPower, 
            -baseJumpPower);

        if(NPC.velocity.Y == 0 && _timeGrounded >= GROUND_TIME_FOR_ATTACK && RoarAttackCooldown <= 0 && broadLineOfSight && distanceToTarget >= roarAttackMinRange && distanceToTarget <= roarAttackMaxRange) {
            if(Main.rand.NextBool(20)) {
                CurrentState = State.RoarTelegraph;
                RoarAttackCooldown = 60 * 5;
            }
            return;
        }

        if(MaceAttackCooldown <= 0 && broadLineOfSight && distanceToTarget < maceAttackRange && _timeGrounded >= GROUND_TIME_FOR_ATTACK) {
            if(Main.rand.NextBool(30)) {
                CurrentState = State.MaceSpinning;
                MaceAttackCooldown = 60 * 5;
            }
            return;
        }

        bool shouldRun = distanceToTarget > runThreshold;
        CurrentState = shouldRun ? State.Running : State.Walking;

        float maxSpeed = shouldRun ? 8f : 4f;
        float acceleration = shouldRun ? 0.08f : 0.04f;

        if(Math.Abs(NPC.velocity.X) > maxSpeed && NPC.velocity.Y != 0) {
            maxSpeed = MathHelper.Lerp(Math.Abs(NPC.velocity.X), maxSpeed, 0.1f);
        }

        NPC.velocity.X += acceleration * NPC.direction;
        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * maxSpeed, 0.01f);
        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        if(distanceToPlayerX < 50f && NPC.velocity.Y == 0) {
            NPC.velocity.X *= 0.85f;
        }

        if (NPC.collideX && NPC.velocity.Y == 0) {
            NPC.velocity.Y = dynamicJumpVelocity;
            _timeGrounded = 0;
            NPC.noTileCollide = true;
        }

        if (NPC.velocity.Y == 0 && Target.Top.Y < NPC.Bottom.Y && Helper.HoleAtPosition(NPC, NPC.Center.X + NPC.velocity.X)) {
            NPC.velocity.Y = dynamicJumpVelocity;
            _timeGrounded = 0;
            NPC.noTileCollide = true;
        }

        NPC.noTileCollide = NPC.velocity.Y switch
        {
            < 0f => true,
            >= 0f => false,
            _ => NPC.noTileCollide,
        };

        NPC.direction = (Target.Center.X < NPC.Center.X) ? -1 : 1;
        NPC.spriteDirection = NPC.direction;

        NPC.spriteDirection = NPC.direction;
        NPC.rotation = -NPC.velocity.Y * 0.06f * -NPC.direction;
        NPC.rotation = Math.Clamp(NPC.rotation, -0.2f, 0.2f);
    }

    private void MaceSpinning() {
        NPC.velocity.X *= 0.9f;
        Timer++;

        if(Timer == 1) {
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.MaceSwing.Asset, NPC.Center);
        }

        if(Timer >= MACE_SPIN_DURATION) {
            CurrentState = State.MaceAttacking;
        }

        LookAtTarget();
    }

    private void MaceAttack(ref Player _) {
        NPC.velocity.X *= 0.5f;
        Timer++;

        if(Timer == 1) {
            Vector2 launchOrigin = NPC.Center + new Vector2(NPC.direction * 50, -40);
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.MaceThrow.Asset, NPC.Center);

            float gravity = 0.4f;

            var velocity = Helper.InitialVelocityRequiredToHitPosition(
                launchOrigin,
                Target.Center,
                gravity,
                16f
            );

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                launchOrigin.X,
                launchOrigin.Y,
                velocity.X * 1.7f,
                velocity.Y / 2,
                ModContent.ProjectileType<CursehoundMace>(),
                NPC.damage,
                5f,
                Main.myPlayer,
                (int)CursehoundMace.State.Launched,
                0,
                NPC.whoAmI
            );
        }

        if(Timer >= MACE_DURATION) {
            CurrentState = State.MaceRetracting;
        }

        LookAtTarget();
    }

    private void MaceRetracting() {
        NPC.velocity.X *= 0.2f;
        Timer++;

        if(Timer >= MACE_RETRACT_DURATION) {
            CurrentState = State.Walking;
        }
    }

    private void Roar(ref Player _) {
        NPC.velocity.X *= 0.1f;
        Timer++;

        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(1.25f, 0.9f, NPC.Center, 2000f));

        var roarProjectilePosition = NPC.Center + new Vector2(126f * NPC.direction, 12f);
        if(Timer == 1) {
            SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 1.2f, Pitch = 0.1f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 1.2f, Pitch = 0.1f }, NPC.Center);

            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(22f, 0.6f, NPC.Center, 2000f));

            _roarProjectile = RoarProjectile.New(
                NPC.GetSource_FromAI(),
                roarProjectilePosition,
                1800,
                120);
        }

        var waterShaderData = Filters.Scene["WaterDistortion"].GetShader() as WaterShaderData;
        if(Timer is > 30 and < 90 && Timer % 10 == 0) {
            var searchRadiusTiles = 40;
            List<Point> lavaTiles = new();

            int startTileX = (int)((Target.Center.X - searchRadiusTiles * 16) / 16f);
            int endTileX = (int)((Target.Center.X + searchRadiusTiles * 16) / 16f);
            int startTileY = (int)((Target.Bottom.Y + 10) / 16f);
            int endTileY = (int)((Target.Bottom.Y + 10 + searchRadiusTiles / 2 * 16) / 16f);

            for(int x = startTileX; x < endTileX; x++) {
                for(int y = startTileY; y < endTileY; y++) {
                    if(WorldGen.InWorld(x, y)) {
                        var tile = Main.tile[x, y];
                        if(tile is { LiquidType: LiquidID.Lava, LiquidAmount: > 0 }) {
                            lavaTiles.Add(new Point(x, y));
                        }
                    }
                }
            }

            if(lavaTiles.Count > 0) {
                var randomLavaTile = lavaTiles[Main.rand.Next(lavaTiles.Count)];
                var spawnPos = randomLavaTile.ToWorldCoordinates();
                var velocity = new Vector2(0, Helper.InitialVelocityRequiredToHitPosition(spawnPos, Target.position - new Vector2(0, 40), 0.4f, 16f).Y);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ModContent.ProjectileType<SpiritFireball>(), NPC.damage / 2, 0f, Main.myPlayer);

                waterShaderData?.QueueRipple(spawnPos, 30f, RippleShape.Circle, MathHelper.PiOver4);
            }
        }

        if(Timer is > 40 and < ROAR_DURATION - 30 && Timer % 20 == 0) {
            int numberOfStalactites = Main.rand.Next(2, 4);
            float spawnAreaWidth = 300f;

            for(int i = 0; i < numberOfStalactites; i++) {
                float spawnX = Target.Center.X + Main.rand.NextFloat(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);

                int tileX = (int)(spawnX / 16f);
                int tileY = (int)(Target.position.Y / 16f) - 10;

                for(int y = tileY; y > 0; y--) {
                    if(WorldGen.InWorld(tileX, y) && Main.tile[tileX, y].HasTile && Main.tileSolid[Main.tile[tileX, y].TileType]) {
                        Vector2 spawnPosition = new Vector2(tileX * 16f + 8f, y * 16f + 16f);
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            spawnPosition,
                            Vector2.Zero,
                            ModContent.ProjectileType<StalactiteProjectile>(),
                            NPC.damage / 2,
                            2f,
                            Main.myPlayer
                        );
                        break;
                    }
                }
            }
        }

        if(Timer >= ROAR_DURATION) {
            CurrentState = State.RoarDowntime;
        }
    }

    private void RoarTelegraph(ref Player _) {
        NPC.velocity.X *= 0.8f;
        Timer++;

        if(Timer >= ROAR_TELEGRAPH_DURATION) {
            CurrentState = State.Roaring;
        }

        LookAtTarget();
    }

    private void HandleRoarDowntime() {
        NPC.velocity.X *= 0.5f;
        Timer++;

        if(Timer >= ROAR_DOWNTIME_DURATION) {
            CurrentState = State.Walking;
        }

        LookAtTarget();
    }
 
    private void LookAtTarget() {
        NPC.direction = (Target.Center.X < NPC.Center.X) ? -1 : 1;
        NPC.spriteDirection = NPC.direction;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var tex = TextureAssets.Npc[Type].Value;

        var drawPosition = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY);

        var frame = NPC.frame;

        var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        spriteBatch.Draw(
            tex,
            drawPosition,
            frame,
            drawColor,
            NPC.rotation,
            frame.Size() / 2f,
            NPC.scale,
            effects,
            0f
        );

        spriteBatch.Draw(
            Assets.Images.Corruption.NPCs.Cursehound.CursehoundNPC_Glow.Asset.Value,
            drawPosition,
            frame,
            Color.White,
            NPC.rotation,
            frame.Size() / 2f,
            NPC.scale,
            effects,
            0f
        );

        return false;
    }

    public override void FindFrame(int frameHeight) {
        if(NPC.velocity.Y == 0 && CurrentState != State.MaceAttacking && CurrentState != State.Roaring) {
            NPC.rotation = 0;
        }

        if(NPC.velocity.Y != 0) {
            NPC.frame.Y = 27 * frameHeight;
            NPC.spriteDirection = NPC.direction;
            return;
        }

        NPC.spriteDirection = NPC.direction;

        switch(CurrentState) {
            case State.Idle:
                NPC.frameCounter += 0.15f;
                if(NPC.frameCounter >= 3) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
                break;

            case State.Walking:
                NPC.frameCounter += 0.2f;
                if(NPC.frameCounter >= 9) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (14 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.Running:
                NPC.frameCounter += 0.2f;
                if(NPC.frameCounter >= 6) {
                    NPC.frameCounter = 0;
                    //ueah i hate this but im lazy
                    SoundEngine.PlaySound(Assets.Sounds.Cursehound.CursehoundStep1.Asset with { Pitch = 0.0f, PitchVariance = 0.4f }, NPC.Center);
                }
                NPC.frame.Y = (23 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceSpinning:
                float loops = 3f;
                float maceSpinAnimationSpeed = (3 * loops) / MACE_SPIN_DURATION;

                NPC.frameCounter += maceSpinAnimationSpeed;

                if(NPC.frameCounter >= loops) {
                    NPC.frameCounter -= loops;
                }
                NPC.frame.Y = (3 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceAttacking:
                NPC.frameCounter = Timer / (MACE_DURATION / 3f);
                if(NPC.frameCounter >= 3) {
                    NPC.frameCounter = 2;
                }
                NPC.frame.Y = (6 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceRetracting:
                NPC.frame.Y = 8 * frameHeight;
                break;

            case State.RoarTelegraph:
                NPC.frameCounter = Timer / (ROAR_TELEGRAPH_DURATION / 3f);
                if(NPC.frameCounter >= 3) {
                    NPC.frameCounter = 2;
                }
                NPC.frame.Y = (9 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.Roaring:
                NPC.frameCounter += 0.25f;
                if(NPC.frameCounter >= 2) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (12 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.RoarDowntime:
                NPC.frameCounter += 0.15f;
                if(NPC.frameCounter >= 3) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
                break;
        }
    }
}