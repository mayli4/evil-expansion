using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;


public sealed class BloodWarden : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.MeatPrisonArmor.BloodWarden.KEY;

    public enum State {
        Idle,
        Attacking
    }

    public State CurrentState {
        get => (State)Projectile.ai[0];
        set {
            Projectile.ai[0] = (float)value;
            Timer = 0;

            Projectile.netUpdate = true;
        }
    }

    ref float TargetId => ref Projectile.ai[1];
    ref float Timer => ref Projectile.ai[2];

    public Player Owner => Main.player[Projectile.owner];
    NPC? Target => TargetId != -1 ? Main.npc[(int)TargetId] : null;
    bool IsTargetValid => Target is not null && Target.active && Target.CanBeChasedBy(this) && !Target.CountsAsACritter;

    const float MAX_ATTACK_RANGE = 30 * 16;
    const float MAX_DISTANCE_FROM_OWNER = 30 * 16;

    const int IDLE_ANIMATION_SPEED = 6;
    const int ATTACK_ANIMATION_SPEED = 3;

    const int RETARGET_DELAY = 4 * 60;

    const float HITBOX_OFFSET = 30;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 13;
        Main.projPet[Projectile.type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 60;
        Projectile.height = 60;

        Projectile.tileCollide = false;

        Projectile.minion = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 5000;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = ATTACK_ANIMATION_SPEED * 2;
        Projectile.manualDirectionChange = true;

        Projectile.damage = 30;

        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanCutTiles() => CurrentState == State.Attacking;
    public override bool MinionContactDamage() => CurrentState == State.Attacking;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return Collision.CheckAABBvAABBCollision(
            projHitbox.TopLeft() + Vector2.UnitX * Projectile.direction * Projectile.Size.X,
            projHitbox.Size(),
            targetHitbox.TopLeft(),
            targetHitbox.Size());
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 6 * 60);
    }

    public override void OnSpawn(IEntitySource source) {
        CurrentState = State.Idle;
    }

    public override void AI() {
        if(Owner.HasBuff<BloodWardenBuff>()) {
            Projectile.timeLeft = 2;
        }
        else {
            Projectile.Kill();
        }

        Projectile.frameCounter++;

        switch(CurrentState) {
            case State.Idle:
                FindBestTarget();

                var distanceToOwner = Projectile.Distance(Owner.Center);
                if(IsTargetValid && distanceToOwner < MAX_ATTACK_RANGE) {
                    CurrentState = State.Attacking;
                    break;
                }

                Projectile.direction = (Owner.Center.X < Projectile.Center.X) ? -1 : 1;
                Projectile.spriteDirection = Projectile.direction;

                var targetPosition = Owner.Center + new Vector2(
                    Owner.direction == 1 ? -80f : 80f,
                    MathF.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 0.1f) * 10f - 30f);

                if(distanceToOwner > MAX_ATTACK_RANGE * 2.25f) {
                    Projectile.Center = targetPosition;
                }

                Vector2 vectorToTarget = targetPosition - Projectile.Center;
                float distance = vectorToTarget.Length();

                if(distance > 20f) {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, vectorToTarget / distance * 10f, 0.1f);
                }
                else {
                    Projectile.velocity *= 0.9f;
                }

                if(Projectile.velocity.Length() < 0.1f) {
                    Projectile.velocity.Y += 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
                }

                if(Projectile.frameCounter >= IDLE_ANIMATION_SPEED) {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if(Projectile.frame > 4) {
                        Projectile.frame = 0;
                    }
                }

                break;
            case State.Attacking:
                if(Owner.HasMinionAttackTargetNPC) {
                    TargetId = Owner.MinionAttackTargetNPC;
                }
                else if(Timer > RETARGET_DELAY) {
                    FindBestTarget();
                    Timer = 0;
                }

                if(!IsTargetValid || Projectile.Distance(Owner.Center) > MAX_ATTACK_RANGE) {
                    CurrentState = State.Idle;
                    TargetId = -1;

                    break;
                }

                Projectile.direction = (Target!.Center.X < Projectile.Center.X) ? -1 : 1;
                Projectile.spriteDirection = Projectile.direction;

                targetPosition = Target!.Center - Projectile.direction * Vector2.UnitX * Projectile.Size.X;
                Projectile.velocity += Projectile.Center.DirectionTo(targetPosition) * 3.5f;
                Projectile.velocity *= 0.8f;

                if(Projectile.frameCounter >= ATTACK_ANIMATION_SPEED) {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if(Projectile.frame > 12) {
                        Projectile.frame = 5;
                    }
                }

                break;
        }

        if(Main.rand.NextBool(12)) {
            var particle = BloodParticle.NewParticle(
                Projectile.Center + Vector2.UnitY * 30f + Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f),
                Vector2.UnitY * Projectile.velocity.Y,
                Main.rand.NextFloat(0.2f, 0.5f),
                new Color(180, 15, 25));
            ParticleEngine.PARTICLES.Add(particle);
        }

        Timer++;
    }

    private void FindBestTarget() {
        NPC? bestTarget = null;
        float bestDistanceSq = MAX_ATTACK_RANGE * MAX_ATTACK_RANGE;

        for(int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if(npc.active && !npc.friendly && !npc.dontTakeDamage && !npc.immortal && !npc.hide && npc.CanBeChasedBy(this, false)) {
                float distanceSq = Owner.Center.DistanceSQ(npc.Center);
                if(distanceSq < bestDistanceSq) {
                    bestDistanceSq = distanceSq;
                    bestTarget = npc;
                }
            }
        }

        TargetId = bestTarget?.whoAmI ?? -1;
    }

    public override bool PreDraw(ref Color lightColor) {
        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var texture = ModContent.Request<Texture2D>(Texture).Value;
        var chainTexture = Assets.Images.Crimson.Items.MeatPrisonArmor.BloodWardenCord.Asset.Value;

        var origin = new Vector2(
            Projectile.spriteDirection == 1 ? 58 : texture.Width - 58,
            texture.Height / Main.projFrames[Projectile.type] / 2f);

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        var source = new Vector4(0f, Projectile.frame * frameHeight, texture.Width, frameHeight);

        var chainStart = Projectile.Center + new Vector2(0f, Projectile.gfxOffY + 10f);
        var chainEnd = Owner.Center;

        List<Vector2> chainPoints = [];
        GenerateWavyChainPoints(chainPoints, chainStart, chainEnd, 20, 5, 0.5f, 0.2f);

        using var pipeline = Graphics.Begin(Graphics.WorldTransformMatrix);

        pipeline
            .SetTexture(chainTexture)
            .DrawTrail(CollectionsMarshal.AsSpan(chainPoints), 6, lightColor);

        pipeline.DrawTexture(new()
        {
            Texture = texture,
            Position = Projectile.Center - Vector2.UnitY * 10f,
            Source = source,
            Color = lightColor,
            Rotation = Projectile.rotation,
            Origin = origin,
            Scale = Vector2.One * Projectile.scale,
            SpriteEffects = effects,
        });

        return false;
    }

    private void GenerateWavyChainPoints(List<Vector2> pointsList, Vector2 start, Vector2 end, int segments, float waveAmplitude, float waveFrequency, float waveSpeed) {
        pointsList.Clear();
        pointsList.Add(start);

        var delta = end - start;
        var distance = delta.Length();
        var direction = delta / distance;

        var perpendicular = new Vector2(-direction.Y, direction.X);

        var instancePhaseOffset = Main.GameUpdateCount * waveSpeed + Projectile.whoAmI * 0.1f;

        for(int i = 1; i < segments - 1; i++) {
            var t = (float)i / (segments - 1);
            var basePoint = Vector2.Lerp(start, end, t);

            var waveDisplacement =
                (float)Math.Sin(t * MathHelper.TwoPi * waveFrequency + instancePhaseOffset)
                * waveAmplitude * (1f - t);

            var gravityMultiplier = MathF.Sin(MathHelper.Pi * i / segments) * 0.3f;

            pointsList.Add(basePoint + perpendicular * waveDisplacement - gravityMultiplier * Vector2.UnitY * MathF.Min(distance - 220f, 0f));
        }

        pointsList.Add(end);
    }
}