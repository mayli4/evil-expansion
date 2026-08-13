using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LingeringFlameProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public int ParentNPCID => (int)Projectile.ai[0];

    private int max_lifetime = 60 * 5;

    public override void SetDefaults() {
        Projectile.width = 90;
        Projectile.height = 90;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 1;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = max_lifetime;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if(Projectile.timeLeft > max_lifetime / 2 && Projectile.alpha > 0) {
            Projectile.alpha -= 15;
            if(Projectile.alpha < 0) Projectile.alpha = 0;
        }
        else if(Projectile.timeLeft < 30 && Projectile.alpha < 255) {
            Projectile.alpha += 8;
            if(Projectile.alpha > 255) Projectile.alpha = 255;
        }

        if(Main.rand.NextBool(15)) {
            Dust.NewDust(
                Projectile.position,
                100,
                100,
                DustID.Firefly,
                Main.rand.NextFloat(-1f, 1f),
                Main.rand.NextFloat(-1f, 1f) - 0.5f,
                100,
                default,
                Main.rand.NextFloat(0.8f, 1.2f)
            );
        }

        var newDustData = new Smoke.Data()
        {
            InitialLifetime = 40,
            ElapsedFrames = 0,
            InitialOpacity = 0.5f,
            ColorStart = Color.Black,
            ColorFade = new Color(69, 69, 113),
            Spin = 0f,
            InitialScale = Main.rand.NextFloat(0.5f, 2f)
        };

        if(Main.rand.NextBool(15)) {
            var newDust = Dust.NewDustPerfect(
                Projectile.Center - new Vector2(0, 30),
                ModContent.DustType<Smoke>(),
                null,
                0,
                newColor: Color.White,
                newDustData.InitialScale
            );

            newDust.customData = newDustData;
        }

        Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3());
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overWiresUI.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var flameShader = Assets.Effects.Pixel.LingeringFlame.Asset.Value;
        var noiseTexture1 = Assets.Textures.Sample.Pebbles.Asset.Value;
        var circleTexture = Assets.Textures.Misc.Circle.Asset.Value;
        var glowTexture = Assets.Textures.Sample.Glow1.Asset.Value;


        float flameScaleFactor = 1f;
        if(Projectile.timeLeft < max_lifetime / 2) {
            flameScaleFactor = Projectile.timeLeft / (max_lifetime / 2f);
        }
        flameScaleFactor = MathHelper.Clamp(flameScaleFactor, 0.1f, 1f);
        float currentFlameSize = 1.2f * flameScaleFactor;

        float alphaFactor = (1f - Projectile.alpha / 255f);
        currentFlameSize *= alphaFactor;
        currentFlameSize = Math.Max(currentFlameSize, 0.0f);

        Graphics.BeginPipeline(0.5f)
            .EffectParams(
                flameShader,
                ("time", 0.01f * Main.GameUpdateCount + Projectile.whoAmI + 10),
                ("size", new Vector2(150, 150)),
                ("flameColor", Color.Black.ToVector4() * 0.5f),
                ("coreColor", Color.Black.ToVector4() * 0.5f),
                ("noiseScale", 0.5f),
                ("flameSize", currentFlameSize),
                ("tex1", noiseTexture1)
            )
            .SetBlendState(BlendState.Additive)
            .DrawSprite(
                circleTexture,
                new Rectangle(
                    (int)(Projectile.position.X - Main.screenPosition.X - 25 - 30),
                    (int)(Projectile.position.Y - Main.screenPosition.Y - 40 - 50),
                    200,
                    200
                ),
                color: Projectile.GetAlpha(lightColor),
                rotation: Projectile.rotation,
                effect: flameShader
            )
            .ApplyOutline(Color.Black * 0.4f)
            .Flush();

        Graphics.BeginPipeline(0.5f)
            .EffectParams(
                flameShader,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI + 10),
                ("size", new Vector2(Projectile.width, Projectile.height)),
                ("flameColor", new Color(255, 106, 0).ToVector4()),
                ("coreColor", new Color(234, 255, 0).ToVector4()),
                ("outerCoreColor", new Color(255, 150, 0).ToVector4()),
                ("noiseScale", 1.0f),
                ("flameSize", currentFlameSize),
                ("tex1", noiseTexture1)
            )
            .SetBlendState(BlendState.Additive)
            .DrawSprite(
                circleTexture,
                new Rectangle(
                    (int)(Projectile.position.X - Main.screenPosition.X - 30),
                    (int)(Projectile.position.Y - Main.screenPosition.Y - 50),
                    150,
                    150
                ),
                color: Projectile.GetAlpha(lightColor),
                rotation: Projectile.rotation,
                effect: flameShader
            )
            .ApplyOutline(new Color(255, 150, 0))
            .Flush();

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - new Vector2(),
            null,
            new Color(255, 106, 0) * 0.25f * (1f - Projectile.alpha / 255f),
            0f,
            glowTexture.Size() / 2f,
            currentFlameSize,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);

        return false;
    }
}