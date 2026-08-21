using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class SpamonAStickItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.SpamonAStick.SpamonAStickItem.KEY;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.useAnimation = 10;
        Item.useTime = 10;
        Item.channel = true;
        Item.shootSpeed = 20;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;
        
        Item.shoot = ModContent.ProjectileType<SpamOnAStickProjectile>();

        Item.damage = 50;
        Item.knockBack = 8;
        Item.crit = 4;
        Item.value = Item.sellPrice(gold: 1, silver: 2);
        Item.rare = ItemRarityID.Pink;
    }

    public override bool CanShoot(Player player) {
        return player.ownedProjectileCounts[Item.shoot] < 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        return true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<PolypBarItem>(), 12)
            .AddIngredient(ItemID.RottenChunk, 8)
            .AddIngredient(ItemID.Terrarium)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class SpamOnAStickProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.SpamonAStick.SpamonAStickItem.KEY;

    protected Texture2D ChainTexture => Assets.Images.Corruption.Items.SpamonAStick.SpamonAStick_Chain.Asset.Value;
    protected Texture2D BlockTexture => Assets.Images.Corruption.Items.SpamonAStick.SpamonAStick_Block.Asset.Value;

    public enum AIState {
        Spinning,
        LaunchingForward,
        Retracting,
        ForcedRetracting,
        StuckToGround,
        Dropping
    }

    public AIState CurrentAIState {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    public ref float StateTimer => ref Projectile.ai[1];
    public ref float SpinningStateTimer => ref Projectile.ai[02];

    private float visualTimer;
    private bool hasBounced;

    public Player Owner => Main.player[Projectile.owner];

    public float GroundSplatDuration = 30f;
    public float MaxLength = 650f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        Projectile.netImportant = true;
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.tileCollide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source) {
        hasBounced = false;
        visualTimer = 0f;
        ChangeState(AIState.Spinning);
    }

    public void ChangeState(AIState newState) {
        CurrentAIState = newState;
        StateTimer = 0f;

        if (newState is AIState.LaunchingForward or AIState.Dropping) {
            hasBounced = false;
        }

        if (newState == AIState.ForcedRetracting) {
            Projectile.tileCollide = false;
        }

        Projectile.netUpdate = true;
    }

    public virtual void OnImpact(bool wasTile) {
        visualTimer = GroundSplatDuration;

        if (wasTile) {
            SoundEngine.PlaySound(Assets.Sounds.SpamOnAStickSmash.Asset with { PitchVariance = 0.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.6f }, Projectile.Center);
            
            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(7f, 0.7f));

            for(int i = 0; i < Main.rand.Next(4, 8); i++) {
                Dust.NewDustPerfect(Projectile.Center, DustID.CorruptGibs, Main.rand.NextVector2Circular(5, 5));
            }
        }
        else {
            SoundEngine.PlaySound(Assets.Sounds.SpamOnAStickSmash.Asset with { PitchVariance = 0.4f, Pitch = -0.4f, Volume = 0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCHit13 with { Pitch = -0.2f }, Projectile.Center);
        }
    }

    public override void AI() {
        if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed || Vector2.Distance(Projectile.Center, Owner.Center) > 1200f) {
            Projectile.Kill();
            return;
        }

        if (Main.myPlayer == Projectile.owner && Main.mapFullscreen) {
            Projectile.Kill();
            return;
        }

        if (visualTimer > 0) {
            visualTimer--;
        }

        Vector2 mountedCenter = Owner.MountedCenter;
        bool shouldOwnerHitCheck = false;

        switch (CurrentAIState) {
            case AIState.Spinning:
                shouldOwnerHitCheck = HandleSpinningState(mountedCenter);
                break;
            case AIState.LaunchingForward:
                HandleLaunchingState(mountedCenter);
                break;
            case AIState.StuckToGround:
                HandleStuckState(mountedCenter);
                break;
            case AIState.Dropping:
                HandleDroppingState(mountedCenter);
                break;
            case AIState.Retracting:
                HandleRetractingState(mountedCenter);
                break;
            case AIState.ForcedRetracting:
                HandleForcedRetractingState(mountedCenter);
                break;
        }

        Projectile.direction = Projectile.velocity.X.NonZeroSign();
        Projectile.ownerHitCheck = shouldOwnerHitCheck;

        if (CurrentAIState != AIState.StuckToGround) {
            Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
            Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;
            if (CurrentAIState == AIState.Dropping) {
                Projectile.rotation += Projectile.velocity.ToRotation() * Projectile.direction * 0.1f;
            }
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Projectile.Center - Owner.Center).ToRotation() - MathHelper.PiOver2);
        Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, (Projectile.Center - Owner.Center).ToRotation() - MathHelper.PiOver2 - 0.1f * Owner.direction);

        Projectile.timeLeft = 2;
        Owner.heldProj = Projectile.whoAmI;
        Owner.SetDummyItemTime(2);
    }

    private bool HandleSpinningState(Vector2 mountedCenter) {
        if (Projectile.owner == Main.myPlayer) {
            Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * Owner.direction);
            Owner.ChangeDir((unitVectorTowardsMouse.X > 0f) ? 1 : -1);

            if (!Owner.channel) {
                float launchSpeed = 18f * Owner.GetAttackSpeed(DamageClass.Melee);
                Projectile.velocity = unitVectorTowardsMouse * launchSpeed + Owner.velocity;
                Projectile.Center = mountedCenter;
                Projectile.ResetLocalNPCHitImmunity();
                
                ChangeState(AIState.LaunchingForward);
                return true;
            }
        }

        SpinningStateTimer += 1f;
        Vector2 offsetFromPlayer = new Vector2(Owner.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * Owner.direction);
        offsetFromPlayer.Y *= 0.8f;
        if (offsetFromPlayer.Y * Owner.gravDir > 0f) {
            offsetFromPlayer.Y *= 0.5f;
        }

        Projectile.Center = mountedCenter + offsetFromPlayer * 30f;
        Projectile.velocity = Vector2.Zero;
        return true;
    }

    private void HandleLaunchingState(Vector2 mountedCenter) {
        int launchTimeLimit = 15;
        bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit || Projectile.Distance(mountedCenter) >= MaxLength;

        if (shouldSwitchToRetracting) {
            if (Owner.controlUseItem) {
                Projectile.velocity *= 0.2f;
                ChangeState(AIState.Dropping);
            } else {
                ChangeState(AIState.Retracting);
            }
        }

        Owner.ChangeDir((Owner.Center.X < Projectile.Center.X) ? 1 : -1);
    }

    private void HandleStuckState(Vector2 mountedCenter) {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = 0f;

        if (Vector2.Distance(mountedCenter, Projectile.Center) >= MaxLength) {
            ChangeState(AIState.ForcedRetracting);
        }
        else if (!Owner.controlUseItem) {
            ChangeState(AIState.Retracting);
        }
    }

    private void HandleDroppingState(Vector2 mountedCenter) {
        if (!Owner.controlUseItem || Projectile.Distance(mountedCenter) > MaxLength + 160f) {
            ChangeState(AIState.ForcedRetracting);
        } else {
            Projectile.velocity.Y += 0.8f;
            Projectile.velocity.X *= 0.95f;
            Owner.ChangeDir((Owner.Center.X < Projectile.Center.X) ? 1 : -1);
        }
    }

    private void HandleRetractingState(Vector2 mountedCenter) {
        float meleeSpeed = Owner.GetAttackSpeed(DamageClass.Melee);
        float retractAcceleration = 3f * meleeSpeed;
        float maxRetractSpeed = 25f * meleeSpeed;

        if (Projectile.Distance(mountedCenter) <= maxRetractSpeed) {
            Projectile.Kill();
            return;
        }

        if (Owner.controlUseItem) {
            Projectile.velocity *= 0.2f;
            ChangeState(AIState.Dropping);
        } else {
            Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
            Projectile.velocity *= 0.98f;
            Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
            Owner.ChangeDir((Owner.Center.X < Projectile.Center.X) ? 1 : -1);
        }
    }

    private void HandleForcedRetractingState(Vector2 mountedCenter) {
        float forcedRetractAcceleration = 6f;
        float maxForcedRetractSpeed = 30f;

        if (Projectile.Distance(mountedCenter) <= maxForcedRetractSpeed) {
            Projectile.Kill();
            return;
        }

        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        Projectile.velocity *= 0.98f;
        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxForcedRetractSpeed, forcedRetractAcceleration);
        
        Vector2 target = Projectile.Center + Projectile.velocity;
        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f) {
            Projectile.Kill();
            return;
        }
        Owner.ChangeDir((Owner.Center.X < Projectile.Center.X) ? 1 : -1);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Dropping) {
            OnImpact(true);

            bool hitHorizontalSurface = oldVelocity.Y != Projectile.velocity.Y;

            if (hitHorizontalSurface && oldVelocity.Y > 0f) {
                if (!hasBounced) {
                    float bounceFactor = 0.4f;
                    Projectile.velocity.Y = -oldVelocity.Y * bounceFactor;
                    hasBounced = true;
                    Projectile.netUpdate = true;
                }
                else {
                    Projectile.velocity = Vector2.Zero;
                    ChangeState(AIState.StuckToGround);
                    Projectile.rotation = 0f;
                }
            }
            else {
                ChangeState(AIState.Retracting);
            }

            return false;
        }

        if (CurrentAIState == AIState.StuckToGround) {
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = 0f;
        }

        return false;
    }

    public override bool? CanDamage() {
        if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
            return false;
        return base.CanDamage();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (CurrentAIState == AIState.Spinning) {
            Vector2 mountedCenter = Owner.MountedCenter;
            Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f;
            return shortestVectorFromPlayerToTarget.Length() <= 55f;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        OnImpact(false);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (CurrentAIState == AIState.Spinning)
            modifiers.SourceDamage *= 0.6f;

        if (CurrentAIState is AIState.LaunchingForward or AIState.Retracting)
            modifiers.SourceDamage *= 1f;

        if(CurrentAIState is AIState.Dropping)
            modifiers.SourceDamage *= 4f;

        modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X) ? 1 : -1;

        if (CurrentAIState == AIState.Spinning)
            modifiers.Knockback *= 0.35f;
        if (CurrentAIState == AIState.Dropping)
            modifiers.Knockback *= 0.5f;
    }

    public override bool PreDraw(ref Color lightColor) {
        float drawRotation = Projectile.rotation;
        Vector2 drawScale = Vector2.One * Projectile.scale;
        Vector2 ballPos = Projectile.Center;

        float speed = Projectile.velocity.Length();
        if (speed > 1f) {
            float stretch = MathHelper.Clamp(speed * 0.025f, 0f, 0.5f);
            drawScale.X *= (1f - stretch * 0.4f);
            drawScale.Y *= (1f + stretch);
            drawRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        if (visualTimer > 0) {
            float progress = 1f - (visualTimer / GroundSplatDuration);
            float damping = MathF.Exp(-progress * 4.5f);
            float oscillation = MathF.Sin(progress * MathHelper.TwoPi * 3f);
            float squish = 0.4f * damping * oscillation;

            drawScale.X *= (1f + squish);
            drawScale.Y *= (1f - squish);
        }

        if (CurrentAIState == AIState.StuckToGround) {
            drawRotation = 0f;
            float heightShrinkOffset = (BlockTexture.Height / 2f) * Projectile.scale * (1f - drawScale.Y);
            ballPos.Y += heightShrinkOffset;
        }

        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile) + new Vector2(Owner.direction * -4f, -3f);
        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Rectangle? chainSourceRectangle = null;
        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (ChainTexture.Size() / 2f);

        float chainSegmentDrawLength = chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : ChainTexture.Height;
        if (chainSegmentDrawLength == 0) {
            chainSegmentDrawLength = 10;
        }

        float chainRotation = (Projectile.Center - playerArmPosition).ToRotation() + MathHelper.PiOver2;
        float chainLengthRemainingToDraw = Vector2.Distance(playerArmPosition, Projectile.Center) + chainSegmentDrawLength / 2f;

        Vector2 currentChainDrawPosition = playerArmPosition;
        Vector2 unitVectorTowardsFlail = (Projectile.Center - playerArmPosition).SafeNormalize(Vector2.UnitY);

        while (chainLengthRemainingToDraw > 0f) {
            Color chainDrawColor = Lighting.GetColor((int)(currentChainDrawPosition.X / 16f), (int)(currentChainDrawPosition.Y / 16f));

            Main.spriteBatch.Draw(
                ChainTexture, 
                currentChainDrawPosition - Main.screenPosition, 
                chainSourceRectangle, 
                chainDrawColor, 
                chainRotation, 
                chainOrigin, 
                1f, 
                SpriteEffects.None, 
                0f
            );

            currentChainDrawPosition += unitVectorTowardsFlail * chainSegmentDrawLength;
            chainLengthRemainingToDraw -= chainSegmentDrawLength;
        }

        Main.EntitySpriteDraw(
            BlockTexture,
            ballPos - Main.screenPosition,
            null,
            lightColor,
            drawRotation,
            BlockTexture.Size() / 2f,
            drawScale,
            SpriteEffects.None
        );

        return false;
    }
}