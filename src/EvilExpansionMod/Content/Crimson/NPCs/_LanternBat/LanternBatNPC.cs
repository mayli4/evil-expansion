using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LanternBatNPC : ModNPC {
    public enum State {
        IdleFlight,
        DashTelegraph,
        Dashing,
        PostDashCooldown
    }

    public override string Texture => Assets.Images.Crimson.NPCs.LanternBat.LanternBatNPC.KEY;
    public static string LanternTexturePath => Assets.Images.Crimson.NPCs.LanternBat.LanternBat_Lantern.KEY;

    public State CurrentState {
        get => (State)NPC.ai[0];
        set {
            NPC.ai[0] = (float)value;
            StateTimer = 0;
            NPC.netUpdate = true;
        }
    }
    public ref float StateTimer => ref NPC.ai[1];

    public Player Target => Main.player[NPC.target];

    private const int anim_speed = 6;
    private Vector2 storedDashDirection;
    private ref float LanternLightIntensity => ref NPC.localAI[1];

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Position = new Vector2(20f, 0f),
            PortraitPositionXOverride = 10f,
            PortraitPositionYOverride = -10f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 120;
        NPC.damage = 25;
        NPC.defense = 8;
        NPC.knockBackResist = 0.2f;
        NPC.value = 650f;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath4;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.buffImmune[BuffID.Bleeding] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<LanternBatBannerItem>();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.2f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSlicesItem>(), 1, 2, 4));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FireInALanternItem>(), 50, 1, 1));
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.LanternBatNPCBestiary.KEY),
        });
    }
    public override void OnSpawn(IEntitySource source) {
        LanternLightIntensity = 0f;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;

        for(var i = 0; i < 2; i++) {
            Gore.NewGoreDirect(
                NPC.GetSource_Death(),
                NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
                Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
                Mod.Find<ModGore>($"LanternBatGore{i}").Type
            );
        }

        Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + new Vector2(NPC.spriteDirection * 15, 40) + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>("LanternGore").Type
        );
    }

    public override void AI() {
        NPC.TargetClosest();
        if(Target.dead || !Target.active) {
            return;
        }

        switch(CurrentState) {
            case State.IdleFlight:
                Vector2 idealIdlePosition = Target.Center + new Vector2(NPC.direction * 200, -100);
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(idealIdlePosition) * 4, 0.05f);

                StateTimer++;
                int minIdleTime = 60 * 1;
                int maxIdleTime = 60 * 3;
                int dashRange = 16 * 25;

                float dashThresholdProgress = Math.Min(1f, (StateTimer - minIdleTime) / (maxIdleTime - minIdleTime));

                LanternLightIntensity = MathF.Pow(dashThresholdProgress, 3f) * 2.5f;
                LanternLightIntensity = Math.Min(LanternLightIntensity, 2.5f);
                LanternLightIntensity = Math.Max(0.2f, LanternLightIntensity);

                UpdateDirection();

                if(NPC.Distance(Target.Center) < dashRange && StateTimer > Main.rand.Next(minIdleTime, maxIdleTime)) {
                    Vector2 dashTarget = Target.Center + Target.velocity * 0.5f - Vector2.UnitY * 80;
                    storedDashDirection = NPC.DirectionTo(dashTarget);

                    NPC.spriteDirection = NPC.direction = storedDashDirection.X > 0 ? 1 : -1;

                    CurrentState = State.DashTelegraph;
                }

                break;
            case State.DashTelegraph:
                NPC.velocity -= storedDashDirection * 0.065f;
                NPC.velocity *= 0.95f;

                StateTimer++;
                if(StateTimer >= 35) {
                    NPC.velocity = storedDashDirection * 2f;
                    CurrentState = State.Dashing;

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<LingeringFlameProjectile>(),
                        20,
                        0,
                        Main.myPlayer,
                        NPC.whoAmI,
                        storedDashDirection.X > 0 ? 1 : -1
                    );

                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.1f * Main.rand.NextFloatDirection() }, NPC.Center);
                }

                break;
            case State.Dashing:
                NPC.velocity += storedDashDirection * 0.45f;

                NPC.noTileCollide = true;
                NPC.noGravity = true;

                LanternLightIntensity = 1.5f;

                StateTimer++;
                if(StateTimer >= 45) {
                    CurrentState = State.PostDashCooldown;
                    NPC.noTileCollide = true;
                    NPC.noGravity = true;
                    NPC.velocity *= 0.5f;
                }

                break;

            case State.PostDashCooldown:
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 4, 0.03f);

                StateTimer++;
                LanternLightIntensity = Math.Max(0f, 1.5f * (1f - StateTimer / (float)(60 * 2)));
                LanternLightIntensity = Math.Max(0.2f, LanternLightIntensity);

                UpdateDirection();

                if(StateTimer >= 60 * 2) {
                    CurrentState = State.IdleFlight;
                }

                break;
        }

        if(StateTimer % 30 == 0) {
            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFurySwing with { Pitch = 0.1f * Main.rand.NextFloatDirection() }, NPC.Center);
        }

        NPC.rotation = Utils.AngleLerp(NPC.rotation, Math.Clamp(NPC.velocity.X * 0.15f, -0.65f, 0.65f), 0.1f);

        if(NPC.velocity.Length() < 0.1f && CurrentState != State.Dashing) {
            NPC.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
        }
    }

    private void UpdateDirection() {
        NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0) ? 1 : -1;
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;
        if(NPC.frameCounter >= anim_speed * 4) {
            NPC.frameCounter = 0;
        }
        NPC.frame.Y = (int)(NPC.frameCounter / anim_speed) * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var lanternOffsetVector = new Vector2(-8, 24);
        if(NPC.spriteDirection == -1) {
            lanternOffsetVector.X *= -1;
        }

        var rotationVector = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
        var batClawPosition = NPC.Center + rotationVector * 24f;

        Lighting.AddLight(batClawPosition + rotationVector * 12f, Color.Orange.ToVector3() * LanternLightIntensity);

        var lanternTex = Assets.Images.Crimson.NPCs.LanternBat.LanternBat_Lantern.Asset.Value;
        var lanternInside = Assets.Images.Crimson.NPCs.LanternBat.LanternBat_LanternFlame.Asset.Value;
        var lanternOrigin = new Vector2(lanternTex.Width / 2, 0);

        var lanternEffects = SpriteEffects.None;
        if(NPC.spriteDirection == -1) {
            lanternEffects = SpriteEffects.FlipHorizontally;
        }

        var lightEffectColor = Color.Orange * LanternLightIntensity;
        var lanternRotation = NPC.rotation * 0.75f + 0.2f * MathF.Sin(Main.GameUpdateCount * 0.1f + NPC.whoAmI * 7238.27f);

        Main.EntitySpriteDraw(
            lanternInside,
            batClawPosition - screenPos,
            null,
            lightEffectColor,
            lanternRotation,
            lanternOrigin,
            NPC.scale,
            lanternEffects
        );

        Main.EntitySpriteDraw(
            lanternTex,
            batClawPosition - screenPos,
            null,
            NPC.GetAlpha(drawColor),
            lanternRotation,
            lanternOrigin,
            NPC.scale,
            lanternEffects
        );

        var batTexture = TextureAssets.Npc[NPC.type].Value;
        var batOrigin = NPC.frame.Size() / 2f;

        Main.EntitySpriteDraw(
            batTexture,
            NPC.Center - screenPos,
            NPC.frame,
            NPC.GetAlpha(drawColor),
            NPC.rotation,
            batOrigin,
            NPC.scale,
            (NPC.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );

        return false;
    }
}