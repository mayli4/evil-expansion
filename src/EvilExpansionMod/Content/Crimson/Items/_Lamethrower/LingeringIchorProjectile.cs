using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EvilExpansionMod.Content.Crimson;

public class LingeringIchorProjectile : ModProjectile, ITileMask {
    public override string Texture => Helper.PlaceholderTextureKey;
    readonly static int DisappearFrames = 40;
    public override void SetDefaults() {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10; // Hits enemies up to 6 times per second
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Oiled, 60 * 20, false);
        target.AddBuff(BuffID.Ichor, 60 * 15, false);
        target.AddBuff(BuffID.OnFire3, 60 * 2, false);
    }
    int Compustiontimer = 1;
    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if(Main.rand.NextBool(70)) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IchorTorch);
        }

        if (Compustiontimer != 510){
            Compustiontimer++;

        }
        if(Compustiontimer == 400) {
            SoundStyle ignite = new SoundStyle($"EvilExpansionMod/Assets/Sounds/LameThrower/MinecraftFlintnSteel");
            SoundEngine.PlaySound(ignite, Projectile.position);
        }

        if(Compustiontimer >= 400) {

            if(Main.rand.NextBool(5)) {

                var particle = SmokeParticle.Pool.RequestParticle();

                Vector2 randomVelocity = new Vector2(
                    Main.rand.NextFloat(-1.5f, 1.5f),
                    Main.rand.NextFloat(-2f, -0.5f)
                );

                Color smokeColor = Color.Lerp(Color.White, Color.Black, Main.rand.NextFloat());
                float scale = Main.rand.NextFloat(0.2f, 1.0f);
                int lifetime = Main.rand.Next(20, 40);

                particle.Spawn(Projectile.Left - Main.rand.NextVector2Circular(10, 0), randomVelocity, smokeColor, scale, lifetime);
            }

            if(Main.rand.NextBool(600 / Compustiontimer) && Compustiontimer != 510) {
                int dustIndex = Dust.NewDust(
                    Projectile.Left,
                    Projectile.width,
                    Projectile.height + -16,
                    DustID.InfernoFork,
                    0,
                    -3f,
                    (int)Main.rand.NextFloat(0f, 100f),
                    default,
                    Main.rand.NextFloat(0.9f, 0.1f)

                );
            }
        }
        
        if(Compustiontimer == 509) { // Create "Compustion" projectiles
            //Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier((1), 0.10f));
            SoundEngine.PlaySound(SoundID.Item100 with {
                Volume = 0.4f,
                PitchRange = (0f, 1.5f),
                MaxInstances = 40,
                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
            }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item74 with {
                Volume = 0.2f,
                PitchRange = (-0.5f, 0.5f),
                MaxInstances = 40,
                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
            }, Projectile.position);
            for(int i = 0; i < 3; i++) { 
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + new Vector2(0, 0),
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(12f, 1f)),
                    ModContent.ProjectileType<IchorCompustion>(),
                    Projectile.damage * 3, // Triple damage
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }

    public void DrawTileMask(SpriteBatch spriteBatch) {
        var alpha = MathF.Min(Projectile.timeLeft / (float)DisappearFrames, 1f);
        var texture = Assets.Images.Crimson.Items.Lamethrower.IchorSplat.Asset.Value;
        spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition - Vector2.UnitY * 16f,
            null,
            Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * alpha,
            0f,
            new(texture.Width / 2f, 0f),
            1f,
            SpriteEffects.None,
            0f
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var alpha = MathF.Min(Projectile.timeLeft / (float)DisappearFrames, 1f);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            new Color(241, 194, 92) * alpha * 0.05f,
            0f,
            glowTexture.Size() / 2f,
            Main.rand.NextFloat() * 0.1f + 0.9f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}

public class IchorCompustion : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.IchorFlame.KEY;   
    int flameVariant = Main.rand.Next(1, 4);
    
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 18;
    }
    
    public override void SetDefaults() {
        Projectile.width = 16 * 2;
        Projectile.height = 16 * 12;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 6; // Hits enemies up to 10 times per second
        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = -1;
    }
    public override void PostDraw(Color lightColor) {
       
        Texture2D FlameGlowmask = Assets.Images.Crimson.Items.Lamethrower.IchorFlame_glow.Asset.Value;

        Main.EntitySpriteDraw(FlameGlowmask, Projectile.position - Main.screenPosition, Utils.Frame(FlameGlowmask, 1, Main.projFrames[Projectile.type], 0, Projectile.frame), Color.White, 0f, new Vector2(0,0), 1f, SpriteEffects.None);
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;
        Projectile.frame = flameVariant switch
        {
            1 => 0,
            2 => 6,
            3 => 12,
            _ => Projectile.frame
        };

    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Ichor, 60 * 10, false);
        target.AddBuff(BuffID.OnFire3, 60 * 1, false);
    }
    public override void AI() {
        if(Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }

        Projectile.velocity *= 0.94f;
        Projectile.rotation = 180 * MathHelper.PiOver2;

        Projectile.frameCounter++;
        if(Projectile.frameCounter >= 6) {
            Projectile.frameCounter = 0;

            Projectile.frame++;

            if(flameVariant == 1)
                if(Projectile.frame >= 5) {
                    Projectile.Kill();
                }
            if(flameVariant == 2)
                if(Projectile.frame >= 11) {
                    Projectile.Kill();
                }
            if(flameVariant == 3)
                if(Projectile.frame >= 17) {
                    Projectile.Kill();
                }
        }
        if(Main.rand.NextBool(10)) {
            int dustIndex = Dust.NewDust(
                Projectile.Top,
                Projectile.width,
                Projectile.height,
                DustID.Torch,
                Main.rand.NextFloat(-3f, 3f),
                Main.rand.NextFloat(-7f, 0f),
                (int)Main.rand.NextFloat(0f, 100f),
                default,
                Main.rand.NextFloat(0.5f, 1.5f)

            );
        }
    }
}

