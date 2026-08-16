using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

enum CultistState {
    FlyToTarget,
    SpearAttack,
    EyeAttack
}

public class ThoughtfulCultistNPC : ModNPC {
    public override string Texture => Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistBrain.KEY;
    Player Target => Main.player[NPC.target];
    CultistState State => Unsafe.BitCast<float, CultistState>(NPC.ai[0]);

    float _timer;
    float _robeOffset;
    float _portalRotation;

    void ChangeState(CultistState state) {
        NPC.ai[0] = Unsafe.BitCast<CultistState, float>(state);
        _timer = 0;
        NPC.netUpdate = true;
    }

    public override void SetDefaults() {
        NPC.width = 38;
        NPC.height = 38;
        NPC.lifeMax = 700;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<ThoughtfulCultistBannerItem>();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.05f : 0;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSlicesItem>(), 1, 2, 4));
    }

    public override void OnSpawn(IEntitySource source) {
        ChangeState(CultistState.FlyToTarget);
    }

    public override void AI() {
        NPC.TargetClosest();

        var directionToTarget = Vector2.Zero;
        var distanceToTarget = float.MaxValue;
        if(Target != null) {
            var targetDelta = Target.Center - Vector2.UnitY * 80f - NPC.Center;
            distanceToTarget = targetDelta.Length();
            directionToTarget = targetDelta / distanceToTarget;
        }

        switch(State) {
            case CultistState.FlyToTarget:
                if(distanceToTarget > 400) {
                    NPC.velocity += directionToTarget * 0.15f;
                    NPC.velocity *= 0.97f;
                }
                else if(Main.netMode != NetmodeID.MultiplayerClient && _timer > 120) {
                    if(Main.rand.NextBool()) {
                        _portalRotation = Main.rand.NextFloat(0, -MathF.PI);
                        ChangeState(CultistState.EyeAttack);
                    }
                    else {
                        _portalRotation = Main.rand.NextFloat(0, 2 * MathF.PI);
                        ChangeState(CultistState.SpearAttack);
                    }
                }
                break;
            case CultistState.SpearAttack:
                NPC.velocity *= 0.99f;
                if(Target == null) {
                    ChangeState(CultistState.FlyToTarget);
                }
                else if(_timer > 60 && (int)_timer % 30 == 0) {
                    var position = Target.Center - 105f * _portalRotation.ToRotationVector2();
                    var direction = position.DirectionTo(Target.Center);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        position,
                        direction,
                        ModContent.ProjectileType<CultistPortal>(),
                        20,
                        0.2f,
                        ai0: (float)PortalType.Spear,
                        ai1: 120
                    );

                    _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                    SoundEngine.PlaySound(SoundID.Item79, position);
                }

                if(_timer > 150) {
                    ChangeState(CultistState.FlyToTarget);
                }
                break;
            case CultistState.EyeAttack:
                NPC.velocity *= 0.99f;
                if(Target == null) {
                }
                else if(_timer > 60 && (int)_timer % 30 == 0) {
                    var position = Target.Center + _portalRotation.ToRotationVector2() * Main.rand.NextFloat(300, 400);
                    var direction = position.DirectionTo(Target.Center);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        position,
                        direction,
                        ModContent.ProjectileType<CultistPortal>(),
                        20,
                        0.2f,
                        ai0: (float)PortalType.Blood,
                        ai1: 360
                    );

                    _portalRotation += Main.rand.NextFloat(MathF.PI / 4f, MathF.PI / 2f);
                    SoundEngine.PlaySound(SoundID.Item79, position);
                }

                if(_timer > 120) {
                    ChangeState(CultistState.FlyToTarget);
                }
                break;
        }

        _timer += 1;

        var offsetMax = 12f;
        _robeOffset = Math.Clamp(_robeOffset + NPC.velocity.X * 0.1f, -offsetMax, offsetMax);
        _robeOffset *= 0.98f;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        for(var i = 1; i < 4; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"CultistBrainGore{i}").Type
        );

        for(var i = 0; i < 5; i++) {
            Gore.NewGoreDirect(
                NPC.GetSource_Death(),
                NPC.Center + Main.rand.NextVector2Unit() * 40f + Vector2.UnitY * 30f,
                Vector2.Zero,
                Mod.Find<ModGore>($"CultistRobeGore{Main.rand.Next(1, 4)}").Type
            );
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var brainTexture = TextureAssets.Npc[Type].Value;
        var robeTextureBack = Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeBack.Asset.Value;
        var robeTextureFront = Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeFront.Asset.Value;
        var pendantTexture = Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistPendant.Asset.Value;
        var pendantGlowmaskTexture = Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistPendantGlowmask.Asset.Value;
        var chainTexture = Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistChain.Asset.Value;

        if(NPC.IsABestiaryIconDummy)
            return true;

        Span<Vector2> robeTrailPositions = new Vector2[7];
        robeTrailPositions[0] = NPC.Center - Vector2.UnitY * 7f;

        for(var i = 1; i < robeTrailPositions.Length; i++) {
            robeTrailPositions[i] = robeTrailPositions[i - 1];
            robeTrailPositions[i].Y += 29;
            robeTrailPositions[i].X -=
                (float)i / robeTrailPositions.Length
                * _robeOffset
                + 1.25f * MathF.Sin(NPC.whoAmI * 23.2f + Main.GameUpdateCount * 0.03f);
        }

        var center = NPC.Center + Vector2.UnitY * 120;

        var offsetX = 30;
        var offsetY = 80;
        var bezierRight = center + new Vector2(offsetX, -offsetY);
        var bezierLeft = center + new Vector2(-offsetX, -offsetY);
        var bezierCenter = center - Vector2.UnitX * _robeOffset * 2f;

        var bezier = new BezierCurve(bezierLeft, bezierCenter, bezierRight);
        var chainPoints = bezier.GetPoints(13).ToArray();

        var pendantOutlineColor = Color.Transparent;
        switch(State) {
            case CultistState.SpearAttack:
                if(_timer < 60) pendantOutlineColor = Color.Lerp(
                    pendantOutlineColor,
                    Color.Orange,
                    MathF.Sin(MathF.PI * _timer / 60)
                );
                break;
            case CultistState.EyeAttack:
                if(_timer < 60) pendantOutlineColor = Color.Lerp(
                    pendantOutlineColor,
                    Color.Red,
                    MathF.Sin(MathF.PI * _timer / 60)
                );
                break;
        }

        Renderer.BeginPipeline(1f, Graphics.WorldTransformMatrix)
            .SetSamplerState(0, SamplerState.PointWrap)
            .SetTexture(robeTextureBack)
            .DrawTrail(robeTrailPositions, static _ => 88, _ => drawColor, spriteRotation: 1)
            .SetTexture(chainTexture)
            .DrawTrail(chainPoints, static _ => 6, _ => drawColor)
            .SetTexture(robeTextureFront)
            .DrawTrail(robeTrailPositions, static _ => 88, _ => drawColor, spriteRotation: 1)
            .End();

        Renderer.BeginPipeline()
            .DrawTexture(new()
            {
                Texture = pendantTexture,
                Position = chainPoints[chainPoints.Length / 2] - screenPos,
                Color = drawColor,
                Rotation = 0f,
                Origin = pendantTexture.Size() / 2f,
            })
            .DrawTexture(new()
            {
                Texture = pendantGlowmaskTexture,
                Position = chainPoints[chainPoints.Length / 2] - screenPos,
                Color = pendantOutlineColor,
                Rotation = 0f,
                Origin = pendantTexture.Size() / 2f,
            })
            .ApplyOutline(pendantOutlineColor)
            .End();

        spriteBatch.Draw(brainTexture, NPC.Center - screenPos, null, drawColor, 0f, new Vector2(53, 55), 1f, SpriteEffects.None, 0f);
        return false;
    }
}