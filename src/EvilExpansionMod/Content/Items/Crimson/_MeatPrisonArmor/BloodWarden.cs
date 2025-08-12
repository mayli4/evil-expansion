using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
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

    private const float attack_range = 30 * 16; // 30 tiles
    private const float follow_speed_max = 10f;
    private const float attack_speed_max = 15f;
    
    private const int anim_speed = 6;
    
    private static readonly Vector2 attachmentPoint = new Vector2(15f, 40f);
    
    private bool _canDealDamage;

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

        Projectile.damage = 50;
        
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => CurrentState == State.Attacking && _canDealDamage;

    public override void AI() {
        _canDealDamage = false;
        
        if (Owner.HasBuff<BloodWardenBuff>())
            Projectile.timeLeft = 2;
        else
            Projectile.Kill();

        Projectile.spriteDirection = (Owner.Center.X < Projectile.Center.X) ? -1 : 1;
        
        NPC target = FindTarget();

        if (CurrentState == State.Idle) {
            if (target != null) {
                CurrentState = State.Attacking;
                TargetNPCID = target.whoAmI;
            }
        }
        else if (CurrentState == State.Attacking) {
            if (target == null || target.whoAmI != TargetNPCID || !target.active || !target.CanBeChasedBy(this) && target.CountsAsACritter) {
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
        
        Projectile.frameCounter++;

        if (CurrentState == State.Idle) {
            if (Projectile.frameCounter >= anim_speed) {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4) {
                    Projectile.frame = 0;
                }
            }
        }
        else if (CurrentState == State.Attacking) {
            if (Projectile.frameCounter >= anim_speed) {
                Projectile.frameCounter = 0;
                Projectile.frame++; 
                if (Projectile.frame > 12) {
                    Projectile.frame = 5;
                }
                //lol?
                if (Projectile.frame == 6 || Projectile.frame == 8 || Projectile.frame == 10 || Projectile.frame == 12) {
                    _canDealDamage = true;
                }
            }
        }
    }

    private NPC FindTarget() {
        NPC bestTarget = null;
        float bestDistanceSq = attack_range * attack_range;

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

        targetPos.Y += (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 0.1f) * 10f;

        Vector2 vectorToTarget = targetPos - Projectile.Center;
        float distance = vectorToTarget.Length();

        if (distance > 20f) { 
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, vectorToTarget.SafeNormalize(Vector2.Zero) * follow_speed_max, 0.1f);
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
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, vectorToTarget.SafeNormalize(Vector2.Zero) * attack_speed_max, 0.1f);
        } else {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.1f);
        }

        Projectile.spriteDirection = (target.Center.X < Projectile.Center.X) ? -1 : 1;
    }

    public override bool PreDraw(ref Color lightColor) {
        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        var chainTexture = Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.BloodWardenCord.Value;
        
        var origin = new Vector2(texture.Width / 2f, texture.Height / Main.projFrames[Projectile.type] / 2f);
        
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        var sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        

        Vector2 attachmentOffsetFromFrameCenter = attachmentPoint - new Vector2(35 , 30);

        if (Projectile.spriteDirection == -1) {
            attachmentOffsetFromFrameCenter.X = -attachmentOffsetFromFrameCenter.X;
        }
        
        Vector2 chainStart = Projectile.Center + attachmentOffsetFromFrameCenter + new Vector2(0f, Projectile.gfxOffY);

        Vector2 chainEnd = Owner.Center;
        
        List<Vector2> chainPoints = new();
        GenerateWavyChainPoints(chainPoints, chainStart, chainEnd, 20, 5, 0.5f, 0.2f);
        
        Graphics.BeginPipeline()
            .DrawBasicTrail(chainPoints.ToArray(), static _ => 6, chainTexture, Color.White)
            .Flush();
        
        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            sourceRectangle,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            effects,
            0f
        );
        
        return false;
    }
    
    private void GenerateWavyChainPoints(List<Vector2> pointsList, Vector2 start, Vector2 end, int segments, float waveAmplitude, float waveFrequency, float waveSpeed) {
        pointsList.Clear();
        pointsList.Add(start);

        Vector2 direction = Vector2.Normalize(end - start);
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        float instancePhaseOffset = Main.GameUpdateCount * waveSpeed + Projectile.whoAmI * 0.1f;

        for (int i = 1; i < segments - 1; i++) {
            float t = (float)i / (segments - 1);
            Vector2 basePoint = Vector2.Lerp(start, end, t);
            
            float waveDisplacement = (float)Math.Sin(t * MathHelper.TwoPi * waveFrequency + instancePhaseOffset)
                                     * waveAmplitude * (1f - t); 

            pointsList.Add(basePoint + perpendicular * waveDisplacement);
        }

        pointsList.Add(end);
    }
}