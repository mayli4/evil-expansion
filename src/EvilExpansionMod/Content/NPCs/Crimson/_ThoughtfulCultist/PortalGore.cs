using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;
public class PortalGore : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_PortalGore1;

    public override void SetDefaults() {
        Projectile.width = 45;
        Projectile.height = 140;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 200;

        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.ai[0] = Main.rand.Next(1, 10);
        Projectile.netUpdate = true;

        var (width, height) = (int)Projectile.ai[0] switch
        {
            1 => (18, 18),
            2 => (22, 22),
            3 => (30, 30),
            4 => (40, 40),
            5 => (20, 20),
            6 => (20, 20),
            7 => (10, 10),
            8 => (40, 40),
            _ => (40, 40),
        };
        Projectile.Resize(width, height);
    }

    public override void AI() {
        Projectile.velocity.Y += 0.2f;
        Projectile.rotation += MathF.Sign(Projectile.velocity.X) * 0.1f;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = ModContent.Request<Texture2D>(
            $"{nameof(EvilExpansionMod)}/Assets/Textures/NPCs/Crimson/ThoughtfulCultist/PortalGore{(int)Projectile.ai[0]}",
            AssetRequestMode.ImmediateLoad
        ).Value;

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            texture.Size() / 2f,
            1f,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}