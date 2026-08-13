using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

// ReSharper disable CompareOfFloatsByEqualityOperator
public class SpamonAStickItem : ModItem {
    public override string Texture => Assets.Textures.Items.Corruption.SpamonAStick.SpamonAStickItem.KEY;

    public override void SetDefaults() {
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.useAnimation = 10;
        Item.useTime = 10;
        Item.channel = true;
        Item.shootSpeed = 12;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.shoot = ModContent.ProjectileType<SpamOnAStickProjectile>();

        Item.damage = 30;
        Item.knockBack = 6f;
        Item.crit = 4;
        Item.value = Item.sellPrice(gold: 1);
    }

    public override bool CanShoot(Player player) {
        return player.ownedProjectileCounts[Item.shoot] < 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        return true;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<HellDemoniteBarItem>(), 12)
            .AddIngredient(ItemID.RottenChunk, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

public class SpamOnAStickProjectile : ModProjectile {
    public override string Texture => Assets.Textures.Items.Corruption.SpamonAStick.SpamonAStickItem.KEY;

    protected Texture2D ChainTexture => Assets.Textures.Items.Corruption.SpamonAStick.SpamonAStick_Chain.Asset.Value;
    protected Texture2D BlockTexture => Assets.Textures.Items.Corruption.SpamonAStick.SpamonAStick_Block.Asset.Value;

    public int MaxLength = 650;

    public ref float Timer => ref Projectile.ai[0];
    public ref float State => ref Projectile.ai[1];
    public ref float Length => ref Projectile.ai[2];

    private ref float _visualTimer => ref Projectile.localAI[0];
    private ref float _hasBounced => ref Projectile.localAI[1];

    public Player Owner => Main.player[Projectile.owner];

    public float ExtendGravity = 0.5f;
    public float ExtendDrag = 0.99f;
    public float RetractSpeed = 25f;
    public float GroundSplatDuration = 30f;

    public virtual void OnImpact(bool wasTile) {
        _visualTimer = GroundSplatDuration;
        if(wasTile) {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(5f, 0.6f));

            for(int i = 0; i < 7; i++) {
                Dust.NewDustPerfect(Projectile.Center, DustID.CorruptGibs, Main.rand.NextVector2Circular(5f, 5f), Scale: Main.rand.NextFloat(1f, 2f));
                Dust.NewDustPerfect(Projectile.Center, DustID.Corruption, Main.rand.NextVector2Circular(5f, 5f), Scale: Main.rand.NextFloat(1f, 2f));
            }
        }
    }

    public override void SetDefaults() {
        Projectile.friendly = true;
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 180;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
    }

    public override void OnSpawn(IEntitySource source) {
        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
        _hasBounced = 0f;
    }

    public override void AI() {
        Owner.itemAnimation = Owner.itemAnimationMax;
        Owner.heldProj = Projectile.whoAmI;

        if(_visualTimer > 0) {
            _visualTimer--;
        }

        if(Owner.channel) {
            Projectile.timeLeft = 180;
        }
        else if(State != 4 && State != 5) {
            Projectile.velocity *= 0.5f;
            Timer = 0;
            State = 4;
            Projectile.netUpdate = true;
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Projectile.Center - Owner.Center).ToRotation() - MathHelper.PiOver2);
        Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, (Projectile.Center - Owner.Center).ToRotation() - MathHelper.PiOver2 - 0.1f * Owner.direction);
        Owner.direction = Projectile.Center.X > Owner.Center.X ? 1 : -1;

        if(State == 0) {
            Projectile.velocity.X *= ExtendDrag;
            Projectile.velocity.Y += ExtendGravity;

            if(Projectile.velocity.Length() > RetractSpeed * 1.5f) {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * RetractSpeed * 1.5f;
            }

            if(Vector2.Distance(Owner.Center, Projectile.Center) >= MaxLength) {
                Projectile.velocity = Vector2.Zero;
                Length = MaxLength;
                State = 4;
                Projectile.netUpdate = true;
            }
        }
        else if(State == 5) {
            Projectile.velocity = Vector2.Zero;

            if(!Owner.channel) {
                Timer = 0;
                State = 4;
                Projectile.netUpdate = true;
            }

            if(Vector2.Distance(Owner.Center, Projectile.Center) >= MaxLength) {
                Projectile.velocity = Vector2.Zero;
                Length = MaxLength;
                State = 4;
                Projectile.netUpdate = true;
            }
        }
        else if(State == 4) {
            Timer++;

            float retractProgress = Timer / 60f;
            float currentRetractionSpeed = MathHelper.Lerp(10f, RetractSpeed, MathF.Pow(retractProgress, 0.5f));

            Projectile.velocity = Projectile.DirectionTo(Owner.Center) * currentRetractionSpeed;
            Projectile.tileCollide = false;

            if(Vector2.Distance(Owner.Center, Projectile.Center) < 20f) {
                Projectile.Kill();
            }
        }

        Projectile.rotation += Projectile.velocity.X / 100;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.rotation = 0;

        if(State == 0) {
            OnImpact(true);

            bool hitHorizontalSurface = oldVelocity.Y != Projectile.velocity.Y;
            bool hitVerticalSurface = oldVelocity.X != Projectile.velocity.X;

            if(hitHorizontalSurface && oldVelocity.Y > 0f) {
                if(_hasBounced == 0f) {
                    float bounceFactor = 0.5f;
                    Projectile.velocity.Y = -oldVelocity.Y * bounceFactor;
                    _hasBounced = 1f;
                    Projectile.netUpdate = true;
                }
                else {
                    Projectile.velocity = Vector2.Zero;

                    Length = Vector2.Distance(Owner.Center, Projectile.Center);
                    State = 5;
                    Projectile.netUpdate = true;
                }
            }
            else if(hitHorizontalSurface && oldVelocity.Y < 0f) {
                Projectile.velocity = Vector2.Zero;
                State = 4;
                Projectile.netUpdate = true;
            }
            else if(hitVerticalSurface) {
                Projectile.velocity = Vector2.Zero;
                State = 4;
                Projectile.netUpdate = true;
            }

            return false;
        }
        if(State == 5) {
            Projectile.velocity = Vector2.Zero;
        }

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        Rectangle collisionHitbox = Projectile.Hitbox;

        return collisionHitbox.Intersects(targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        OnImpact(false);
    }

    //from examplemod
    public override bool PreDraw(ref Color lightColor) {
        float squishProgress = _visualTimer / GroundSplatDuration;
        float easedSquish = MathF.Sin(squishProgress * MathF.PI);

        const float MaxSquishAmount = 0.2f;
        Vector2 squishScale = new Vector2(1f + easedSquish * MaxSquishAmount, 1f - easedSquish * MaxSquishAmount);
        Vector2 finalDrawScale = squishScale * Projectile.scale;

        Vector2 ballPos = Projectile.Center;

        if(easedSquish > 0.01f) {
            float visualHeightShrinkAmount = BlockTexture.Height * Projectile.scale * (1f - squishScale.Y) / 2f;
            ballPos.Y += visualHeightShrinkAmount;
        }

        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile) + new Vector2(Owner.direction * 10f, 6f);
        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Rectangle? chainSourceRectangle = null;
        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (ChainTexture.Size() / 2f);

        float chainSegmentDrawLength = chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : ChainTexture.Height;
        if(chainSegmentDrawLength == 0) {
            chainSegmentDrawLength = 10;
        }
        float chainRotation = (Projectile.Center - playerArmPosition).ToRotation() + MathHelper.PiOver2;
        float chainLengthRemainingToDraw = Vector2.Distance(playerArmPosition, Projectile.Center) + chainSegmentDrawLength / 2f;

        Vector2 currentChainDrawPosition = playerArmPosition; // Start drawing from the player's arm
        Vector2 unitVectorTowardsFlail = (Projectile.Center - playerArmPosition).SafeNormalize(Vector2.UnitY);


        while(chainLengthRemainingToDraw > 0f) {
            Color chainDrawColor = Lighting.GetColor((int)currentChainDrawPosition.X / 16, (int)(currentChainDrawPosition.Y / 16f));

            Main.spriteBatch.Draw(ChainTexture, currentChainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            currentChainDrawPosition += unitVectorTowardsFlail * chainSegmentDrawLength;
            chainLengthRemainingToDraw -= chainSegmentDrawLength;
        }

        Main.EntitySpriteDraw(
            BlockTexture,
            ballPos - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            BlockTexture.Size() / 2f,
            finalDrawScale,
            SpriteEffects.None
        );

        return false;
    }
}
// ReSharper restore CompareOfFloatsByEqualityOperator