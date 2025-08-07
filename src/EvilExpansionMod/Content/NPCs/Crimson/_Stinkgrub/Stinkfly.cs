using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

//todo rewrite their ai, very simple but make them move very erratically

public class StinkflyItem : ModItem {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_BigFlyItem;
    
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
        Item.bait = 40;
        Item.makeNPC = (short)ModContent.NPCType<StinkflyNPC>();
        Item.rare = ItemRarityID.Green;
    }
}

public class StinkflyNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_BigFly;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 2;
    }

    public override void SetDefaults() {
        NPC.width = 14;
        NPC.height = 14;
        NPC.lifeMax = 15;
        NPC.value = 0f;
        NPC.noTileCollide = false;
        NPC.noGravity = true;
        
        NPCID.Sets.CountsAsCritter[NPC.type] = true;
        Main.npcCatchable[NPC.type] = true;
        NPC.catchItem = (short)ModContent.ItemType<StinkflyItem>();

        NPC.aiStyle = NPCAIStyleID.Butterfly;
    }

    public override void PostAI() {
        NPC closestGrub = null;
        float closestDistSq = float.MaxValue;
        float detectionRadius = 20 * 15;
        float detectionRadiusSq = detectionRadius * detectionRadius;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC grub = Main.npc[i];
            if (grub.active && grub.type == ModContent.NPCType<StinkgrubNPC>()) {
                float distSq = NPC.DistanceSQ(grub.Center);
                if (distSq < closestDistSq && distSq < detectionRadiusSq)
                {
                    closestDistSq = distSq;
                    closestGrub = grub;
                }
            }
        }

        if (closestGrub != null) {
            float hoverHeight = -30f;
            float sideOffset = 20f;

            var baseHoverPoint = closestGrub.Center + new Vector2(closestGrub.direction * sideOffset, hoverHeight);

            float wobbleSpeed = 0.05f;
            float wobbleAmount = 5f;
            var wobble = new Vector2(
                (float)Math.Sin(Main.GameUpdateCount * wobbleSpeed + NPC.whoAmI * 0.1f) * wobbleAmount,
                (float)Math.Cos(Main.GameUpdateCount * wobbleSpeed * 0.7f + NPC.whoAmI * 0.2f) * (wobbleAmount * 0.5f)
            );

            var desiredPosition = baseHoverPoint + wobble;

            var steerStrength = 0.02f;
            var maxInfluenceSpeed = 2f;

            var vectorToDesired = desiredPosition - NPC.Center;
            float distanceToDesired = vectorToDesired.Length();

            if (distanceToDesired > 10f) {
                NPC.velocity = Vector2.Lerp(NPC.velocity, vectorToDesired.SafeNormalize(Vector2.Zero) * maxInfluenceSpeed, steerStrength);
            }
            else {
                NPC.velocity *= 10.98f; 
            }
        }
    }
    
    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;
        if (NPC.frameCounter >= 6)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y >= Main.npcFrameCount[NPC.type] * frameHeight)
            {
                NPC.frame.Y = 0;
            }
        }
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.6f : 0f;
    }
}

public class SmallStinkflyNpc : StinkflyNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_SmallFly;
}