using EvilExpansionMod.Common;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace EvilExpansionMod.Content.Corruption;

public class HeadPounderChargeShine : ModProjectile {
    private bool InitialVisualSet = false;
    private Vector2 relativeOffset;
    public static bool IsMaxCharge = false;

    private float MaxFlash = 0;
    private float FlashAccel = 0;
    private float Variant = 0;
    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.HeadPounderShine.KEY;
    public override void SetDefaults() {
        Projectile.width = 54;
        Projectile.height = 54;
        Projectile.DamageType = DamageClass.Default;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = int.MaxValue;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.scale = 10f;
        Projectile.alpha = 255;
        Projectile.aiStyle = -1;
    }
    // Projectile. ai0 is the starting pulse size,
    // Projectile. ai1 is the "speed" of the pulse
    // Projectile. ai2 determines whether it is the "Growing" (0) variant or "Maxed" (1) variant
    public override void OnSpawn(IEntitySource source) {

        MaxFlash = Projectile.ai[0];
        FlashAccel = Projectile.ai[1];
        Variant = Projectile.ai[2];

        Projectile.scale = MaxFlash;
        Projectile.alpha = 0;
        Player player = Main.player[Projectile.owner];
        relativeOffset = Projectile.Center - player.Center;

        if(Variant == 1) {
            SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1f, Pitch = 1.0f }); //Shine sfx
            IsMaxCharge = true;
        }
        else IsMaxCharge = false;
    }
    public override void AI() {

        if(Variant == 0) { // If "Growing"
            Lighting.AddLight(Projectile.Center, 1.2f * (MaxFlash + FlashAccel), 1.5f * (MaxFlash + FlashAccel), 0.0f);

            if(!IsMaxCharge) {
                if(Projectile.scale <= MaxFlash + FlashAccel) {
                    Projectile.scale += FlashAccel * Main.rand.NextFloat(0.2f, 2f);
                    FlashAccel += 0.0035f;
                }
                else {
                    Projectile.scale = (MaxFlash + FlashAccel) * Main.rand.NextFloat(0.7f, 0.2f);
                }
            }
            else { // If "Maxed" exists
                Projectile.scale += FlashAccel * 2;
                if(Projectile.scale >= (MaxFlash + FlashAccel) * 8) {
                    Projectile.Kill();
                    return;
                }
            }
        }
        else { // If "Maxed"
            Lighting.AddLight(Projectile.Center, 1.2f, 1.5f, 0.0f);

            if(Projectile.scale <= MaxFlash) {
                Projectile.scale += FlashAccel * Main.rand.NextFloat(0.2f, 5f);
            }
            else {
                Projectile.scale = MaxFlash * Main.rand.NextFloat(0.7f, 0.2f);
            }
        }

        Player player = Main.player[Projectile.owner];
        if(Projectile.owner == Main.myPlayer) {
            Projectile.Center = player.Center + relativeOffset; // Follow player position (don't touch this)
            if(!player.channel && !player.controlUseItem) {
                Projectile.Kill();
                return;
            }
        }
    }
    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new Rectangle(
            0,
            Projectile.frame * frameHeight,
            texture.Width,
            frameHeight
        );

        Vector2 drawOrigin = new Vector2(texture.Width / 2f, frameHeight / 2f);
        float fadeOut = (255f - Projectile.alpha) / 255f; // If Alpha = 255, fadeOut = 0, if Alpha = 0, fadeOut = 1
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            sourceRectangle,
            lightColor * fadeOut,
            Projectile.rotation,
            drawOrigin,
            Projectile.scale,
            SpriteEffects.None
        );

        return false;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }
}