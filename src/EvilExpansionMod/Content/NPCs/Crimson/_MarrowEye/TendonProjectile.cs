using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;
public class TendonProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.MarrowEye.KEY_Tendon;

    public int AttachedEye { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
    static int DisapearFrames = 120;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 180;
        Projectile.hide = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if(Main.npc[AttachedEye] != null && Main.npc[AttachedEye].active) Projectile.timeLeft = DisapearFrames;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        const int CellWidth = 32;
        var index = Projectile.whoAmI % 3;
        var source = new Rectangle(index * CellWidth, 0, CellWidth, texture.Height);

        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            source,
            lightColor * ((float)Projectile.timeLeft / DisapearFrames),
            Projectile.rotation,
            source.Size() / 2f,
            new Vector2(1f, Projectile.scale / texture.Height),
            SpriteEffects.None,
            0f
        );

        return false;
    }
}
