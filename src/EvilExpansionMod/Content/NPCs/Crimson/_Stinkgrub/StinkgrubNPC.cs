using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public sealed class StinkgrubNPC : ModNPC {
    public enum State {
        Idle,
        Moving
    }
    
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_StinkgrubNPC;

    public State CurrentState => Unsafe.BitCast<float, State>(NPC.ai[0]);
    
    void ChangeState(State state) {
        NPC.ai[0] = Unsafe.BitCast<State, float>(state);
        StateTimer = 0;
        NPC.netUpdate = true;
    }

    private ref float StateTimer => ref NPC.ai[1];
    private ref float GasTimer => ref NPC.ai[2];
    private ref float PusBottleNPCID => ref NPC.ai[3];

    public bool IsPusCarrier => PusBottleNPCID >= 0;

    private Player Target => Main.player[NPC.target];
    
    private const int anim_speed = 7;
    private const int gas_interval = 60;
    
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 6;
    }

    public override void SetDefaults() {
        NPC.width = 32;
        NPC.height = 20;
        NPC.lifeMax = 80;
        NPC.value = 100f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0.2f;
        NPC.friendly = false;
        NPC.damage = 15;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.buffImmune[BuffID.Poisoned] = true;
        NPC.lavaImmune = true;
    }

    public override void OnSpawn(IEntitySource source) {
        if (Main.rand.NextBool(1)) {
            int npcIndex = NPC.NewNPC(
                NPC.GetSource_FromThis(),
                (int)NPC.Center.X,
                (int)NPC.Center.Y - NPC.height / 2,
                ModContent.NPCType<PusBottleNPC>()
            );

            if (npcIndex != -1 && Main.npc[npcIndex].active) {
                PusBottleNPCID = Main.npc[npcIndex].whoAmI;

                Main.npc[npcIndex].ai[0] = NPC.whoAmI;
                Main.npc[npcIndex].netUpdate = true;
            }
            else {
                PusBottleNPCID = -1;
            }
        }
        else {
            PusBottleNPCID = -1;
        }
    }

    public override void AI() {
        NPC.TargetClosest();
        if (Target.dead || !Target.active) {
            NPC.velocity.Y += 0.1f;
            if (NPC.timeLeft > 10) NPC.timeLeft = 10;
            return;
        }

        if (IsPusCarrier) {
            NPC pusBottleNPC;
            if (PusBottleNPCID >= 0 && PusBottleNPCID < Main.maxNPCs) {
                pusBottleNPC = Main.npc[(int)PusBottleNPCID];
                if (!pusBottleNPC.active || pusBottleNPC.type != ModContent.NPCType<PusBottleNPC>() || (int)pusBottleNPC.ai[0] != NPC.whoAmI) {
                    PusBottleNPCID = -1;
                    NPC.lifeMax = 80;
                    NPC.life = Math.Min(NPC.life, NPC.lifeMax);
                    NPC.scale = 1f;
                    NPC.netUpdate = true;
                }
            }
            else {
                PusBottleNPCID = -1;
            }
        }

        NPC.direction = (Target.Center.X < NPC.Center.X) ? -1 : 1;
        NPC.spriteDirection = NPC.direction;

        NPC.velocity.Y += 0.2f;
        if (NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;

        if (CurrentState == State.Idle) {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, 0.1f);
            if (NPC.collideY) NPC.velocity.Y = 0;

            StateTimer++;
            if (StateTimer >= Main.rand.Next(60, 120)) {
                ChangeState(State.Moving);
            }
        }
        else if (CurrentState == State.Moving) {
            float moveSpeed = 0.8f;
            float acceleration = 0.05f;

            if (Math.Abs(NPC.Center.X - Target.Center.X) > NPC.width * 3) {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * moveSpeed, acceleration);
            }
            else {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * Main.rand.NextFloat(-0.5f, 0.5f), acceleration);
            }

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

            StateTimer++;
            if (StateTimer >= Main.rand.Next(60 * 3, 60 * 5)) {
                ChangeState(State.Idle);
            }
        }

        GasTimer++;
        if (GasTimer >= gas_interval) {
            GasTimer = 0;
        }
    }
    
    public override void OnKill() {
        if (IsPusCarrier) {
            NPC pusBottleNPC;
            if (PusBottleNPCID >= 0 && PusBottleNPCID < Main.maxNPCs) {
                pusBottleNPC = Main.npc[(int)PusBottleNPCID];
                if (pusBottleNPC.active && pusBottleNPC.type == ModContent.NPCType<PusBottleNPC>() && (int)pusBottleNPC.ai[0] == NPC.whoAmI) {
                    pusBottleNPC.ai[1] = 1;
                    pusBottleNPC.netUpdate = true;
                }
            }
        }
    }
    
    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        for(var i = 0; i < 3; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"StinkgrubGore{i}").Type
        );
    }
    
    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;

        if (CurrentState == State.Moving) {
            if (NPC.frameCounter < anim_speed) {
                NPC.frame.Y = 0 * frameHeight;
            }
            else if (NPC.frameCounter < anim_speed * 2) {
                NPC.frame.Y = 1 * frameHeight;
            }
            else if (NPC.frameCounter < anim_speed * 3) {
                NPC.frame.Y = 2 * frameHeight;
            }
            else if (NPC.frameCounter < anim_speed * 4) {
                NPC.frame.Y = 3 * frameHeight;
            }
            else if (NPC.frameCounter < anim_speed * 5) {
                NPC.frame.Y = 4 * frameHeight;
            }
            else {
                NPC.frameCounter = 0;
            }
        }
        else {
            NPC.frame.Y = 5 * frameHeight;
        }
    }
}