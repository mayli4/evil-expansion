using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._MarrowEye;

public class MarrowEyeNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.MarrowEye.KEY_MarrowEyeNPC;

    Player Target => Main.player[NPC.target];
    Vector2 _lookDirection;

    public override void SetDefaults() {
        NPC.width = 50;
        NPC.height = 50;
        NPC.lifeMax = 100;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.Ichor] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void OnSpawn(IEntitySource source) {
    }

    public override void AI() {
        NPC.rotation = 0f;

        NPC.TargetClosest();
        if(Target != null) {
            var targetDelta = Target.Center - NPC.Center;
            var distanceToTarget = targetDelta.Length();
            if(distanceToTarget < 400f) {
                _lookDirection = Vector2.Lerp(_lookDirection, targetDelta / distanceToTarget, 0.04f);
                NPC.frameCounter = Math.Min(NPC.frameCounter + 0.2, 2);
            }
            else {
                _lookDirection *= 0.95f;
                NPC.frameCounter = Math.Max(NPC.frameCounter - 0.1, 0);
            }
        }
        else {
            _lookDirection *= 0.95f;
            NPC.frameCounter = Math.Max(NPC.frameCounter - 0.1, 0);
        }

        NPC.rotation = MathF.Sin(Main.GameUpdateCount * 0.03f + NPC.whoAmI * 574f) * 0.1f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        var whitesTexture = Assets.Assets.Textures.NPCs.Crimson.MarrowEye.MarrowEyeWhites.Value;
        var irisTexture = Assets.Assets.Textures.NPCs.Crimson.MarrowEye.MarrowEyeIris.Value;

        var position = NPC.Center + new Vector2(-4f, -38f);
        var origin = new Vector2(34, 8);

        spriteBatch.Draw(
            whitesTexture,
            position - screenPos,
            null,
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            irisTexture,
            position - screenPos + _lookDirection * 7f,
            null,
            drawColor,
            NPC.rotation,
            origin + new Vector2(-30, -35),
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            texture,
            position - screenPos,
            new(0, (int)NPC.frameCounter * 82, 72, 82),
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );
        return false;
    }
}
