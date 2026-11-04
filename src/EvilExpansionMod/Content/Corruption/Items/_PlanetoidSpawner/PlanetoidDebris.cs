using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Content.Projectiles;
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
namespace EvilExpansionMod.Content.Corruption.Items._PlanetoidSpawner;

public class PlanetoidDebris : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Planetoids.PlanetoidDebris.KEY;
    public override void SetDefaults() {
        Projectile.width = 58;
        Projectile.height = 58;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.knockBack = 1f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 1200;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.scale = 1;
        Projectile.alpha = 255; // Transparent at start
        Projectile.aiStyle = -1;

        Projectile.hide = true;

        Main.projFrames[Projectile.type] = 9;
    }
    // Projectile.ai[0] is Projectile.whoAmI of the PlanetoidProjectile that spawned this debris
    // Projectile.ai[1] = 0 for visual flair, 1 for damaging projectile
    // Projectile.ai[2] = 1 for stage 1, 2 for stage 2, 3 for stage 3, etc.
    bool IsOrbital = false;
    public override void OnSpawn(IEntitySource source) {
        //Main.NewText("I exist!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", Microsoft.Xna.Framework.Color.White);
        if(Projectile.ai[1] == 1) {
            Projectile.velocity.Y = -Main.rand.NextFloat(1f, 3f * (Projectile.ai[2]));
            Projectile.velocity.X = Main.rand.NextFloat(-1f, 1f) * (Projectile.ai[2]);
            Projectile.alpha = 0;
        }

        Projectile.frame = Main.rand.Next(0, (int)MathF.Min(8, 2 + (int)(Projectile.ai[2]) * 2));
        if(Main.rand.NextBool(3) && Projectile.frame < 7) Projectile.frame += 1;
        //if(Main.rand.NextBool(10) && Projectile.ai[1] == 0 && Projectile.frame < 6) IsOrbital = true;
        //if(Main.rand.NextBool(20) && Projectile.ai[1] == 0) IsOrbital = true;
        Projectile.rotation = Main.rand.NextFloat(0, 1);
        //Main.NewText("Projectile.frame" + Projectile.frame, Microsoft.Xna.Framework.Color.White);
    }
    private float Acceleration = Main.rand.NextFloat(0.15f, 1f);
    private const float MaxSpeed = 16f;
    private float KillDistance = 16f;
    bool PlanetoidDead = false;
    bool FadedIn = false;
    public override void AI() {
        Player player = Main.player[Projectile.owner];
        int parentIndex = (int)Projectile.ai[0];
        Projectile parent = Main.projectile[parentIndex];

        if (parent.active && parent.type == ModContent.ProjectileType<PlanetoidProjectile>() && (PlanetoidDead == false)) {
            Projectile.ai[1] = 0;
        }
        else if (PlanetoidDead == false) {
            Projectile.ai[1] = 1;
            Projectile.velocity.Y -= Main.rand.NextFloat(3f, 3f * (Projectile.frame));
            PlanetoidDead = true;
        }
        var directionToMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);

        if(Projectile.ai[1] == 0) { // As Visual

            if(Projectile.alpha > 0 && !FadedIn) Projectile.alpha -= Main.rand.Next(3 * (int)(4 - Projectile.ai[2]), 45); // Fade in
            else if(!FadedIn) {
                Projectile.alpha = 0;
                FadedIn = true;
            }

            Projectile.position += parent.velocity; // This makes the debris move relative to the planetoid projectile

            Vector2 toPlanetoid = (parent.Center - Projectile.Center);
            float distanceToPlanetoid = toPlanetoid.Length();

            KillDistance = 16f * ( 1 + Projectile.ai[2] * 0.8f );
            if (distanceToPlanetoid <= KillDistance && IsOrbital == false) { // Kill when within range unless orbital
                Projectile.Kill();
                return;
            }

            toPlanetoid.Normalize();
            Projectile.velocity += toPlanetoid * Acceleration; // Accelerate towards parent

            if(Projectile.velocity.Length() > MaxSpeed) {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed; // Max speed cap
            }

            if(IsOrbital) {
                Projectile.velocity += Main.rand.NextFloat().ToRotationVector2() * (Main.rand.NextFloat(-0.4f, 0.4f) * 2); // Velocity offset to make orbitals orbit elliptically
                if(Projectile.frame > 2) Projectile.friendly = true; // Larger orbitals deal damage whilst orbiting
            }
        }
        else { // As Projectile
            if (Projectile.frame > 4) Projectile.friendly = true;
            if(!FadedIn) {
                Projectile.alpha = 0;
                FadedIn = true;
            }
            Projectile.tileCollide = true;

            float gravity = Projectile.frame > 5? 0.6f : Projectile.frame > 2? 0.5f : 0.4f; // Gravity
            Projectile.velocity.Y += gravity;
        }

        Projectile.rotation += 0.1f * (1 + Projectile.alpha / 255); // Spin

        if(Projectile.timeLeft < 90) {
            Projectile.alpha += 3; // Fade out
            if(Projectile.alpha > 255) Projectile.alpha = 255;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) { // This does nothing at the moment
        if(Projectile.friendly == true) {
            Projectile.Kill();
        }
        return true;
    }

    public override void OnKill(int timeLeft) {
        if(Projectile.ai[1] == 0) {
            if(Projectile.frame < 6) {
                SoundEngine.PlaySound(SoundID.Dig with
                { Pitch = Main.rand.NextFloat(1f, 2f) - (Projectile.frame / 3), Volume = Main.rand.NextFloat(0.0f, 0.4f) + (Projectile.frame / 6) },
                Projectile.position
                );
            }
            else {
                int soundNum = Main.rand.Next(1, 4);
                SoundStyle caltropSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacstone_impact{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(-0.8f, 0.3f),
                    Volume = Main.rand.NextFloat(0.2f, 0.4f) * (1 + Projectile.frame / 8),
                };

                SoundEngine.PlaySound(caltropSound, Projectile.position);
            }
        }
        else {
            if(Projectile.frame < 6) {
                SoundEngine.PlaySound(SoundID.Dig with
                { Pitch = Main.rand.NextFloat(1f, 2f) - (Projectile.frame / 3), Volume = Main.rand.NextFloat(0.2f, 0.4f) + (Projectile.frame / 6) },
                Projectile.position
                );
            }
            else {
                int soundNum = Main.rand.Next(1, 4);
                SoundStyle caltropSound = new SoundStyle($"EvilExpansionMod/Assets/Sounds/Cursehound/Isaacstone_impact{soundNum}")
                {
                    Pitch = Main.rand.NextFloat(0f, 1f),
                    Volume = Main.rand.NextFloat(0.2f, 0.4f) * (1 + Projectile.frame / 8),
                };

                SoundEngine.PlaySound(caltropSound, Projectile.position);
            }

            for(int i = 0; i < Projectile.frame * 3; i++) {
                Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-5f, 0.5f));
                Vector2 Spawnpt = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-1f, 1f) * MathF.Max(1f, Projectile.frame), Main.rand.NextFloat(-20f, 0f) * MathF.Max(0.5f, Projectile.frame / 4));

                int dd = Dust.NewDust(Spawnpt, 5, -10, DustID.Dirt, default, default, default, default, Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dd].velocity = burstVelocity * MathF.Max(Main.rand.NextFloat(0.3f, 0.6f), Projectile.frame / 6);
                Main.dust[dd].scale = Main.rand.NextFloat(0f, 1.0f) * MathF.Max(1f, Projectile.frame / 6);
                Main.dust[dd].noGravity = false;

                int dc = Dust.NewDust(Spawnpt + new Vector2(0,16), 5, 10, DustID.Corruption, default, default, default, default, Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dc].velocity = burstVelocity * MathF.Max(Main.rand.NextFloat(0.2f, 0.5f), Projectile.frame / 12);
                Main.dust[dc].scale = Main.rand.NextFloat(0f, 1.0f) * MathF.Max(Main.rand.NextFloat(0.5f, 1f), Projectile.frame / 5);
                Main.dust[dc].noGravity = false;
            }
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.FlatBonusDamage += Projectile.frame * 5f;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        if((IsOrbital && Main.rand.NextBool(2)) || Main.rand.NextBool(5)) overPlayers.Add(index);
        else behindProjectiles.Add(index);
    }
    public override bool PreDraw(ref Color lightColor) { // Fuck vanilla rendering
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
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            sourceRectangle,
            Lighting.GetColor(Projectile.position.ToTileCoordinates()) * fadeOut,
            Projectile.rotation,
            drawOrigin,
            Projectile.scale,
            SpriteEffects.None
        );

        return false;
    }
}