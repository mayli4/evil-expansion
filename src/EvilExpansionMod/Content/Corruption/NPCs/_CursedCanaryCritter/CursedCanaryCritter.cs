using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class CursedCanaryCritter : ModNPC {
    public override string Texture => Assets.Images.Corruption.NPCs.CursedCanaryCritter.KEY;

    int State { get => (int)NPC.ai[1]; set => NPC.ai[1] = value; }
    float Timer { get => NPC.ai[2]; set => NPC.ai[2] = value; }

    Player Target => Main.player[NPC.target];

    bool ValidTarget => Target != null && Target.active;

    private int _dustTimer = 0;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 17;
        Main.npcCatchable[Type] = true;

        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
        NPCID.Sets.TownCritter[Type] = true;
    }

    public override void SetDefaults() {
        NPC.width = 12;
        NPC.height = 10;
        NPC.damage = 0;
        NPC.aiStyle = NPCAIStyleID.Bird;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.gravity = 0.1f;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath6 with { Volume = 0.8f };
        NPC.catchItem = ModContent.ItemType<CursedCanaryItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.2f : 0f;
    }

    public override void PostAI() {
        NPC.TargetClosest(false);

        var range = 340;
        var targetInRange = ValidTarget && Target.Center.DistanceSQ(NPC.Center) < (range * range);

        Lighting.AddLight(NPC.Center, 0.77f, 1f, 0.56f);

        _dustTimer++;
        if(_dustTimer >= Main.rand.NextFloat(10, 100)) // Spawns every x ticks
        {
            if(Main.netMode != NetmodeID.Server) {
                int dustIndex = Dust.NewDust(
                    NPC.position,
                    NPC.width,
                    NPC.height,
                    DustID.CursedTorch,
                    0f, 0f,
                    100,
                    default,
                    Main.rand.NextFloat(0.5f, 1.5f)
                );
                Main.dust[dustIndex].noGravity = false;
            }
            _dustTimer = 0;
        }
        switch(State) {
            case 0:
            case 1:
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPC.velocity.X = 0f;
                if(NPC.collideY) {
                    NPC.velocity.Y = 0f;
                }
                else {
                    NPC.velocity.Y += NPC.gravity;
                }

                if(targetInRange) {
                    State = 2;
                    Timer = 0;

                    NPC.netUpdate = true;
                    NPC.velocity.Y -= 2f;
                    break;
                }

                if(Timer <= 0f) {
                    Timer = Main.rand.Next(60, 180);
                    State = Main.rand.Next(2);

                    NPC.direction = Main.rand.NextBool() ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;

                    NPC.netUpdate = true;
                }
                else {
                    Timer -= 1f;
                }
                break;

            case 2:
                NPC.noGravity = false;
                NPC.noTileCollide = false;

                var hasLavaBelow = false;
                var i1 = (int)(NPC.Center.X / 16f);
                var jOffset1 = (int)(NPC.Center.Y / 16f);
                for(var j = 0; !hasLavaBelow && j < 8; j++) {
                    if(j + jOffset1 < Main.maxTilesY && Main.tile[i1, j + jOffset1].LiquidAmount >= 1) hasLavaBelow = true;
                }

                if(!targetInRange && !hasLavaBelow) {
                    if(NPC.collideY) {
                        State = Main.rand.Next(2);
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                        Timer = 0;
                    }
                    break;
                }

                if(NPC.collideX) {
                    NPC.velocity.X = -NPC.velocity.X;
                    NPC.direction *= -1;
                }

                var hasTilesBelow = false;
                var i = (int)(NPC.Center.X / 16f);
                var jOffset = (int)(NPC.Center.Y / 16f);
                for(var j = 0; !hasTilesBelow && j < 8; j++) {
                    if(j + jOffset < Main.maxTilesY && Main.tile[i, j + jOffset].HasTile || Main.tile[i, j + jOffset].LiquidAmount >= 1) hasTilesBelow = true;
                }

                if(hasTilesBelow) NPC.velocity.Y -= Main.rand.NextFloat() * 1.15f;

                var flyDirection = ValidTarget ? Math.Sign(NPC.Center.X - Target.Center.X) : NPC.spriteDirection;
                NPC.velocity.X += flyDirection * 0.1f;

                NPC.spriteDirection = flyDirection;
                NPC.velocity *= 0.998f;
                break;
        }
    }

    public override void OnKill() {
        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            NPC.Center,
            Vector2.Zero,
            ModContent.ProjectileType<SpiritContactExplosion>(),
            80,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
            );
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, NPC.Center);
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter += 0.2f;
        switch(State) {
            case 0:
                NPC.frameCounter = Math.Max(NPC.frameCounter % 5, 5);
                break;
            case 1:
                NPC.frameCounter = Math.Max(5, NPC.frameCounter % 13);
                break;
            case 2:
                NPC.frameCounter = Math.Max(13, NPC.frameCounter % 17);
                break;
        }

        var texture = TextureAssets.Npc[Type].Value;
        var cellHeight = texture.Height / Main.npcFrameCount[Type];
        NPC.frame = new(0, (int)NPC.frameCounter * cellHeight, texture.Width, cellHeight);
    }
}

public sealed class CursedCanaryItem : ModItem {
    public override string Texture => Assets.Images.Corruption.NPCs.CursedCanaryItem.KEY;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 16;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.noUseGraphic = true;
        Item.value = Item.buyPrice(0, 0, 40);
        Item.makeNPC = (short)ModContent.NPCType<CursedCanaryCritter>();
        Item.rare = ItemRarityID.Green;
    }
}