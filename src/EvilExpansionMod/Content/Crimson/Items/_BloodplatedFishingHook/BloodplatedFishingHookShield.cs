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
    private const int JAB_DURATION = 5;
    private const float MAX_SCALE_INCREASE = 0.2f;
    float baseScale = 1.0f;
    bool conjureAnimationDone = false;
    bool initialSpawn = false;
    float conjureAcceleration = 0.15f;
    public override void SetDefaults() {
        Projectile.width = 147;
        Projectile.height = 147;
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

        Vector2 vectorToTarget = player.Center - Projectile.Center; //Tracks player inperfectly
        float distance = vectorToTarget.Length();

        if(distance > 5f) {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity *= 1.05f, vectorToTarget / distance * 10f, 0.1f);
        }
        else {
            Projectile.velocity *= 0.85f;
        }

        if(Projectile.velocity.Length() < 0.1f) {
            Projectile.velocity.Y += 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
        }

        Projectile.rotation += 0.002f;

        if(modPlayer.IsActivelyFishingWithBloodplated(player) && modPlayer._plateTier > 0) {
            Projectile.timeLeft = 2; //Keep the projectile alive while the player is actively fishing & shield durability above 0
            if(modPlayer._plateTier == 3) Projectile.frame = 0;
            else if(modPlayer._plateTier == 2) Projectile.frame = 1;
            else if(modPlayer._plateTier == 1) Projectile.frame = 2;
        }
        else {
            if(modPlayer._plateTier == 0) {
                player.AddBuff(ModContent.BuffType<BloodPlatedBroken>(), 45 * 60); //Inflict debuff if sh. durab. = 0
            }
            Projectile.Kill(); //Kill the projectile if the player is not actively fishing or shield durability is 0
        }

        if(!conjureAnimationDone) {
            if(!initialSpawn) {
                Projectile.rotation = Main.rand.NextFloat(0, 360);
                Projectile.scale = 0;
                initialSpawn = true;
            }
            if((int)Projectile.scale != (int)baseScale) {
                if(Projectile.scale < baseScale*1.2) {
                    Projectile.scale += conjureAcceleration;
                    conjureAcceleration *= 1.25f;
                }
                else {
                    Projectile.scale += conjureAcceleration;
                    conjureAcceleration -= 20f;
                }
            }
            else {
                conjureAnimationDone = true;
            }
        }
        else if(JabTimer > 0) {
            JabTimer--;
            float progress = 1f - (JabTimer / JAB_DURATION); // To keep track of where in the animation we are
            float jabFactor = (float)System.Math.Sin(progress * MathHelper.Pi); // Smooth "scaling" from 1 to 0
            Projectile.scale = baseScale + (MAX_SCALE_INCREASE * jabFactor);
        }
        else {
            Projectile.scale = baseScale;
        }

        if(Main.rand.NextBool(12)) { // DUUUUUUUST DUST passive dust
            Dust.NewDust(
                player.position + Main.rand.NextVector2Circular(45, 45) + Main.rand.NextVector2Circular(45, 45),
                2,
                2,
                DustID.Blood,
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f),
                default,
                default,
                Main.rand.NextFloat(0.7f, 1.2f)
            );
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if(JabTimer <= 0) {
            JabTimer = JAB_DURATION;

            Projectile.netUpdate = true;
        }
        for(int i = 0; i < Main.rand.NextFloat(17f, 24f); i++) { // You can have a little blood dust on-hit as a treat
            Dust.NewDust(
                target.position,
                target.width,
                target.height,
                DustID.Blood,
                Main.rand.NextFloat(-1f, 7f),
                Main.rand.NextFloat(-7f, 1f),
                default,
                default,
                Main.rand.NextFloat(0.7f, 1.2f)
            );
        }
    }
    public override bool? CanCutTiles() => JabTimer > 0;
    public override bool PreKill(int timeLeft) { // Make gores on shatter or dust when de-conjured
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();

        if(Main.dedServ) return true;
        if(modPlayer._plateTier == 0) {
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
        }
        return true;
    }
    public override void OnKill(int timeLeft) { // Make gores on shatter or dust when de-conjured
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();
        for(var i = 0; i < 16; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BloodWater);
        }
        for(var i = 0; i < 16; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grubby,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    default,
                    default,
                    Main.rand.NextFloat(0.7f, 1f));
        }
        if(modPlayer._plateTier >= 0) {
            for(int i = 0; i < Main.rand.NextFloat(40f, 50f); i++) { // Circle of blood dust
                Dust.NewDust(
                    player.position + Main.rand.NextVector2Circular(58, 58) + Main.rand.NextVector2Circular(58, 58),
                    2,
                    2,
                    DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    default,
                    default,
                    Main.rand.NextFloat(1.2f, 1.4f)
                );
            }
        }
    }
}