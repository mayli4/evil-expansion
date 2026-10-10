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

public class BloodplateHookShield : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookShield.KEY;
    private const int JAB_DURATION = 10;
    private const float MAX_SCALE_INCREASE = 0.2f;
    public override void SetDefaults() {
        Projectile.width = 110;
        Projectile.height = 110;
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
        Projectile.localNPCHitCooldown = JAB_DURATION;
        Main.projFrames[Projectile.type] = 3;
    }
    public float JabTimer {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    public override void AI() {
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();
        
        Vector2 vectorToTarget = player.Center - Projectile.Center;
                float distance = vectorToTarget.Length();

                if(distance > 20f) {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity *= 1.15f, vectorToTarget / distance * 10f, 0.1f);
                }
                else {
                    Projectile.velocity *= 0.95f;
                }

        if (modPlayer.IsActivelyFishingWithBloodplated(player) && modPlayer._plateTier > 0) {
            Projectile.timeLeft = 2; //Keep the projectile alive while the player is actively fishing & shield durability above 0
            modPlayer.HasActiveShield = true;
            if (modPlayer._plateTier == 3) Projectile.frame = 1;
            else if (modPlayer._plateTier == 2) Projectile.frame = 2;
            else if (modPlayer._plateTier == 1) Projectile.frame = 3;
        }
        else {
            modPlayer.HasActiveShield = false;
            if (modPlayer._plateTier == 0) {
                player.AddBuff(ModContent.BuffType<BloodPlatedBroken>(), 45 * 60); //Inflict debuff if sh. durab. = 0
            }
            Projectile.Kill(); //Kill the projectile if the player is not actively fishing or shield durability is 0
        }

        float baseScale = 1.0f;

        if (JabTimer > 0) {
            JabTimer--;
            float progress = 1f - (JabTimer / JAB_DURATION); // To keep track of where in the animation we are
            float jabFactor = (float)System.Math.Sin (progress*MathHelper.Pi); // Smooth "scaling" from 1 to 0
            Projectile.scale = baseScale + (MAX_SCALE_INCREASE * jabFactor);
        }
        else {
            Projectile.scale = baseScale;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (JabTimer <= 0) {
            JabTimer = JAB_DURATION;

            Projectile.netUpdate = true;
        }
        for(int i = 0; i < Main.rand.NextFloat(8f, 17f); i++) { // You can have a little blood dust on-hit as a treat
            Dust.NewDust(
                target.position,
                target.width,
                target.height,
                DustID.Blood,
                Main.rand.NextFloat(3f,6f),
                Main.rand.NextFloat(3f,6f),
                125,
                default,
                Main.rand.NextFloat(0.4f, 1.2f)
            ); 
        }
    }
    public override bool? CanCutTiles() => JabTimer > 0;
    public override bool PreKill(int timeLeft) { // Make gores on kill
        if(Main.dedServ) return true;

        var rotation = Main.rand.NextFloat();
        for(var i = 0; i < 5; i++) {
            var direction = rotation.ToRotationVector2();
            Gore.NewGoreDirect(
                Projectile.GetSource_Death(),
                Projectile.Center + direction * 10f - new Vector2(8, 8),
                direction * Main.rand.NextFloat(3f, 4f) + Projectile.velocity,
                Mod.Find<ModGore>("BloodplateGore" + i).Type
            );

            rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
        }

        for(var i = 0; i < 16; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BloodWater);
        }
        return true;
    }

    public override bool PreDraw(ref Color lightColor) { // Not sure if this chunk of coded is needed at all. 
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        Vector2 origin = sourceRectangle.Size() / 2f;

        Main.spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

        return false; // Return false to prevent the default drawing behavior
    }
    
}