using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class NormalRevolverItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalRevolver;

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 2;
        Item.useAnimation = 3;
        
        Item.DamageType = DamageClass.Magic;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.shoot = ModContent.ProjectileType<CirclingPlanetoidProjectile>();
        Item.shootSpeed = 1f;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Pink;
        Item.autoReuse = false;
    }
    
    
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<CirclingPlanetoidProjectile>()] < 1) {
            return true;
        }

        return false; 
    }
}

public class CirclingPlanetoidProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalPlanetoid;

    private ref float OrbitOffset => ref Projectile.localAI[0];

    public override void SetStaticDefaults() {
        Main.projPet[Type] = true;
        ProjectileID.Sets.LightPet[Type] = false;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.minion = true;
        Projectile.minionSlots = 0;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        OrbitOffset = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active) {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;

        var orbitCenter = player.MountedCenter;

        float currentAngle = Main.GameUpdateCount * 0.03f + OrbitOffset;
        var circularOffset = new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)) * 60f;

        float hoverOffset = MathF.Sin(Main.GameUpdateCount * 0.08f + OrbitOffset * 0.5f) * 5f;
        circularOffset.Y += hoverOffset;

        var targetPosition = orbitCenter + circularOffset;

        float lerpFactor = 0.1f;
        Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, lerpFactor);

        Projectile.rotation += 0.07f;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = ModContent.Request<Texture2D>(Texture).Value;
    
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            texture.Size() / 2f,
            1f,
            SpriteEffects.None
        );
    
        return false;
    }
}