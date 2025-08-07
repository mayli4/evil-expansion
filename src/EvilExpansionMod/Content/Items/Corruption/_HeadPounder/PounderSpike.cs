using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption._HeadPounder;
public class PounderSpike : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HeadPounder.KEY_PounderSpike;

    int SpikeIndex => (int)Projectile.ai[0];

    static int MaxTimeLeft = 120;
    static int PopUpFrames = 8;

    float Scale => Projectile.timeLeft > MaxTimeLeft - PopUpFrames ? (float)(MaxTimeLeft - Projectile.timeLeft) / PopUpFrames : 1f;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.aiStyle = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Projectile.timeLeft < MaxTimeLeft - PopUpFrames - 5) return false;

        var _ = 0f;
        var spikeWidth = SpikeIndex switch
        {
            0 or 1 or 2 => 20,
            3 or 4 or 5 => 25,
            _ => 35,
        };

        var spikeHeight = SpikeIndex switch
        {
            0 or 1 or 2 => 45,
            3 or 4 or 5 => 60,
            _ => 95,
        };

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            Projectile.Center + (Projectile.rotation - MathF.PI / 2f).ToRotationVector2() * spikeHeight,
            spikeWidth,
            ref _
            );
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var cellWidth = 40;
        var source = new Rectangle(SpikeIndex * cellWidth, 0, cellWidth, texture.Height);

        Renderer.BeginPipeline(Scale)
            .DrawSprite(
                texture,
                Projectile.Center - Main.screenPosition,
                lightColor * MathF.Min(Projectile.timeLeft / 10f, 1f),
                source,
                Projectile.rotation,
                new Vector2(cellWidth / 2f, 90),
                Vector2.One * Scale,
                SpriteEffects.None
            )
            .ApplyOutline(Color.Green)
            .Flush();

        return false;
    }
}
