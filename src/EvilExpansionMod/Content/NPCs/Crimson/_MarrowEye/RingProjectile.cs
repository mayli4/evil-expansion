using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._MarrowEye;
public class RingProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.MarrowEye.KEY_Ring;
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
        Projectile.alpha = 255 - (int)(255f * (float)Projectile.timeLeft / DisapearFrames);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }
}
