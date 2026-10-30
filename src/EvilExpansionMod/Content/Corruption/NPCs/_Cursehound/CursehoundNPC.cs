using EvilExpansionMod.Common;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Filters = Terraria.Graphics.Effects.Filters;

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

    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;

    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.CursehoundNPC.KEY;

    public Player Target => Main.player[NPC.target];

    private Projectile _roarProjectile = null!;
    private float _timeGrounded;
    private const int GROUND_TIME_FOR_ATTACK = 1 * 60;

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Position = new Vector2(50f, 50f),
            PortraitPositionXOverride = 20f,
            PortraitPositionYOverride = 40f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        ContentSamples.NpcBestiaryRarityStars[Type] = 3;
        Main.npcFrameCount[Type] = 29;
    }

    public override void SetDefaults() {
        (NPC.width, NPC.height) = (125, 100);

        NPC.lifeMax = 2950;
        NPC.damage = 30;
        NPC.defense = 25;
        NPC.value = Item.buyPrice(gold: 1, silver: 50);
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath60;

        ItemID.Sets.KillsToBanner[BannerItem] = 25;
        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<CursehoundBannerItem>();
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
        if(Main.expertMode) {
            NPC.defense = 28;
        }
    }
    public override void Load() {
        for(int j = 1; j <= 8; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/CursehoundGore" + j);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => Main.hardMode && spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.05f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RawShadowScalesItem>(), 1, 2, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ImputedFlameItem>(), 1, 3, 4));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CurseknightsHelm>(), 10, 1, 1));
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.CursehoundNPCBestiary.KEY),
        });
    }
    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 1; i <= 8; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("CursehoundGore" + i).Type);
        }
    }

    public override bool? CanBeHitByProjectile(Projectile projectile) {
        if(projectile.hostile) { return false; }
        Rectangle largerHurtbox = new Rectangle((int)NPC.position.X - 75, (int)NPC.position.Y - 75, 150, 150);
        if(projectile.Hitbox.Intersects(largerHurtbox)) {
            return true;
        }
        return false;
    }

    private SlotId activeSoundSlot;
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
                if(distanceToTarget < 60 * 16f && broadLos) {
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

        //CurrentState = State.Roaring;
    }
    bool amStuck = false;
    private float nothingHappeningX;
    private float nothingHappeningY;
    static int nothingHappeningInterval = 60;
    int nothingHappeningTimer = nothingHappeningInterval;
    int tooLongAboveYou = 0;
    int jumpPhasingTimer = 0;
    bool amJumping = false;
    int airTime = 0;

    private void Movement(float distanceToTarget, float distanceToPlayerX, bool broadLineOfSight) {
        bool isInsideSolid = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

        float maceAttackRange = 550f;
        float roarAttackMinRange = 200f;
        float roarAttackMaxRange = 700f;
        float runThreshold = 300f;

        float baseJumpPower = 5f;
        float jumpScaleFactor = 0.05f;
        float maxJumpPower = 12f;

        float verticalDifference = NPC.Center.Y - Target.Center.Y;
        float dynamicJumpVelocity = MathHelper.Clamp(
            -(baseJumpPower + Math.Max(0, verticalDifference) * jumpScaleFactor),
            -maxJumpPower - 50,
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

        float maxSpeed = shouldRun ? (int)(8f * (Math.Sqrt(DifficultyScaler))) : 4f;
        float acceleration = shouldRun ? (int)(0.08f * (Math.Sqrt(DifficultyScaler))) : 0.04f;

        if(Math.Abs(NPC.velocity.X) > maxSpeed && NPC.velocity.Y != 0) {
            maxSpeed = MathHelper.Lerp(Math.Abs(NPC.velocity.X), maxSpeed, 0.1f);
        }
        if(!isInsideSolid) {
            NPC.velocity.X += acceleration * NPC.direction;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * maxSpeed, 0.01f);
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
        }

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        if(distanceToPlayerX < 10*16f && NPC.velocity.Y == 0 && !isInsideSolid) {
            NPC.velocity.X *= 0.9f;
            if(distanceToPlayerX < 6*16f) NPC.velocity.X *= 0.9f;
            if(distanceToPlayerX < 2*16f) NPC.velocity.X *= 0f;
        }

        //if(NPC.collideX && NPC.velocity.Y == 0) {
        //    amStuck = true;
        //    //NPC.velocity.Y = dynamicJumpVelocity;
        //    //_timeGrounded = 0;
        //    //NPC.noTileCollide = true;
        //}

        //if(NPC.velocity.Y == 0 && Target.Top.Y > NPC.Bottom.Y && Helper.HoleAtPosition(NPC, NPC.Center.X + NPC.velocity.X)) {
        //    amStuck = true;
        //    //NPC.velocity.Y = dynamicJumpVelocity;
        //    //_timeGrounded = 0;
        //    //NPC.noTileCollide = true;
        //}

        if(nothingHappeningTimer > 0) {
            nothingHappeningTimer--;
        }
        else {
            nothingHappeningX = NPC.Bottom.X;
            nothingHappeningY = NPC.Bottom.Y;
            nothingHappeningTimer = nothingHappeningInterval;
        }

        if(nothingHappeningTimer == nothingHappeningInterval - 1 && nothingHappeningX == NPC.Bottom.X && nothingHappeningY == NPC.Bottom.Y && NPC.velocity.Y == 0) amStuck = true;
        else amStuck = false;

        if(amStuck) {
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.CursehoundStep1.Asset with { Pitch = 0.4f, PitchVariance = 0.4f }, NPC.Center);

            for(int i = 0; i < 6 + (distanceToTarget * 0.0625 * 0.1); i++) {
                var dustPos = NPC.Bottom + Main.rand.NextVector2Circular(120f, 20f)
                    + Vector2.UnitY * 1.5f;
                var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f)
                    + NPC.Bottom.DirectionTo(dustPos) * 5f;

                var dustColorStart = new Color(133, 122, 94);
                var dustColorFade = dustColorStart * 0.4f;

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.8f,
                    ColorStart = dustColorStart,
                    ColorFade = dustColorFade,
                    Spin = 0.03f,
                    InitialScale = Main.rand.NextFloat(0.25f, 0.75f) * (float)(1 + distanceToTarget * 0.0625 * 0.02)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    dustVelocity,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;
            }
            if (distanceToTarget > 16 * 35) {
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 1f }, NPC.Center);
            }
            jumpPhasingTimer = (int)(distanceToTarget * 0.001 * 60);
            amJumping = true;
            NPC.velocity.Y = dynamicJumpVelocity * 0.8f - (Target.Bottom.Y - NPC.Bottom.Y) * 0.015f;
            if(NPC.Bottom.X > Target.Bottom.X) NPC.velocity.X = -(distanceToPlayerX) * 0.015f;
            else NPC.velocity.X = (distanceToPlayerX) * 0.015f;
        }
        if(Target.Bottom.Y <= NPC.Bottom.Y && NPC.velocity.Y > 0 || Target.Bottom.Y >= NPC.Bottom.Y && NPC.velocity.Y < 0) amJumping = false;
        // amJumping is true when Hound is moving UP AND is below player, or moving DOWN AND is above

        if(jumpPhasingTimer > 0) {
            jumpPhasingTimer--;
           //if(NPC.velocity.Y > 0 && distanceToTarget < 16 * 5) jumpPhasingTimer = 0;
        }

        if(Target.Bottom.Y > NPC.Bottom.Y && distanceToPlayerX < 16 * 8) {
            tooLongAboveYou++;
            if(distanceToTarget > 16 * 40) tooLongAboveYou++;
            if(distanceToTarget > 16 * 80) tooLongAboveYou++;
        }
        else tooLongAboveYou = 0;

        if(tooLongAboveYou > 1 || NPC.velocity.Y < 0 || amStuck || jumpPhasingTimer > 0 && tooLongAboveYou > 100 || (Target.Bottom.Y > NPC.Bottom.Y || amJumping || jumpPhasingTimer > 0)) {
            // No collision when aerial AND (above player OR jumping)
            NPC.noTileCollide = true;
        }
        else NPC.noTileCollide = false;

        if(NPC.velocity.Y != 0) airTime++;
        Main.NewText("airTime" + airTime, Microsoft.Xna.Framework.Color.White);
        if(airTime > 60 * 2 && NPC.velocity.Y == 0) {
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.CursehoundStep1.Asset with { Pitch = -0.3f, PitchVariance = 0.2f }, NPC.Center);
            if(airTime > 60 * 4) SoundEngine.PlaySound(SoundID.Item14 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 0.4f }, NPC.Center);

            for(int i = 0; i < 6 + (distanceToTarget * 0.0625 * 0.1); i++) {
                var dustPos = NPC.Bottom + Main.rand.NextVector2Circular(120f, 20f)
                    + Vector2.UnitY * 1.5f;
                var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f)
                    + NPC.Bottom.DirectionTo(dustPos) * 5f;

                var dustColorStart = new Color(133, 122, 94);
                var dustColorFade = dustColorStart * 0.4f;

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.8f,
                    ColorStart = dustColorStart,
                    ColorFade = dustColorFade,
                    Spin = 0.03f,
                    InitialScale = Main.rand.NextFloat(0.25f, 0.75f) * (float)(1 + distanceToTarget * 0.0625 * 0.02)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    dustVelocity,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;
            }
            airTime = 0;
        }

        NPC.direction = (Target.Center.X < NPC.Center.X) ? -1 : 1;
        NPC.spriteDirection = NPC.direction;

        NPC.spriteDirection = NPC.direction;
        NPC.rotation = Utils.AngleLerp(NPC.rotation, -NPC.velocity.Y * 0.06f * -NPC.direction, 0.1f);
        NPC.rotation = Math.Clamp(NPC.rotation, -0.2f, 0.2f);

        //Main.NewText("NPC.velocity.X" + NPC.velocity.X, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("NPC.velocity.Y" + NPC.velocity.Y, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("NPC.Bottom.X" + NPC.Bottom.X, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("NPC.Bottom.Y" + NPC.Bottom.Y, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("nothingHappeningX" + nothingHappeningX, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("nothingHappeningY" + nothingHappeningY, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("nothingHappeningTimer" + nothingHappeningTimer, Microsoft.Xna.Framework.Color.White);
        ////Main.NewText("amStuck" + amStuck, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("NPC.noTileCollide" + NPC.noTileCollide, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("amJumping" + amJumping, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("jumpPhasingTimer" + jumpPhasingTimer, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("tooLongAboveYou" + tooLongAboveYou, Microsoft.Xna.Framework.Color.White);
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
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.MaceThrow.Asset with { Volume = 1.2f }, NPC.Center);

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
                velocity.X * 1.9f,
                velocity.Y / 1.9f,
                ModContent.ProjectileType<CursehoundMace>(),
                (int)(NPC.damage * 0.5f),
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
            for(int i = 0; i < 2; i++) {
                SoundEngine.PlaySound(SoundID.DD2_BetsyDeath with { Volume = 1.5f, PitchRange = (0.1f, 0.3f) }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 2.5f, PitchRange = (0.1f, 0.3f) }, NPC.Center);
                SoundEngine.PlaySound(SoundID.ScaryScream with { Volume = 0.1f, PitchRange = (-0.5f, -0.3f) }, NPC.Center);
            }

            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(22f, 0.6f, NPC.Center, 2000f));

            _roarProjectile = RoarProjectile.New(
                NPC.GetSource_FromAI(),
                roarProjectilePosition,
                1800,
                120);
        }

        if(Timer == 20) {
            int searchRadiusTiles = 40;
            List<Point> lavaTiles = new();

            int startTileX = (int)((Target.Center.X - searchRadiusTiles * 16) / 16f);
            int endTileX = (int)((Target.Center.X + searchRadiusTiles * 16) / 16f);
            int startTileY = (int)((Target.Bottom.Y - 1) / 16f);
            int endTileY = (int)((Target.Bottom.Y + 10 + searchRadiusTiles / 2 * 16) / 16f);

            for(int x = startTileX; x < endTileX; x++) {
                for(int y = startTileY; y < endTileY; y++) {
                    if(WorldGen.InWorld(x, y)) {
                        Tile tile = Main.tile[x, y];
                        if(tile is { LiquidType: LiquidID.Lava, LiquidAmount: > 0 } && Main.tile[x, y - 1].LiquidAmount == 0 && !Main.tile[x, y - 1].HasTile) {
                            lavaTiles.Add(new Point(x, y));
                        }
                    }
                }
            }

            int totalTelegraphs = (int)(4 * (Math.Sqrt(DifficultyScaler)));
            int telegraphDuration = 45;

            for(int i = 0; i < totalTelegraphs && lavaTiles.Count > 0; i++) {
                int index = Main.rand.Next(lavaTiles.Count);
                Point lavaTile = lavaTiles[index];
                lavaTiles.RemoveAt(index);

                Vector2 spawnPos = lavaTile.ToWorldCoordinates();

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<LavaTelegraphProjectile>(),
                    30,
                    0f,
                    Main.myPlayer,
                    ai0: telegraphDuration,
                    ai2: NPC.damage / 2
                );
            }
        }

        if(Timer is > 40 and < ROAR_DURATION - 30 && Timer % 20 == 0) {
            int numberOfStalactites = (int)(Main.rand.Next(3, 5) * (Math.Sqrt(DifficultyScaler)));
            float spawnAreaWidth = (int)(550f * (Math.Sqrt(DifficultyScaler)));

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
            drawPosition - Vector2.UnitY * 28f,
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
            drawPosition - Vector2.UnitY * 28f,
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
                    // I'll kill you Sucs
                    SoundEngine.PlaySound(Assets.Sounds.Cursehound.CursehoundStep1.Asset with { Pitch = 0.0f, PitchVariance = 0.4f }, NPC.Center);
                    for(int i = 0; i < Main.rand.NextFloat(3f,5f); i++) {
                        var dustPos = NPC.Bottom + Main.rand.NextVector2Circular(120f, 20f)
                            + Vector2.UnitY * 1.5f;
                        var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(4f, 8f)
                            + NPC.Bottom.DirectionTo(dustPos) * 5f;

                        var dustColorStart = new Color(133, 122, 94);
                        var dustColorFade = dustColorStart * 0.4f;

                        var newDustData = new Smoke.Data()
                        {
                            InitialLifetime = 40,
                            ElapsedFrames = 0,
                            InitialOpacity = Main.rand.NextFloat(0.1f, 0.5f),
                            ColorStart = dustColorStart,
                            ColorFade = dustColorFade,
                            Spin = 0.01f,
                            InitialScale = Main.rand.NextFloat(0.25f, 1f)
                        };

                        var newDust = Dust.NewDustPerfect(
                            dustPos,
                            ModContent.DustType<Smoke>(),
                            dustVelocity,
                            0,
                            newColor: Color.White,
                            newDustData.InitialScale
                        );

                        newDust.customData = newDustData;
                    }
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

internal sealed class LavaTelegraphProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.LavaTelegraph.KEY;

    public ref float MaxTime => ref Projectile.ai[0];
    public ref float Damage => ref Projectile.ai[1];

    private bool hasSpawnedProjectile;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
    }

    public override void AI() {
        if(MaxTime == 0) {
            MaxTime = Projectile.timeLeft;
        }

        float progress = 1f - (Projectile.timeLeft / MaxTime);
        float lightAlpha = MathHelper.Clamp((float)Math.Sin(progress * MathHelper.Pi), 0f, 1f);
        Lighting.AddLight(Projectile.Center, new Color(214, 237, 5).ToVector3() * lightAlpha * 0.8f);

        float spawnThreshold = 0.7f;
        if(progress >= spawnThreshold && !hasSpawnedProjectile) {
            hasSpawnedProjectile = true;

            if(Main.myPlayer == Projectile.owner) {
                Vector2 upwardVelocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-16f, -12f));

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Bottom,
                    upwardVelocity,
                    ModContent.ProjectileType<SpiritFireball>(),
                    (int)Damage,
                    0f,
                    Main.myPlayer
                );
            }

            var waterShaderData = Filters.Scene["WaterDistortion"]?.GetShader() as WaterShaderData;
            waterShaderData?.QueueRipple(Projectile.Bottom, 10f, RippleShape.Circle, MathHelper.PiOver4);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        float progress = 1f - (Projectile.timeLeft / MaxTime);
        float maxTelegraphHeight = 150f;

        float growthProgress = MathHelper.Clamp(progress / 0.3f, 0f, 1f);
        float currentHeight = MathHelper.SmoothStep(0f, maxTelegraphHeight, growthProgress);

        float scaleX = progress < 0.3f ? MathHelper.Lerp(0.2f, 1.3f, progress / 0.3f) :
            MathHelper.Lerp(1.3f, 0.8f, (progress - 0.3f) / 0.7f);

        float alpha;
        if(progress < 0.2f) {
            alpha = progress / 0.2f;
        }
        else if(progress > 0.6f) {
            alpha = 1f - ((progress - 0.6f) / 0.4f);
        }
        else {
            alpha = 1f;
        }

        Vector2 drawPos = Projectile.Bottom - Main.screenPosition;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height);

        Color drawColor = new Color(214, 237, 5) * alpha * 0.85f;
        drawColor.A = 0;

        Vector2 scale = new Vector2(scaleX, currentHeight / texture.Height);

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            null,
            drawColor,
            0f,
            origin,
            scale,
            SpriteEffects.None
        );

        return false;
    }
}