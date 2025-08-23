using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class LanternBatNPC : ModNPC {
    public enum State {
        IdleFlight,
        Dashing,
        PostDashCooldown
    }
    
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.LanternBat.KEY_LanternBatNPC;
    public string LanternTexturePath => Assets.Assets.Textures.NPCs.Crimson.LanternBat.KEY_LanternBat_Lantern;

    public State CurrentState {
        get => (State)NPC.ai[0];
        set
        {
            NPC.ai[0] = (float)value;
            StateTimer = 0;
            NPC.netUpdate = true;
        }
    }
    public ref float StateTimer => ref NPC.ai[1];

    public Player Target => Main.player[NPC.target];

    private const int anim_speed = 6;
    private Vector2 _storedDashVelocity;
    
    private const int trail_length = 15;
    private Vector2[] _fireTrailPositions;
    
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 120;
        NPC.damage = 25;
        NPC.defense = 8;
        NPC.knockBackResist = 0.2f;
        NPC.value = 300f;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath4;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.buffImmune[BuffID.Bleeding] = true;
        NPC.lavaImmune = true;
    }
    
    public override void OnSpawn(IEntitySource source) {
        _fireTrailPositions = new Vector2[trail_length];
        for (int i = 0; i < trail_length; i++) _fireTrailPositions[i] = NPC.Center;
    }

    public override void AI() {
        NPC.TargetClosest();
        if (Target.dead || !Target.active) {
            return;
        }
        
        Vector2 lanternOffset = new Vector2(NPC.spriteDirection * 15, 40);
        Vector2 lanternWorldPosition = NPC.Center + lanternOffset;

        switch (CurrentState) {
            case State.IdleFlight:
                Vector2 idealIdlePosition = Target.Center + new Vector2(NPC.direction * 200, -100);
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(idealIdlePosition) * 4, 0.05f);

                StateTimer++;
                if (NPC.Distance(Target.Center) < 16 * 25 && StateTimer > Main.rand.Next(60 * 1, 60 * 3)) {
                    Vector2 dashTarget = Target.Center + Target.velocity * 0.5f;
                    _storedDashVelocity = NPC.DirectionTo(dashTarget) * 16;
                    
                    for (int i = 0; i < _fireTrailPositions.Length; i++) _fireTrailPositions[i] = NPC.Center;
                    
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<LingeringFlameProjectile>(),
                        NPC.damage,
                        0, Main.myPlayer,
                        NPC.whoAmI
                    );

                    CurrentState = State.Dashing;
                }
                break;

            case State.Dashing:
                NPC.velocity = _storedDashVelocity; 
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                for (int i = _fireTrailPositions.Length - 1; i > 0; i--)
                {
                    _fireTrailPositions[i] = _fireTrailPositions[i - 1];
                }
                _fireTrailPositions[0] = NPC.Center;

                StateTimer++;
                if (StateTimer >= 45)
                {
                    CurrentState = State.PostDashCooldown;
                    NPC.noTileCollide = true;
                    NPC.noGravity = true;
                    NPC.velocity *= 0.5f;
                }
                break;

            case State.PostDashCooldown:
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 4, 0.03f);
                
                StateTimer++;
                if (StateTimer >= 60 * 2) {
                    CurrentState = State.IdleFlight;
                }
                break;
        }

        NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0) ? 1 : -1;

        if (NPC.velocity.Length() < 0.1f && CurrentState != State.Dashing) {
            NPC.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;
        if (NPC.frameCounter >= anim_speed * 4) {
            NPC.frameCounter = 0;
        }
        NPC.frame.Y = (int)(NPC.frameCounter / anim_speed) * frameHeight;
    }
    
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 lanternOffsetVector = new Vector2(0, 40);
        
        if (NPC.spriteDirection == -1) {
            lanternOffsetVector.X *= -1;
        }

        Vector2 lanternDrawPosition = NPC.Center + lanternOffsetVector;

        float lanternRotation = NPC.velocity.X * 0.05f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

        Texture2D lanternTex = ModContent.Request<Texture2D>(LanternTexturePath).Value;
        Vector2 lanternOrigin = new Vector2(lanternTex.Width / 2, 0);
        
        SpriteEffects lanternEffects = SpriteEffects.None;
        if (NPC.spriteDirection == -1) {
            lanternEffects = SpriteEffects.FlipHorizontally;
        }
        
        Main.EntitySpriteDraw(
            lanternTex,
            lanternDrawPosition - screenPos,
            null,
            NPC.GetAlpha(drawColor),
            lanternRotation,
            lanternOrigin,
            NPC.scale,
            lanternEffects
        );

        Texture2D batTex = TextureAssets.Npc[NPC.type].Value;
        Vector2 batOrigin = NPC.frame.Size() / 2f;

        Main.EntitySpriteDraw(
            batTex,
            NPC.Center - screenPos,
            NPC.frame,
            NPC.GetAlpha(drawColor),
            NPC.rotation,
            batOrigin,
            NPC.scale,
            (NPC.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally
        );

        return false;
    }
}