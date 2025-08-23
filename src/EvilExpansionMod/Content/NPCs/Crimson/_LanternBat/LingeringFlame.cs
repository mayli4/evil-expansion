using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class LingeringFlameProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";
    
    public int ParentNPCID => (int)Projectile.ai[0];
    
    private const int max_lifetime = 60 * 3;

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 1;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = max_lifetime;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
    }

    public override void AI() {
        NPC parentNPC = Main.npc[ParentNPCID];

        if (parentNPC.active && parentNPC.type == ModContent.NPCType<LanternBatNPC>() && parentNPC.ModNPC is LanternBatNPC bat && bat.CurrentState == LanternBatNPC.State.Dashing) {
            Projectile.timeLeft = max_lifetime;
            Projectile.alpha = 0;
        }
        else {
            if(Projectile.timeLeft < 30) Projectile.alpha += 8;
            else if (Projectile.alpha > 0) Projectile.alpha -= 5;
            if (Projectile.alpha < 0) Projectile.alpha = 0;
            if (Projectile.alpha > 255) Projectile.alpha = 255;
        }
        
        Vector2 dustPos = Projectile.Center;

        Dust.NewDust(
            dustPos - Vector2.One * 4f, 8, 8,
            DustID.Torch,
            Main.rand.NextFloat(-1f, 1f),
            Main.rand.NextFloat(-1f, 1f) - 0.5f,
            100, default, Main.rand.NextFloat(0.8f, 1.2f)
        );
                
        var newDustData = new Smoke.Data() {
            InitialLifetime = 40,
            ElapsedFrames = 0,
            InitialOpacity = 0.5f,
            ColorStart = Color.Black,
            ColorFade = new Color(69, 69, 113),
            Spin = 0f,
            InitialScale = Main.rand.NextFloat(0.5f, 2f)
        };

        if(Main.rand.NextBool(10)) {
            var newDust = Dust.NewDustPerfect(
                dustPos,
                ModContent.DustType<Smoke>(),
                null,
                0,
                newColor: Color.White,
                newDustData.InitialScale
            );
                    
            newDust.customData = newDustData;
        }
        
        Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3());
    }
    
    // public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
    //     if (_flameTrailPoints == null || _flameTrailPoints.Count < 2) return false;
    //
    //     float collisionPoint = 0f;
    //     for (int i = 0; i < _flameTrailPoints.Count - 1; i++) {
    //         Vector2 p1 = _flameTrailPoints[i];
    //         Vector2 p2 = _flameTrailPoints[i + 1];
    //
    //         if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p1, p2, 30, ref collisionPoint)) {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}