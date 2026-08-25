using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class InflatableDevilOWarItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.InflatableDevilOWar.InflatableDevilOWarItem.KEY;

    private int _projectileID = -1;

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateVanity(Player player) {
        if(player.whoAmI == Main.myPlayer) {
            if(_projectileID != -1 && Main.projectile[_projectileID].active && Main.projectile[_projectileID].owner == player.whoAmI && Main.projectile[_projectileID].type == ModContent.ProjectileType<InflatableDevilOWarProjectile>()) {
                Main.projectile[_projectileID].timeLeft = 2;
                Main.projectile[_projectileID].ai[0] = 0f;
                Main.projectile[_projectileID].netUpdate = true;
            }
            else {
                _projectileID = Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<InflatableDevilOWarProjectile>(),
                    0,
                    0f,
                    player.whoAmI,
                    0f
                );
            }
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.jumpSpeedBoost = 3f;
        player.jumpBoost = true;
        
        if(player.whoAmI == Main.myPlayer) {
            if(_projectileID != -1 && Main.projectile[_projectileID].active && Main.projectile[_projectileID].owner == player.whoAmI && Main.projectile[_projectileID].type == ModContent.ProjectileType<InflatableDevilOWarProjectile>()) {
                Main.projectile[_projectileID].timeLeft = 2;
                Main.projectile[_projectileID].ai[0] = hideVisual ? 1f : 0f;
                Main.projectile[_projectileID].netUpdate = true;
            }
            else {
                _projectileID = Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<InflatableDevilOWarProjectile>(),
                    0,
                    0f,
                    player.whoAmI,
                    hideVisual ? 1f : 0f
                );
            }
        }
    }
}
public class InflatableDevilOWarProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.InflatableDevilOWar.InflatableDevilOWarHead.KEY;

    private Vector2[][] _tentacleTrailPositions;
    private float[] _tentacleWaveDirections;
    private Vector2[] _stringTrailPositions;
    private const int tentacle_segment_count = 8;
    private const float base_scale = 0.5f;
    private const int string_segment_count = 3;

    private bool IsHidden => Projectile.ai[0] == 1f;

    public override void SetDefaults() {
        Projectile.width = (int)(36 * base_scale);
        Projectile.height = (int)(36 * base_scale);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 2;
        Projectile.alpha = 0;
        Projectile.scale = base_scale;
    }

    public override void OnSpawn(IEntitySource source) {
        _tentacleTrailPositions = new Vector2[4][];
        for(int i = 0; i < _tentacleTrailPositions.Length; i++) {
            _tentacleTrailPositions[i] = new Vector2[tentacle_segment_count];
            for(int j = 0; j < tentacle_segment_count; j++) {
                _tentacleTrailPositions[i][j] = Projectile.Center;
            }
        }
        _tentacleWaveDirections = new float[_tentacleTrailPositions.Length];
        for(int i = 0; i < _tentacleWaveDirections.Length; i++) {
            _tentacleWaveDirections[i] = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        _stringTrailPositions = new Vector2[string_segment_count];
        for(int i = 0; i < string_segment_count; i++) {
            _stringTrailPositions[i] = Projectile.Center;
        }
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Vector2 targetPos = player.Center + new Vector2(player.direction * 20f, -player.height / 2f - 40f);

        Projectile.spriteDirection = -player.direction;

        float speed = 0.3f;
        Vector2 velocityToTarget = (targetPos - Projectile.Center);
        Projectile.velocity = velocityToTarget * speed;

        Projectile.velocity.Y += MathF.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 0.1f) * 1.0f;

        Projectile.rotation = -Projectile.velocity.X * 0.02f;

        Projectile.hide = IsHidden;
    }

    public override bool? CanCutTiles() => false;

    private void PopulateTrailsForDrawing(Vector2 interpolatedBodyPosition, Color _, Player player) {
        float Equation(float x) {
            return 0.2f * MathF.Sin(x) + 0.8f * MathF.Cos(x + MathHelper.PiOver4);
        }

        var initialRelativePositions = new[] {
            new Vector2(-0.3f, 0.3f),
            new Vector2(0.3f, 0.2f),
            new Vector2(0.4f, 0.1f),
            new Vector2(-0.2f, 0.4f)
        };

        var rotatedBodyOffset = new Vector2(Projectile.spriteDirection * -3, 10).RotatedBy(Projectile.rotation);
        var bodyDrawPosition = interpolatedBodyPosition + rotatedBodyOffset;

        for(int i = 0; i < _tentacleTrailPositions.Length; i++) {
            var positions = _tentacleTrailPositions[i];
            var currentTentacleBase = bodyDrawPosition + initialRelativePositions[i] * 16f * Projectile.scale;

            positions[0] = currentTentacleBase;
            var moveDirection = initialRelativePositions[i].SafeNormalize(Vector2.Zero);

            var perpendicular = new Vector2(-moveDirection.Y, moveDirection.X);
            perpendicular = perpendicular.RotatedBy(_tentacleWaveDirections[i]);

            float phaseOffset = Projectile.whoAmI * 0.123f;

            for(int j = 1; j < tentacle_segment_count; j++) {
                float factor = j / (tentacle_segment_count - 1f);
                positions[j] = currentTentacleBase
                               + moveDirection
                               * MathHelper.Lerp(60, 80, MathF.Sin(Main.GameUpdateCount * (0.02f + i * 0.003f) + i * 0.6f + phaseOffset))
                               * factor * Projectile.scale
                               + perpendicular
                               * Equation(Main.GameUpdateCount * (0.04f + i * 0.005f) + factor * 4f + factor + i * 0.4f + phaseOffset * 0.5f)
                               * 10f * Projectile.scale;
            }
        }

        _stringTrailPositions[0] = player.MountedCenter + new Vector2(0, player.height / 4f);
        _stringTrailPositions[string_segment_count - 1] = interpolatedBodyPosition + new Vector2(0, 10);

        if(string_segment_count > 2) {
            var start = _stringTrailPositions[0];
            var end = _stringTrailPositions[string_segment_count - 1];
            var midPoint = (start + end) / 2f;

            float swayAmplitude = 5f * Projectile.scale;
            var swayDirection = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY);

            midPoint += swayDirection * MathF.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI * 0.5f) * swayAmplitude;
            midPoint.Y += 10f * Projectile.scale;

            for(int i = 1; i < string_segment_count - 1; i++) {
                float t = i / (string_segment_count - 1f);
                _stringTrailPositions[i] = Vector2.Lerp(start, midPoint, t) + Vector2.Lerp(midPoint, end, t) - midPoint;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        if(IsHidden) {
            return false;
        }

        var player = Main.player[Projectile.owner];
        PopulateTrailsForDrawing(Projectile.Center, lightColor, player);

        var headTexture = ModContent.Request<Texture2D>(Texture).Value;
        var insidesTexture = Assets.Images.Corruption.Items.InflatableDevilOWar.InflatableDevilOWarBody.Asset.Value;
        var tentacleTexture = Assets.Images.Corruption.Items.InflatableDevilOWar.InflatableDevilOWarTentacle.Asset.Value;

        bool flipped = Projectile.spriteDirection != -1;
        var spriteEffects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var origin = headTexture.Size() / 2f;
        origin.X = flipped ? headTexture.Width - origin.X : origin.X;

        using var pipeline = Renderer.Begin(Graphics.WorldTransformMatrix);
        if(_tentacleTrailPositions != null) {

            pipeline.SetSamplerState(0, SamplerState.PointClamp);
            pipeline.SetTexture(tentacleTexture);

            foreach(var positions in _tentacleTrailPositions) {
                pipeline.DrawTrail(positions, 10, lightColor);
            }

            if(_stringTrailPositions != null) {
                pipeline
                    .SetSamplerState(SamplerState.PointClamp)
                    .SetTexture(TextureAssets.MagicPixel.Value)
                    .DrawTrail(
                        _stringTrailPositions,
                        static _ => 2f,
                        static _ => Color.White
                    );
            }
        }

        var insidesOffset = new Vector2(0, 24 * Projectile.scale).RotatedBy(Projectile.rotation);
        pipeline.DrawTexture(new()
        {
            Texture = insidesTexture,
            Position = Projectile.Center + insidesOffset,
            Color = lightColor,
            Rotation = Projectile.rotation,
            Origin = insidesTexture.Size() / 2f,
            SpriteEffects = spriteEffects,
        });

        var headOffset = new Vector2(0, -4 * Projectile.scale).RotatedBy(Projectile.rotation);
        pipeline.DrawTexture(new()
        {
            Texture = headTexture,
            Position = Projectile.Center + headOffset,
            Color = lightColor * 0.8f,
            Rotation = Projectile.rotation,
            Origin = origin,
            SpriteEffects = spriteEffects,
        });

        return false;
    }
}