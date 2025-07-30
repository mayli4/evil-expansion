using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;
public class ThoughtfulCultistNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_CultistBrain;
    Player Target => Main.player[NPC.target];

    float _robeOffset;

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

    public override void AI() {
        NPC.TargetClosest();

        var directionToTarget = Vector2.Zero;
        var distanceToTarget = float.MaxValue;
        if(Target != null) {
            var targetDelta = Target.Center - Vector2.UnitY * 80f - NPC.Center;
            distanceToTarget = targetDelta.Length();
            directionToTarget = targetDelta / distanceToTarget;
        }

        var moveDelta = directionToTarget * 0.075f;
        NPC.velocity += moveDelta;
        NPC.velocity *= 0.98f;

        var offsetMax = 12f;
        _robeOffset = Math.Clamp(_robeOffset + moveDelta.X, -offsetMax, offsetMax);
        _robeOffset *= 0.98f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var brainTexture = TextureAssets.Npc[Type].Value;
        var robeTextureBack = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeBack.Value;
        var robeTextureFront = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobeFront.Value;
        var pendantTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistPendant.Value;
        var chainTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistChain.Value;

        Span<Vector2> robeTrailPositions = new Vector2[7]; // TODO: change to stackalloc
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

        new Renderer.Pipeline()
            .DrawBasicTrail(robeTrailPositions, static _ => 88, robeTextureBack, drawColor, 1)
            .DrawBasicTrail(chainPoints, static _ => 6, chainTexture, drawColor)
            .DrawBasicTrail(robeTrailPositions, static _ => 88, robeTextureFront, drawColor, 1)
            .Flush();

        spriteBatch.Draw(
            pendantTexture,
            chainPoints[chainPoints.Length / 2] - screenPos,
            null,
            drawColor,
            0f,
            pendantTexture.Size() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );
        spriteBatch.Draw(brainTexture, NPC.Center - screenPos, null, drawColor, 0f, new Vector2(53, 55), 1f, SpriteEffects.None, 0f);
        return false;
    }
}
