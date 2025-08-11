using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public sealed class BloodWarden : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_BloodWarden;

    public enum State {
        Idle,
        Attacking
    }

    public State CurrentState
    {
        get => (State)Projectile.ai[0];
        set
        {
            Projectile.ai[0] = (float)value;
            Timer = 0;
            Projectile.netUpdate = true;
        }
    }

    private ref float Timer => ref Projectile.ai[1];
    private ref float TargetNPCID => ref Projectile.ai[2];

    public Player Owner => Main.player[Projectile.owner];

    private const float AttackRange = 300f;
    private const float MaxFollowSpeed = 10f;
    private const float MaxAttackSpeed = 15f;
    private const float IdleHoverAmplitude = 10f;
    private const float IdleHoverFrequency = 0.05f;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 13;
        Main.projPet[Projectile.type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 90;
        Projectile.height = 90;

        Projectile.tileCollide = false;

        Projectile.minion = true; 
        Projectile.penetrate = -1;
        Projectile.timeLeft = 5000;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
        
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => false;

    public override void AI() {
        if (Owner.HasBuff<BloodWardenBuff>())
            Projectile.timeLeft = 2;
        else
            Projectile.Kill();

        Projectile.spriteDirection = Owner.direction;
        
        NPC target = FindTarget();

        if (CurrentState == State.Idle) {
            if (target != null) {
                CurrentState = State.Attacking;
                TargetNPCID = target.whoAmI;
            }
        }
        else if (CurrentState == State.Attacking) {
            if (target == null || target.whoAmI != TargetNPCID || !target.active || !target.CanBeChasedBy(this)) {
                CurrentState = State.Idle;
                TargetNPCID = -1;
            }
        }

        if (CurrentState == State.Idle) {
            DoIdleMovement();
        }
        else if (CurrentState == State.Attacking) {
            DoAttackMovement(target);
            if (Projectile.Distance(target.Center) < Projectile.width + 20) {
                Projectile.Center = Vector2.Lerp(Projectile.Center, target.Center, 0.1f);
            }
        }
        
        Timer++;
    }

    private NPC FindTarget() {
        NPC bestTarget = null;
        float bestDistanceSq = AttackRange * AttackRange;

        for (int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if (npc.active && !npc.friendly && !npc.dontTakeDamage && !npc.immortal && !npc.hide && npc.CanBeChasedBy(this, false)) {
                float distanceSq = Projectile.DistanceSQ(npc.Center);
                if (distanceSq < bestDistanceSq) {
                    bestDistanceSq = distanceSq;
                    bestTarget = npc;
                }
            }
        }
        return bestTarget;
    }

    private void DoIdleMovement() {
        float horizontalOffset = (Owner.direction == 1) ? -80f : 80f;

        var targetPos = Owner.Center + new Vector2(horizontalOffset, -30f);

        targetPos.Y += (float)Math.Sin(Main.GameUpdateCount * IdleHoverFrequency + Projectile.whoAmI * 0.1f) * IdleHoverAmplitude;

        Vector2 vectorToTarget = targetPos - Projectile.Center;
        float distance = vectorToTarget.Length();

        if (distance > 20f) { 
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, vectorToTarget.SafeNormalize(Vector2.Zero) * MaxFollowSpeed, 0.1f);
        } else {
            Projectile.velocity *= 0.9f;
        }

        if (Projectile.velocity.Length() < 0.1f) {
            Projectile.velocity.Y += 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
        }
    }

    private void DoAttackMovement(NPC target) {
        if (target == null) {
            CurrentState = State.Idle;
            return;
        }

        Vector2 vectorToTarget = target.Center - Projectile.Center;
        float distance = vectorToTarget.Length();

        if (distance > 20f) {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, vectorToTarget.SafeNormalize(Vector2.Zero) * MaxAttackSpeed, 0.1f);
        } else {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.1f);
        }

        Projectile.spriteDirection = (target.Center.X < Projectile.Center.X) ? -1 : 1;
    }

    public override bool PreDraw(ref Color lightColor) {
        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        
        var origin = new Vector2(texture.Width / 2f, texture.Height / Main.projFrames[Projectile.type] / 2f);
        
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            null,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            effects,
            0f
        );
        
        return false;
    }
}