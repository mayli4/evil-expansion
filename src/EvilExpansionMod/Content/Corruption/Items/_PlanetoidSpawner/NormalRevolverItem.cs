using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class NormalRevolverItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.Planetoids.NormalRevolver.KEY;

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.ZephyrFish);

        Item.shoot = ModContent.ProjectileType<NormalPlanetoidProjectile>();
        Item.buffType = ModContent.BuffType<NormalPlanetoidBuff>();

        Item.value = Item.sellPrice(0, 5);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        player.AddBuff(Item.buffType, 2);

        return false;
    }
}

public class NormalPlanetoidBuff : ModBuff {
    public override string Texture => Assets.Images.Corruption.Items.Planetoids.NormalPlanetoidBuff.KEY;

    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        bool _ = false;
        player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref _, ModContent.ProjectileType<NormalPlanetoidProjectile>());
    }
}


public class NormalPlanetoidProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Planetoids.NormalPlanetoid.KEY;

    private ref float _currentFaceFrame => ref Projectile.localAI[0];
    private ref float _faceFrameTimer => ref Projectile.localAI[1];
    private ref float _faceAnimationState => ref Projectile.localAI[2];
    private float _nextExpressionChangeTimer;

    private const int anim_speed = 8;
    private const int smile_duration = 60;
    private int _expressionDecisionMin = 20;
    private int _expressionDecisionMax = 100;

    private Vector2 _currentVelocity;
    private Vector2 _currentRelativePosition;
    private Vector2 _targetRelativePosition;
    private float _timeToNextMoveDecision;

    private float _faceRotationAngle;
    private float _faceRotationSpeed;

    public override void SetStaticDefaults() {
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 5)
            .WithOffset(-2, -22f)
            .WithCode(CharacterPreviewCustomization);
    }

    public static void CharacterPreviewCustomization(Projectile proj, bool walking) {
        float half = 0.5f;
        float timer = (float)Main.timeForVisualEffects % 60f / 60f;
        float speed = 1f;
        proj.position.Y += half + (float)(Math.Cos(timer * MathHelper.TwoPi * speed) * half * 2f);
        proj.position.X -= 10;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.EyeOfCthulhuPet);

        Projectile.width = 44;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        _currentFaceFrame = 0;
        _faceFrameTimer = 0;
        _faceAnimationState = 0;
        _nextExpressionChangeTimer = Main.rand.Next(_expressionDecisionMin, _expressionDecisionMax);

        _currentRelativePosition = Main.rand.NextVector2Circular(80f, 60f);
        _targetRelativePosition = _currentRelativePosition;
        _currentVelocity = Vector2.Zero;
        _timeToNextMoveDecision = Main.rand.Next(180, 300);

        _faceRotationSpeed = Main.rand.NextFloat(-0.005f, 0.005f);
        if(_faceRotationSpeed == 0) _faceRotationSpeed = 0.01f;
        _faceRotationAngle = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        if(!player.HasBuff(ModContent.BuffType<NormalPlanetoidBuff>())) {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;

        _timeToNextMoveDecision--;
        if(_timeToNextMoveDecision <= 0) {
            float targetRangeX = 50f;
            float targetRangeY = 50f;
            _targetRelativePosition = new Vector2(
                Main.rand.NextFloat(-targetRangeX, targetRangeX),
                Main.rand.NextFloat(-targetRangeY, targetRangeY)
            );
            _timeToNextMoveDecision = Main.rand.Next(180, 300);
        }

        Vector2 directionToTarget = (_targetRelativePosition - _currentRelativePosition);
        float approachStrength = 0.0005f;
        _currentVelocity += directionToTarget * approachStrength;

        float driftMagnitude = 0.05f;
        _currentVelocity += Main.rand.NextVector2Circular(driftMagnitude, driftMagnitude);

        float damping = 0.98f;
        _currentVelocity *= damping;

        _currentRelativePosition += _currentVelocity;

        Projectile.Center = player.MountedCenter + _currentRelativePosition;
        _faceRotationAngle += _faceRotationSpeed;

        _faceFrameTimer++;

        if(_faceAnimationState == 0) {
            _currentFaceFrame = 0;
        }
        else if(_faceAnimationState == 1) {
            if(_faceFrameTimer < anim_speed) {
                _currentFaceFrame = 1;
            }
            else if(_faceFrameTimer < anim_speed * 2) {
                _currentFaceFrame = 2;
            }
            else if(_faceFrameTimer < anim_speed * 2 + smile_duration) {
                _currentFaceFrame = 2;
            }
            else if(_faceFrameTimer < anim_speed * 2 + smile_duration + 10) {
                _currentFaceFrame = (int)MathHelper.Lerp(2, 0, (_faceFrameTimer - (anim_speed * 2 + smile_duration)) / 10);
                _currentFaceFrame = Math.Max(0, Math.Min(2, (int)_currentFaceFrame));
            }
            else {
                _faceAnimationState = 0;
                _currentFaceFrame = 0;
                _faceFrameTimer = 0;
            }
        }
        else if(_faceAnimationState == 2) {
            _currentFaceFrame = 3;
            if(_faceFrameTimer >= 5) {
                _faceFrameTimer = 0;
                _faceAnimationState = 0;
                _currentFaceFrame = 0;
            }
        }

        _nextExpressionChangeTimer--;
        if(_nextExpressionChangeTimer <= 0) {
            if(_faceAnimationState == 0) {
                int randomChoice = Main.rand.Next(100);

                if(randomChoice < 25) {
                    _faceAnimationState = 2;
                    _faceFrameTimer = 0;
                }
                else if(randomChoice < 30) {
                    _faceAnimationState = 1;
                    _faceFrameTimer = 0;
                }
            }
            _nextExpressionChangeTimer = Main.rand.Next(_expressionDecisionMin, _expressionDecisionMax);
        }
    }

    public override void PostDraw(Color lightColor) {
        Texture2D planetoidTexture = Assets.Images.Corruption.Items.Planetoids.NormalPlanetoid.Asset.Value;
        Texture2D planetoidGrassTexture = Assets.Images.Corruption.Items.Planetoids.NormalPlanetoid_Grass.Asset.Value;
        Texture2D faceTexture = Assets.Images.Corruption.Items.Planetoids.NormalPlanetoid_Faces.Asset.Value;

        int faceFrameWidth = 16;
        int faceFrameHeight = 18;

        var faceSourceRect = new Rectangle((int)_currentFaceFrame * faceFrameWidth, 0, faceFrameWidth, faceFrameHeight);

        var faceDrawPosition = Projectile.Center;
        var faceOrigin = faceSourceRect.Size() / 2f;

        if(Projectile.isAPreviewDummy) {
            Main.EntitySpriteDraw(
                Assets.Images.Corruption.Items.Planetoids.NormalPlanetoid_Preview.Asset.Value,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                planetoidTexture.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None
            );
            return;
        }

        Main.EntitySpriteDraw(
            planetoidGrassTexture,
            Projectile.Center - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            planetoidTexture.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None
        );

        Main.spriteBatch.Draw(planetoidTexture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, planetoidTexture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(faceTexture, faceDrawPosition - Main.screenPosition, faceSourceRect, lightColor, _faceRotationAngle, faceOrigin, Projectile.scale, SpriteEffects.None, 0f);
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}