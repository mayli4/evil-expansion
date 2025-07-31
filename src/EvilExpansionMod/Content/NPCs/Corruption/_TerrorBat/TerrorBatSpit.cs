using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

public class TerrorBatSpit : ModProjectile {
    public override string Texture => "Terraria/Images/NPC_112";

    private PrimitiveTrail sparkleTrail;
    private PositionCache positionCache;
    private bool trailInit;

    public const int TRAIL_SIZE = 20;

    public static readonly int MaxTimeLeft = 300;

    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;

        Projectile.penetrate = 2;

        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 1;

        positionCache = new(30);
        sparkleTrail = new PrimitiveTrail(
            positionCache.Positions,
            _ => TRAIL_SIZE * Scale,
            _ => new Color(72, 96, 36, 255)
        );
    }

    public override void AI() {
        Projectile.velocity.Y += 0.1f;
        if(Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();

        var pos = Projectile.Center;

        if(!trailInit) {
            positionCache.SetAll(pos);
            Projectile.oldPos = Projectile.oldPos.Select(_ => Projectile.position).ToArray();
            trailInit = true;
        }

        Dust.NewDustDirect(Projectile.position, 10, 10, DustID.CursedTorch);

        positionCache.Add(pos);
        sparkleTrail.Positions = positionCache.Positions;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

        Projectile.penetrate--;

        if(Projectile.penetrate <= 0) {
            Projectile.Kill();
        }
        else {
            if(Projectile.velocity.X != oldVelocity.X) {
                Projectile.velocity.X = -oldVelocity.X * 0.85f;
            }
            if(Projectile.velocity.Y != oldVelocity.Y) {
                Projectile.velocity.Y = -oldVelocity.Y * 0.85f;
            }
        }
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var shader = Assets.Assets.Effects.Trail.CursedSpiritFire.Value;

        shader.Parameters["time"].SetValue(0.025f * Main.GameUpdateCount);
        shader.Parameters["mat"].SetValue(MathUtilities.WorldTransformationMatrix);
        shader.Parameters["stepY"].SetValue(0.25f);
        shader.Parameters["scale"].SetValue(0.25f);
        shader.Parameters["texture1"].SetValue(Assets.Assets.Textures.Sample.Pebbles.Value);
        shader.Parameters["texture2"].SetValue(Assets.Assets.Textures.Sample.Noise2.Value);

        sparkleTrail.Draw(shader);

        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;

        var fade = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * Projectile.whoAmI) + MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var glowColor = new Color(72, 96, 36, 255) * (0.3f + 0.3f * fade);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Projectile.velocity * 0.6f,
            null,
            glowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.3f * Scale,
            SpriteEffects.None,
            0
        );

        Main.spriteBatch.EndBegin(snapshot);

        return false;
    }
}