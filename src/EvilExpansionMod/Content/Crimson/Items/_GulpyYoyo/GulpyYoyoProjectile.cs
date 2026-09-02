using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class GulpyYoyoProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.GulpyYoyo.GulpyYoyoProjectile.KEY;

    int Timer { get => (int)Projectile.ai[2]; set => Projectile.ai[2] = value; }

    const int MAX_CHOMP_STACKS = 3;
    int _chompStacks = 0;

    float ChompProgress => (float)_chompStacks / MAX_CHOMP_STACKS;

    Vector2 _twitch;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f;
        ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
        Main.projFrames[Projectile.type] = 3;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.aiStyle = ProjAIStyleID.Yoyo;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
    }

    public override void AI() {
        var maxDistance = 200f * 16f;
        if(Projectile.Center.DistanceSQ(Main.player[Projectile.owner].Center) > maxDistance * maxDistance) {
            Projectile.Kill();
            return;
        }

        if(Main.rand.NextBool(15)) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Crimson);
        }

        // NOTE: +0.45f is the rotation that the yoyo gets applied automatically.
        // Projectile.rotation -= 0.25f + 0.1f * (1f - ChompProgress);
        Projectile.rotation = 0;

        var chompingFrequency = 8;
        if(Timer > 10) {
            Projectile.friendly = false;
            Projectile.frame = 1 + (Timer % (chompingFrequency * 2) / chompingFrequency);
            Timer -= 1;
        }
        else if(Timer > 0) {
            Projectile.frame = 0;
            if(Timer == 10 && _chompStacks == MAX_CHOMP_STACKS) {
                _chompStacks = 0;
                Projectile.scale = 2.5f;

                SoundEngine.PlaySound(SoundID.Zombie64, Projectile.Center);

                for(var i = 0; i < 6; i++) {
                    var velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 6f);
                    Dust.NewDust(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.Bone,
                        velocity.X,
                        velocity.Y
                    );
                }

                for(var i = 0; i < 6; i++) {
                    var velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(7f, 15f);
                    var particle = BloodParticle.NewParticle(
                        Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f),
                        velocity,
                        Main.rand.NextFloat(0.2f, 0.5f),
                        new Color(180, 15, 25));
                    ParticleEngine.PARTICLES.Add(particle);
                }

                for(var i = 0; i < 3; i++) {
                    var velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 9f);
                    var projectile = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f),
                        velocity,
                        ProjectileID.Bone,
                        Projectile.damage,
                        0.3f,
                        Projectile.owner
                    );

                    projectile.friendly = true;
                    projectile.hostile = false;
                    projectile.penetrate = -1;
                }

                for(var i = 0; i < 3; i++) {
                    var velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 9f);
                    var projectile = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f),
                        velocity,
                        ModContent.ProjectileType<PortalGore>(),
                        Projectile.damage,
                        0.3f,
                        Projectile.owner
                    );

                    projectile.friendly = true;
                    projectile.hostile = false;
                    projectile.penetrate = -1;
                }
            }

            Timer -= 1;
        }
        else {
            Projectile.friendly = true;
        }

        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f + ChompProgress * 0.55f, 0.1f);

        _twitch *= 0.92f;
        if(Main.rand.NextBool(7 - (int)(ChompProgress * 4))) _twitch = Main.rand.NextVector2Unit() * ChompProgress * 3f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        for(var i = 0; i < 12; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
        }

        for(var i = 0; i < 2; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone);
        }

        SoundEngine.PlaySound(SoundID.Zombie27, Projectile.Center);

        Timer = 45;
        _chompStacks += 1;
        Projectile.netUpdate = true;
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public override void PostDraw(Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var frameHeight = texture.Height / Main.projFrames[Type];
        var source = new Vector4(0f, Projectile.frame * frameHeight, texture.Width, frameHeight);

        var drawPosition = Projectile.Center + _twitch;
        var origin = new Vector2(source.Z, source.W) / 2f;

        var outlineColor = Color.Lerp(Color.Transparent, Color.Red, (Projectile.scale - 1f) * 0.55f);
        Main.spriteBatch.Draw(texture, new Rectangle(0, 0, 1, 1), Color.Red);
        Main.spriteBatch.End(out var ss);

        Renderer.BeginPixelated(Graphics.WorldTransformMatrix)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = drawPosition,
                Rotation = Projectile.rotation,
                Source = source,
                Color = outlineColor,
                Origin = origin,
                Scale = Vector2.One * Projectile.scale,
            })
            .ApplyOutline(outlineColor)
            .End();

        Renderer.Begin(Graphics.WorldTransformMatrix)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = drawPosition,
                Rotation = Projectile.rotation,
                Source = source,
                Color = lightColor,
                Origin = origin,
                Scale = Vector2.One * Projectile.scale,
            })
            .ApplyTint(Color.Lerp(Color.Transparent, Color.Red, (Projectile.scale - 1f) * 0.3f))
            .End();

        Main.spriteBatch.Begin(ss);
    }
}
