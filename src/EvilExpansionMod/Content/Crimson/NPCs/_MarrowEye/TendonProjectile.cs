using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class TendonProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.NPCs.MarrowEye.Tendon.KEY;
    public const int DisapearFrames = 120;

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

    // public override void OnKill(int timeLeft) {
    //     var rotation = Main.rand.NextFloat();
    //     for(var i = 0; i < 1; i++) {
    //         var direction = rotation.ToRotationVector2();
    //         Gore.NewGoreDirect(
    //             Projectile.GetSource_Death(),
    //             Projectile.Center + direction * 10f - new Vector2(8, 8),
    //             direction * Main.rand.NextFloat(3f, 5f),
    //             Mod.Find<ModGore>("MuscleGore" + i).Type
    //         );

    //         rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
    //     }
    // }

    public override void AI() {
        Projectile.alpha = 255 - (int)(255f * Projectile.timeLeft / DisapearFrames);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        const int CellWidth = 32;
        var index = Projectile.whoAmI % 3;

        var sourceX = index * CellWidth;

        var bulbHeight = 14;
        var middlePartHeight = texture.Height - bulbHeight * 2;

        var rotation = Projectile.rotation - MathF.PI / 2f;
        var alpha = (255 - Projectile.alpha) / 255f;

        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            new Rectangle(sourceX, bulbHeight, CellWidth, middlePartHeight),
            lightColor * alpha,
            rotation,
            new(CellWidth / 2f, middlePartHeight / 2f),
            new Vector2(1f, (Projectile.scale - bulbHeight) / middlePartHeight),
            SpriteEffects.None,
            0f
        );

        var rotationVector = Projectile.rotation.ToRotationVector2();
        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition - rotationVector * Projectile.scale / 2f,
            new Rectangle(sourceX, 0, CellWidth, bulbHeight),
            lightColor * alpha,
            rotation,
            new(CellWidth / 2f, bulbHeight / 2f),
            1f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition + rotationVector * Projectile.scale / 2f,
            new Rectangle(sourceX, texture.Height - bulbHeight, CellWidth, bulbHeight),
            lightColor * alpha,
            rotation,
            new(CellWidth / 2f, bulbHeight / 2f),
            1f,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}
