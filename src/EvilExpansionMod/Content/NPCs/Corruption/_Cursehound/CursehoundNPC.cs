using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;

namespace EvilExpansionMod.Content.NPCs.Corruption;

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

    public ref struct MappedAI(NPC npc) {
        public State CurrentState {
            get => (State)npc.ai[0];
            set {
                if (npc.ai[0] != (int)value) {
                    npc.ai[0] = (int)value;
                    npc.ai[1] = 0;
                    npc.netUpdate = true; // sync, i think
                }
            }
        }

        public ref float Timer => ref npc.ai[1];
        public ref float MaceAttackCooldown => ref npc.ai[2];
        public ref float RoarAttackCooldown => ref npc.ai[3];
    }
    
    private const int roar_telegraph_duration = (int)(0.5 * 60);
    private const int roar_duration = 2 * 60;
    private const int roar_downtime_duration = 1 * 60;
    
    private const int mace_spin_duration = 1 * 60;
    private const int mace_duration = (int)(2.5f * 60);
    private const int MaceRetractDuration = 1 * 60;

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Cursehound.KEY_CursehoundNPC;

    public Player Target => Main.player[NPC.target];
    
    private float _timeGrounded;
    private const int ground_time_for_attack = 1 * 60;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 31;
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
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void AI() {
        var ai = new MappedAI(NPC);

        NPC.TargetClosest();
        var target = Target;

        if (target.dead || !target.active) {
            NPC.velocity.X = 0;
            ai.CurrentState = State.Idle;
            return;
        }

        ai.MaceAttackCooldown -= 1f;
        ai.RoarAttackCooldown -= 1f;
        
        if (NPC.velocity.Y == 0) {
            _timeGrounded++;
        }
        else {
            _timeGrounded = 0;
        }

        var los = Collision.CanHitLine(target.position, target.width, target.height, NPC.Top, 1, 1);
        var broadLos = los;

        if (!broadLos && Collision.CanHitLine(NPC.Top, 1, 1, NPC.Center - Vector2.UnitY * 100f, 1, 1)) {
            broadLos = Collision.CanHitLine(target.position, target.width, target.height, NPC.Center - Vector2.UnitY * 100f, 1, 1);
        }

        float distanceToTarget = NPC.Distance(target.Center);
        float distanceToPlayerX = Math.Abs(target.Center.X - NPC.Center.X);

        NPC.direction = (target.Center.X < NPC.Center.X) ? -1 : 1;
        NPC.spriteDirection = NPC.direction; 

        switch (ai.CurrentState) {
            case State.Idle:
                if (distanceToTarget < 1000f && broadLos) {
                    ai.CurrentState = State.Walking;
                }
                NPC.velocity.X *= 0.9f;
                break;

            case State.Walking:
            case State.Running:
                Movement(ref ai, distanceToTarget, distanceToPlayerX, broadLos);
                break;

            case State.MaceSpinning:
                MaceSpinning(ref ai);
                break;
            case State.MaceAttacking:
                MaceAttack(ref ai, target);
                break;
            case State.MaceRetracting:
                MaceRetracting(ref ai);
                break;
            case State.RoarTelegraph:
                RoarTelegraph(ref ai, target);
                break;
            case State.Roaring:
                Roar(ref ai, target);
                break;
            case State.RoarDowntime:
                HandleRoarDowntime(ref ai);
                break;
        }
    }

    private void Movement(ref MappedAI ai, float distanceToTarget, float distanceToPlayerX, bool broadLineOfSight) {
        float maceAttackRange = 250f;
        float roarAttackMinRange = 200f;
        float roarAttackMaxRange = 500f;
        float runThreshold = 700f;
        
        float baseJumpPower = 10f;
        float jumpScaleFactor = 0.05f;
        float maxJumpPower = 20f;
        
        float verticalDifference = NPC.Center.Y - Target.Center.Y;
        float dynamicJumpVelocity = -(baseJumpPower + Math.Max(0, verticalDifference) * jumpScaleFactor);
        dynamicJumpVelocity = MathHelper.Clamp(dynamicJumpVelocity, -maxJumpPower, -baseJumpPower);

        if (NPC.velocity.Y == 0 && _timeGrounded >= ground_time_for_attack && ai.RoarAttackCooldown <= 0 && broadLineOfSight && distanceToTarget >= roarAttackMinRange && distanceToTarget <= roarAttackMaxRange) {
            ai.CurrentState = State.RoarTelegraph;
            ai.RoarAttackCooldown = 60 * 15;
            return;
        }

        if (ai.MaceAttackCooldown <= 0 && broadLineOfSight && distanceToTarget < maceAttackRange && _timeGrounded >= ground_time_for_attack) {
            ai.CurrentState = State.MaceSpinning;
            ai.MaceAttackCooldown = 60 * 5;
            return;
        }

        bool shouldRun = distanceToTarget > runThreshold;
        if (shouldRun) {
            ai.CurrentState = State.Running;
        }
        else {
            ai.CurrentState = State.Walking;
        }

        float maxSpeed = shouldRun ? 8f : 4f;
        float acceleration = shouldRun ? 0.08f : 0.04f;

        if (Math.Abs(NPC.velocity.X) > maxSpeed && NPC.velocity.Y != 0) {
            maxSpeed = MathHelper.Lerp(Math.Abs(NPC.velocity.X), maxSpeed, 0.1f);
        }

        NPC.velocity.X += acceleration * NPC.direction;
        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * maxSpeed, 0.01f);
        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        if (distanceToPlayerX < 50f && NPC.velocity.Y == 0) {
            NPC.velocity.X *= 0.85f;
        }
        
        if ((NPC.collideX) && NPC.velocity.Y == 0) {
            NPC.velocity.Y = dynamicJumpVelocity;
            _timeGrounded = 0;
        }
        
        
        if (NPC.velocity.Y == 0 && Target.Top.Y < NPC.Bottom.Y && MathUtilities.HoleAtPosition(NPC, NPC.Center.X + NPC.velocity.X)) {
            NPC.velocity.Y = dynamicJumpVelocity;
        }

        NPC.spriteDirection = NPC.direction;
        NPC.rotation = -NPC.velocity.Y * 0.06f * -NPC.direction;
        NPC.rotation = Math.Clamp(NPC.rotation, -0.2f, 0.2f);
    }

    private void MaceSpinning(ref MappedAI ai) {
        NPC.velocity.X *= 0.9f;
        ai.Timer++;

        if (ai.Timer >= mace_spin_duration) {
            ai.CurrentState = State.MaceAttacking;
        }
    }

    private void MaceAttack(ref MappedAI ai, Player target) {
        NPC.velocity.X *= 0.5f;
        ai.Timer++;

        if (ai.Timer == 1) {
            SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound with { Volume = 0.7f, Pitch = 0.5f }, NPC.Center);
            CombatText.NewText(NPC.Hitbox, Color.White, "mace");

            Vector2 launchOrigin = NPC.Center + new Vector2(NPC.direction * 50, -40);

            float horizontalSpeed = 16f;
            float gravity = 0.4f;

            Vector2 targetPosition = target.Center;

            float dx = targetPosition.X - launchOrigin.X;
            float dy = targetPosition.Y - launchOrigin.Y;

            float initialVx = horizontalSpeed * NPC.direction;
        
            float timeToTarget = Math.Abs(initialVx) > 0.01f ? dx / initialVx : 0;
        
            if (timeToTarget <= 0.1f) {
                timeToTarget = 0.5f;
                initialVx = NPC.direction * horizontalSpeed;
            }
        
            float initialVy = (dy - 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;
        
            float maxInitialVy = -20f;
            float minInitialVy = 15f;
            initialVy = MathHelper.Clamp(initialVy, maxInitialVy, minInitialVy);

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                launchOrigin.X,
                launchOrigin.Y,
                initialVx,
                initialVy,
                ModContent.ProjectileType<CursehoundMace>(),
                NPC.damage,
                5f,
                Main.myPlayer,
                (int)CursehoundMace.AIState.Launched,
                0,
                NPC.whoAmI
            );
        }

        if (ai.Timer >= mace_duration) {
            ai.CurrentState = State.MaceRetracting;
        }
    }

    private void MaceRetracting(ref MappedAI ai) {
        NPC.velocity.X *= 0.2f;
        ai.Timer++;

        if (ai.Timer >= MaceRetractDuration) {
            ai.CurrentState = State.Walking;
        }
    }

    private void Roar(ref MappedAI ai, Player target) {
        
        NPC.velocity.X *= 0.1f;
        ai.Timer++;

        if (ai.Timer == 1) {
            SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 1.2f, Pitch = 0.5f }, NPC.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 1.2f, Pitch = 0.1f }, NPC.Center);
            
            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.6f));
        }

        if (ai.Timer > 30 && ai.Timer < 90 && ai.Timer % 10 == 0) {
            var searchRadiusTiles = 20;
            List<Point> lavaTiles = new();

            int startTileX = (int)((Target.Center.X - searchRadiusTiles * 16) / 16f);
            int endTileX = (int)((Target.Center.X + searchRadiusTiles * 16) / 16f);
            int startTileY = (int)((Target.Bottom.Y + 10) / 16f);
            int endTileY = (int)((Target.Bottom.Y + 10 + searchRadiusTiles / 2 * 16) / 16f);

            for (int x = startTileX; x < endTileX; x++) {
                for (int y = startTileY; y < endTileY; y++) {
                    if (WorldGen.InWorld(x, y)) {
                        var tile = Main.tile[x, y];
                        if (tile.LiquidType == LiquidID.Lava && tile.LiquidAmount > 0) {
                            lavaTiles.Add(new Point(x, y));
                        }
                    }
                }
            }

            if (lavaTiles.Count > 0) {
                var randomLavaTile = lavaTiles[Main.rand.Next(lavaTiles.Count)];
                var spawnPos = randomLavaTile.ToWorldCoordinates();
                var velocity = new Vector2(0, Main.rand.NextFloat(-10f, -15f));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ModContent.ProjectileType<SpiritFireball>(), NPC.damage / 2, 0f, Main.myPlayer);
                
                if (Filters.Scene["WaterDistortion"].GetShader() is not WaterShaderData data) {
                    return;
                }
            
                data.QueueRipple(spawnPos, 30f, RippleShape.Circle, MathHelper.PiOver4);
            }
        }

        if (ai.Timer >= roar_duration) {
            ai.CurrentState = State.RoarDowntime;
        }
    }
    
    private void RoarTelegraph(ref MappedAI ai, Player target) {
        NPC.velocity.X *= 0.8f;
        ai.Timer++;

        if (ai.Timer >= roar_telegraph_duration) {
            ai.CurrentState = State.Roaring;
        }
    }

    private void HandleRoarDowntime(ref MappedAI ai) {
        NPC.velocity.X *= 0.5f;
        ai.Timer++;

        if (ai.Timer >= roar_downtime_duration) {
            ai.CurrentState = State.Walking;
        }
    }
    
    public override void FindFrame(int frameHeight) {
        var ai = new MappedAI(NPC);

        if (NPC.velocity.Y == 0 && ai.CurrentState != State.MaceAttacking && ai.CurrentState != State.Roaring) {
            NPC.rotation = 0;
        }
        
        if (NPC.velocity.Y != 0) {
            NPC.frame.Y = 30 * frameHeight;
            NPC.spriteDirection = NPC.direction;
            return;
        }

        NPC.spriteDirection = NPC.direction;

        switch (ai.CurrentState) {
            case State.Idle:
                NPC.frameCounter += 0.15f;
                if (NPC.frameCounter >= 3)
                {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
                break;

            case State.Walking:
                NPC.frameCounter += 0.2f;
                if (NPC.frameCounter >= 9) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (14 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.Running:
                NPC.frameCounter += 0.2f;
                if (NPC.frameCounter >= 6) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (23 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceSpinning:
                float loops = 3f;
                float maceSpinAnimationSpeed = (3 * loops) / mace_spin_duration;

                NPC.frameCounter += maceSpinAnimationSpeed;

                if (NPC.frameCounter >= loops) {
                    NPC.frameCounter -= loops;
                }
                NPC.frame.Y = (3 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceAttacking:
                NPC.frameCounter = ai.Timer / (mace_duration / 3f);
                if (NPC.frameCounter >= 3) {
                    NPC.frameCounter = 2;
                }
                NPC.frame.Y = (6 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.MaceRetracting:
                NPC.frame.Y = 8 * frameHeight;
                break;

            case State.RoarTelegraph:
                NPC.frameCounter = ai.Timer / (roar_telegraph_duration / 3f);
                if (NPC.frameCounter >= 3) {
                    NPC.frameCounter = 2;
                }
                NPC.frame.Y = (9 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.Roaring:
                NPC.frameCounter += 0.25f;
                if (NPC.frameCounter >= 2) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (12 + (int)NPC.frameCounter) * frameHeight;
                break;

            case State.RoarDowntime:
                NPC.frameCounter += 0.15f;
                if (NPC.frameCounter >= 3) {
                    NPC.frameCounter = 0;
                }
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
                break;
        }
    }
}