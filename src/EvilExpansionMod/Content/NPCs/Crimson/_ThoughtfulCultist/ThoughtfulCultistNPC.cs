using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

enum CultistState {
    FlyToTarget,
    SpearAttack,
    EyeAttack
}

public class ThoughtfulCultistNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_CultistBrain;
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
        NPC.lifeMax = 500;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.001f;
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
                    NPC.velocity += directionToTarget * 0.03f;
                    NPC.velocity *= 0.98f;
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
                    ChangeState(CultistState.FlyToTarget);
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
            var gore = Gore.NewGoreDirect(
                NPC.GetSource_Death(),
                NPC.Center + Main.rand.NextVector2Unit() * 40f + Vector2.UnitY * 30f,
                Vector2.Zero,
                Mod.Find<ModGore>($"CultistRobeGore{Main.rand.Next(1, 4)}").Type
            );
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var brainTexture = TextureAssets.Npc[Type].Value;
        var robeTextureBack = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeBack.Value;
        var robeTextureFront = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeFront.Value;
        var pendantTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistPendant.Value;
        var pendantGlowmaskTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistPendantGlowmask.Value;
        var chainTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistChain.Value;

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

        new Renderer.Pipeline()
            .DrawBasicTrail(robeTrailPositions, static _ => 88, robeTextureBack, drawColor, 1)
            .DrawBasicTrail(chainPoints, static _ => 6, chainTexture, drawColor)
            .DrawBasicTrail(robeTrailPositions, static _ => 88, robeTextureFront, drawColor, 1)
            .BeginPixelate()
            .DrawSprite(
                pendantTexture,
                chainPoints[chainPoints.Length / 2] - screenPos,
                color: drawColor,
                rotation: 0f,
                origin: pendantTexture.Size() / 2f
            )
            .DrawSprite(
                pendantGlowmaskTexture,
                chainPoints[chainPoints.Length / 2] - screenPos,
                color: pendantOutlineColor,
                rotation: 0f,
                origin: pendantTexture.Size() / 2f
            )
            .ApplyOutline(pendantOutlineColor)
            .End()
            .Flush();

        spriteBatch.Draw(brainTexture, NPC.Center - screenPos, null, drawColor, 0f, new Vector2(53, 55), 1f, SpriteEffects.None, 0f);
        return false;
    }
}
