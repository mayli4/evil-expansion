using EvilExpansionMod.Content.Biomes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class PusImpNPC : ModNPC {
    public enum State {
        Idle,
        Spitting,
        Disintegrating,
        Reappearing
    }
    
    public State CurrentState => Unsafe.BitCast<float, State>(NPC.ai[0]);
    public ref float Timer => ref NPC.ai[1];
    
    void ChangeState(State state) {
        NPC.ai[0] = Unsafe.BitCast<State, float>(state);
        Timer = 0;
        NPC.netUpdate = true;
    }

    private const int spit_time = (int)(0.75 * 60);
    private const int spit_cooldown = 60 * 2;

    private const int melt_time = (int)(0.5 * 60);
    private const int unmelt_time = (int)(0.5 * 60);

    private const int teleport_range = 30 * 16;
    private const int teleport_cooldown = 60 * 4;

    private int _attackCooldownTimer;
    private int _teleportCooldownTimer;

    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.PusImp.KEY_PusImpNPC;

    public Player Target => Main.player[NPC.target];

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 21;
    }

    public override void SetDefaults() {
        NPC.width = 22;
        NPC.height = 22;
        NPC.lifeMax = 100;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.Ichor] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void AI() {
        NPC.TargetClosest();
        if (Target.dead || !Target.active || Vector2.Distance(NPC.Center, Target.Center) > 1500f) {
            NPC.velocity.X = 0;
            ChangeState(State.Idle);
            return;
        }

        if (_attackCooldownTimer > 0) _attackCooldownTimer--;
        if (_teleportCooldownTimer > 0) _teleportCooldownTimer--;

        switch (CurrentState) {
            case State.Idle:
                Idle();
                break;
            case State.Spitting:
                break;
            case State.Disintegrating:
                Melt();
                break;
            case State.Reappearing:
                Unmelt();
                break;
        }
    }

    private void Idle() {
        Timer++;
        if (Timer >= Main.rand.Next(120, 240)) {
            var canAttack = _attackCooldownTimer <= 0;
            var canTeleport = _teleportCooldownTimer <= 0;
            var lineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, Target.position, Target.width, Target.height);

            if (canAttack && lineOfSight && Main.rand.NextBool(2)) {
                ChangeState(State.Spitting);
                _attackCooldownTimer = spit_cooldown;
            } else if (canTeleport) {
                ChangeState(State.Disintegrating);
                _teleportCooldownTimer = teleport_cooldown;
            } else {
                Timer = 0;
            }
        }
    }

    private void Melt() {
        Timer++;
        NPC.velocity.X = 0;
        NPC.alpha = (int)MathHelper.Lerp(0, 255, Timer / melt_time); 

        if (Timer >= melt_time) {
            Teleport();
            ChangeState(State.Reappearing);
        }
    }

    private void Unmelt() {
        Timer++;
        NPC.velocity.X = 0;
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Timer / unmelt_time);

        if (Timer >= unmelt_time) {
            NPC.alpha = 0;
            ChangeState(State.Idle);
        }
    }

    private void Teleport() {
        int attempts = 0;
        var teleportPosition = NPC.Center;
        bool foundSpot = false;

        while (attempts < 50 && !foundSpot) {
            attempts++;
            float teleportX = Target.Center.X + Main.rand.NextFloat(-teleport_range / 2, teleport_range / 2);
            float teleportY = Target.position.Y;

            var tileX = (int)(teleportX / 16f);
            var tileY = (int)(teleportY / 16f);

            for (int i = 0; i < 20; i++) {
                if (WorldGen.InWorld(tileX, tileY + i) && Main.tile[tileX, tileY + i].HasTile && Main.tileSolid[Main.tile[tileX, tileY + i].TileType]) {
                    teleportPosition = new Vector2(tileX * 16f + NPC.width / 2, (tileY + i) * 16f - NPC.height);
                    
                    foundSpot = true;
                    break;
                }
            }
        }

        if (foundSpot) {
            NPC.Center = teleportPosition;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var tex = Assets.Assets.Textures.NPCs.Crimson.PusImp.PusImpNPC.Value;
        
        if(NPC.IsABestiaryIconDummy) {
            return true;
        }
        
        var flipped = NPC.direction != -1;
        var effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        Main.spriteBatch.Draw(tex, NPC.position - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, 1.0f, effects, 0f);
        
        return false;
    }

    public override void FindFrame(int frameHeight) {
        switch (CurrentState) {
            case State.Idle:
                NPC.frameCounter++;
                if (NPC.frameCounter >= 8) {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y / frameHeight >= 5) {
                        NPC.frame.Y = 0 * frameHeight;
                    }
                }
                break;

            case State.Spitting:
                NPC.frame.Y = (5 + (int)(Timer / (spit_time / 4f))) * frameHeight;
                if (NPC.frame.Y / frameHeight > 8) {
                    NPC.frame.Y = 8 * frameHeight;
                }
                break;

            case State.Disintegrating:
                NPC.frame.Y = (9 + (int)(Timer / (melt_time / 6f))) * frameHeight;
                if (NPC.frame.Y / frameHeight > 14) {
                    NPC.frame.Y = 14 * frameHeight;
                }
                break;

            case State.Reappearing:
                NPC.frame.Y = (15 + (int)(Timer / (unmelt_time / 6f))) * frameHeight;
                if (NPC.frame.Y / frameHeight > 20) {
                    NPC.frame.Y = 20 * frameHeight;
                }
                break;
        }
    }
}