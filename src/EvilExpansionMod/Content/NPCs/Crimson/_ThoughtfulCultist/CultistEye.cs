using EvilExpansionMod.Common.Bestiary;
using EvilExpansionMod.Content.Biomes;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;
public class CultistEye : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_CultistEye;
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 3;
    }

    public override void SetDefaults() {
        NPC.width = 22;
        NPC.height = 22;
        NPC.lifeMax = 100;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        // Banner = NPC.type;
        // BannerItem = ModContent.ItemType<CursedSpiritBannerItem>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");

    public override void AI() {
        NPC.TargetClosest();

        var target = Main.player[NPC.target];
        if(target == null) return;

        var moveDirection = NPC.Center.DirectionTo(target.Center);
        NPC.velocity += moveDirection * 0.25f;
        NPC.velocity += 0.75f * Main.rand.NextFloatDirection()
            * MathF.Sin(NPC.whoAmI * 0.3f + Main.GameUpdateCount * 0.1f)
            * moveDirection.RotatedBy(MathF.PI / 2f);

        NPC.velocity *= 0.93f;
        NPC.rotation = NPC.velocity.ToRotation() + MathF.PI;

        if(Main.rand.NextBool(6)) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.frame = new(0, (int)NPC.frameCounter * frameHeight, 40, frameHeight);
        NPC.frameCounter = (NPC.frameCounter + 0.2f) % 3;
    }
}