using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.NPCs.EaterOfSoil;

namespace EvilExpansionMod.Content.Corruption;

    // These three class showcase usage of the WormHead, WormBody and WormTail classes from Worm.cs
    internal class EaterOfSoilHead : WormHead {
        public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilHead.KEY;
        public override int BodyType => ModContent.NPCType<EaterOfSoilBody>();

        public override int TailType => ModContent.NPCType<EaterOfSoilTail>();

        public override void SetStaticDefaults() {
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            { // Influences how the NPC looks in the Bestiary
                CustomTexturePath = Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoil_Bestiary.KEY, // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void SetDefaults() {
            NPC.width = 12;
            NPC.height = 10;
            NPC.damage = 0;
            NPC.aiStyle = -1;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.gravity = 0.1f;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6 with { Volume = 0.8f };
            NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

            SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.lavaImmune = true;

            NPC.noTileCollide = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				//BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                //BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.EaterOfSoilCritterBestiary.KEY),
            ]);
        }

        public override void Init() {
            // Set the segment variance
            // If you want the segment length to be constant, set these two properties to the same value
            MinSegmentLength = 4;
            MaxSegmentLength = 4;

            CommonWormInit(this);
        }

        // This method is invoked from EaterOfSoilHead, EaterOfSoilBody and EaterOfSoilTail
        internal static void CommonWormInit(Worm worm) {
            // These two properties handle the movement of the worm
            worm.MoveSpeed = 5.5f;
            worm.Acceleration = 0.045f;
        }

        private int attackCounter;
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(attackCounter);
        }

        public override void ReceiveExtraAI(BinaryReader reader) {
            attackCounter = reader.ReadInt32();
        }

        public override void AI() {
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                if(attackCounter > 0) {
                    attackCounter--; // tick down the attack counter.
                }

                Player target = Main.player[NPC.target];
                // If the attack counter is 0, this NPC is less than 12.5 tiles away from its target, and has a path to the target unobstructed by blocks, summon a projectile.
                if(attackCounter <= 0 && Vector2.Distance(NPC.Center, target.Center) < 200 && Collision.CanHit(NPC.Center, 1, 1, target.Center, 1, 1)) {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    direction = direction.RotatedByRandom(MathHelper.ToRadians(10));

                    int projectile = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, direction * 1, ProjectileID.ShadowBeamHostile, 5, 0, Main.myPlayer);
                    Main.projectile[projectile].timeLeft = 300;
                    attackCounter = 500;
                    NPC.netUpdate = true;
                }
            }
        }
    }

    internal class EaterOfSoilBody : WormBody {
        public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilBody1.KEY;
        public override void SetStaticDefaults() {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<EaterOfSoilHead>();
        }

        public override void SetDefaults() {
            NPC.width = 12;
            NPC.height = 10;
            NPC.damage = 0;
            NPC.aiStyle = -1;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.gravity = 0.1f;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6 with { Volume = 0.8f };
            NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

            SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.lavaImmune = true;
        }
        public override void Init() {
            EaterOfSoilHead.CommonWormInit(this);
        }
    }

    internal class EaterOfSoilTail : WormTail {
        public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilTail.KEY;
        public override void SetStaticDefaults() {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<EaterOfSoilHead>();
        }

        public override void SetDefaults() {
            NPC.width = 12;
            NPC.height = 10;
            NPC.damage = 0;
            NPC.aiStyle = -1;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.gravity = 0.1f;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6 with { Volume = 0.8f };
            NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

            SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.lavaImmune = true;
        }

        public override void Init() {
            EaterOfSoilHead.CommonWormInit(this);
        }
    }


public sealed class EaterOfSoilItem : ModItem {
    public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilItem.KEY;

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
        Item.bait = 30;
        Item.makeNPC = (short)ModContent.NPCType<EaterOfSoilHead>();
        Item.rare = ItemRarityID.Green;
    }
}