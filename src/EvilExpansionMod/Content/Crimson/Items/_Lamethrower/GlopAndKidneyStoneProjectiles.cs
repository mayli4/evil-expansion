using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.GlobalTileHooks;
using Player = Terraria.Player;

namespace EvilExpansionMod.Content.Crimson;

public class Glop : ModProjectile, IPreDrawEverythingBulk<Glop> {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.PusGlop.KEY;
    private int glopVariant = Main.rand.Next(1, 4);
    private Vector2[] trailPositions;
    public static float Gravity = 0.2f;
    private bool hasCollided = false;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 12;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians (Main.rand.NextFloat(-10f, 10f)) );
        Projectile.netUpdate = true;

        Projectile.frame = glopVariant switch
        {
            1 => 0,
            2 => 4,
            3 => 8,
            _ => Projectile.frame
        };
    }

    public override void AI() {
        if(-Projectile.velocity.Y > -35) {
            Projectile.velocity.Y += Gravity * 1.25f * (1 - glopVariant / 9); // Bigger glops are heavier
            if(Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }
        }

        trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 4)]; // The number here controls trail length
        for(int i = trailPositions.Length - 1; i > 0; i--) {
            trailPositions[i] = trailPositions[i - 1];
        }
        trailPositions[0] = Projectile.Center + Projectile.velocity;


        Projectile.frameCounter++;
        if(Projectile.frameCounter >= 8) {
            Projectile.frameCounter = 0;

            Projectile.frame++;

            if(glopVariant == 1)
                if(Projectile.frame >= 3) {
                    Projectile.frame = 0;
                }
            if(glopVariant == 2)
                if(Projectile.frame >= 7) {
                    Projectile.frame = 4;
                }
            if(glopVariant == 3)
                if(Projectile.frame >= 11) {
                    Projectile.frame = 8;
                }
        }

    }
    public void PreDrawEverythingBulk(ReadOnlySpan<Glop> entities) {
        using var pipeline = new RenderPipeline(Graphics.PreDrawNPCsQueue, 0.5f, Graphics.WorldTransformMatrix);

        var trailEffect = Assets.Shaders.Pixel.CursedSpiritFire.Asset.Value;
        var noise = Assets.Images.Sample.Pixel.Asset.Value;
        var texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];

        foreach(var entity in entities) {
            var proj = entity.Projectile;
            if(entity.trailPositions is null || entity.trailPositions.Length < 1) continue;

            var lightColor = Lighting.GetColor(proj.Center.ToTileCoordinates());
            var source = new Vector4(0f, proj.frame * frameHeight, texture.Width, frameHeight);
            var origin = new Vector2(source.Z, source.W) / 2f;

            pipeline
                .SetTexture(0, noise)
                .SetTexture(1, noise)
                .SetEffectParams(
                    trailEffect,
                    ("uTime", 0.0075f * Main.GameUpdateCount + proj.whoAmI * 34.432f),
                    ("uStepY", 0.176f),
                    ("uColor1", new Color(133, 8, 0)),
                    ("uColor2", new Color(141, 73, 40)),
                    ("uStepColor", 0.1f), // Previously 0.07f
                    ("uScale", 6f))
                .DrawTrail(
                    entity.trailPositions,
                    10f * proj.scale,
                    lightColor,
                    trailEffect)
                .DrawTexture(new()
                {
                    Texture = texture,
                    Position = proj.Center,
                    Color = lightColor,
                    Source = source,
                    Rotation = proj.rotation,
                    Origin = origin,
                    Scale = Vector2.One * proj.scale,
                });
        }
    }

    public override void OnKill(int timeLeft) {
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<PusDeath>(),
            0,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
        );

        SoundEngine.PlaySound(SoundID.NPCHit13 with
        {
            Pitch = Main.rand.NextFloat(-1.5f, 0f),
            Volume = Main.rand.NextFloat(0.6f, 1f)
        },
            Projectile.position);

        Projectile.NewProjectile(
            Projectile.GetSource_FromAI(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<LingeringIchorProjectile>(),
            Projectile.damage, // Creep inherits base damage
            Projectile.knockBack
        );
    }
}
public class KidneyStone : ModProjectile{
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.KidneyStone.KEY;
    private int StoneVariant = Main.rand.Next(1, 4);
    private float gravityBuilding = 0;
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 3;
    }

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 20;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;
        Projectile.damage = 100;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.extraUpdates = 1;

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.netUpdate = true;

        Projectile.frame = StoneVariant switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            _ => Projectile.frame
        };
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        // Reduce the projectile's remaining damage by 10% on every hit
        Projectile.damage = (int)(Projectile.damage * 0.90f);

        if(Projectile.damage < 1) {
            Projectile.damage = 1;
        }
    }
    public override void AI() {
        //if(Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }
        Projectile.velocity.Y += gravityBuilding;
        gravityBuilding += 0.005f;
        //Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.02f, 0.02f));
        Projectile.rotation += 0.1f * MathHelper.PiOver2;
    }
    public override void OnKill(int timeLeft) {
        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier((1), 0.10f));

        int soundNum = Main.rand.Next(1, 4);
        SoundStyle stonecrushSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacstone_impact{soundNum}") {
            Pitch = Main.rand.NextFloat(-0.8f, 0.3f),
            Volume = Main.rand.NextFloat(0.4f, 0.8f),
        };
        SoundEngine.PlaySound(stonecrushSound, Projectile.position);

        var rotation = Main.rand.NextFloat();
        for(var ii = 0; ii < 3; ii++) {
            for(var i = 0; i < 2; i++) {
                var direction = rotation.ToRotationVector2();

                var gore = Mod.Find<ModGore>("KidneystoneGore" + i);
                var size = TextureAssets.Gore[gore.Type].Size();

                Gore.NewGorePerfect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * (Main.rand.NextFloat(0.5f) + 0.5f) * 42 - size / 2f,
                    direction * Main.rand.NextFloat(1f, 3f),
                    gore.Type
                );

                rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
            }
        }
        SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.8f } with { PitchRange = (0.4f, 1f) });

        for(int i = 0; i < 6; i++) {
            var dustPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f)
                + Vector2.UnitY * 1.5f;
            var dustVelocity = Projectile.Center.DirectionTo(dustPos) * 5f + Projectile.oldVelocity;

            var dustColorStart = new Color(133, 122, 94);
            var dustColorFade = dustColorStart * 0.4f;

            var newDustData = new Smoke.Data()
            {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.8f,
                ColorStart = dustColorStart,
                ColorFade = dustColorFade,
                Spin = 0.03f,
                InitialScale = Main.rand.NextFloat(0.25f, 0.75f)
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
    }
    public override bool PreDraw(ref Color lightColor) {
        // Afterimages
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);

        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

        for(int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length * 0.5f);
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                sourceRectangle,
                color,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }
        return true;
    }
}

public class PusDeath : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.PusDeath.KEY;
    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 30;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 5;
    }
    public override void OnSpawn(IEntitySource source) {
        if(Projectile.ai[0] == 1) {
            Projectile.hostile = true;
        }
        else {
            Projectile.hostile = false;
        }
        if(Projectile.ai[1] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }
    }
    public override void AI() {
        AnimateProjectile();
    }
    private const int FrameCount = 5;
    private void AnimateProjectile() {
        int frameDuration = 1;
        Projectile.frameCounter++;
        if(Projectile.frameCounter >= frameDuration) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
            if(Projectile.frame >= FrameCount) {
                Projectile.active = false;
            }
        }
    }
}