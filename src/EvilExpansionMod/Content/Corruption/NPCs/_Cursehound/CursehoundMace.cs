using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent; // For TextureAssets
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.GlobalTileHooks;

namespace EvilExpansionMod.Content.Corruption;

public sealed class CursehoundMace : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.CursehoundMace.KEY;

    public enum State {
        Launched,
        Embedded,
        Retracting
    }

    private State CurrentState {
        get => (State)Projectile.ai[0];
        set {
            if(Projectile.ai[0] != (float)value) {
                Projectile.ai[0] = (float)value;
                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
            }
        }
    }

    public ref float Timer => ref Projectile.ai[1];
    public ref float OwningNPCWhoAmI => ref Projectile.ai[2];

    private const int embedded_duration = 45;
    private const float retract_speed = 20f;

    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.rotation = 0f;
        Projectile.timeLeft = 60 * 5;
        Projectile.netImportant = true;
    }

    public override void OnSpawn(IEntitySource source) {
        float LavaMaceCounterweight;
        if(Projectile.wet) { LavaMaceCounterweight = 2.5f; }
        else { LavaMaceCounterweight = 1f; }

        Projectile.velocity.Y *= LavaMaceCounterweight;
    }

    public override bool ShouldUpdatePosition() => CurrentState == State.Launched || CurrentState == State.Retracting;

    public override void AI() {
        NPC owningNPC = Main.npc[(int)OwningNPCWhoAmI];

        if(!owningNPC.active || owningNPC.type != ModContent.NPCType<CursehoundNPC>()) {
            if(CurrentState != State.Retracting) {
                CurrentState = State.Retracting;
                Projectile.tileCollide = false;
                Projectile.friendly = false;
                Projectile.extraUpdates = 1;
            }
            if(!owningNPC.active) Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60);
        }

        switch(CurrentState) {
            case State.Launched:
                Projectile.velocity.Y += 0.5f;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if(Projectile.velocity.Length() > 25f) {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 25f;
                }
                break;

            case State.Embedded:
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Timer++;

                if(Timer >= embedded_duration + 60) {
                    CurrentState = State.Retracting;
                    Projectile.tileCollide = false;
                }
                break;

            case State.Retracting:
                Vector2 targetPosition = owningNPC.Center + new Vector2(owningNPC.direction * 70, -30);

                if(Projectile.Distance(targetPosition) < retract_speed) {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = Projectile.DirectionTo(targetPosition) * retract_speed;

                Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
                break;
        }

        Projectile.rotation %= MathHelper.TwoPi;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if(CurrentState == State.Launched) {
            CurrentState = State.Embedded;

            for(int i = 0; i < 5; i++) {
                int soundNum = Main.rand.Next(1, 4);
                SoundStyle caltropSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacballandchainhit{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(-0.5f, 0.5f),
                    Volume = Main.rand.NextFloat(0.3f, 1f) + (Projectile.velocity.Y / 50),
                };
            }
            SoundEngine.PlaySound(Assets.Sounds.Cursehound.MaceSlam.Asset, Vector2.Lerp(Main.LocalPlayer.Center, Projectile.Center, 0.7f));
            SoundEngine.PlaySound(SoundID.NPCHit52 with { Volume = 0.8f });

            for(int i = 0; i < 18; i++) {
                Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -0.5f)); // More charge = crazier dust burst
                int ds = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Stone);
                Main.dust[ds].velocity = Vector2.Max(burstVelocity * 1.2f, burstVelocity);
                Main.dust[ds].scale = Math.Max(Main.rand.NextFloat(0.45f, 0.27f), 0.6f);
            }
            for(int i = 0; i < Math.Max(14, 10); i++) {
                Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-8f, -4f)); // Likewise
                int dc = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.CursedTorch);
                Main.dust[dc].velocity = Vector2.Max(burstVelocity * 0.8f, burstVelocity);
                Main.dust[dc].scale = Math.Max(Main.rand.NextFloat(0.45f, 0.27f), 0.55f);
                //Main.dust[d].noGravity = true;
            }
            for(int i = 0; i < Math.Max(12, 7); i++) {
                Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-8f, -4f)); // Likewise
                int dp = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Enchanted_Pink);
                Main.dust[dp].velocity = Vector2.Max(burstVelocity * 0.6f, burstVelocity);
                Main.dust[dp].scale = Math.Max(Main.rand.NextFloat(0.45f, 0.27f), 1.2f);
                Main.dust[dp].color = Color.HotPink;
                //Main.dust[d].noGravity = true;
            }
        }
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 12, Vector2.Zero, ModContent.ProjectileType<MaceCrack>(), 0, 0);

        int amount = (int) (Main.rand.Next(4, 8) * (Main.expertMode ? 1.5f : 1f));

        for(int k = 0; k < amount; k++) {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4 * 0.7f) * Main.rand.NextFloat(6f, 12f) * (Main.expertMode ? 1.3f : 1f),
                ModContent.ProjectileType<MaceDebris>(),
                Projectile.damage / 3,
                0,
                Projectile.owner);
        }

        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.6f));

        for(int i = 0; i < 20; i++) {
            Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(120f, 20f);
            Vector2 dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f);
            dustVelocity += Projectile.Center.DirectionTo(dustPos) * 6f;
            dustPos += Vector2.UnitY * Projectile.height * 1.5f;

            Color dustColorStart = new Color(133, 122, 94);
            Color dustColorFade = dustColorStart * 0.4f;

            var newDustData = new Smoke.Data()
            {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.8f,
                ColorStart = dustColorStart,
                ColorFade = dustColorFade,
                Spin = 0.03f,
                InitialScale = Main.rand.NextFloat(0.75f, 1.2f)
            };

            var newDust = Dust.NewDustPerfect(
                dustPos,
                ModContent.DustType<Smoke>(),
                dustVelocity,
                0,
                newColor: Color.White,
                newDustData.InitialScale
            );

            newDust.customData = newDustData;
        }

        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        //idk?
    }

    public override bool PreDraw(ref Color lightColor) {
        NPC owningNPC = Main.npc[(int)OwningNPCWhoAmI];

        if(owningNPC.active && owningNPC.type == ModContent.NPCType<CursehoundNPC>()) {
            Vector2 chainOrigin = owningNPC.Center + new Vector2(owningNPC.direction * 30, -30) - Vector2.UnitY * 26f;

            var diff = Projectile.Center - chainOrigin;
            float length = diff.Length();
            var unit = diff / length;
            float rotation = unit.ToRotation() + MathHelper.PiOver2;

            var chainTexture = Assets.Images.Corruption.NPCs.Cursehound.CursehoundMace_Chain.Asset.Value;

            var chainRect = new Rectangle(0, 0, 10, 12);
            var baseRect = new Rectangle(0, 14, 10, 10);

            float drawLength = chainRect.Height;
            for(float i = 0f; i < length; i += drawLength) {
                Vector2 drawPos = chainOrigin + unit * i - Main.screenPosition;
                Main.EntitySpriteDraw(chainTexture, drawPos, chainRect, lightColor, rotation, chainRect.Size() / 2f, 1f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(chainTexture, chainOrigin, baseRect, lightColor, 0, baseRect.Size() / 2f, 1f, SpriteEffects.None, 0);
        }

        Texture2D maceTexture = TextureAssets.Projectile[Type].Value;
        Texture2D maceGlow = Assets.Images.Corruption.NPCs.Cursehound.CursehoundMace_Glow.Asset.Value;
        Vector2 drawOrigin = maceTexture.Size() / 2f;
        Main.EntitySpriteDraw(maceTexture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(maceGlow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        if(Projectile.velocity.Y > 0 && CurrentState == State.Launched) {
            fallThrough = false;
        }
        else {
            fallThrough = true;
        }

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
}

internal class MaceDebris : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.Debris.KEY;

    private Rectangle _frame;
    Color glowColor => Color.White * ((255f - Projectile.alpha * 2f) / 255f);

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 320;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 15;
        Projectile.aiStyle = -1;
        Projectile.penetrate = 15;
    }

    public override void OnSpawn(IEntitySource source) {
        _frame = new Rectangle(26 * Main.rand.Next(3), 0, 24, 24);
    }

    public override void AI() {
        Projectile.velocity.Y += 0.3f;
        Projectile.rotation += Projectile.velocity.X * 0.1f;

        Projectile.velocity.X *= 0.99f;

        if(Projectile.timeLeft < 45)
            Projectile.alpha = (int)((1 - Projectile.timeLeft / 45f) * 255);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if(Projectile.penetrate <= 0)
            return true;
        //Main.NewText("Projectile.velocity.Y" + Projectile.velocity.Y, Microsoft.Xna.Framework.Color.White);
        if(Projectile.velocity.Y > 1f) {
            int soundNum = Main.rand.Next(1, 4);
            SoundStyle caltropSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacballandchainhit{soundNum}")
            {
                Pitch = Main.rand.NextFloat(-0.5f, 0.5f),
                Volume = Main.rand.NextFloat(0.0f, 0.05f) + (Projectile.velocity.Y / 50),
            };

            SoundEngine.PlaySound(caltropSound, Projectile.position);
        }
        Projectile.velocity.Y += oldVelocity.Y * -0.8f;
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var tex = TextureAssets.Projectile[Type].Value;
        Texture2D maceGlow = Assets.Images.Corruption.NPCs.Cursehound.Debris_Glow.Asset.Value;

        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, _frame, Projectile.GetAlpha(lightColor), Projectile.rotation, _frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(maceGlow, Projectile.Center - Main.screenPosition, _frame, glowColor, Projectile.rotation, _frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}