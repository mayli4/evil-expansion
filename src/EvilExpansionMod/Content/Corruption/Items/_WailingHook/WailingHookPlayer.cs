using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static tModPorter.ProgressUpdate;
using Filters = Terraria.Graphics.Effects.Filters;
namespace EvilExpansionMod.Content.Corruption;


public class WailingHookPlayer : ModPlayer {
    public bool HasSoulsuckingFishingAccessory = false; // UpdateAccessory uses this to tell Modplayer if equipped
    public int nearbyEnemyCount = 0; // UpdateAccessory uses this to track the # of nearby enemies
    private float _spawnTimer = 0; // Internal timer for spawning ghosts

    public override void ResetEffects() {
        HasSoulsuckingFishingAccessory = false;
        nearbyEnemyCount = 0;
    }
    public bool IsActivelyFishingWithSoulsucker(Player player){ // Check if the player is actively fishing
        // Check if accessory is equipped
        if (!HasSoulsuckingFishingAccessory)
            return false;

        // Check if the currently held item is a fishing rod
        Item heldItem = player.HeldItem;
        if (heldItem == null || heldItem.fishingPole <= 0)
            return false;

        // Check if the player has an active bobber projectile spawned
        for (int i = 0; i < Main.maxProjectiles; i++){
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.bobber){
                return true; // Found
            }
        }
        return false;
    }
    public override void PostUpdate(){
        if (Player.whoAmI != Main.myPlayer)
            return;
        if (!IsActivelyFishingWithSoulsucker(Player)){
            _spawnTimer = 0; // Reset timer when not fishing
            return;
        }

        // Increment the timer, with the amount scaling with enemy count
        float enemyBonus = (float)Math.Log(1 + nearbyEnemyCount) * 50.0f;
        _spawnTimer += 1 + enemyBonus;
        if (_spawnTimer < 120)
            return;
        Main.NewText("enemyBonus" + enemyBonus, Microsoft.Xna.Framework.Color.White);
        _spawnTimer = 0; // Reset loop interval

        // Loop backwards through projectiles. Projectiles spawned last are at the end of the array
        for (int i = Main.maxProjectiles - 1; i >= 0; i--){
            Projectile proj = Main.projectile[i];

            // Combined check order: check activity/owner first (fast integers), then check bobber flag
            if (proj.active && proj.owner == Player.whoAmI && proj.bobber && proj.wet){
                // Spawn the projectile directly at the cached vector center
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(), 
                    proj.Right, 
                    Vector2.Zero,
                    ModContent.ProjectileType<WailingTelegraphProjectile>(),
                    0,
                    5f,
                    Main.myPlayer,
                    ai0: 45 / (float)(1 + Math.Pow(enemyBonus, 0.1)),
                    ai1: 16
                );

                break;
            }
        }
    }
}
internal sealed class WailingTelegraphProjectile : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Cursehound.LavaTelegraph.KEY;

    public ref float MaxTime => ref Projectile.ai[0];
    public ref float Damage => ref Projectile.ai[1];

    private bool hasSpawnedProjectile;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
    }

    public override void AI() {
        if(MaxTime == 0) {
            MaxTime = Projectile.timeLeft;
        }

        float progress = 1f - (Projectile.timeLeft / MaxTime);
        float lightAlpha = MathHelper.Clamp((float)Math.Sin(progress * MathHelper.Pi), 0f, 1f);
        Lighting.AddLight(Projectile.Center, new Color(214, 237, 5).ToVector3() * lightAlpha * 0.8f);

        float spawnThreshold = 0.7f;
        if(progress >= spawnThreshold && !hasSpawnedProjectile) {
            hasSpawnedProjectile = true;

            if(Main.myPlayer == Projectile.owner) {
                Vector2 upwardVelocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-12f, -3f));

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Bottom,
                    upwardVelocity,
                    ModContent.ProjectileType<WailingFireball>(),
                    (int)Damage,
                    Projectile.knockBack,
                    Main.myPlayer
                );
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        float progress = 1f - (Projectile.timeLeft / MaxTime);
        float maxTelegraphHeight = 75f;

        float growthProgress = MathHelper.Clamp(progress / 0.3f, 0f, 1f);
        float currentHeight = MathHelper.SmoothStep(0f, maxTelegraphHeight, growthProgress);

        float scaleX = progress < 0.3f ? MathHelper.Lerp(0.2f, 1f, progress / 0.3f) :
            MathHelper.Lerp(1f, 0.5f, (progress - 0.3f) / 0.7f);

        float alpha;
        if(progress < 0.2f) {
            alpha = progress / 0.2f;
        }
        else if(progress > 0.6f) {
            alpha = 1f - ((progress - 0.6f) / 0.4f);
        }
        else {
            alpha = 1f;
        }

        Vector2 drawPos = Projectile.Bottom - Main.screenPosition;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height);

        Color drawColor = new Color(214, 237, 5) * alpha * 0.85f;
        drawColor.A = 0;

        Vector2 scale = new Vector2(scaleX, currentHeight / texture.Height);

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            null,
            drawColor,
            0f,
            origin,
            scale,
            SpriteEffects.None
        );

        return false;
    }
}
public class WailingFireball : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;

    Vector2[] _trailPositions;
    public static readonly int MaxTimeLeft = 60 * 5;

    // Tracks enemies already pierced so the fireball moves on to the next target
    private readonly HashSet<int> _hitNPCs = [];

    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);

    public readonly static Color GhostColor1 = new(214, 237, 5);
    public readonly static Color GhostColor2 = new(181, 200, 4);

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.penetrate = -1;
        //Projectile.alpha = 255;

        Projectile.aiStyle = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20; // Hits at a maximum of 3 times every second
    }
    public override void OnSpawn(IEntitySource source) {
        SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.15f, Pitch = Main.rand.NextFloat(-0.7f, 0.2f) }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.5f, 0.5f) }, Projectile.Center);
    }
    public override bool CanHitPlayer(Player target) {
        return true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.CursedInferno, 60 * 10);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.CursedInferno, 60 * 10);
        _hitNPCs.Add(target.whoAmI); // Tracking other enemies
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        Player owner = Main.player[Projectile.owner];
        modifiers.HitDirectionOverride = target.Center.X >= owner.Center.X ? 1 : -1; // KB enemies away from user
    }
    public override void AI() {

        float maxDetectRadius = 16 * 80f;
        float projSpeed = 16f;

        if (Projectile.timeLeft < 100f) {
            Projectile.Opacity = (int)MathHelper.Lerp(1, 0, 100f / (Projectile.timeLeft + 100f));
        }

        NPC targetNPC = FindClosestNPC(maxDetectRadius);
        if(targetNPC != null) {
            Vector2 toTarget = targetNPC.Center - Projectile.Center;
            if(toTarget != Vector2.Zero) {
                toTarget.Normalize();
            }
            Vector2 desiredVelocity = toTarget * projSpeed;
            Projectile.velocity = (Projectile.velocity * Main.rand.NextFloat(15.5f, 15.7f) + desiredVelocity) / 16f; // Swerve to hit target
        }

        _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 12)]; // Visuals just straight ripped from SpiritFireball
        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = Projectile.Center + Projectile.velocity;

        if(Main.rand.NextBool(10)) {
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.CursedTorch,
                newColor: Main.rand.NextFromList(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2)
            );

            var dir = Projectile.velocity != Vector2.Zero
                ? -Vector2.Normalize(Projectile.velocity)
                : -Vector2.UnitY;

            float coneAngle = Main.rand.NextFloat(-0.3f, 0.3f);
            float backwardSpeed = Main.rand.NextFloat(0.5f, 2f);

            var spawnPosition = Projectile.Center - (Projectile.velocity * 0.5f)+ Main.rand.NextVector2Circular(4f, 4f);

            var flame = DustFlameParticle.RequestNew(
                spawnPosition,
                dir.RotatedBy(coneAngle) * backwardSpeed,
                GhostColor1,
                GhostColor1,
                Main.rand.NextFloat(0.8f, 1.4f),
                Main.rand.Next(18, 28)
            );

            flame.LossPerFrame = 0.06f;
            flame.Swirly = false;
            flame.ApplyLighting = false;

            ParticleEngine.PARTICLES.Add(flame);
        }
    }
    private NPC FindClosestNPC(float maxDetectDistance) {
        NPC closestNPC = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

        // Whip prio
        if(Projectile.OwnerMinionAttackTargetNPC != null) {
            NPC markedNPC = Projectile.OwnerMinionAttackTargetNPC;
            if(markedNPC.CanBeChasedBy() && !_hitNPCs.Contains(markedNPC.whoAmI)) {
                float sqrDist = Vector2.DistanceSquared(markedNPC.Center, Projectile.Center);
                if(sqrDist < sqrMaxDetectDistance) {
                    return markedNPC;
                }
            }
        }
        // Closest non-pierced enemy
        for(int k = 0; k < Main.maxNPCs; k++) {
            NPC target = Main.npc[k];
            if(target.CanBeChasedBy() && !_hitNPCs.Contains(target.whoAmI)) {
                float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                if(sqrDistanceToTarget < sqrMaxDetectDistance) {
                    sqrMaxDetectDistance = sqrDistanceToTarget;
                    closestNPC = target;
                }
            }
        }
        // If all enemies hit, circle back
        if(closestNPC == null && _hitNPCs.Count > 0) {
            _hitNPCs.Clear();
        }
        return closestNPC;
    }
    public override bool PreDraw(ref Color lightColor) {
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var blinker = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * Projectile.whoAmI) +
            MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var bigGlowColor = CursedSpiritNPC.GhostColor2 * (0.25f + 0.25f * blinker);
        var smallGlowColor = CursedSpiritNPC.GhostColor1;

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Projectile.velocity * 0.6f,
            null,
            bigGlowColor * Projectile.Opacity,
            0f,
            glowTexture.Size() * 0.5f,
            0.3f * Scale,
            SpriteEffects.None,
            0
        );
        Main.spriteBatch.EndBegin(snapshot);

        var trailEffect = Assets.Shaders.Pixel.CursedSpiritFire.Asset.Value;
        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetTexture(0, Assets.Images.Sample.Pebbles.Asset.Value)
            .SetTexture(1, Assets.Images.Sample.Noise3.Asset.Value)
            .SetEffectParams(
                trailEffect,
                ("uTime", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 34.432f),
                ("uStepY", 0.25f),
                ("uColor1", GhostColor1),
                ("uColor2", GhostColor2),
                ("uStepColor", 0.05f),
                ("uScale", 0.6f))
            .DrawTrail(
                _trailPositions,
                static _ => 8f,
                static t => Color.Lerp(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2, t + 0.7f),
                trailEffect
            )
            .DrawTexture(new()
            {
                Texture = Assets.Images.Corruption.Items.WailingHookGhost.Asset.Value, //Assets.Images.Corruption.Items.WailingHookItem
                Position = Projectile.Center,
                Color = smallGlowColor * Projectile.Opacity,
                Origin = 10f * Vector2.One,
                Scale = Vector2.One * 0.3f,
            })
            .ApplyOutline(CursedSpiritNPC.GhostColor1)
            .End();

        return false;
    }
}