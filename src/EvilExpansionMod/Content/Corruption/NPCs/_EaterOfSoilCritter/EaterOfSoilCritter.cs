using EvilExpansionMod.Content.Crimson;
using EvilExpansionMod.Content.Items.Food;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;


namespace EvilExpansionMod.Content.Corruption;

internal class EaterOfSoilHead : WormHead {
    public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilHead.KEY;
    public override int BodyType => ModContent.NPCType<EaterOfSoilBody>();
    public override int TailType => ModContent.NPCType<EaterOfSoilTail>();

    public override bool HasCustomBodySegments => true;

    public override int SpawnBodySegments(int segmentCount) {
        var source = NPC.GetSource_FromAI();
        int latestNPC = NPC.whoAmI;
        for(int i = 0; i < segmentCount; i++) {
            int type = i == 1 ? ModContent.NPCType<EaterOfSoilBody>() : ModContent.NPCType<EaterOfSoilBody2>();
            latestNPC = SpawnSegment(source, type, latestNPC);
        }
        return latestNPC;
    }

    protected override bool IsWormSegment(int type) {
        return base.IsWormSegment(type) || type == ModContent.NPCType<EaterOfSoilBody2>();
    }

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            CustomTexturePath = Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoil_Bestiary.KEY,
            Position = new Vector2(0f, 12f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 12f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        Main.npcCatchable[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = false;
    }

    public override void SetDefaults() {
        NPC.width = 8;
        NPC.height = 8;
        NPC.damage = 0;
        NPC.aiStyle = -1;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 0.8f };
        NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.behindTiles = true;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;

        NPC.rarity = 1;
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        if(spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>()) {
            if(spawnInfo.SpawnTileType == TileID.ObsidianBrick ||
                spawnInfo.SpawnTileType == TileID.HellstoneBrick ||
                spawnInfo.SpawnTileType == ModContent.TileType<CrimsonAshGrass>() ||
                spawnInfo.SpawnTileType == ModContent.TileType<CorruptAshGrass>() ||
                spawnInfo.PlayerInTown ||
                spawnInfo.PlayerSafe) {
                return 0.6f;
            }
        }
        return 0f;
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.EaterOfSoilCritterBestiary.KEY),
        ]);
    }

    public override void Init() {
        MinSegmentLength = 5;
        MaxSegmentLength = 5;

        CommonWormInit(this);
    }

    public override void OnSpawn(IEntitySource source) {
        NPC.friendly = false;
        NPC.catchableNPCOriginallyFriendly = false;
        NPC.catchableNPCTempImmunityCounter = 0;
    }

    internal static void CommonWormInit(Worm worm) {
        worm.MoveSpeed = 5.5f;
        worm.Acceleration = 0.045f;
    }

    public override void Load() {
        for(int j = 0; j <= 3; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/EaterOfSoilGore" + j);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 0; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("EaterOfSoilGore" + i).Type);
        }
    }
}

    internal class EaterOfSoilBody : WormBody {
        public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilBody1.KEY;

    public override void SetStaticDefaults() {
        NPCID.Sets.NPCBestiaryDrawModifiers value = new()
        {
            Hide = true,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<EaterOfSoilHead>();
        Main.npcCatchable[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
    }

    public override void SetDefaults() {
        NPC.width = 8;
        NPC.height = 8;
        NPC.damage = 0;
        NPC.aiStyle = -1;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 0.8f };
        NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.behindTiles = true;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 0f;
        NPC.friendly = false;
    }

    public override void Init() {
        EaterOfSoilHead.CommonWormInit(this);
        NPC.altTexture = Main.npc[(int)NPC.ai[1]].type == ModContent.NPCType<EaterOfSoilHead>() ? (byte)0 : (byte)1;
    }

    public override void OnSpawn(IEntitySource source) {
        NPC.friendly = false;
        NPC.catchableNPCOriginallyFriendly = false;
        NPC.catchableNPCTempImmunityCounter = 0;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 0; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("EaterOfSoilGore" + i).Type);
        }
    }
}

internal class EaterOfSoilBody2 : EaterOfSoilBody {
    public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilBody2.KEY;
}

internal class EaterOfSoilTail : WormTail {
    public override string Texture => Assets.Images.Corruption.NPCs.EaterOfSoil.EaterOfSoilTail.KEY;
    
    public override void SetStaticDefaults() {
        NPCID.Sets.NPCBestiaryDrawModifiers value = new()
        {
            Hide = true,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        NPCID.Sets.RespawnEnemyID[Type] = ModContent.NPCType<EaterOfSoilHead>();
        Main.npcCatchable[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
    }

    public override void SetDefaults() {
        NPC.width = 8;
        NPC.height = 8;
        NPC.damage = 0;
        NPC.aiStyle = -1;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1 with { Volume = 0.8f };
        NPC.catchItem = ModContent.ItemType<EaterOfSoilItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.behindTiles = true;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 0f;
        NPC.friendly = false;
    }

    public override void Init() {
        EaterOfSoilHead.CommonWormInit(this);
    }

    public override void OnSpawn(IEntitySource source) {
        NPC.friendly = false;
        NPC.catchableNPCOriginallyFriendly = false;
        NPC.catchableNPCTempImmunityCounter = 0;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 0; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("EaterOfSoilGore" + i).Type);
        }
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
        ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;
    }
    public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
        // Pick a random item type to reward
        if(Main.rand.NextBool(5)) {
            // 20% chance to yield Meatloaf
            resultType = ModContent.ItemType<EvilMeatloaf>();
            resultStack = 1;
        }
        else {
            if(Main.rand.NextBool(2)) {
                resultType = ModContent.ItemType<RawShadowScalesItem>();
                resultStack = Main.rand.Next(1, 6);
            }
            else {
                resultType = ItemID.RottenChunk;
                resultStack = Main.rand.Next(1, 6);
            }
        }
    }
}
