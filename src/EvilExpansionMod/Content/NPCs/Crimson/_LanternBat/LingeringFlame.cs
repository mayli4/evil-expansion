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
    
    private List<Vector2> _flameTrailPoints; 
    private const int max_lifetime = 60 * 3;
    
    private int _initialDashDirection;

    public override void SetDefaults() {
        Projectile.width = 1;
        Projectile.height = 1;
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
        _flameTrailPoints = new List<Vector2>();
        NPC parentNPC = Main.npc[ParentNPCID];
        if (parentNPC.active && parentNPC.type == ModContent.NPCType<LanternBatNPC>()) {
            _flameTrailPoints.Add(parentNPC.Center + new Vector2(parentNPC.spriteDirection * 15, 10));
            _initialDashDirection = parentNPC.spriteDirection;
        }
    }

    public override void AI() {
        NPC parentNPC = Main.npc[ParentNPCID];

        if (parentNPC.active && parentNPC.type == ModContent.NPCType<LanternBatNPC>() && parentNPC.ModNPC is LanternBatNPC bat && bat.CurrentState == LanternBatNPC.State.Dashing) {
            Vector2 currentLanternPos = bat.NPC.Center;
            if (_flameTrailPoints.Count == 0 || Vector2.Distance(_flameTrailPoints.Last(), currentLanternPos) > 5) {
                _flameTrailPoints.Add(currentLanternPos);
                if (_flameTrailPoints.Count > 50) _flameTrailPoints.RemoveAt(0);
            }
            
            Projectile.timeLeft = max_lifetime; 
            Projectile.alpha = 0;
        }
        
        if (_flameTrailPoints.Count > 1) {
            int numDustsToSpawn = Main.rand.Next(1, 3);
            for (int i = 0; i < numDustsToSpawn; i++) {
                int segmentIndex = Main.rand.Next(_flameTrailPoints.Count - 1);
                Vector2 p1 = _flameTrailPoints[segmentIndex];
                Vector2 p2 = _flameTrailPoints[segmentIndex + 1];

                float lerpFactor = Main.rand.NextFloat();
                Vector2 dustPos = Vector2.Lerp(p1, p2, lerpFactor);

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
            }
        }
        
        Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3());
    }
    
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (_flameTrailPoints == null || _flameTrailPoints.Count < 2) return false;

        float collisionPoint = 0f;
        for (int i = 0; i < _flameTrailPoints.Count - 1; i++) {
            Vector2 p1 = _flameTrailPoints[i];
            Vector2 p2 = _flameTrailPoints[i + 1];

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p1, p2, 30, ref collisionPoint)) {
                return true;
            }
        }
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_flameTrailPoints.Count < 2) return false;
        
        float lifetimeProgress = 1f - (float)Projectile.timeLeft / max_lifetime;
        
        float thinningFactor = MathHelper.Lerp(1f, 0f, lifetimeProgress);
        
        Vector2[] trailToDraw = _flameTrailPoints.ToArray();
        bool isFlippedForShader = (_initialDashDirection == -1);
        
        float trailWorldLength = 0f;
        for (int i = 0; i < trailToDraw.Length - 1; i++) {
            trailWorldLength += Vector2.Distance(trailToDraw[i], trailToDraw[i + 1]);
        }
        
        float scaleX = trailWorldLength / 100f * 1;
        
        Graphics.BeginPipeline(0.5f, new SpriteBatchSnapshot() with { BlendState = BlendState.Additive })
            .DrawTrail(
                trailToDraw,
                t => MathF.Sin(t * MathF.PI) * 240 * thinningFactor,
                t => Color.Red, 
                Assets.Assets.Effects.Trail.LingeringFireTrail.Value,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 3.432f),
                ("baseColor", Color.Yellow.ToVector3()),
                ("mat", Graphics.WorldTransformMatrix),
                ("stepY", 0.2f),
                ("scale", 2f),
                ("texture1", Assets.Assets.Textures.Sample.Noise1.Value),
                ("texture2", Assets.Assets.Textures.Sample.Noise2.Value),
                ("flipped", isFlippedForShader ? 0 : 1),
                ("uvScaleX", scaleX),
                ("uvScaleY", 1.5f)
                )
            //.ApplyOutline(Color.White * 0.4f)
            .Flush();
        
        return false;
    }
}