using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Core.LocalizationReferences.Mods.EvilExpansionMod.NPCs;

namespace EvilExpansionMod.Content.Crimson;

public class CultistEye : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistEye.KEY;
    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 1.2f : 1.1f) : 1f;
    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Position = new Vector2(0f, 4f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 2f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        Main.npcFrameCount[Type] = 3;

        NPCID.Sets.TrailCacheLength[Type] = 8;
        NPCID.Sets.TrailingMode[Type] = 0;
        //NPCID.Sets.DontDoHardmodeScaling[NPC.type] = true;
        // Doesn't work??????????????? lol
    }

    public override void SetDefaults() {
        NPC.width = 22;
        NPC.height = 22;
        NPC.lifeMax = 250;
        NPC.defense = 25;
        NPC.value = 1f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 2f;
        NPC.friendly = false;
        NPC.damage = 45;
        if(!NPC.IsABestiaryIconDummy) NPC.alpha = 255;
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
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.CultistEyeBestiary.KEY),
        });
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
        if(Main.expertMode) {
            NPC.lifeMax = 350 / 2;
        }
        if(Main.masterMode) {
            NPC.lifeMax = 450 / 3;
        }
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

    int DashCooldownTimer = 60 * Main.rand.Next(4, 8);
    int DashCooldown = (int)(60 * (Main.expertMode ? 4 : 8) * Main.rand.NextFloat(0.8f, 1.2f));
    int DashWindupTimer = 0;
    int DashWindupMax = 60;
    int DashTimer = 15;
    float DashMultiplier => ((float)(DashWindupMax - DashWindupTimer) / DashWindupMax);
    private Vector2 dashDirection;

    public override void AI() {
        Main.NewText("DashMultiplier" + DashMultiplier, Microsoft.Xna.Framework.Color.White);
        Main.NewText("DashWindupTimer" + DashWindupTimer, Microsoft.Xna.Framework.Color.White);
        Main.NewText("DashWindupMax" + DashWindupMax, Microsoft.Xna.Framework.Color.White);
        Main.NewText("DashCooldownTimer" + DashCooldownTimer, Microsoft.Xna.Framework.Color.White);
        Main.NewText("NPC.alpha" + NPC.alpha, Microsoft.Xna.Framework.Color.White);
        NPC.TargetClosest();

        NPC.Opacity += 0.05f;

        var target = Main.player[NPC.target];
        if(target == null) return;

        float distanceToTarget = NPC.Distance(target.Center);
        var moveDirection = NPC.Center.DirectionTo(target.Center);

        //this is really wack and i did this on a whim but it seemed easiest to do it like this so here's my explanation and thoughtprocess to any unlucky future programmer or future tobias lol;
        if(Main.rand.NextBool(20) && (DashWindupTimer <= 0) && DashCooldownTimer == 0 && distanceToTarget < 16 * 20) { // Dash conditions
            DashWindupTimer = (int)(60 * 0.5);
            DashWindupMax = DashWindupTimer;
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.7f } with { PitchRange = (0f, 1f) });
        }
        else { // Typical movement behaviour
            NPC.velocity += moveDirection * 0.2f * DifficultyScaler * (1 + distanceToTarget / 16 / 16);

            NPC.velocity = Vector2.Lerp(NPC.velocity, moveDirection, 0.02f);

            NPC.velocity *= 0.93f;
            NPC.rotation = NPC.velocity.ToRotation() + MathF.PI;
        }
            
        if(DashWindupTimer != 0) { //windup timer starts
            DashWindupTimer--;
            NPC.velocity -= moveDirection * 1.3f;
            NPC.velocity += 1f * MathF.Sin(NPC.whoAmI * 0.3f + Main.GameUpdateCount * 0.1f) * moveDirection.RotatedBy(MathF.PI / 2f);

            NPC.velocity *= (float)(1 - 0.1 * DashMultiplier); // Slows the eye down as the windup progresses

            //NPC.rotation = NPC.Center.DirectionTo(target.Center).ToRotation() - MathHelper.Pi; // Faces the player

            Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Shadowflame,
                0f, 0f,
                100,
                default,
                Main.rand.NextFloat(0.5f, 1.5f) * DashMultiplier
            );
        }

        if(DashWindupTimer == 1) { // When DashWindupTimer = 1
            dashDirection = moveDirection;
            DashTimer = 25;
            DashCooldownTimer = DashCooldown; // Set CooldownTimer here; number of ticks before another dash can be performed
            SoundEngine.PlaySound(SoundID.Item131 with { MaxInstances = 10, PitchRange = (-0.25f, 0.25f), PitchVariance = 0.2f }, NPC.position);
        }

        if(DashTimer != 0) { //timer that decides how long the dash should last, uses "dashdirection" so it doesnt perfectly home in on the player
            DashTimer--;
            moveDirection = dashDirection;
            NPC.velocity += moveDirection * 3f;

            Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Shadowflame,
                0f, 0f,
                100,
                default,
                Main.rand.NextFloat(0.5f, 1.5f) * DashMultiplier
            );
        } //thanks for coming to my tedtalk
        else if (DashCooldownTimer > 0) DashCooldownTimer--; // When not dashing, decrease CooldownTimer

        if(Main.rand.NextBool(6)) { // Passive dust
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
        } 

        if(Main.rand.NextBool(20)) { // Passive dust 2
            Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Shadowflame,
                0f, 0f,
                100,
                default,
                Main.rand.NextFloat(0.5f, 1.5f)
            );
        }

        float separationRadius = 16 * 4f; // Code that makes them space each other apart
        float pushStrength = 0.05f;    

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC other = Main.npc[i];

            if (other.active && i != NPC.whoAmI && other.type == NPC.type) {
                float distance = Vector2.Distance(NPC.Center, other.Center); // Distance between two eyeballs

                if (distance < separationRadius) {
                    if (distance == 0) {
                        NPC.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
                    }
                    else {
                        Vector2 pushDirection = Vector2.Normalize(NPC.Center - other.Center);
                        NPC.velocity += pushDirection * pushStrength;
                    }
                }
            }
        }
    }
    public override void HitEffect(NPC.HitInfo hit) {
        if(DashTimer != 0) {
            DashTimer = 0;
            NPC.velocity -= NPC.Center.DirectionTo(Main.player[NPC.target].Center) * 2f;
            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 1f } with { PitchRange = (-0.5f, 0f) });
        }

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
        Color c = NPC.GetNPCColorTintedByBuffs(drawColor) * NPC.Opacity;

        if(DashWindupTimer != 0 || DashTimer != 0) { // Chargeup/ dash trail
            for(int i = NPC.oldPos.Length - 1; i >= 0; i--) {
                float opacity = (1f - (i / (float)NPC.oldPos.Length));
                spriteBatch.Draw(
                    texture,
                    NPC.oldPos[i] - screenPos + NPC.Size / 2,
                    NPC.frame,
                    new Color(180, 30, 255, 100) * opacity,
                    NPC.rotation,
                    NPC.frame.Size() / 2,
                    NPC.scale,
                    SpriteEffects.None,
                    0
                );
            }
        }
        spriteBatch.Draw( // Enemy
            texture,
            position - screenPos,
            NPC.frame,
            c,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );
        
        if(NPC.Opacity < 1) { // Spawn shine
            texture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.Purple with { A = 0 } * (1f - NPC.Opacity), 0, texture.Size() / 2, 2f - NPC.Opacity, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.White with { A = 0 } * (1f - NPC.Opacity) * 0.5f, 0, texture.Size() / 2, 0.8f, SpriteEffects.None, 0);

            spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.Purple with { A = 0 } * (1f - NPC.Opacity), MathHelper.PiOver2, texture.Size() / 2, 1.5f - NPC.Opacity, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.White with { A = 0 } * (1f - NPC.Opacity) * 0.5f, MathHelper.PiOver2, texture.Size() / 2, 0.6f, SpriteEffects.None, 0);
        }
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