using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class SlippedWhipProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.SlippedWhip.SlippedWhipProjectile.KEY;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.WhipSettings.Segments = 10;
        Projectile.WhipSettings.RangeMultiplier = 1f;
    }

    private float Timer {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
        Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

        Timer++;

        float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
        if(Timer >= swingTime || owner.itemAnimation <= 0) {
            Projectile.Kill();
            return;
        }

        owner.heldProj = Projectile.whoAmI;
        if(Timer == swingTime / 2) {
            // Plays a whipcrack sound at the tip of the whip.
            List<Vector2> points = Projectile.WhipPointsForCollision;
            Projectile.FillWhipControlPoints(Projectile, points);
            SoundEngine.PlaySound(SoundID.Item153, points[^1]);
        }

        var swingProgress = Timer / swingTime;
        if(Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && !Main.rand.NextBool(3)) {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);

            int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
            int dustType = DustID.Blood;
            if(Main.rand.NextBool(2))
                dustType = DustID.Bone;

            Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 100, Color.White);
            dust.position = points[pointIndex];
            dust.fadeIn = 0.3f;
            dust.noGravity = true;

            Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
            dust.velocity += spinningPoint.RotatedBy(owner.direction * ((float)Math.PI / 2f));
            dust.velocity *= 0.5f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * 0.9f); // multihit penalty

        if(Main.rand.NextFloat() < SlippedWhipItem.CageSpawnChance) {
            var actualTarget = target.whoAmI;
            if(target.realLife >= 0 && Main.npc[target.realLife] != null && Main.npc[target.realLife].active) {
                actualTarget = target.realLife;
            }

            Projectile.NewProjectile(
                Projectile.GetSource_OnHit(target),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<SlippedWhipCageProjectile>(),
                0,
                0f,
                Projectile.owner,
                actualTarget
            );
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        List<Vector2> whipPositions = new(Projectile.WhipPointsForCollision.Count);
        Projectile.FillWhipControlPoints(Projectile, whipPositions);

        var deltas = new Vector2[whipPositions.Count - 1];
        var deltaLengths = new float[whipPositions.Count - 1];

        var lineTexture = TextureAssets.FishingLine.Value;
        var lineSource = lineTexture.Frame();
        var lineOrigin = new Vector2(lineSource.Width / 2, 2);

        for(int i = 0; i < whipPositions.Count - 1; i++) {
            var position = whipPositions[i];
            deltas[i] = whipPositions[i + 1] - position;
            deltaLengths[i] = deltas[i].Length();

            Main.EntitySpriteDraw(
                lineTexture,
                position - Main.screenPosition,
                lineSource,
                Lighting.GetColor(position.ToTileCoordinates(), Color.Red),
                deltas[i].ToRotation() - MathHelper.PiOver2,
                lineOrigin,
                new Vector2(2, (deltaLengths[i] + 2) / lineSource.Height),
                SpriteEffects.None,
                0
            );
        }

        // Main.DrawWhip_WhipBland(Projectile, list);
        // The code below is for custom drawing.
        // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
        // However, you must adhere to how they draw if you do.

        var texture = TextureAssets.Projectile[Type].Value;

        var rotationOffset = 0f;
        var flip = SpriteEffects.None;
        var origin = new Vector2(0, 4);
        if(Projectile.spriteDirection > 0) {
            flip = SpriteEffects.FlipHorizontally;
            rotationOffset = MathHelper.Pi;
        }

        Main.EntitySpriteDraw(
            texture,
            whipPositions[0] - Main.screenPosition,
            new Rectangle(0, 0, 16, 10),
            lightColor,
            deltas[0].ToRotation() + rotationOffset,
            Projectile.spriteDirection < 0 ? origin : new Vector2(16, 10 - origin.Y),
            new Vector2(deltaLengths[0] / 16f, 1) * Projectile.scale,
            flip,
            0
        );

        for(int i = 1; i < whipPositions.Count - 1; i++) {
            var position = whipPositions[i];
            var source = new Rectangle(18, 0, 14, 8);
            Main.EntitySpriteDraw(
                texture,
                position - Main.screenPosition,
                source,
                Lighting.GetColor(position.ToTileCoordinates()),
                deltas[i].ToRotation() + rotationOffset,
                Projectile.spriteDirection < 0 ? origin : new Vector2(source.Width, source.Height - origin.Y),
                new Vector2(deltaLengths[i] / source.Width, 1) * Projectile.scale,
                flip,
                0
            );
        }

        Main.EntitySpriteDraw(
            texture,
            whipPositions[^1] - Main.screenPosition,
            new Rectangle(34, 0, 40, 24),
            lightColor,
            deltas[^1].ToRotation() + rotationOffset,
            Projectile.spriteDirection < 0 ? origin : new Vector2(28, origin.Y),
            Projectile.scale,
            flip,
            0
        );

        return false;
    }
}