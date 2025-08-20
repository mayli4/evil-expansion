using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class PlanetoidSpawnerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_PlanetoidSpawnerItem;

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

        Item.damage = 20;
        Item.knockBack = 1f;
        Item.crit = 4;

        Item.shoot = ModContent.ProjectileType<PlanetoidProjectile>();
        Item.shootSpeed = 1f;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Pink;
        Item.autoReuse = false;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[ModContent.ProjectileType<PlanetoidProjectile>()] < 1 
               && player.ownedProjectileCounts[ModContent.ProjectileType<PlanetoidLauncherHeldProjectile>()] < 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
        int damage, float knockback) {
        Projectile.NewProjectile(
            player.GetSource_ItemUse(Item),
            Main.MouseWorld,
            Vector2.Zero,
            type,
            (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(damage),
            knockback,
            player.whoAmI
        );
        
        Projectile.NewProjectile(
            source,
            player.Center,
            Vector2.Zero,
            ModContent.ProjectileType<PlanetoidLauncherHeldProjectile>(),
            0,
            0,
            player.whoAmI
        );
        return false;
    }

    public override bool CanRightClick() => true;
    public override bool ConsumeItem(Player player) => false;


    public override void RightClick(Player player) {
        // Only perform conversion if Purification Powder is held
        if (player.HeldItem.type == ItemID.PurificationPowder) {
            SoundEngine.PlaySound(SoundID.Item4, player.Center);
            player.ConsumeItem(ItemID.PurificationPowder);

            Item.SetDefaults(ModContent.ItemType<NormalRevolverItem>());
            
            Item.stack++;
            player.QuickSpawnItem(player.GetSource_OpenItem(ModContent.ItemType<NormalRevolverItem>()), Item);
            Item.stack--;
        }
    }
}

public class PlanetoidLauncherHeldProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_PlanetoidSpawner;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.alpha = 0;
        Projectile.ownerHitCheck = true;
        Projectile.hide = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        var player = Main.player[Projectile.owner];

        if (!player.channel || player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<PlanetoidSpawnerItem>()) {
            Projectile.Kill();
            return;
        }
        
        Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitY);

        player.heldProj = Projectile.whoAmI;
        player.ChangeDir(Math.Sign(Projectile.velocity.X));
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * player.gravDir - MathHelper.PiOver2);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() * player.gravDir - MathHelper.PiOver2 - MathHelper.PiOver4 * 0.5f * player.direction);

        player.SetDummyItemTime(2);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.Center = player.MountedCenter;
        Projectile.timeLeft = 2;

        Projectile.spriteDirection = player.direction;
    }

    public override bool? CanCutTiles() => false;

    public override bool PreDraw(ref Color lightColor) {
        var player = Main.player[Projectile.owner];
        
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new Vector2(10, texture.Height / 2f);
        SpriteEffects effect = SpriteEffects.None;
        if (player.direction * player.gravDir < 0)
        {
            effect = SpriteEffects.FlipVertically;
        }
        
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
        Vector2 position = player.MountedCenter + direction * 10f + Vector2.UnitY * player.gfxOffY;
        Main.EntitySpriteDraw(texture, position - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effect, 0);

        return false;
    }
}