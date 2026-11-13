using Daybreak.Common.Features.Hooks;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.GlobalTileHooks;
using static Daybreak.Common.Mathematics.Angle;
using static tModPorter.ProgressUpdate;
namespace EvilExpansionMod.Content.Crimson;

public class ThoughtfulPendantPlayer : ModPlayer {
    public bool HasThoughtfulPendantAccessory = false; // UpdateAccessory uses this to tell Modplayer if equipped
    public readonly int DamageThreshold = 1200; // Damage threshold for the pendant to activate
    public int DamageDealt = 0; // Tracks damage dealt
    private int DamageWearOff = 0; // Decreases stored damage over time
    public bool IsChargeBarPresent = false; // Tracks if the charge bar is present
    public int attackCooldown = 60; // Number of frames before damage can accumulate again

    public float Progress => Math.Min((float)DamageDealt / DamageThreshold, 1f); // Progress towards activation

    public override void ResetEffects() {
        HasThoughtfulPendantAccessory = false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if(HasThoughtfulPendantAccessory) {
            DamageDealt += damageDone;
        }
    }
    public override void OnHurt (Player.HurtInfo info) {
        if(!info.PvP) return;

        int attackerIndex = info.DamageSource.SourcePlayerIndex; // I can't really check if this works or not ATM
        if(attackerIndex >= 0 && attackerIndex < Main.maxPlayers) {
            Player attacker = Main.player[attackerIndex];
            if(attacker != null && attacker.active && HasThoughtfulPendantAccessory) {
                DamageDealt += info.Damage;
            }
        }
    }
    public bool IsCultistPresent(Player player) { // Check if the shield exists
        for(int i = 0; i < Main.maxProjectiles; i++) {
            Projectile proj = Main.projectile[i];

            if(proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ThoughtlessCultist>()) {
                return true;
            }
        }
        return false;
    }
    public override void PostUpdate() {
        if(Player.whoAmI != Main.myPlayer)
            return;

        if(!HasThoughtfulPendantAccessory) { // Reset if not equipped
            DamageDealt = 0;
            DamageWearOff = 0;
        }
        else if(DamageDealt > 0 && DamageWearOff == 0 && Progress < 1) { // Stored damage is continually lost
            DamageDealt -= 3;
            DamageWearOff = (int) (60 * 0.1); // 10 times per second
        }
        if(!IsCultistPresent(Player)) {
            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.position,
                Vector2.Zero,
                ModContent.ProjectileType<ThoughtlessCultist>(),
                0,
                0
            );
        }

        if (DamageWearOff > 0) DamageWearOff--; // Timer increments
        if(!IsChargeBarPresent && attackCooldown > 0) { // Cooldown only sticks down when the charge bar is no more
            attackCooldown--;
            DamageDealt = 1; // Basically ignore damage dealt during CD
        }
        //IsChargeBarPresent = false;
        Main.NewText("DamageDealt" + DamageDealt, Microsoft.Xna.Framework.Color.White);
        Main.NewText("attackCooldown" + attackCooldown, Microsoft.Xna.Framework.Color.White);
        Main.NewText("Progress" + Progress, Microsoft.Xna.Framework.Color.White);
        Main.NewText("IsChargeBarPresent" + IsChargeBarPresent, Microsoft.Xna.Framework.Color.White);
        Main.NewText("IsCultistPresent" + IsCultistPresent, Microsoft.Xna.Framework.Color.White);
    }
}

public class ThoughtlessCultist : ModProjectile {
    private int attackTimer;
    private int pulseTimer;
    private int attackTimerMax = 30; // How many frames the attack will be performed for
    private float _portalCounter;
    private float _portalSpacing = 16 * 2f;
    private float _portalRotation;
    Vector2 mouseDirection;
    public override string Texture => Assets.Images.Crimson.Items.CursedPendant.ThoughtlessCultist.KEY;
    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Default;
        Projectile.timeLeft = int.MaxValue;
    }

    public override void OnSpawn(IEntitySource source) {
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<ThoughtfulPendantPlayer>();
        if (modPlayer.Progress == 1) modPlayer.DamageDealt = 1;
        attackTimer = -1;
        pulseTimer = 0;
    }
    public override void AI() {
        Main.NewText("attackTimer" + attackTimer, Microsoft.Xna.Framework.Color.White);
        Main.NewText("pulseTimer" + pulseTimer, Microsoft.Xna.Framework.Color.White);
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<ThoughtfulPendantPlayer>();

        if(player.dead || !modPlayer.HasThoughtfulPendantAccessory) {
            Projectile.Kill();
            return;
        }

        // Player tracking code that I fucking stole from the Blood Warden's .cs file
        var distanceToOwner = Projectile.Distance(player.Center);

        Projectile.direction = (player.Center.X < Projectile.Center.X) ? -1 : 1;
        Projectile.spriteDirection = Projectile.direction;

        var targetPosition = player.Center + new Vector2(
            player.direction == 1 ? 80f : -80f,
            MathF.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 0.1f) * 10f - 30f);

        Vector2 vectorToTarget = targetPosition - Projectile.Center;
        float distance = vectorToTarget.Length();

        if(distance > 20f) {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity *= 1.05f, vectorToTarget / distance * 10f, 0.1f);
        }
        else {
            Projectile.velocity *= 0.95f;
        }

        if(Projectile.velocity.Length() < 0.1f) {
            Projectile.velocity.Y += 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
        }

        // Whether to draw the charge bar, whether to initiate an attack
        if(player.dead || !modPlayer.HasThoughtfulPendantAccessory || modPlayer.DamageDealt <= 0 || attackTimer == 0) { // If player dead, unequipped, or no damage stored, or attack done performing, stop drawing the bar
            modPlayer.IsChargeBarPresent = false;
        }
        else if (modPlayer.attackCooldown <= 0)modPlayer.IsChargeBarPresent = true; // Otherwise, draw it

        if(modPlayer.Progress == 1) { // Attack!
            if(attackTimer == -1) {
                mouseDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                attackTimer = attackTimerMax;
                modPlayer.attackCooldown = 60; // This is where CD is determined
            }
            if(attackTimer == 0) {
                modPlayer.DamageDealt = 1;
                attackTimer = -1;
                pulseTimer = 0;
            }
        }
        if(pulseTimer < attackTimerMax && modPlayer.Progress == 1) {
            pulseTimer++;
        }

        if(attackTimer > 0 && modPlayer.Progress >= 1) {
            if(attackTimer % 30 == 0) {
                for(int i = 0; i < (int)Main.rand.Next(1, 3); i++) {
                    Vector2 summonPosition = Main.MouseWorld;
                    var position = summonPosition - 30 * 16f * _portalRotation.ToRotationVector2();
                    var direction = position.DirectionTo(summonPosition);

                    Projectile.NewProjectile(
                        player.GetSource_FromThis(),
                        position,
                        direction,
                        ModContent.ProjectileType<FriendlyCultistPortal>(),
                        (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(35),
                        0.2f,
                        ai0: (float)PortalType.Knives,
                        ai1: 80
                    );
                    _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                }
            }
            attackTimer--;
        }
    }
    public override bool PreDraw(ref Color lightColor) {
        Player player = Main.player[Projectile.owner];
        var modPlayer = player.GetModPlayer<ThoughtfulPendantPlayer>();

        // OUR LITTLE FRIEND
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

        // CHARGE BAR
        Texture2D chargeFront = Assets.Images.Crimson.Items.CursedPendant.CursedPendantChargebarFront.Asset.Value;
        Texture2D chargeBack = Assets.Images.Crimson.Items.CursedPendant.CursedPendantChargebarBack.Asset.Value;
        int backBarWidth = 12;
        int backBarHeight = 46;
        int frontBarWidth = 12;
        int frontBarHeight = 46;

        int DamageDealtOffset = modPlayer.DamageDealt + modPlayer.DamageThreshold / 10;

        float goingFuckingCrazy = modPlayer.Progress > 0.9 ? Math.Min(modPlayer.Progress * modPlayer.Progress, 1f) : 0f;
        float shakey = Main.rand.NextFloat(-1.5f, 1.5f) * goingFuckingCrazy;
        float shakex = Main.rand.NextFloat(-2.5f, 2.5f) * goingFuckingCrazy;

        Vector2 pivotPoint = new(backBarWidth / 2f, backBarHeight / 2f);
        Vector2 position = new Vector2(32 + shakex, 0 + shakey) + Projectile.position - Main.screenPosition;
        Vector2 frontPos = position - pivotPoint;
        int offsetHeight = frontBarHeight - (int)(frontBarHeight * Math.Min(((float)DamageDealtOffset) / modPlayer.DamageThreshold, 1f));
        int readingHeight = (int)(frontBarHeight * Math.Min(((float)DamageDealtOffset) / modPlayer.DamageThreshold, 1f));
        //Main.NewText("offsetHeight" + offsetHeight, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("readingHeight" + readingHeight, Microsoft.Xna.Framework.Color.White);

        Rectangle frontCrop = new(0, offsetHeight, frontBarWidth, readingHeight);

        if(modPlayer.IsChargeBarPresent && modPlayer.DamageDealt > 1) {
            Main.EntitySpriteDraw(chargeBack, position, null, Color.White, 0f, pivotPoint, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(chargeFront, frontPos + Vector2.UnitY * offsetHeight, frontCrop, Color.Lerp(Color.White, Color.Yellow, goingFuckingCrazy), 0, Vector2.Zero, 1, SpriteEffects.None);
        }

        // PENDANT PULSE
        var pendantPulseTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPulseSmall.Asset.Value;
        var pulseFrameHeight = 82; // 408 large, 82 small
        var pulseSource = new Rectangle(0, 0, pendantPulseTexture.Width, pulseFrameHeight);

        float pulseProgress = (float)pulseTimer / attackTimerMax;
        float pulseScale = (float)(pulseProgress < 0.8 ? 1f + (0.8f - pulseProgress) / 10f : 1f + (pulseProgress - 0.8) * 2f); //(float)( pulseProgress < 0.8 ? 1f + ( 0.8f - pulseProgress) / 10f : 1f + (pulseProgress-0.8) * 2f )
        float pulseOpacity = (float)(pulseProgress < 0.7 ? (0.3f + pulseProgress) : (1f - ((pulseProgress - 0.7f) / 0.3f)));
        Vector2 pulseOrigin = pulseSource.Size() / 2f;

        //Main.NewText("pulseSource" + pulseSource, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("pulseSource.Y" + pulseSource.Y, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("pulseProgress" + pulseProgress, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("pulseScale" + pulseScale, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("pulseOpacity" + pulseOpacity, Microsoft.Xna.Framework.Color.White);

        if(modPlayer.Progress == 1) {
            var t = MathF.Sin(MathHelper.Pi * pulseTimer / 60f);

            Main.spriteBatch.Draw(
                pendantPulseTexture,
                Projectile.position - Main.screenPosition + new Vector2() * Main.rand.NextFloat(0f, 0f) * pulseOpacity + 2f * Main.rand.NextVector2Unit() * t,
                pulseSource,
                new Color(255, 255, 255, 255 * (pulseProgress > 0.7 ? 1f - pulseProgress : pulseProgress + 0.5f)) * pulseOpacity,
                pulseProgress * 2,
                pulseOrigin,
                pulseScale,
                SpriteEffects.None,
                0f
            );
        }
        return false;
    }
}

//if(attackTimer % 1 == 0) {
//    float side = ( (_portalCounter % 2 == 1) ? 1f : -1f ) * Main.rand.NextFloat(1.1f, 1.3f);
//    float angled = (Main.MouseWorld - player.Center).ToRotation() + (MathHelper.PiOver2 * side);
//    Vector2 offsetDirection = angled.ToRotationVector2();

//    var position = player.Center + mouseDirection * (_portalSpacing * _portalCounter) + offsetDirection * 16f * 6;
//    var direction = -offsetDirection;

//    Projectile.NewProjectile(
//        player.GetSource_FromThis(),
//        position,
//        direction,
//        ModContent.ProjectileType<FriendlyCultistPortal>(),
//        (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(60),
//        0.2f,
//        ai0: (float)PortalType.Spear,
//        ai1: 60
//    );
//}

// Old attack pattern concept