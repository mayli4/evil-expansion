using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class GulpyYoyoProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.GulpyYoyo.KEY_GulpyYoyoProjectile;

    int Timer { get => (int)Projectile.ai[2]; set => Projectile.ai[2] = value; }

    public override void SetStaticDefaults() {
        ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 5.5f;
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

    public override void PostAI() {
        if(Main.rand.NextBool(15)) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Crimson);
        }

        var chompingFrequency = 8;
        if(Timer > 10) {
            Projectile.friendly = false;
            Projectile.frame = 1 + (Timer % (chompingFrequency * 2) / chompingFrequency);
            Timer -= 1;
        }
        else if(Timer > 0) {
            Projectile.frame = 0;
            if(Timer == 10) {
                SoundEngine.PlaySound(SoundID.Zombie64, Projectile.Center);

                var spitDirection = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
                for(var i = 0; i < 22; i++) {
                    var speed = 5f;
                    Dust.NewDust(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.Blood,
                        spitDirection.X * speed,
                        spitDirection.Y * speed
                    );
                }

                for(var i = 0; i < 6; i++) {
                    var speed = 5f;
                    Dust.NewDust(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.Bone,
                        spitDirection.X * speed,
                        spitDirection.Y * speed
                    );
                }

                var projectile = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    spitDirection * 10f,
                    ProjectileID.Bone,
                    Projectile.damage * 3,
                    0.3f,
                    Projectile.owner
                );
                projectile.friendly = true;
                projectile.hostile = false;
            }

            Timer -= 1;
        }
        else {
            Projectile.friendly = true;
        }
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
        Projectile.netUpdate = true;
    }
}
