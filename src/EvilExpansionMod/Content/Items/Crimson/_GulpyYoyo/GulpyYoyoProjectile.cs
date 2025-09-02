using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class GulpyYoyoProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.GulpyYoyo.KEY_GulpyYoyoProjectile;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 3.5f;
        ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f;
        ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
        Main.projFrames[Projectile.type] = 3;
    }

    public override void SetDefaults() {
        Projectile.width = 22;
        Projectile.height = 22;

        Projectile.aiStyle = ProjAIStyleID.Yoyo;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<GulpyHandProjectile>(),
            0,
            0f,
            Projectile.owner,
            Projectile.whoAmI
        );
    }

    // notes for aiStyle 99: 
    // localAI[0] is used for timing up to YoyosLifeTimeMultiplier
    // localAI[1] can be used freely by specific types
    // ai[0] and ai[1] usually point towards the x and y world coordinate hover point
    // ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
    // ai[0] being negative makes the yoyo move back towards the player
    // Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

    public override void PostAI() {
        Projectile.rotation = 0f;
        if(Main.rand.NextBool(5)) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Crimson);
        }
    }
}
