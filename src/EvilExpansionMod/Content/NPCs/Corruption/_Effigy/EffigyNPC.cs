using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;
public sealed class EffigyNPC : ModNPC {
    byte _spawnedSprits = 0;

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_EffigyNPC;
    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 90;
        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.damage = 0;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void AI() { }

    void SpawnSpirit(Entity attacker) {
        var position = NPC.position + Vector2.UnitX * NPC.width / 2f;
        NPC.NewNPC(NPC.GetSource_OnHurt(attacker), (int)position.X, (int)position.Y, ModContent.NPCType<CursedSpiritNPC>());
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
        SpawnSpirit(projectile);
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
        SpawnSpirit(player);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        Main.EntitySpriteDraw(
            texture,
            NPC.Center - screenPos,
            NPC.frame,
            drawColor,
            0,
            NPC.frame.Size() / 2f,
            NPC.scale,
            NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );
        return false;
    }
}
