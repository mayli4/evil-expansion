using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;

enum PortalType {
    Spear,
    Blood
}

public class CultistPortal : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_PortalSpear;

    PortalType PortalType => (PortalType)Projectile.ai[0];
    bool _spawnedEye;

    public override void SetDefaults() {
        Projectile.width = 45;
        Projectile.height = 140;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 10;

        Projectile.aiStyle = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 999;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = (int)Projectile.ai[1];
        Projectile.netUpdate = true;
    }

    public override void AI() {
        // if(Main.rand.NextBool(12)) {
        //     Dust.NewDustPerfect(
        //         Projectile.Center
        //             + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f)
        //             + Projectile.velocity * 10f,
        //         DustID.SilverFlame,
        //         Projectile.velocity * 1f
        //     );
        // }

        var t = Projectile.timeLeft / Projectile.ai[1];
        switch(PortalType) {
            case PortalType.Blood:
                if(Main.netMode != NetmodeID.MultiplayerClient && !_spawnedEye && t < 0.6f) {
                    _spawnedEye = true;

                    var spawnPosition = Projectile.Center + Projectile.velocity * 20f;
                    var npc = NPC.NewNPCDirect(
                        Projectile.GetSource_FromAI(),
                        (int)spawnPosition.X,
                        (int)spawnPosition.Y,
                        ModContent.NPCType<CultistEye>()
                    );
                    npc.velocity = Projectile.velocity * 12f;
                }
                break;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var t = Projectile.timeLeft / Projectile.ai[1];
        if(PortalType != PortalType.Spear || t < 0.8f || t > 0.9f) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            Projectile.Center + Projectile.velocity * 120f,
            10,
            ref _
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var spearTexture = TextureAssets.Projectile[Type].Value;
        var sampleTexture0 = Assets.Assets.Textures.Sample.PerlinNoise.Value;
        var sampleTexture1 = Assets.Assets.Textures.Sample.Noise2.Value;
        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;

        var effect = Assets.Assets.Effects.Pixel.CultistPortal.Value;
        var destination = new Rectangle(
            (int)(Projectile.Center.X - Main.screenPosition.X),
            (int)(Projectile.Center.Y - Main.screenPosition.Y),
            Projectile.width,
            Projectile.height
        );


        var t = Projectile.timeLeft / Projectile.ai[1];
        var scale = t < 0.1f ? t / 0.1f : (t > 0.9f ? (0.1f - (t - 0.9f)) / 0.1f : 1f);

        var color1 = new Color(249, 197, 55);
        var color2 = new Color(242, 95, 2);
        var rotation = Projectile.velocity.ToRotation();

        var middleColor = PortalType switch
        {
            PortalType.Spear => new Color(34, 11, 23),
            PortalType.Blood => new Color(90, 21, 30),
        };
        var circleTexture = Assets.Assets.Textures.Misc.Circle.Value;
        new Renderer.Pipeline()
            .BeginPixelate()
            .DrawSprite(
                circleTexture,
                Projectile.Center - Main.screenPosition + Projectile.velocity * 20f,
                color: middleColor,
                rotation: rotation,
                origin: circleTexture.Size() / 2f + Vector2.UnitY * 4f,
                scale: scale * new Vector2(0.7f, 2.2f)
            )
            .ApplyOutline(color2)
            .ApplyOutline(middleColor)
            .End()
            .BeginPixelate(new() { CustomEffect = effect })
            .EffectParams(
                effect,
                ("tex1", sampleTexture1),
                ("size", scale),
                ("time", Main.GameUpdateCount * 0.05f),
                ("color1", color1.ToVector4()),
                ("color2", color2.ToVector4())
            )
            .DrawSprite(
                sampleTexture0,
                destination,
                rotation: rotation,
                origin: new(Projectile.width, Projectile.height * 4 - 7)
            )
            .ApplyOutline(color2)
            .ApplyOutline(middleColor)
            .End()
            .Flush();

        switch(PortalType) {
            case PortalType.Blood:
                break;
            case PortalType.Spear:
                // dont ask
                var spearX = t < 0.2f ? 0f :
                    (t < 0.3f ? (t - 0.2f) / 0.1f :
                    (t < 0.8f ? 1 : t < 0.9f ? (0.1f - (t - 0.8f)) / 0.1f : 0f));

                Main.spriteBatch.Draw(
                    spearTexture,
                    Projectile.Center - Main.screenPosition + Projectile.velocity * 14f,
                    new Rectangle(0, 0, (int)(spearX * spearTexture.Width), spearTexture.Height),
                    lightColor,
                    rotation,
                    Vector2.UnitY * 18,
                    1f,
                    SpriteEffects.FlipHorizontally,
                    0f
                );
                break;
        }

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition + Projectile.velocity * 20f,
            null,
            color1 * 0.2f,
            rotation,
            glowTexture.Size() * 0.5f,
            0.55f * scale * new Vector2(1.25f, 2.2f),
            SpriteEffects.None,
            0
        );
        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}
