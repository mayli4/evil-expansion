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

public class HeadPounderCharge : ModProjectile {
    private bool InitialVisualSet = false;
    private Vector2 relativeOffset;
    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.HeadPounderCharge.KEY;
    public override void SetDefaults() {
        Projectile.width = 48;
        Projectile.height = 48;
        Projectile.DamageType = DamageClass.Default;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.scale = 10f;
        Projectile.alpha = 255;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.scale = Projectile.ai[0]; // Projectile. ai0 is the starting pulse size,
        Projectile.alpha = 255;

        Player player = Main.player[Projectile.owner];
        relativeOffset = Projectile.Center - player.Center;
    }
    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.6f, 0.8f, 0.0f);

        if(Projectile.scale > 0f) {
            Projectile.scale -= Projectile.ai[1]; // Projectile. ai1 is the "speed" of the pulse
            Projectile.ai[1] *= 1.1f;
        }
        else {
            Projectile.Kill();
        }

        if(Projectile.alpha > 0) {
            Projectile.alpha += -5;
        }
        else Projectile.alpha = 0;

        Player player = Main.player[Projectile.owner];
        Projectile.Center = player.Center + relativeOffset;

        //Main.NewText("Projectile.scale" + Projectile.scale, Microsoft.Xna.Framework.Color.White);
        Main.NewText("Projectile.alpha" + Projectile.alpha, Microsoft.Xna.Framework.Color.White);
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
        float fadeOut = 1f - (Projectile.alpha / 255); // If Alpha = 255, fadeOut = 0, if Alpha = 0, fadeOut = 1
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