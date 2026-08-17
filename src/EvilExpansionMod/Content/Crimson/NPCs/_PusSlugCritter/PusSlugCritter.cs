using EvilExpansionMod.Content.Biomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public sealed class PusSlugCritter : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.PusSlugNPC.KEY;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 3;
        Main.npcCatchable[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
    }

    public override void SetDefaults() {
        NPC.width = 1;
        NPC.height = 4;
        NPC.lifeMax = 15;
        NPC.damage = 0;
        NPC.aiStyle = NPCAIStyleID.Snail;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.gravity = 0.1f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.catchItem = ModContent.ItemType<PusSlugItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.lavaImmune = true;
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;
        if(NPC.frameCounter >= 8) {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if(NPC.frame.Y >= Main.npcFrameCount[NPC.type] * frameHeight) {
                NPC.frame.Y = 0;
            }
        }
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.6f : 0f;
    }
}

public class PusSlugItem : ModItem {
    public override string Texture => Assets.Images.Crimson.NPCs.PusSlugItem.KEY;

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
        Item.value = Item.buyPrice(0, 0, 40, 0);
        Item.bait = 15;
        Item.makeNPC = (short)ModContent.NPCType<PusSlugCritter>();
        Item.rare = ItemRarityID.Green;
    }
}