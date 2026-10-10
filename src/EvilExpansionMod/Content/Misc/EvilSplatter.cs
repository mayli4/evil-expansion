using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc;
public class EvilSplatter : ModProjectile {
    private bool isCrimson;
    private int randomVisualVariant;
    public override string Texture => Assets.Images.Misc.EvilSplatter.KEY;
    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 30;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 35;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.hostile = false;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 6;
    }
    public override bool PreDraw(ref Color lightColor) {
        if(Projectile.ai[1] == 1) {
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
        }
        else {
            lightColor = Color.White; 
        }
        return true;
    }
    public override void AI() {
        if(Projectile.ai[0] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }
        
        if (Projectile.localAI[0] == 0f){ // Run this setup exactly once when the projectile spawns
            Projectile.localAI[0] = 1f;

            if(Projectile.ai[1] == 1) {
                isCrimson = true;
            }
            else {
                isCrimson = false;
            }

            randomVisualVariant = Main.rand.Next(0, 3);
        }

        if (isCrimson){
            Projectile.frame = randomVisualVariant; 
        }
        else{
            Projectile.frame = 4 + randomVisualVariant; 
        }

        // Standard AI movement/rotation logic goes here
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (Projectile.timeLeft < 17){
        // Increase alpha from 0 (opaque) to 255 (fully transparent) smoothly
            Projectile.alpha += 15; // 15 * 17 = 255 (caps at 255 automatically or clamp it)
            
            if (Projectile.alpha > 255){
                Projectile.alpha = 255;
            }
        }
    }
}