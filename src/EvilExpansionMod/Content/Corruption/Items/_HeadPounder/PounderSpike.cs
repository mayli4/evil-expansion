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
    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.PounderSpike.KEY;

    int SpikeIndex => (int)Projectile.ai[0];

    int MaxTimeLeft = 120; //peak at 120, drop after
    int PopUpFrames = 5;
    int DropDownFrames = 8;

    float Scale {
        get {
            if(Projectile.timeLeft > MaxTimeLeft - PopUpFrames) {
                return (float)(MaxTimeLeft - Projectile.timeLeft) / PopUpFrames;
            }
            if(Projectile.timeLeft < DropDownFrames) {
                return (float)Projectile.timeLeft / DropDownFrames;
            }
            if(Projectile.timeLeft > MaxTimeLeft - PopUpFrames - 1) {
                return 1.2f;
            }
            return 1f;
        }
    }

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
    public override bool? CanCutTiles() {
        return true;
    }
    public override void CutTiles() {
        // 1. Colliding hook
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
        // 2. Start point and end point of the spike's damage line
        Vector2 startPos = Projectile.Center;
        Vector2 endPos = Projectile.Center + (Projectile.rotation - MathF.PI / 2f).ToRotationVector2() * spikeHeight;
        // 3. Set the context to an attacking projectile and plot the cut line across tiles
        DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;
        Utils.PlotTileLine(startPos, endPos, spikeWidth, DelegateMethods.CutTiles);
    }
    public override void OnSpawn(IEntitySource source) {
        SoundEngine.PlaySound(SoundID.Item62 with
        {
            Pitch = Main.rand.NextFloat(-0f, 1.7f),
            Volume = 0.7f
        }, Projectile.position);
        SoundEngine.PlaySound(SoundID.Item71 with
        {
            Pitch = Main.rand.NextFloat(-0.7f, 0f),
            Volume = 0.5f
        }, Projectile.position);

        int dustCount = 7; 
        float burstSpeed = 4f; 

        for (int i = 0; i < dustCount; i++) {
            float progress = (float)i / (dustCount - 1);
            float angle = MathHelper.Lerp(MathHelper.ToRadians(-90), MathHelper.ToRadians(-90), progress);
            Vector2 velocity = angle.ToRotationVector2() * burstSpeed;
            velocity.X += Main.rand.NextFloat(-0.0f, 0.0f);
            velocity.Y += Main.rand.NextFloat(-0.55f, 0.55f);
            
            Dust dust = Dust.NewDustDirect(Projectile.Center, 5, -10, DustID.Dirt, velocity.X, velocity.Y, default, default, Main.rand.NextFloat(0.8f, 1.2f));
        }

        //for(int i = 0; i < Main.rand.NextFloat(0f, 2f) ; i++) {
        //    var dustPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 2f)
        //        - Vector2.UnitY * Projectile.height * 0.6f;
        //    var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f)
        //        + Projectile.Center.DirectionTo(dustPos) * 5f;

        //    var dustColorStart = new Color(133, 122, 94);
        //    var dustColorFade = dustColorStart * 0.4f;

        //    var newDustData = new Smoke.Data()
        //    {
        //        InitialLifetime = (int)(45 * Main.rand.NextFloat(0.7f, 1.5f)),
        //        ElapsedFrames = 0,
        //        InitialOpacity = 0.2f * Main.rand.NextFloat(0.5f, 1f),
        //        ColorStart = dustColorStart,
        //        ColorFade = dustColorFade,
        //        Spin = 0.01f,
        //        InitialScale = Main.rand.NextFloat(0.5f, 1f)
        //    };

        //    var newDust = Dust.NewDustPerfect(
        //        dustPos,
        //        ModContent.DustType<Smoke>(),
        //        dustVelocity,
        //        0,
        //        newColor: Color.White,
        //        newDustData.InitialScale
        //    );

        //    newDust.customData = newDustData;
        //}
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
        behindNPCsAndTiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var cellWidth = 40;
        var source = new Vector4(SpikeIndex * cellWidth, 0, cellWidth, texture.Height);

        Graphics.Begin(scale: Scale)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = Projectile.Center - Main.screenPosition,
                Color = Lighting.GetColor(Projectile.position.ToTileCoordinates()) * MathF.Min(Projectile.timeLeft / 10f, 1f),
                Source = source,
                Rotation = Projectile.rotation,
                Origin = new Vector2(cellWidth / 2f, 90),
                Scale = Vector2.One * Scale,
                SpriteEffects = SpriteEffects.None,
            })
            .End();

        return false;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { // Unsure if this actually does anything
        // Check if the NPC is actually vulnerable to knockback mechanics
        if(!target.boss && target.knockBackResist > 0f) {
            // Force Terraria to run custom physics in the OnHitNPC step next
            modifiers.DisableKnockback(); // Centers horizontal knockback calculations
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { // KBs all non-boss, non-KB-immune enemies upwards and cuts their horizontal movement
        if(!target.boss && target.knockBackResist > 0f) {

            target.velocity.Y = -10f;
            target.position.Y += -6f;
            target.velocity.X *= 0f;

            // Keep the server updated in multiplayer contexts
            if(Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
            }
        }
    }
}
