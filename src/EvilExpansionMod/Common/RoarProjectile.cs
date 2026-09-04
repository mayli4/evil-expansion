using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common;

internal class RoarProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";

    public static Projectile New(
        IEntitySource source,
        Vector2 position,
        int size,
        int timeLeft) {
        var projectile = Projectile.NewProjectileDirect(
            source,
            position,
            Vector2.Zero,
            ModContent.ProjectileType<RoarProjectile>(),
            0,
            0f
        );

        projectile.timeLeft = timeLeft;
        projectile.width = projectile.height = size;
        projectile.Center = position;
        projectile.rotation = Main.rand.NextFloatDirection() * 14f;
        projectile.netUpdate = true;

        return projectile;
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.hide = false;
        Projectile.friendly = false;
        Projectile.hostile = false;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var effect = Assets.Shaders.Pixel.Scream.Asset.Value;
        var alpha = Projectile.timeLeft > 20 ? 1f : Projectile.timeLeft / 10f;

        Graphics.Graphics.Begin(Graphics.Graphics.WorldTransformMatrix) // ?
            .SetSamplerState(0, SamplerState.PointWrap)
            .SetSamplerState(1, SamplerState.PointWrap)
            .SetTexture(1, Assets.Images.Sample.Noise3.Asset.Value)
            .SetEffectParams(effect,
                ("uTime", Main.GameUpdateCount * 0.9f),
                ("uSize", (float)Projectile.width),
                ("uColor", Color.Black * 0.45f * alpha))
            .DrawTexture(new()
            {
                Texture = Assets.Images.Sample.Noise1.Asset.Value,
                Position = Projectile.position,
                Size = Projectile.Size,
                Effect = effect,
            })
            .ApplyBloom(2f)
            .End();
        return false;
    }
}
