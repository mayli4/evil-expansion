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

namespace EvilExpansionMod.Content.Corruption;

public class HeadPounderChargeShine : ModProjectile {
    private bool InitialVisualSet = false;
    private Vector2 relativeOffset;
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
    public override void OnSpawn(IEntitySource source) {
        Projectile.scale = Projectile.ai[0];
        Projectile.alpha = 0;
        Player player = Main.player[Projectile.owner];
        relativeOffset = Projectile.Center - player.Center;

        SoundEngine.PlaySound(SoundID.Item28 with { Volume = 1f, Pitch = 1.0f }); //Shine sfx
    }
    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.6f, 0.8f, 0.0f);

        if(Projectile.scale <= Projectile.ai[0]) {
            Projectile.scale += Projectile.ai[1] * Main.rand.NextFloat(0.2f, 5f); // Projectile. ai1 is the "speed" of the pulse
        }
        else {
            Projectile.scale = Projectile.ai[0] * Main.rand.NextFloat(0.7f, 0.2f);
        }

        Player player = Main.player[Projectile.owner];
        if(Projectile.owner == Main.myPlayer) {
            Projectile.Center = player.Center + relativeOffset; // Follow player position (don't touch this)
            if(!player.channel && !player.controlUseItem) {
                Projectile.Kill();
                return;
            }
        }

        //Main.NewText("Projectile.scale" + Projectile.scale, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("Projectile.alpha" + Projectile.alpha, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("Initalvisual" + InitialVisualSet, Microsoft.Xna.Framework.Color.White);

        //if(Projectile.alpha < 100) Projectile.frame = 0;
        //else if(Projectile.alpha < 200) Projectile.frame = 1;
        //else Projectile.frame = 2;
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