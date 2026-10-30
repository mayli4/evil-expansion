using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class StalactiteProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.Stalactites.KEY;

    private Rectangle frame;

    public int StaticTelegraphTimer = 60 * 1;
    private int TelegraphTimer;
    private float TelegraphMultiplier;

    float shakeIntensity = 10f;
    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 50;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 30;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = false;
        Projectile.alpha = 255;
    }
    public override void OnSpawn(IEntitySource source) {
        frame = new Rectangle(18 * Main.rand.Next(3), 0, 16, 50);
        TelegraphTimer = (int)(StaticTelegraphTimer * Main.rand.NextFloat(0.66f , 1.5f));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.BrokenArmor, 60 * 5);
    }
    public override void AI() {
        TelegraphMultiplier = 1f + (float)(StaticTelegraphTimer - TelegraphTimer) / StaticTelegraphTimer;

        if(TelegraphTimer > 0) {
            TelegraphTimer--;
            if (Projectile.alpha > 0) Projectile.alpha += -10;
            if(Main.rand.NextBool(3)) {
                for(int i = 0; i < TelegraphMultiplier; i++) {
                    Vector2 dustVelocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(0f, 5f));

                    Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.Demonite,
                        dustVelocity * TelegraphMultiplier,
                        Main.rand.Next(0, 255) - (int)(10 * TelegraphMultiplier),
                        default,
                        Main.rand.NextFloat(0.1f, 0.6f) * TelegraphMultiplier
                        );

                    Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.Bone,
                        dustVelocity * 1.5f * TelegraphMultiplier,
                        Main.rand.Next(0, 255) - (int)(10 * TelegraphMultiplier),
                        default,
                        Main.rand.NextFloat(0.1f, 0.6f) * TelegraphMultiplier
                        );

                    Dust.NewDustPerfect(
                        Projectile.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(50f, 250f)),
                        DustID.Smoke,
                        dustVelocity * 1.5f * TelegraphMultiplier,
                        Main.rand.Next(150, 255) - (int)(10 * TelegraphMultiplier),
                        default,
                        Main.rand.NextFloat(0.1f, 1f) * TelegraphMultiplier
                        );

                    Dust.NewDustPerfect(
                        Projectile.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(125f, 175f)),
                        DustID.Smoke,
                        dustVelocity * 1.5f * TelegraphMultiplier,
                        Main.rand.Next(150, 255) - (int)(10 * TelegraphMultiplier),
                        default,
                        Main.rand.NextFloat(0.1f, 1f) * TelegraphMultiplier
                        );
                }
            }
        }
        else {
            Projectile.velocity.Y += 0.6f;
            if(Projectile.velocity.Y > 25f) {
                Projectile.velocity.Y = 25f;
            }
            Projectile.velocity.X = 0f;

            Projectile.ai[0]++;

            if(Projectile.ai[0] >= 30) {
                Projectile.tileCollide = true;
            }

            if(Projectile.timeLeft < 60) {
                Projectile.alpha = (int)((1 - Projectile.timeLeft / 60f) * 255);
            }
            else {
                Projectile.alpha = 0;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Item70 with
        { Pitch = Main.rand.NextFloat(-0.2f, 1.2f), Volume = 0.25f },
        Projectile.position
        );

        if(Projectile.velocity.Y > 1f) {
            int soundNum = Main.rand.Next(1, 4);
            SoundStyle caltropSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacstone_impact{soundNum}")
            {
                Pitch = Main.rand.NextFloat(-0.8f, 0.3f),
                Volume = Main.rand.NextFloat(0.4f, 0.8f),
            };

            SoundEngine.PlaySound(caltropSound, Projectile.position);
        }

        for(int i = 0; i < 25; i++) {
            Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-5f, -0f));
            Vector2 Spawnpt = Projectile.Center + Main.rand.NextVector2Circular(15f, 30f) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(15f, 20f));
            int dc = Dust.NewDust(Spawnpt, 5, -10, DustID.Dirt, default, default, default, default, Main.rand.NextFloat(0.8f, 1.2f));
            Main.dust[dc].velocity = burstVelocity;
            Main.dust[dc].scale = Main.rand.NextFloat(0f, 1.2f);
            Main.dust[dc].noGravity = false;
        }
        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

        if(TelegraphTimer > 0) {
            shakeIntensity = 0.5f * (float)Math.Pow(TelegraphMultiplier, 2);
            //Main.NewText("StaticTelegraphTimer" + StaticTelegraphTimer, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("TelegraphTimer" + TelegraphTimer, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("TelegraphMult" + TelegraphMultiplier, Microsoft.Xna.Framework.Color.White);
        }
        else shakeIntensity = 0f;
        float shakeOffset = MathF.Sin(Projectile.timeLeft * 2.5f) * shakeIntensity; // Working as intended????

        Vector2 drawPos = (Projectile.Center - Main.screenPosition) + Vector2.UnitX * shakeOffset;

        Main.spriteBatch.Draw(
            texture,
            drawPos,
            frame,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            frame.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }

    public override bool PreKill(int timeLeft) { // Make gores on death
        if(Main.dedServ) return true;
        Vector2 direction = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), -Main.rand.NextFloat(0.5f, 1f));
        for(var i = 0; i < 4; i++) {
            Gore.NewGoreDirect(
                Projectile.GetSource_Death(),
                Projectile.Center + direction * 10f - new Vector2(8, 8),
                direction * Main.rand.NextFloat(6f, 9f) + Projectile.velocity,
                Mod.Find<ModGore>("StalactiteGore" + i).Type
            );
        }
        return true;
    }
}