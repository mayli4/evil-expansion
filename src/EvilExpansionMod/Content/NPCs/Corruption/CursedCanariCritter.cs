using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

public sealed class CursedCanariCritter : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.KEY_CursedCanariCritter;

    int State { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
    Player Target => Main.player[NPC.target];
    
    bool ValidTarget => Target != null && Target.active;

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
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.catchItem = ModContent.ItemType<CursedCanariItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.lavaImmune = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.2f : 0f;
    }

    public override void PostAI() {
        NPC.TargetClosest(false);

        var range = 440;
        var targetInRange = ValidTarget && Target.Center.DistanceSQ(NPC.Center) < (range * range);

        switch(State) {
            case 0:
            case 1:
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPC.velocity.X = 0f;
                if (NPC.collideY) {
                    NPC.velocity.Y = 0f;
                } else {
                    NPC.velocity.Y += NPC.gravity;
                }

                if(targetInRange) {
                    State = 2;
                    NPC.netUpdate = true;
                    NPC.velocity.Y -= 0.4f;
                    NPC.ai[1] = 0f;
                    break;
                }

                if(NPC.ai[1] <= 0f) {
                    NPC.ai[1] = Main.rand.NextFloat(60, 180);
                    State = Main.rand.Next(2);
                    NPC.netUpdate = true;
                } else {
                    NPC.ai[1] -= 1f;
                }
                break;

            case 2:
                NPC.noGravity = false;
                NPC.noTileCollide = false;

                var hasLavaBelow = false;
                var i1 = (int)(NPC.Center.X / 16f);
                var jOffset1 = (int)(NPC.Center.Y / 16f);
                for(var j = 0; !hasLavaBelow && j < 8; j++) {
                    if(j + jOffset1 < Main.maxTilesY &&  Main.tile[i1, j + jOffset1].LiquidAmount >= 1) hasLavaBelow = true;
                }

                if(!targetInRange && !hasLavaBelow) {
                    if(NPC.collideY) {
                        State = Main.rand.Next(2);
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                        NPC.ai[1] = 0f;
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

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter += 0.2f;
        switch(State) {
            case 0:
                NPC.frameCounter %= 5;
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

public sealed class CursedCanariItem : ModItem {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.KEY_CursedCanariItem;

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
        Item.makeNPC = (short)ModContent.NPCType<CursedCanariCritter>();
        Item.rare = ItemRarityID.Green;
    }
}