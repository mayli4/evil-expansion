using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

//todo: pus shader

public class PusBottleNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_PusBottle;

    private int ParentNPCID => (int)NPC.ai[0];
    private ref float IsDetached => ref NPC.ai[1];
    
    public ref float SquishTimer => ref NPC.localAI[0];

    private const int spew_interval = 60 * 2;
    private const int detached_lifetime = 60 * 10;
    
    private float MaxSquishTime = 30f;

    public override void SetDefaults() {
        NPC.width = 16;
        NPC.height = 16;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.damage = 0;
        NPC.lifeMax = 1100;
        NPC.knockBackResist = 0f;
        NPC.value = 0f;

        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit3;
        NPC.DeathSound = SoundID.Shatter;
    }

    public override void AI() {
        NPC parentNPC = Main.npc[ParentNPCID];

        if (SquishTimer > 0) 
            SquishTimer--;
        
        if (IsDetached == 0) {
            if (parentNPC.active && parentNPC.type == ModContent.NPCType<StinkgrubNPC>())
            {
                StinkgrubNPC grub = (StinkgrubNPC)parentNPC.ModNPC;
                if (grub.IsPusCarrier) {
                    NPC.Center = parentNPC.Center + new Vector2(parentNPC.spriteDirection, -parentNPC.height / 2 * parentNPC.scale);
                    NPC.velocity = parentNPC.velocity;
                    NPC.gfxOffY = parentNPC.gfxOffY;
                    
                    NPC.direction = parentNPC.direction;
                    NPC.spriteDirection = parentNPC.spriteDirection;

                    float tiltAngle = 0.2f;
                    NPC.rotation = (NPC.direction == 1) ? -tiltAngle : tiltAngle;
                    
                    NPC.ai[2]++;
                    if (NPC.ai[2] >= spew_interval) {
                        FirePus();
                        NPC.ai[2] = 0;
                    }
                    NPC.timeLeft = 2;
                }
                else {
                    IsDetached = 1;
                    NPC.timeLeft = detached_lifetime;
                    NPC.netUpdate = true;
                }
            }
            else {
                IsDetached = 1;
                NPC.timeLeft = detached_lifetime;
                NPC.netUpdate = true;
            }
        }
        else {
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.velocity.Y += 0.2f;
            if (NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;

            NPC.velocity.X *= 0.98f;
            
            NPC.rotation += 0.1f;
            
            if (NPC.collideX || NPC.collideY) {
                NPC.StrikeInstantKill();
            }
        }
    }

    private void FirePus() {
        var amount = Main.rand.Next(2, 4);
        
        SquishTimer = MaxSquishTime;

        for(int i = 0; i < amount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed; 
            
            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(0, 130),
                velocity,
                ModContent.ProjectileType<PusGlob>(),
                (int)(ParentNPCID != -1 && Main.npc[ParentNPCID].active ? Main.npc[ParentNPCID].damage * 0.75f : 10),
                0.5f,
                Main.myPlayer
            );
        }
        
        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = Main.rand.NextFloat(0.5f, 0.8f) }, NPC.Center);
    }

    public override void OnKill() {
        if (Main.netMode == NetmodeID.Server) return;

        int pusImpCount = Main.rand.Next(2, 4);
        for (int i = 0; i < pusImpCount; i++) {
            NPC.NewNPC(
                NPC.GetSource_FromThis(),
                (int)NPC.Center.X + Main.rand.Next(-10, 10),
                (int)NPC.Center.Y + Main.rand.Next(-10, 10),
                ModContent.NPCType<PusImpNPC>()
            );
        }

        for (int i = 0; i < 15; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Glass, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 0, default, 1.2f);
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ichor, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 0, default, 0.8f);
        }
        SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
    }
    
    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        for(var i = 0; i < 3; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"BottleGore{i}").Type
        );
    }
    
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.PusBottle.Value;

        var origin = texture.Size() / 2f;
        
        float squishFactor = Math.Clamp(SquishTimer / 40, 0f, 1f);
        float easedSquish = MathF.Sin(squishFactor * MathHelper.Pi);
        float currentSquishX = 1f + easedSquish * 0.2f;
        float currentSquishY = 1f - easedSquish * 0.2f;

        var finalScale = new Vector2(currentSquishX, currentSquishY);
        
        var flipped = NPC.direction != -1;
        var effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        const float maxVerticalOffset = 12f;
        float verticalSquishOffset = easedSquish * maxVerticalOffset;
        
        var offset = flipped ? new Vector2(0, 60) : new Vector2(-14, 60);
        
        spriteBatch.Draw(
            texture,
            NPC.position - screenPos - offset + new Vector2(0, verticalSquishOffset),
            null,
            drawColor,
            NPC.rotation,
            origin,
            finalScale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}