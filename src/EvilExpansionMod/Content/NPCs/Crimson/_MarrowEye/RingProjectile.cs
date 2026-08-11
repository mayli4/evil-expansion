using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class RingProjectile : ModProjectile {
    public override string Texture => Assets.Textures.NPCs.Crimson.MarrowEye.KEY_Ring;
    public const int DisapearFrames = 120;

    public override void SetDefaults() {
        Projectile.width = 42;
        Projectile.height = 42;
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
        //Projectile.alpha = 255 - (int)(255f * (float)Projectile.timeLeft / DisapearFrames);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override void OnKill(int timeLeft) {
        var rotation = Main.rand.NextFloat();
        var direction = rotation.ToRotationVector2();
        Gore.NewGoreDirect(
            Projectile.GetSource_Death(),
            Projectile.Center + direction * 10f - new Vector2(8, 8),
            direction * Main.rand.NextFloat(3f, 5f),
            Mod.Find<ModGore>("RingGore").Type
        );
    }
}
