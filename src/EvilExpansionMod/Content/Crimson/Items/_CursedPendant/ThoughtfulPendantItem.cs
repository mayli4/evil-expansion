using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using EvilExpansionMod.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Mathematics.Angle;

namespace EvilExpansionMod.Content.Crimson;

internal sealed class FriendlyCultistPortal : CultistPortal {
    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.hostile = false;
        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Magic;

        Projectile.knockBack = 5f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Terraria.Player player = Main.player[Projectile.owner];
        float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(40f), MathHelper.ToRadians(40f));
        Projectile.NewProjectile(
            player.GetSource_FromThis(),
            target.position,
            Projectile.velocity * angleOffset * 2f,
            ModContent.ProjectileType<ThoughtfulOrbProjectile>(),
            0,
            0.2f
        );
        Dust.NewDustPerfect(
            Projectile.Center + Projectile.velocity * 20f
            + Main.rand.NextFloatDirection() * 20f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
            DustID.Blood,
            Velocity: Projectile.velocity * Main.rand.NextFloat(1f, 6f),
            Main.rand.Next(0, 50),
            default,
            Main.rand.NextFloat(0.75f, 1.2f)
        );
    }
}

[AutoloadEquip(EquipType.Neck)]
public class ThoughtfulPendantItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CursedPendant.CursedPendantitem.KEY;

    private int _portalCooldown;
    float _portalRotation;

    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1, silver: 30);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<ThoughtfulPendantPlayer>();
        modPlayer.HasThoughtfulPendantAccessory = true;
    }
}
public class PortalKnives : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.CursedPendant.CultistKnives.KEY;

    private bool iHitAWall = false;
    private bool abovePlayer = false;
    private bool leftOfPlayer = false;
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults() {
        Projectile.width = 26 * 2;
        Projectile.height = 8 * 2;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 60 * 5;

        Projectile.aiStyle = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        //Projectile.usesIDStaticNPCImmunity = true;
        //Projectile.idStaticNPCHitCooldown = 6;

        Projectile.extraUpdates = 8;
        Main.projFrames[Projectile.type] = 3;
    }

    public override void OnSpawn(IEntitySource source) {
        Terraria.Player player = Main.player[Projectile.owner];

        Projectile.frame = Main.rand.Next(0, 2 + 1);
        Projectile.rotation = Projectile.velocity.ToRotation();
        if(Projectile.position.X < player.position.X) leftOfPlayer = true;
        if(Projectile.position.Y < player.position.Y) abovePlayer = true;
    }
    public override void AI() {
        Terraria.Player player = Main.player[Projectile.owner];

        if(Projectile.timeLeft < 60 * 2 ||
            leftOfPlayer && Projectile.position.X > player.position.X ||
            !leftOfPlayer && Projectile.position.X < player.position.X ||
            abovePlayer && Projectile.position.Y > player.position.Y ||
            !abovePlayer && Projectile.position.Y < player.position.Y
            ) Projectile.tileCollide = true;

        if(Projectile.timeLeft <= 60f) {
            Projectile.Opacity = MathHelper.Lerp(1, 0, (60f - Projectile.timeLeft) / 60f); // Timeleft = 60, Opcaity = 1, Timeleft = 0, Opacity = 0,
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Terraria.Player player = Main.player[Projectile.owner];
        float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(40f), MathHelper.ToRadians(40f));
        Projectile.NewProjectile(
            player.GetSource_FromThis(),
            target.position,
            Projectile.velocity * angleOffset * 0.5f,
            ModContent.ProjectileType<ThoughtfulOrbProjectile>(),
            0,
            0.2f
        );
        for(var i = 0; i < 30; i++) {
            Dust dust = Dust.NewDustPerfect(
                target.Center + new Vector2(
                    target.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-18f, 18f),
                    target.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-18f, 18f)
                ),
                DustID.Blood,
                (Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.2f, 10f)).RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(22.5f), MathHelper.ToRadians(22.5f))),
                default,
                default,
                Main.rand.NextFloat(0.7f, 1.5f)
            );
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity = Projectile.velocity * 0.000001f;
        iHitAWall = true;
        return false;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new Rectangle(
            0,
            Projectile.frame * frameHeight,
            texture.Width,
            frameHeight
        );
        Vector2 drawOrigin = new Vector2(texture.Width / 2f, frameHeight / 2f);
        float fadeOut = (255f - Projectile.alpha) / 255f; // If Alpha = 255, fadeOut = 0, if Alpha = 0, fadeOut = 1

        // Afterimage
        if(!iHitAWall) {
            for(int k = Projectile.oldPos.Length - 1; k >= 0; k--) {
                if(Projectile.oldPos[k] == Vector2.Zero)
                    continue;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + Projectile.Size / 2f + new Vector2(0f, Projectile.gfxOffY);

                float progress = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Color afterimageColor = Color.Red * progress * 0.6f;

                //afterimageColor.A = 20; // Afterimage is fullbright
                afterimageColor.A = (byte)Main.rand.Next(0, 75); // Afterimage is fullbright

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    sourceRectangle,
                    afterimageColor,
                    Projectile.oldRot[k],
                    drawOrigin,
                    Projectile.scale * 0.5f,
                    SpriteEffects.None,
                    0
                );
            }
        }

        // Knife itself
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            sourceRectangle,
            lightColor * fadeOut,
            Projectile.rotation,
            drawOrigin,
            Projectile.scale,
            SpriteEffects.None
        );
        
        return false;
    }
}
public class ThoughtfulOrbProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.CursedPendant.PendantOrb.KEY;

    readonly static int MaxTimeLeft = 60 * 15;
    public Player Owner => Main.player[Projectile.owner];
    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
    }
    bool Closeenough = false;
    public override void OnSpawn(IEntitySource source) {
        Projectile.timeLeft = (int)(MaxTimeLeft + Main.rand.NextFloat(-60f, 60f));
        Projectile.scale = Main.rand.NextFloat(0.85f, 1.1f);
        SoundEngine.PlaySound(SoundID.NPCHit25 with { Volume = 0.5f, PitchRange = (0.7f, 1.3f) }, Projectile.Center);
    }
    public override void AI() {
        Projectile.ai[0]++;
        Projectile.velocity *= 0.98f;
        Projectile.velocity.Y -= MathF.Cos(Projectile.ai[0] * 0.05f) * 0.1f;

        if(Projectile.timeLeft <= 6f) {
            Projectile.scale = MathHelper.Lerp(1, 2, (6f - Projectile.timeLeft) / 6f); // Timeleft = 20, Scale = 1, Timeleft = 0, Scale = 2,
        }

        var owner = Main.player[Projectile.owner];
        if(
            Main.netMode != NetmodeID.MultiplayerClient
            && owner != null
            && !owner.dead
            && owner.Hitbox.Intersects(Projectile.Hitbox)
        ) {
            Projectile.timeLeft = 45;
            Projectile.velocity = Vector2.Lerp(
                owner.Center.DirectionTo(Projectile.Center),
                owner.velocity.SafeNormalize(Vector2.Zero),
                0.5f
            ) * 14f;
            owner.Heal(Main.rand.Next(2, 4 + 1));
            Projectile.netUpdate = true;
            SoundEngine.PlaySound(SoundID.Item3 with { Volume = 0.8f, PitchRange = (0.7f, 1.3f) }, Projectile.Center);
            Projectile.Kill();
        }

        // We implement a little QOL player tracking. For gamefeel purposes
        float distanceToOwner = Projectile.Distance(Owner.Center);

        if(distanceToOwner <= 16 * Main.rand.NextFloat(5f, 30f) && distanceToOwner >= 10f) {
            Closeenough = true;
            Projectile.tileCollide = false;
            Vector2 direction = Vector2.Normalize(Owner.Center - Projectile.Center); // Get normalized direction
            float speed = Main.rand.NextFloat(25f,55f);   // Speed cap
            float inertia = Main.rand.NextFloat(20f, 21f); // Turning smoothness (higher = wider turns)

            Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction * speed) / inertia;
        }

        if(Main.rand.NextBool(5)){
            Dust.NewDustPerfect(
                Projectile.Center + Main.rand.NextVector2Circular(6f, 6f) - Main.rand.NextVector2Circular(6f, 6f),
                DustID.Blood,
                Vector2.One * Main.rand.NextFloat(-1f, 1f),
                Main.rand.Next(0, 255),
                default,
                Main.rand.NextFloat(0.75f, 1f)
            );
        }
        Lighting.AddLight(Projectile.Center, 0.4f, 0.02f, 0.02f);
    }
    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item54 with { Volume = 0.8f, PitchRange = (0.7f, 1.3f) }, Projectile.Center);
        var rotation = Main.rand.NextFloat();
        for(var i = 0; i < 10; i++) {
            var direction = rotation.ToRotationVector2();
            Dust.NewDustPerfect(
                Projectile.Center + Main.rand.NextFloatDirection() * 2f * Projectile.velocity.RotatedBy(MathF.PI / 2f),
                DustID.Blood,
                direction * Main.rand.NextFloat(0.2f, 2f),
                Main.rand.Next(0, 50),
                default,
                Main.rand.NextFloat(0.75f, 1.2f)
            );
            rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }
    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var stretchMax = 0.4f;
        var stretch = stretchMax / MathF.Max(Projectile.velocity.Length() * 0.8f, 1f);
        float stretchAmount = stretchMax - stretch;

        // Squash X and stretch Y when moving vertically, or vice-versa based on velocity direction
        Vector2 velocityDir = Projectile.velocity.SafeNormalize(Vector2.Zero);
        float verticalBias = MathF.Abs(velocityDir.Y) - MathF.Abs(velocityDir.X); // +1 when moving purely vertical, -1 when purely horizontal

        Vector2 directionalScale = new Vector2(
            1f - stretchAmount * verticalBias,
            1f + stretchAmount * verticalBias
        ) * Projectile.scale;

        Graphics.Begin()
            .DrawTexture(new()
            {
                Texture = texture,
                Position = Projectile.Center - Main.screenPosition,
                Color = lightColor * Projectile.Opacity,
                Rotation = 0f, // Keeps sprite upright
                Origin = texture.Size() / 2f,
                Scale = directionalScale,
                SpriteEffects = SpriteEffects.None, // No upside-down flipping
            })
            .End();

        return false;
    }
}