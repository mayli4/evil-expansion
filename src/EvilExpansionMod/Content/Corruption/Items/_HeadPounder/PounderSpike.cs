using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class PounderSpike : ModProjectile {
    public override void OnSpawn(IEntitySource source) {

    SoundEngine.PlaySound(SoundID.Item51, Projectile.position);
        // No. of dust particles
    int dustCount = 8; 
        // Velocity
    float burstSpeed = 2f; 

    for (int i = 0; i < dustCount; i++) {
            // Angle
        float progress = (float)i / (dustCount - 1);
        float angle = MathHelper.Lerp(MathHelper.ToRadians(-95), MathHelper.ToRadians(-85), progress);
            // Convert the angle into a velocity vector
        Vector2 velocity = angle.ToRotationVector2() * burstSpeed;
            // Randomness
        velocity.X += Main.rand.NextFloat(-0.95f, 0.95f);
        velocity.Y += Main.rand.NextFloat(-0.95f, 0.95f);
            // Spawn the dust at the projectile's center
        Dust dust = Dust.NewDustDirect(Projectile.Center, 5, -10, DustID.Dirt, velocity.X, velocity.Y);
        
        dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
    }
    }
    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.PounderSpike.KEY;

    int SpikeIndex => (int)Projectile.ai[0];

    static int MaxTimeLeft = 120;
    static int PopUpFrames = 8;

    float Scale => Projectile.timeLeft > MaxTimeLeft - PopUpFrames ? (float)(MaxTimeLeft - Projectile.timeLeft) / PopUpFrames : 1f;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.aiStyle = -1;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Projectile.timeLeft < MaxTimeLeft - PopUpFrames - 5) return false;

        var _ = 0f;
        var spikeWidth = SpikeIndex switch
        {
            0 or 1 or 2 => 20,
            3 or 4 or 5 => 25,
            _ => 35,
        };

        var spikeHeight = SpikeIndex switch
        {
            0 or 1 or 2 => 45,
            3 or 4 or 5 => 60,
            _ => 95,
        };

        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            Projectile.Center + (Projectile.rotation - MathF.PI / 2f).ToRotationVector2() * spikeHeight,
            spikeWidth,
            ref _
            );
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var cellWidth = 40;
        var source = new Vector4(SpikeIndex * cellWidth, 0, cellWidth, texture.Height);

        Renderer.BeginPipeline(scale: Scale)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = Projectile.Center - Main.screenPosition,
                Color = lightColor * MathF.Min(Projectile.timeLeft / 10f, 1f),
                Source = source,
                Rotation = Projectile.rotation,
                Origin = new Vector2(cellWidth / 2f, 90),
                Scale = Vector2.One * Scale,
                SpriteEffects = SpriteEffects.None,
            })
            .End();

        return false;
    }
}
