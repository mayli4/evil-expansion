using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Core.LocalizationReferences.Mods.EvilExpansionMod.NPCs;

namespace EvilExpansionMod.Content.Crimson;

public class CultistEye : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistEye.KEY;
    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;
    private int _dustTimer = 0;
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
        NPC.knockBackResist = 0.5f;
        NPC.friendly = false;
        NPC.damage = 45;

        NPC.HitSound = SoundID.NPCHit1;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        // Banner = NPC.type;
        // BannerItem = ModContent.ItemType<CursedSpiritBannerItem>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.CultistEyeBestiary.KEY),
        });
    }
    public bool EyeAltCostume;
    public override void OnSpawn(IEntitySource source) {
        if(Main.rand.NextBool(1, 2)) { // 50/50
            EyeAltCostume = true;
        }
        else {
            return;
        }
    }

    public override void AI() {
        NPC.TargetClosest();

        var target = Main.player[NPC.target];
        if(target == null) return;

        var moveDirection = NPC.Center.DirectionTo(target.Center);
        NPC.velocity += moveDirection * 0.25f * DifficultyScaler;
        NPC.velocity += 1f * Main.rand.NextFloatDirection()
            * MathF.Sin(NPC.whoAmI * 0.3f + Main.GameUpdateCount * 0.1f)
            * moveDirection.RotatedBy(MathF.PI / 2f);

        NPC.velocity *= 0.93f;
        NPC.rotation = NPC.velocity.ToRotation() + MathF.PI;

        if(Main.rand.NextBool(6)) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
        }

        _dustTimer++;
        if(_dustTimer >= Main.rand.NextFloat(4, 50)) // Spawns every x ticks
        {
            if(Main.netMode != NetmodeID.Server) {
                int dustIndex = Dust.NewDust(
                    NPC.position,
                    NPC.width,
                    NPC.height,
                    DustID.Shadowflame,
                    0f, 0f,
                    100,
                    default,
                    Main.rand.NextFloat(0.5f, 1.5f)
                );
                Main.dust[dustIndex].noGravity = false;
            }
            _dustTimer = 0;
        }
    }
    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        for(var i = 0; i < 3; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"CultistEyeGore{i}").Type
        );
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;

        var position = NPC.Center;
        var origin = new Vector2(18, 14);

        spriteBatch.Draw(
            texture,
            position - screenPos,
            NPC.frame,
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
    public override void FindFrame(int frameHeight) {
        if(EyeAltCostume) {
            NPC.frame = new(42, (int)NPC.frameCounter * frameHeight, 40, frameHeight);
            NPC.frameCounter = (NPC.frameCounter + 0.2f) % 3;
        }
        else {
            NPC.frame = new(0, (int)NPC.frameCounter * frameHeight, 40, frameHeight);
            NPC.frameCounter = (NPC.frameCounter + 0.2f) % 3;
        }
    }
}