using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
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
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerHeldProjectile : ModProjectile {

    int Pus = 0;
    Player Owner => Main.player[Projectile.owner];
    Vector2 _rotationVector;

    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.LamethrowerHeldSprite.KEY;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 15;
    }

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 6;
        Projectile.usesLocalNPCImmunity = true;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Oiled, 1200, false);
        target.AddBuff(BuffID.Ichor, 900, false);
        target.AddBuff(BuffID.OnFire3, 120, false);
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        _rotationVector = Projectile.Center.DirectionTo(Main.MouseWorld);
        Projectile.rotation = _rotationVector.ToRotation();
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
        else
        Projectile.Kill();
        return true;

    }

    public override void AI() {

        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        if(Pus != 200) {
        Projectile.frame = (Pus / 25);
        }
        if(Pus == 200) {
            Projectile.frameCounter++;
            if(Projectile.frameCounter >= 8) {
                Projectile.frameCounter = 0;

                Projectile.frame++;
                if(Projectile.frame >= 15) {
                    Projectile.frame = 9;
                }
            }
        }

        Owner.heldProj = Projectile.whoAmI;

        var mouseDirection = Projectile.Center.DirectionTo(Main.MouseWorld);
        _rotationVector = Vector2.Lerp(mouseDirection, _rotationVector, 0.2f);

        Projectile.rotation = _rotationVector.ToRotation();

        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full,
            Projectile.rotation - MathHelper.PiOver2
        );
        if(Pus != 200) {
            Pus++;
        };
    }

    public override void OnKill(int timeLeft) {
        for(int i = 0; i < (Pus / 20); i++) { 
            if(Main.myPlayer != Projectile.owner) return;

        Vector2 spawnPosition = Projectile.Center + _rotationVector;
            SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.8f } with { PitchRange = (0.4f, 1f) });
            Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        spawnPosition,
        Main.rand.NextFloat(5f, 6f) * _rotationVector * MathHelper.PiOver2,
        ModContent.ProjectileType<Glop>(),
        Projectile.damage,
        Projectile.knockBack,
        Projectile.owner
        );

        }
    }

    public override void PostDraw(SpriteBatch spriteBatch) { //i largely looked at ibans ark of the elements charge bar that appears in the inventory, to figure out how to do this, thanks iban tehee
        Texture2D chargeFront = ModContent.Request<Texture2D>(
                "EvilExpansionMod/Assets/Images/Crimson/Items/Lamethrower/LameThrowerChargebarFront").Value;
        Texture2D chargeBack = ModContent.Request<Texture2D>(
                "EvilExpansionMod/Assets/Images/Crimson/Items/Lamethrower/LameThrowerChargebarBack").Value;
        int BackBarWidth = 46;
        int BackBarHeight = 14;
        int FrontBarWidth = 46;
        int FrontBarHeight = 14;
        int cutoff = (int)(FrontBarWidth * Pus);
        Vector2 PivotPoint = new Vector2(BackBarWidth / 2f, BackBarHeight / 2f);
        Vector2 Position = new Vector2(0,32) + Owner.Bottom - Main.screenPosition;
        Vector2 frontPos = Position - PivotPoint;
        Rectangle frontCrop = new Rectangle(0, 0, (FrontBarWidth * Pus / 200), FrontBarHeight); 

        spriteBatch.Draw(chargeBack, Position, null, Color.White, 0f, PivotPoint, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(chargeFront, frontPos, frontCrop, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
    }
}


public class Glop : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.PusGlop.KEY;

    public static float Gravity = 0.2f;
    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = -1;
    }
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 12;
    }
    int GlopVariant = Main.rand.Next(1, 4);
    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;
        //GlopVariant = Main.rand.Next(1, 3);
        if(GlopVariant == 1) { Projectile.frame = 0; }
        if(GlopVariant == 2) { Projectile.frame = 4; }
        if(GlopVariant == 3) { Projectile.frame = 8; }
        //Sync the frame in multiplayer?
        if(Main.netMode == Terraria.ID.NetmodeID.Server) {
            NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
        }
    }

    public override void AI() {
        Projectile.velocity.Y += Gravity * 1.1f;
        if(Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }

        Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.02f, 0.02f));
        Projectile.rotation = Projectile.velocity.ToRotation();


        Projectile.frameCounter++;
        if(Projectile.frameCounter >= 8) {
            Projectile.frameCounter = 0;

            Projectile.frame++;

            if (GlopVariant == 1)
            if(Projectile.frame >= 3) {
                Projectile.frame = 0;
            }
            if(GlopVariant == 2)
                if(Projectile.frame >= 7) {
                    Projectile.frame = 4;
                }
            if(GlopVariant == 3)
                if(Projectile.frame >= 11) {
                    Projectile.frame = 8;
                }
        }

    }
    public override void OnKill(int timeLeft) {
        for(int i = 0; i < 4; i++) {
            int dustIndex = Dust.NewDust(
                    Projectile.Center,
                    default,
                    default,
                    ModContent.DustType<SmallCrytapeteTear>(),
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    0,
                    default,
                    Main.rand.NextFloat(1f, 2f)
            );
            Main.dust[dustIndex].noGravity = false;

            SoundEngine.PlaySound(SoundID.NPCHit9 with { Volume = 0.8f } with { PitchRange = (0.1f, 1f) });
        }

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<TearDeath>(),
            0,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
        );

        SoundEngine.PlaySound(SoundID.Drip with
        {
            Pitch = Main.rand.NextFloat(0f, 1.5f),
            Volume = Main.rand.NextFloat(0.1f, 0.2f)
        },
            Projectile.position);
        Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<LingeringIchorProjectile>(),
                Projectile.damage,
                0f);




    }
}
