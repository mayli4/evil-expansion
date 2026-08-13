using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Crimson;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LanternBatNPC : ModNPC {
    public enum State {
        IdleFlight,
        Dashing,
        PostDashCooldown
    }

    public override string Texture => Assets.Textures.NPCs.Crimson.LanternBat.LanternBatNPC.KEY;
    public string LanternTexturePath => Assets.Textures.NPCs.Crimson.LanternBat.LanternBat_Lantern.KEY;

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
    private Vector2 _storedDashVelocity;
    private ref float _lanternLightIntensity => ref NPC.localAI[1];

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 120;
        NPC.damage = 25;
        NPC.defense = 8;
        NPC.knockBackResist = 0.2f;
        NPC.value = 300f;
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

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.1f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSlicesItem>(), 1, 2, 4));
    }

    public override void OnSpawn(IEntitySource source) {
        _lanternLightIntensity = 0f;
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

                _lanternLightIntensity = MathF.Pow(dashThresholdProgress, 3f) * 2.5f;
                _lanternLightIntensity = Math.Min(_lanternLightIntensity, 2.5f);
                _lanternLightIntensity = Math.Max(0.2f, _lanternLightIntensity);

                if(NPC.Distance(Target.Center) < dashRange && StateTimer > Main.rand.Next(minIdleTime, maxIdleTime)) {
                    Vector2 dashTarget = Target.Center + Target.velocity * 0.5f;
                    _storedDashVelocity = NPC.DirectionTo(dashTarget) * 16;

                    CurrentState = State.Dashing;
                }
                break;
            case State.Dashing:
                NPC.velocity = _storedDashVelocity;
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                _lanternLightIntensity = 1.5f;

                if(StateTimer % 10 == 0) {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<LingeringFlameProjectile>(),
                        20,
                        0, Main.myPlayer,
                        NPC.whoAmI
                    );
                }

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
                _lanternLightIntensity = Math.Max(0f, 1.5f * (1f - StateTimer / (float)(60 * 2)));
                _lanternLightIntensity = Math.Max(0.2f, _lanternLightIntensity);

                if(StateTimer >= 60 * 2) {
                    CurrentState = State.IdleFlight;
                }
                break;
        }

        NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0) ? 1 : -1;

        if(NPC.velocity.Length() < 0.1f && CurrentState != State.Dashing) {
            NPC.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;
        if(NPC.frameCounter >= anim_speed * 4) {
            NPC.frameCounter = 0;
        }
        NPC.frame.Y = (int)(NPC.frameCounter / anim_speed) * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 lanternOffsetVector = new Vector2(0, 40);

        if(NPC.spriteDirection == -1) {
            lanternOffsetVector.X *= -1;
        }

        Vector2 lanternDrawPosition = NPC.Center + lanternOffsetVector;

        Lighting.AddLight(lanternDrawPosition, Color.Orange.ToVector3() * _lanternLightIntensity);

        float lanternRotation = NPC.velocity.X * 0.05f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

        Texture2D lanternTex = Assets.Textures.NPCs.Crimson.LanternBat.LanternBat_Lantern.Asset.Value;
        Texture2D lanternInside = Assets.Textures.NPCs.Crimson.LanternBat.LanternBat_LanternFlame.Asset.Value;
        Vector2 lanternOrigin = new Vector2(lanternTex.Width / 2, 0);

        SpriteEffects lanternEffects = SpriteEffects.None;
        if(NPC.spriteDirection == -1) {
            lanternEffects = SpriteEffects.FlipHorizontally;
        }

        Color lightEffectColor = Color.Orange * _lanternLightIntensity;

        Main.EntitySpriteDraw(
            lanternInside,
            lanternDrawPosition - screenPos,
            null,
            lightEffectColor,
            lanternRotation,
            lanternOrigin,
            NPC.scale,
            lanternEffects
        );

        Main.EntitySpriteDraw(
            lanternTex,
            lanternDrawPosition - screenPos,
            null,
            NPC.GetAlpha(drawColor),
            lanternRotation,
            lanternOrigin,
            NPC.scale,
            lanternEffects
        );

        Texture2D batTex = TextureAssets.Npc[NPC.type].Value;
        Vector2 batOrigin = NPC.frame.Size() / 2f;

        Main.EntitySpriteDraw(
            batTex,
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