using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class NormalRevolverItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalRevolver;

    public override void SetDefaults() {
        Item.DefaultToVanitypet(ModContent.ProjectileType<NormalPlanetoidProjectile>(), BuffID.AbigailMinion);
        
        Item.shoot = ModContent.ProjectileType<NormalPlanetoidProjectile>();
        Item.buffType = ModContent.BuffType<NormalPlanetoidBuff>();
        
        Item.width = 30;
        Item.height = 30;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Pink;
    }
    
    
    public override bool? UseItem(Player player) {
        if (player.altFunctionUse == 2) {
            return false;
        }

        player.AddBuff(Item.buffType, 2); 
        return true;
    }
}

public class NormalPlanetoidBuff : ModBuff {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalPlanetoidBuff;

    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<NormalPlanetoidProjectile>()] > 0) {
            player.buffTime[buffIndex] = 18000;
        }
        else {
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, ModContent.ProjectileType<NormalPlanetoidProjectile>(), 0, 0f, player.whoAmI);
            player.buffTime[buffIndex] = 18000;
        }
    }
}


public class NormalPlanetoidProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalPlanetoid;

    public string FaceTexturePath => Assets.Assets.Textures.Items.Corruption.Planetoids.KEY_NormalPlanetoid_Faces;

    public ref float OrbitOffset => ref Projectile.ai[0]; 
    private ref float _currentFaceFrame => ref Projectile.localAI[0];
    private ref float _faceFrameTimer => ref Projectile.localAI[1];
    private ref float _faceAnimationState => ref Projectile.localAI[2];
    private float _nextExpressionChangeTimer;
    
    private const int anim_speed = 8;
    private const int smile_duration = 60;
    private int _expressionDecisionMin = 20;
    private int _expressionDecisionMax = 100;

    public override void SetStaticDefaults() {
        Main.projPet[Type] = true;
        ProjectileID.Sets.LightPet[Type] = false;
    }

    public override void SetDefaults() {
        Projectile.width = 44; 
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.minion = true;
        Projectile.minionSlots = 0;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        OrbitOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        
        _currentFaceFrame = 0;
        _faceFrameTimer = 0;
        _faceAnimationState = 0;
        _nextExpressionChangeTimer = Main.rand.Next(_expressionDecisionMin, _expressionDecisionMax);
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active) {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;

        var orbitCenter = player.MountedCenter;

        float currentAngle = Main.GameUpdateCount * 0.03f + OrbitOffset;
        var circularOffset = new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)) * 60f;

        float hoverOffset = MathF.Sin(Main.GameUpdateCount * 0.08f + OrbitOffset * 0.5f) * 5f;
        circularOffset.Y += hoverOffset;

        var targetPosition = orbitCenter + circularOffset;

        float lerpFactor = 0.1f;
        Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, lerpFactor);

        Projectile.rotation += 0.07f;

        _faceFrameTimer++;

        if (_faceAnimationState == 0) {
            _currentFaceFrame = 0;
        }
        else if (_faceAnimationState == 1) {
            if (_faceFrameTimer < anim_speed) {
                _currentFaceFrame = 1; 
            }
            else if (_faceFrameTimer < anim_speed * 2) {
                _currentFaceFrame = 2;
            }
            else if (_faceFrameTimer < anim_speed * 2 + smile_duration) {
                _currentFaceFrame = 2;
            }
            else if (_faceFrameTimer < anim_speed * 2 + smile_duration + 10) {
                _currentFaceFrame = (int)MathHelper.Lerp(2, 0, (_faceFrameTimer - (anim_speed * 2 + smile_duration)) / 10);
                _currentFaceFrame = Math.Max(0, Math.Min(2, (int)_currentFaceFrame));
            }
            else {
                _faceAnimationState = 0;
                _currentFaceFrame = 0;
                _faceFrameTimer = 0;
            }
        }
        else if (_faceAnimationState == 2) {
            _currentFaceFrame = 3;
            if (_faceFrameTimer >= 5) {
                _faceFrameTimer = 0;
                _faceAnimationState = 0;
                _currentFaceFrame = 0;
            }
        }

        _nextExpressionChangeTimer--;
        if (_nextExpressionChangeTimer <= 0) {
            if (_faceAnimationState == 0) {
                int randomChoice = Main.rand.Next(100);
                
                if (randomChoice < 25) {
                    _faceAnimationState = 2;
                    _faceFrameTimer = 0;
                }
                else if (randomChoice < 30) {
                    _faceAnimationState = 1;
                    _faceFrameTimer = 0;
                }
            }
            _nextExpressionChangeTimer = Main.rand.Next(_expressionDecisionMin, _expressionDecisionMax);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D planetoidTexture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D faceTexture = ModContent.Request<Texture2D>(FaceTexturePath).Value;

        Main.EntitySpriteDraw(
            planetoidTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation, 
            planetoidTexture.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None
        );

        int faceFrameWidth = 16;
        int faceFrameHeight = 18;
        
        var faceSourceRect = new Rectangle((int)_currentFaceFrame * faceFrameWidth, 0, faceFrameWidth, faceFrameHeight);

        var faceDrawPosition = Projectile.Center;
        var faceOrigin = faceSourceRect.Size() / 2f;

        Main.EntitySpriteDraw(
            faceTexture,
            faceDrawPosition - Main.screenPosition,
            faceSourceRect,
            Projectile.GetAlpha(lightColor),
            0f,
            faceOrigin,
            Projectile.scale,
            SpriteEffects.None
        );
        
        return false;
    }
}