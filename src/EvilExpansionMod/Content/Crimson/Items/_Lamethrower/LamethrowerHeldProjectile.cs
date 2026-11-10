using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.GlobalTileHooks;
using Player = Terraria.Player;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerHeldProjectile : ModProjectile {
    public Player Owner => Main.player[Projectile.owner];
    private Vector2 rotationVector;
    private float attackSpeedMultiplier => (Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Magic)) + (Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Generic)) - 1;
    private int pus = 0;
    private int killTimer = 60;
    private int overchargeTimer = 0;
    private int lastTriggeredMultiple = 0; // For the sole purpose of calculating mana drain
    private int currentMultiple = 0;
    private int chargeSfx = 0;
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.LamethrowerHeldSprite.KEY;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 18;
    }

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 50;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 6;
        Projectile.usesLocalNPCImmunity = true;
    }
    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        rotationVector = Projectile.Center.DirectionTo(Main.MouseWorld);
        Projectile.rotation = rotationVector.ToRotation();
    }

    public override bool PreAI() {
        if(Owner.HeldItem.type != ModContent.ItemType<LamethrowerItem>()) {

        Projectile.Kill();
            return false;
        }

        if(Owner.channel) {
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);
            Owner.itemTime = Projectile.timeLeft;
            Owner.itemAnimation = Projectile.timeLeft;
        }
        else killTimer--;

        if (killTimer == 0) {
            Projectile.Kill();
        }      
        return true;
    }

    public override void AI() {
        //Main.NewText("pus" + pus, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("killTimer" + killTimer, Microsoft.Xna.Framework.Color.White);
        //Main.NewText("overchargeTimer" + overchargeTimer, Microsoft.Xna.Framework.Color.White);
        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        Player player = Main.player[Projectile.owner];
        player.ChangeDir(mouseDirection.X >= 0 ? 1 : -1);

        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);
        if(killTimer == 60) { 
            if(pus <= 140) {
                Projectile.frame = (pus / 11);
            }
            else if(overchargeTimer <= 40) {
                Projectile.frameCounter++;
                if(Projectile.frameCounter >= 8) {
                    Projectile.frameCounter = 0;

                    Projectile.frame++;
                    if(Projectile.frame >= 14) {
                        Projectile.frame = 8;
                    }
                }
            }
            if(overchargeTimer > 0) {
                Projectile.frame = 13 + ((overchargeTimer - 40) / 10);
            }
        }
        if(killTimer != 60 && Projectile.frame != 0) {
            Projectile.frame = ((pus / 11) - ((60 - killTimer) / 5));
        }
        Owner.heldProj = Projectile.whoAmI;

        rotationVector = Vector2.Lerp(mouseDirection, rotationVector, 0.2f);

        Projectile.rotation = rotationVector.ToRotation();

        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full,
            Projectile.rotation - MathHelper.PiOver2
        );

        //Main.NewText("attackSpeedMultiplier" + attackSpeedMultiplier, Microsoft.Xna.Framework.Color.White);
        if(pus != 140 && killTimer == 60) { // Pus increments to a maximum of 140, once per frame, scaling with attack speed
            for(int i = 0; i < 1 * attackSpeedMultiplier; i++) {
                pus++;
            }
            if(pus > 140) overchargeTimer = 140;
            currentMultiple = (pus / 10) * 10;
        }
        if(pus == 140 && overchargeTimer <= 80 && killTimer == 60) { // Overcharge increments to a maximum of 80 in the same manner
            if(overchargeTimer == 0) lastTriggeredMultiple = 0;
            for(int i = 0; i < 1 * attackSpeedMultiplier; i++) {
                overchargeTimer++;
            }
            if(overchargeTimer > 80) overchargeTimer = 80;
            currentMultiple = (overchargeTimer / 10) * 10;
        }

        Vector2 spawnPosition = Projectile.Center + rotationVector * new Vector2(80, 80);

        if(Projectile.owner == Main.myPlayer) { // Mana drain every 10 frames of charging ( 14 mana to max, 22 to overcharge)
            int manaCost = (int)(player.HeldItem.mana * player.manaCost);
            if(player.statMana >= manaCost && killTimer == 60 && currentMultiple > lastTriggeredMultiple) {
                player.statMana -= manaCost;
                //player.ManaEffect(-manaCost);
                lastTriggeredMultiple = currentMultiple;
            }
            else if(player.statMana < manaCost && killTimer == 60) { // No mana
                for(int i = 0; i < 2 * attackSpeedMultiplier; i++) {
                    if(pus > 0) pus -= 1;
                    if(overchargeTimer > 0) overchargeTimer -= 1;
                }
                SoundEngine.PlaySound(SoundID.LiquidsWaterLava with { Volume = 0.2f, Pitch = Main.rand.NextFloat(1f, 1.5f) });
                Dust dust = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.Smoke,
                    new Vector2 (0,-2) * Main.rand.NextFloat(0f, 1.5f),
                    Main.rand.Next(50, 255),
                    default,
                    Scale: Main.rand.NextFloat(0.8f, 1.0f)

                );
                Dust dustt = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.ManaRegeneration,
                    new Vector2(0, -2) * Main.rand.NextFloat(0f, 0.5f),
                    Main.rand.Next(0, 100),
                    default,
                    Scale: Main.rand.NextFloat(0.4f, 1f)

                );
            }
        }

        if (pus == 140 && chargeSfx == 0) { // Charged cue
            SoundEngine.PlaySound(SoundID.Item3 with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.7f, -0.4f) });
            for(int i = 0; i < 5; i++) {
                Vector2 dustVelocity = rotationVector.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.1f, 1f);
                Dust dust = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.Smoke,
                    player.velocity + dustVelocity * Main.rand.NextFloat(1f, 3f),
                    Main.rand.Next(0, 100),
                    default,
                    Scale: Main.rand.NextFloat(0.8f, 1.5f)
                );
            }
            chargeSfx = 1;
        }

        if(overchargeTimer == 80 && chargeSfx == 1) { // Overcharged cue
            SoundEngine.PlaySound(SoundID.NPCHit9 with { Volume = 1f, Pitch = Main.rand.NextFloat(1f, 1.5f) });
            for(int i = 0; i < 25; i++) {
                Vector2 dustVelocity = rotationVector.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.1f, 1f);
                Dust dust = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.Blood,
                    player.velocity + dustVelocity * Main.rand.NextFloat(1f, 3f),
                    Main.rand.Next(0, 100),
                    default,
                    Scale: Main.rand.NextFloat(0.8f, 1.5f)
                );
            }
            chargeSfx = 2;
        }

        if (killTimer == 59) { // Firing behavior
            SoundEngine.PlaySound(SoundID.NPCDeath21 with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.7f, -0.4f) - (pus / 140) });
            SoundEngine.PlaySound(SoundID.Item61 with { Volume = 0.5f + pus / 140, Pitch = Main.rand.NextFloat(-1f, -0.5f)});

            int dustCount = (int)(20 * (0.5 + pus / 140));
            for(int i = 0; i < dustCount; i++) {
                Vector2 dustVelocity = rotationVector.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.1f, 1f);
                Dust dust = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.Smoke,
                    Main.rand.NextFloat(8f, 12f) * dustVelocity * (float)(0.5 + pus / 140),
                    Main.rand.Next(0, 100),
                    default,
                    Scale: Main.rand.NextFloat(0.8f, 1.5f)
                );
                dust.noGravity = true;
                Dust dust2 = Dust.NewDustPerfect(
                    spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                    DustID.Crimson,
                    Main.rand.NextFloat(4f, 7f) * dustVelocity * (float)(0.5 + pus / 140),
                    Main.rand.Next(0, 100),
                    default,
                    Scale: Main.rand.NextFloat(0.8f, 1.5f)
                );
                dust2.noGravity = true;
            }

            if(overchargeTimer != 80) {
                for(int i = 0; i < ( 1 + pus / 10); i++) { // 1-15 shots depending on charge
                    if(Main.myPlayer != Projectile.owner) return;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        Main.rand.NextFloat(4f, 7f) * rotationVector * MathHelper.PiOver2 * (1 + (float)pus / 140), // Scales with charge
                        ModContent.ProjectileType<Glop>(),
                        Projectile.damage, // base damage
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }
            }

            if(overchargeTimer == 80) {
                Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier((2), 0.85f));

                SoundStyle Plop = new SoundStyle($"EvilExpansionMod/Assets/Sounds/LameThrower/TBoiPopSound");
                SoundEngine.PlaySound(Plop with { Volume = 0.5f, PitchRange = (-0.5f, 0f) });
                SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.3f, PitchRange = (-2f, -1.5f) });

                    Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    20f * rotationVector * MathHelper.PiOver2,
                    ModContent.ProjectileType<KidneyStone>(),
                    Projectile.damage * 24, // Pow! Yes, the stone does 480 damage at base, yes I think this is balanced, it takes ages for the player to fire an overcharged shot
                    10f,
                    Projectile.owner
                );

                for(int i = 0; i < 20; i++) { // 20 shots
                    if(Main.myPlayer != Projectile.owner) return;
                    SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.8f } with { PitchRange = (0.4f, 1f) });
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        Main.rand.NextFloat(5f, 16f) * rotationVector * MathHelper.PiOver2,
                        ModContent.ProjectileType<Glop>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    Vector2 dustVelocity = rotationVector.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.1f, 1f);
                    Dust dust = Dust.NewDustPerfect(
                        spawnPosition + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f)),
                        DustID.Blood,
                        player.velocity + dustVelocity * Main.rand.NextFloat(4f, 15f),
                        Main.rand.Next(0, 100),
                        default,
                        Scale: Main.rand.NextFloat(0.8f, 1.5f)
                    );
                }
            }

        }
    }
    public override void OnKill(int timeLeft) {
    }

    public override bool PreDraw(ref Color lightColor) { // Gun
        var texture = TextureAssets.Projectile[Type].Value;
        int frameHeight = texture.Height / Main.projFrames[Type];
        Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

        var origin = new Vector2(-8, 18);
        var recoilThreshold = 58;
        var recoil = (float)killTimer > recoilThreshold ?
            (Owner.direction == -1 ? (1 - Math.Max(killTimer, recoilThreshold) / 60f) : (-1 + Math.Max(killTimer, recoilThreshold) / 60f)) // Recoil
            :
            (Owner.direction == -1 ? float.Lerp(0.5f, 0f, (float)(recoilThreshold - killTimer) / recoilThreshold) : float.Lerp(-0.5f, 0f, (float)(recoilThreshold - killTimer) / recoilThreshold)); // Recovery
        Main.spriteBatch.Draw(
            texture,
            Projectile.position - Main.screenPosition,
            sourceRectangle,
            lightColor,
            Projectile.rotation - (Owner.direction == -1 ? MathF.PI : 0f) + recoil * (1 + pus / 140),
            Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
            Vector2.One * Projectile.scale,
            Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        return false;
    }

    public override void PostDraw(Color lightColor) { // Charge bar
        Texture2D chargeFront = Assets.Images.Crimson.Items.Lamethrower.LameThrowerChargebarFront.Asset.Value;
        Texture2D chargeBack = Assets.Images.Crimson.Items.Lamethrower.LameThrowerChargebarBack.Asset.Value;
        int backBarWidth = 46;
        int backBarHeight = 14;
        int frontBarWidth = 46;
        int frontBarHeight = 14;
        float shakey = Main.rand.NextFloat(-2.5f, 2.5f) * (overchargeTimer / 100f);
        float shakex = Main.rand.NextFloat(-1.5f, 1.5f) * (overchargeTimer / 100f);
        //int cutoff = frontBarWidth * pus; // "What does bro even do?"
        Vector2 pivotPoint = new(backBarWidth / 2f, backBarHeight / 2f);
        Vector2 position = new Vector2(0 + shakex, 32 + shakey) + Owner.Bottom  - Main.screenPosition;
        Vector2 frontPos = position - pivotPoint;
        Rectangle frontCrop = new(0, 0, (int) (frontBarWidth * Math.Min( (pus + 14f) / 140f, 1f) ), frontBarHeight); 

        Main.EntitySpriteDraw(chargeBack, position , null, Color.White * (int)(killTimer / (60f)), 0f, pivotPoint, 1f, SpriteEffects.None);
        Main.EntitySpriteDraw(chargeFront, frontPos, frontCrop, Color.Lerp(Color.White, Color.Red, overchargeTimer / 80f) * (int)(killTimer / 60f), 0, Vector2.Zero, 1, SpriteEffects.None);
    }
}