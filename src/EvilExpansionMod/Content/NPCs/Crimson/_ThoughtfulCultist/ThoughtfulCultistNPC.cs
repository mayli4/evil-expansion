using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
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

    public override void AI() {
        NPC.TargetClosest();

        var directionToTarget = Vector2.Zero;
        var distanceToTarget = 999_999f;
        if(Target != null) {
            var targetDelta = (Target.Center - Vector2.UnitY * 80f) - NPC.Center;
            distanceToTarget = targetDelta.Length();
            directionToTarget = targetDelta / distanceToTarget;
        }

        var moveDelta = directionToTarget * 0.4f;
        NPC.velocity += moveDelta;
        NPC.velocity *= 0.9f;

        var offsetMax = 12f;
        _robeOffset = Math.Clamp(_robeOffset + moveDelta.X, -offsetMax, offsetMax);
        _robeOffset *= 0.98f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var brainTexture = TextureAssets.Npc[Type].Value;
        var robeTexture = Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.CultistRobe.Value;

        Span<Vector2> robeTrailPositions = new Vector2[7]; // TODO: change to stackalloc
        robeTrailPositions[0] = NPC.Center - Vector2.UnitY * 7f;

        for(var i = 1; i < robeTrailPositions.Length; i++) {
            robeTrailPositions[i] = robeTrailPositions[i - 1];
            robeTrailPositions[i].Y += 29;

            robeTrailPositions[i].X -=
                (float)i / robeTrailPositions.Length
                * _robeOffset
                + MathF.Sin(NPC.whoAmI * 23.2f + Main.GameUpdateCount * 0.05f + i);
        }

        new Renderer.Pipeline()
            .DrawBasicTrail(robeTrailPositions, static _ => 88, robeTexture, drawColor, 1)
            .Flush();

        spriteBatch.Draw(brainTexture, NPC.Center - screenPos, null, drawColor, 0f, new Vector2(53, 55), 1f, SpriteEffects.None, 0f);
        return false;
    }
}
