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

namespace EvilExpansionMod.Content.Misc;
public class MeatSplatter : ModProjectile {
    private bool isCrimson;
    private int randomVisualVariant;
    public override string Texture => Assets.Images.Crimson.Items.MeatAxe.MeatSplatter.KEY;
    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 60;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 12;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.hostile = false;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 3;
    }
    public override bool PreDraw(ref Color lightColor) {
        lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
        return true;
    }
    public override void OnSpawn(IEntitySource source) {
        if(Projectile.ai[0] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }

        Projectile.frame = Main.rand.Next(0, 3);

        // Standard AI movement/rotation logic goes here
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }
    public override void AI() {
        if (Projectile.timeLeft < 8){
            // Increase alpha from 0 (opaque) to 255 (fully transparent) smoothly
            Projectile.alpha += 30;
            
            if (Projectile.alpha > 255){
                Projectile.alpha = 255;
            }
        }
    }
}