using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

public class CrytapeteSplatter : ModProjectile {
    private bool isTear;
    private int randomVisualVariant;
    Vector2 screenOffset;
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.TearSplatter.KEY;
    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 32;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 10;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.hostile = false;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 4;
    }
    public override bool PreDraw(ref Color lightColor) {
        if(Projectile.ai[1] == 0) {
            lightColor = Color.Lerp(lightColor, Color.Yellow, 1f);
        }
        return true;
    }
    public override void OnSpawn(IEntitySource source) {
        if(Projectile.ai[1] == 1) {
            isTear = true;
        }
            else {
            isTear = false;
        }

        randomVisualVariant = Main.rand.Next(0, 2);

        if (isTear){
            Projectile.frame = randomVisualVariant; 
        }
        else {
        Projectile.frame = 2 + randomVisualVariant;
        }

        // Standard AI movement/rotation logic goes here
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        Player player = Main.player[Projectile.owner];
        screenOffset = Projectile.position - Main.screenPosition;
    }
    public override void AI() {
        Projectile.position = Main.screenPosition + screenOffset;

        if(Projectile.ai[0] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }

        if (Projectile.timeLeft < 8){
        // Increase alpha from 0 (opaque) to 255 (fully transparent) smoothly
            Projectile.alpha += 31; // X * Y = 255
            
            if (Projectile.alpha > 255){
                Projectile.alpha = 255;
            }
        }
    }
}