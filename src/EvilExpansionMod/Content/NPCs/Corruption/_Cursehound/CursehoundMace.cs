using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent; // For TextureAssets

namespace EvilExpansionMod.Content.NPCs.Corruption;

public sealed class CursehoundMace : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Cursehound.KEY_CursehoundMace;

    public enum State {
        Launched,
        Embedded,
        Retracting
    }

    private State CurrentState {
        get => (State)Projectile.ai[0];
        set {
            if (Projectile.ai[0] != (float)value)
            {
                Projectile.ai[0] = (float)value;
                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
            }
        }
    }

    public ref float Timer => ref Projectile.ai[1];
    public ref float OwningNPCWhoAmI => ref Projectile.ai[2];

    private const int EmbeddedDuration = 45;
    private const float RetractSpeed = 20f;

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

    public override void OnSpawn(IEntitySource source) { }

    public override bool ShouldUpdatePosition() => CurrentState == State.Launched || CurrentState == State.Retracting;

    public override void AI() {
        NPC owningNPC = Main.npc[(int)OwningNPCWhoAmI];

        if (!owningNPC.active || owningNPC.type != ModContent.NPCType<CursehoundNPC>()) {
            if (CurrentState != State.Retracting) {
                CurrentState = State.Retracting;
                Projectile.tileCollide = false;
                Projectile.friendly = false;
                Projectile.extraUpdates = 1;
            }
            if (!owningNPC.active) Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60);
        } 

        switch (CurrentState) {
            case State.Launched:
                Projectile.velocity.Y += 0.5f;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Projectile.velocity.Length() > 25f) {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 25f;
                }
                break;

            case State.Embedded:
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Timer++;

                if (Timer >= EmbeddedDuration + 60) {
                    CurrentState = State.Retracting;
                    Projectile.tileCollide = false;
                }
                break;

            case State.Retracting:
                Vector2 targetPosition = owningNPC.Center + new Vector2(owningNPC.direction * 70, -30);

                if (Projectile.Distance(targetPosition) < RetractSpeed) {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = Projectile.DirectionTo(targetPosition) * RetractSpeed;

                Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
                break;
        }

        Projectile.rotation %= MathHelper.TwoPi;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (CurrentState == State.Launched) {
            CurrentState = State.Embedded;
            SoundEngine.PlaySound(Assets.Assets.Sounds.Cursehound.MaceSlam, Vector2.Lerp(Main.LocalPlayer.Center, Projectile.Center, 0.7f));
            
            for (int i = 0; i < 5; i++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
            }
        }
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<MaceCrack>(), 0, 0);
        
        int amount = Main.rand.Next(4, 8);
        
        for (int k = 0; k < amount; k++) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4 * 0.7f) * Main.rand.NextFloat(4f, 8f), ModContent.ProjectileType<MaceShard>(), Projectile.damage / 2, 0, Projectile.owner);
        }
        
        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.6f));

        for (int i = 0; i < 16; i++) {
            Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(120f, 20f);
            Vector2 dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f);
            dustVelocity += Projectile.Center.DirectionTo(dustPos) * 5f;
            dustPos += Vector2.UnitY * Projectile.height * 1.5f;

            Color dustColorStart = new Color(133, 122, 94);
            Color dustColorFade = dustColorStart * 0.4f;

            var newDustData = new Smoke.Data() {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.8f,
                ColorStart = dustColorStart,
                ColorFade = dustColorFade,
                Spin = 0.03f,
                InitialScale = Main.rand.NextFloat(1f, 2f)
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

        if (owningNPC.active && owningNPC.type == ModContent.NPCType<CursehoundNPC>()) {
            Vector2 chainOrigin = owningNPC.Center + new Vector2(owningNPC.direction * 30, -30);

            var diff = Projectile.Center - chainOrigin;
            float length = diff.Length();
            var unit = diff / length;
            float rotation = unit.ToRotation() + MathHelper.PiOver2;
            
            var chainTexture = Assets.Assets.Textures.NPCs.Corruption.Cursehound.CursehoundMace_Chain.Value;

            var chainRect = new Rectangle(0, 0, 10, 12);
            var baseRect = new Rectangle(0, 14, 10, 10);

            float drawLength = chainRect.Height;
            for (float i = 0f; i < length; i += drawLength) {
                Vector2 drawPos = chainOrigin + unit * i - Main.screenPosition;
                Main.EntitySpriteDraw(chainTexture, drawPos, chainRect, lightColor, rotation, chainRect.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
            
            Main.EntitySpriteDraw(chainTexture, chainOrigin, baseRect, lightColor, 0, baseRect.Size() / 2f, 1f, SpriteEffects.None, 0);
        }

        Texture2D maceTexture = TextureAssets.Projectile[Type].Value;
        Texture2D maceGlow = Assets.Assets.Textures.NPCs.Corruption.Cursehound.CursehoundMace_Glow.Value;
        Vector2 drawOrigin = maceTexture.Size() / 2f;
        Main.EntitySpriteDraw(maceTexture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(maceGlow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
    
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        if (Projectile.velocity.Y > 0 && CurrentState == State.Launched) {
            fallThrough = false;
        }
        else {
            fallThrough = true;
        }

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
}

internal class MaceShard : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Cursehound.KEY_CursehoundMace_Chain;

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

    public override void AI() {
        Projectile.velocity.Y += 0.3f;
        Projectile.rotation += Projectile.velocity.X * 0.1f;

        Projectile.velocity.X *= 0.99f;

        if (Projectile.timeLeft < 30)
            Projectile.alpha = (int)((1 - Projectile.timeLeft / 30f) * 255);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.penetrate <= 0)
            return true;

        Projectile.velocity.Y += oldVelocity.Y * -0.8f;

        return false;
    }

    // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    //     target.AddBuff(BuffID.Cursed, 60);
    // }
}