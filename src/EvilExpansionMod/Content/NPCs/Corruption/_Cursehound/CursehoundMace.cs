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

public sealed class CursehoundMace : ModProjectile
{
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Cursehound.KEY_CursehoundMace;

    public enum AIState {
        Launched,
        Embedded,
        Retracting
    }

    private AIState CurrentAIState {
        get => (AIState)Projectile.ai[0];
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

    public override bool ShouldUpdatePosition() => CurrentAIState == AIState.Launched || CurrentAIState == AIState.Retracting;

    public override void AI() {
        NPC owningNPC = Main.npc[(int)OwningNPCWhoAmI];

        if (!owningNPC.active || owningNPC.type != ModContent.NPCType<CursehoundNPC>()) {
            if (CurrentAIState != AIState.Retracting) {
                CurrentAIState = AIState.Retracting;
                Projectile.tileCollide = false;
                Projectile.friendly = false;
                Projectile.extraUpdates = 1;
            }
            if (!owningNPC.active) Projectile.timeLeft = Math.Min(Projectile.timeLeft, 60);
        }


        switch (CurrentAIState) {
            case AIState.Launched:
                Projectile.velocity.Y += 0.5f;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Projectile.velocity.Length() > 25f) {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 25f;
                }
                break;

            case AIState.Embedded:
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Timer++;

                if (Timer >= EmbeddedDuration + 60) {
                    CurrentAIState = AIState.Retracting;
                    Projectile.tileCollide = false;
                }
                break;

            case AIState.Retracting:
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
        if (CurrentAIState == AIState.Launched) {
            CurrentAIState = AIState.Embedded;
            SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);
            for (int i = 0; i < 5; i++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
            }
        }
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<MaceCrack>(), 0, 0);
        
        int amount = Main.rand.Next(4, 8);
        
        for (int k = 0; k < amount; k++) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4 * 0.7f) * Main.rand.NextFloat(4f, 8f), ModContent.ProjectileType<MaceShard>(), Projectile.damage / 2, 0, Projectile.owner);
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