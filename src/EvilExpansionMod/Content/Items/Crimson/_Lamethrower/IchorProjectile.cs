using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class IchorProjectile : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;
    public override void SetDefaults() {
        Projectile.width = Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 128;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;

        Projectile.hide = true;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.width = Projectile.height = (int)(Projectile.width * Main.rand.NextFloat(0.5f, 1.5f));
        Projectile.rotation = Main.rand.NextFloatDirection();
    }

    public override void AI() {
        Projectile.velocity.Y -= 0.001f;
        Projectile.velocity *= 0.985f;
    }
}

class IchorProjectileDrawingSystem : ModSystem {
    public override void PostUpdateEverything() {
        var pipeline = Graphics.BeginPipeline(0.5f);

        var ichorType = ModContent.ProjectileType<IchorProjectile>();
        for(var i = 0; i < Main.maxProjectiles; i++) {
            var projectile = Main.projectile[i];
            if(projectile == null || !projectile.active || projectile.type != ichorType) continue;

            var destination = projectile.Hitbox;
            destination.Offset((-Main.screenPosition).ToPoint());

            pipeline.DrawSprite(
                TextureAssets.MagicPixel.Value,
                destination,
                new Color(238, 192, 93),
                rotation: projectile.rotation
            );
        }

        pipeline.ApplyOutline(new Color(223, 116, 40));
        pipeline.Schedule(RenderLayer.AfterNPCs);
    }
}
