using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public sealed class PusSlugCritter : ModNPC {
    float difficultyScaler = Main.expertMode ? 2f : 1f;
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
        NPC.DeathSound = SoundID.NPCDeath64;
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
    public override void OnKill() {
        var amount = Main.rand.Next(3, 6) * difficultyScaler;

        for(int i = 0; i < amount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed;

            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(5, 30),
                velocity * Main.rand.NextFloat(0.75f, 1.25f),
                ModContent.ProjectileType<PusGlob>(),
                50 / (int)difficultyScaler,
                0.5f,
                Main.myPlayer
            );
        }
        for(int i = 0; i < Main.rand.NextFloat(1f, 3f); i++) {
            Dust.NewDustPerfect(
                NPC.Center + Main.rand.NextVector2Circular(20f, 20f),
                ModContent.DustType<PusGas>(),
                Vector2.Zero,
                100,
                new Color(98, 90, 40)
            );
            SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.7f, Pitch = Main.rand.NextFloat(0.0f, 0.2f) }, NPC.Center);
        }
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