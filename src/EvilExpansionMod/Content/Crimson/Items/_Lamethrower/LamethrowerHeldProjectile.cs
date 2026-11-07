using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerHeldProjectile : ModProjectile {
    public Player Owner => Main.player[Projectile.owner];
    private Vector2 rotationVector;
    private int pus = 0;
    
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.LamethrowerHeldSprite.KEY;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 15;
    }

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 6;
        Projectile.usesLocalNPCImmunity = true;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Oiled, 1200);
        target.AddBuff(BuffID.Ichor, 900);
        target.AddBuff(BuffID.OnFire3, 120);
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        rotationVector = Projectile.Center.DirectionTo(Main.MouseWorld);
        Projectile.rotation = rotationVector.ToRotation();
    }

    public override bool PreAI() {
        if(Owner.HeldItem.type != ModContent.ItemType<LamethrowerItem>()) {
            Projectile.Kill();
            return false;
        }

        if(Owner.channel) {
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);
            Owner.itemTime = Projectile.timeLeft;
            Owner.itemAnimation = Projectile.timeLeft;
        }
        else
            Projectile.Kill();
        return true;

    }

    public override void AI() {

        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        if(pus != 200) {
            Projectile.frame = (pus / 25);
        }
        if(pus == 200) {
            Projectile.frameCounter++;
            if(Projectile.frameCounter >= 8) {
                Projectile.frameCounter = 0;

                Projectile.frame++;
                if(Projectile.frame >= 15) {
                    Projectile.frame = 9;
                }
            }
        }

        Owner.heldProj = Projectile.whoAmI;

        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        rotationVector = Vector2.Lerp(mouseDirection, rotationVector, 0.2f);

        Projectile.rotation = rotationVector.ToRotation();

        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full,
            Projectile.rotation - MathHelper.PiOver2
        );
        if(pus != 200) {
            pus++;
        }
    }

    public override void OnKill(int timeLeft) {
        for(int i = 0; i < (pus / 20); i++) { 
            if(Main.myPlayer != Projectile.owner) return;

            Vector2 spawnPosition = Projectile.Center + rotationVector;
            SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.8f } with { PitchRange = (0.4f, 1f) });
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                Main.rand.NextFloat(5f, 6f) * rotationVector * MathHelper.PiOver2,
                ModContent.ProjectileType<Glop>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];
        Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

        var origin = new Vector2(-8, 18);
        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            sourceRectangle,
            lightColor,
            Projectile.rotation - (Owner.direction == -1 ? MathF.PI : 0f),
            Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Vector2.One * Projectile.scale,
            Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        return false;
    }

    public override void PostDraw(Color lightColor) {
        Texture2D chargeFront = Assets.Images.Crimson.Items.Lamethrower.LameThrowerChargebarFront.Asset.Value;
        Texture2D chargeBack = Assets.Images.Crimson.Items.Lamethrower.LameThrowerChargebarBack.Asset.Value;
        int backBarWidth = 46;
        int backBarHeight = 14;
        int frontBarWidth = 46;
        int frontBarHeight = 14;
        int cutoff = frontBarWidth * pus;
        Vector2 pivotPoint = new(backBarWidth / 2f, backBarHeight / 2f);
        Vector2 position = new Vector2(0,32) + Owner.Bottom - Main.screenPosition;
        Vector2 frontPos = position - pivotPoint;
        Rectangle frontCrop = new(0, 0, (frontBarWidth * pus / 200), frontBarHeight); 

        Main.EntitySpriteDraw(chargeBack, position, null, Color.White, 0f, pivotPoint, 1f, SpriteEffects.None);
        Main.EntitySpriteDraw(chargeFront, frontPos, frontCrop, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None);
    }
}


public class Glop : ModProjectile, IPreDrawEverythingBulk<Glop> {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.PusGlop.KEY;
    private int glopVariant = Main.rand.Next(1, 4);
    private Vector2[] trailPositions;
    public static float Gravity = 0.2f;
    
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 12;
    }
    
    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = -1;
    }
    
    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;
        
        Projectile.frame = glopVariant switch
        {
            1 => 0,
            2 => 4,
            3 => 8,
            _ => Projectile.frame
        };
    }

    public override void AI() {
        Projectile.velocity.Y += Gravity * 1.1f;
        if(Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }

        Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.02f, 0.02f));
        Projectile.rotation = Projectile.velocity.ToRotation();

        trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 10)];
        for(int i = trailPositions.Length - 1; i > 0; i--) {
            trailPositions[i] = trailPositions[i - 1];
        }
        trailPositions[0] = Projectile.Center + Projectile.velocity;


        Projectile.frameCounter++;
        if(Projectile.frameCounter >= 8) {
            Projectile.frameCounter = 0;

            Projectile.frame++;

            if (glopVariant == 1)
                if(Projectile.frame >= 3) {
                    Projectile.frame = 0;
                }
            if(glopVariant == 2)
                if(Projectile.frame >= 7) {
                    Projectile.frame = 4;
                }
            if(glopVariant == 3)
                if(Projectile.frame >= 11) {
                    Projectile.frame = 8;
                }
        }

    }
    public void PreDrawEverythingBulk(ReadOnlySpan<Glop> entities) {
        using var pipeline = new RenderPipeline(Graphics.PreDrawNPCsQueue, 0.5f, Graphics.WorldTransformMatrix);

        var trailEffect = Assets.Shaders.Pixel.CursedSpiritFire.Asset.Value;
        var noise = Assets.Images.Sample.Noise2.Asset.Value;
        var texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];

        foreach(var entity in entities) {
            var proj = entity.Projectile;
            if(entity.trailPositions is null || entity.trailPositions.Length < 2) continue;

            var lightColor = Lighting.GetColor(proj.Center.ToTileCoordinates());
            var source = new Vector4(0f, proj.frame * frameHeight, texture.Width, frameHeight);
            var origin = new Vector2(source.Z, source.W) / 2f;

            pipeline
                .SetTexture(0, noise)
                .SetTexture(1, noise)
                .SetEffectParams(
                    trailEffect,
                    ("uTime", 0.0075f * Main.GameUpdateCount + proj.whoAmI * 34.432f),
                    ("uStepY", 0.176f),
                    ("uColor1", new Color(141, 73, 6)),
                    ("uColor2", new Color(163, 118, 19)),
                    ("uStepColor", 0.07f),
                    ("uScale", 0.5f))
                .DrawTrail(
                    entity.trailPositions,
                    13f * proj.scale,
                    Color.White,
                    trailEffect)
                .DrawTexture(new()
                {
                    Texture = texture,
                    Position = proj.Center,
                    Color = lightColor,
                    Source = source,
                    Rotation = proj.rotation,
                    Origin = origin,
                    Scale = Vector2.One * proj.scale,
                });
        }

        pipeline.ApplyOutline(new Color(70, 4, 0));
    }

    public override void OnKill(int timeLeft) {
        for(int i = 0; i < 4; i++) {
            int dustIndex = Dust.NewDust(
                    Projectile.Center,
                    0,
                    0,
                    ModContent.DustType<SmallCrytapeteTear>(),
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    0,
                    default,
                    Main.rand.NextFloat(1f, 2f)
            );
            Main.dust[dustIndex].noGravity = false;

            SoundEngine.PlaySound(SoundID.NPCHit9 with { Volume = 0.8f } with { PitchRange = (0.1f, 1f) });
        }

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<TearDeath>(),
            0,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
        );

        SoundEngine.PlaySound(SoundID.Drip with {
            Pitch = Main.rand.NextFloat(0f, 1.5f),
            Volume = Main.rand.NextFloat(0.1f, 0.2f)
        }, 
            Projectile.position);
        
        Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<LingeringIchorProjectile>(),
                Projectile.damage,
                0f);
    }
}
