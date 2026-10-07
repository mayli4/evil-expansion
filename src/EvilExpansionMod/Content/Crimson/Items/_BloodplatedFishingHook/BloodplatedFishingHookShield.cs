using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class BloodplatedFishingHookShield : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookShield.KEY;

    public override void SetDefaults() {
        Projectile.width = 110; //REFERENCE THIS LATER W/ OTHER HM FISHING ITEMS
        Projectile.height = 110; //REFERENCE THIS LATER W/ OTHER HM FISHING ITEMS
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 2;
        Projectile.alpha = 0;
        Projectile.scale = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Main.projFrames[Projectile.type] = 3;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<BloodplatedFishingHookPlayer>();
        Projectile.Center = player.Center; //I not certain if this makes the corner of the graphic the "center"(??) Check later
        if (modPlayer.IsActivelyFishing(player) && modPlayer._plateTier > 0) {
            Projectile.timeLeft = 2; //Keep the projectile alive while the player is actively fishing & shield durability above 0
            modPlayer.HasActiveShield = true;
            if (modPlayer._plateTier == 3) Projectile.frame = 0;
            else if (modPlayer._plateTier == 2) Projectile.frame = 1;
            else if (modPlayer._plateTier == 1) Projectile.frame = 2;
        }
        else {
            modPlayer.HasActiveShield = false;
            Projectile.Kill(); //Kill the projectile if the player is not actively fishing or shield durability is 0
        }
    }
    private const int FrameCount = 7;
    public override bool? CanCutTiles() => false;
    
    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        Vector2 origin = sourceRectangle.Size() / 2f;

        Main.spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

        return false; // Return false to prevent the default drawing behavior
    }
    
}