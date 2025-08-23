using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.NPCs.Crimson._MarrowEye;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

enum State {
    Idle,
    Targeting
}

public class MarrowEyeNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.MarrowEye.KEY_MarrowEyeNPC;

    Player Target => Main.player[NPC.target];
    State State {
        get => (State)NPC.ai[0];
        set {
            NPC.ai[0] = (float)value;
            NPC.netUpdate = true;
        }
    }
    Vector2 _lookDirection;
    int _shootTimer;

    int _ring;
    int _tendonA;
    int _tendonB;
    int _tendonC;

    public override void SetDefaults() {
        NPC.width = 50;
        NPC.height = 50;
        NPC.lifeMax = 100;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];

        NPC.buffImmune[BuffID.Ichor] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void OnSpawn(IEntitySource source) {
        var randomRotation = MathF.PI / 2f + Main.rand.NextFloatDirection() * MathF.PI / 6f;
        var randomDirection = randomRotation.ToRotationVector2();
        var ringCenter = NPC.Center - new Vector2(2f, 51f);
        var minLength = 42f;

        var startA = ringCenter - randomDirection * minLength / 2f;

        var foundEndA = false;
        var endA = ringCenter - randomDirection * minLength;
        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endA, 1, 1)) {
                endA += randomDirection * 11.33f;
                foundEndA = true;
                break;
            }

            endA -= randomDirection * 22.67f;
        }

        if(!foundEndA) {
            NPC.active = false;
            return;
        }

        var startB = ringCenter + randomDirection * minLength / 2f;

        var foundEndB = false;
        var endB = ringCenter + randomDirection * minLength;
        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endB, 1, 1)) {
                endB -= randomDirection * 11.33f;
                foundEndB = true;
                break;
            }

            endB += randomDirection * 22.67f;
        }

        if(!foundEndB) {
            NPC.active = false;
            return;
        }

        _tendonA = NewTendon(startA, endA);
        _tendonB = NewTendon(startB, endB);

        randomDirection = (
            randomRotation
            + (Main.rand.NextBool() ? 1 : -1)
            * Main.rand.NextFloat(MathF.PI / 4f, MathF.PI * 3f / 4f)
        ).ToRotationVector2();

        var startC = ringCenter + randomDirection * minLength / 2f;

        var foundEndC = false;
        var endC = startC + randomDirection * minLength;
        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endC, 1, 1)) {
                foundEndC = true;
                break;
            }

            endC += randomDirection * 22.67f;
        }

        if(foundEndC) _tendonC = NewTendon(startC, endC);

        _ring = Projectile.NewProjectile(
            NPC.GetSource_FromThis(),
            ringCenter,
            Vector2.Zero,
            ModContent.ProjectileType<RingProjectile>(),
            0,
            0f
        );
    }

    int NewTendon(Vector2 positionA, Vector2 positionB) {
        var tendon = Projectile.NewProjectileDirect(
            NPC.GetSource_FromThis(),
            (positionA + positionB) / 2f,
            Vector2.Zero,
            ModContent.ProjectileType<TendonProjectile>(),
            0,
            0f
        );

        var positionDelta = positionB - positionA;
        tendon.rotation = positionDelta.ToRotation();

        var tendonLength = positionDelta.Length();
        tendon.scale = tendonLength;
        tendon.netUpdate = true;

        return tendon.whoAmI;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
        if(NPC.frameCounter == 0) { // eye closed
            modifiers.FinalDamage *= 0.001f;
        }
    }

    public override void AI() {
        NPC.rotation = 0f;
        NPC.rotation = MathF.Sin(Main.GameUpdateCount * 0.03f + NPC.whoAmI * 574f) * 0.1f;

        var origin = new Vector2(-4f, -38f);
        var eyePosition = NPC.Center + origin + _lookDirection * 7f - origin.RotatedBy(NPC.rotation) - Vector2.UnitY * 4f;

        switch(State) {
            case State.Idle:
                _lookDirection *= 0.95f;
                NPC.frameCounter = Math.Max(NPC.frameCounter - 0.1, 0);

                NPC.TargetClosest();

                var targetingDistance = 400f;
                if(Target != null) {
                    if(
                        NPC.Center.DistanceSQ(Target.Center) < targetingDistance * targetingDistance
                        && Collision.CanHit(eyePosition, 1, 1, Target.Center, 1, 1)
                    ) {
                        State = State.Targeting;
                        NPC.netUpdate = true;
                    }
                }

                break;
            case State.Targeting:
                if(Target == null || !Target.active) {
                    State = State.Idle;
                    NPC.netUpdate = true;
                    break;
                }

                var directionToTarget = NPC.Center.DirectionTo(Target.Center);
                _lookDirection = Vector2.Lerp(_lookDirection, directionToTarget, 0.04f);
                NPC.frameCounter = Math.Min(NPC.frameCounter + 0.1, 2);

                _shootTimer -= 1;
                if(Main.netMode != NetmodeID.MultiplayerClient && NPC.frameCounter == 2 && _shootTimer <= 0) {
                    _shootTimer = Main.rand.Next(30, 90);

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        eyePosition,
                        directionToTarget * 14f,
                        ProjectileID.EyeLaser,
                        NPC.damage,
                        0.1f
                    );

                    for(var i = 0; i < 15; i++) {
                        Dust.NewDustPerfect(
                            eyePosition + Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f),
                            DustID.PurpleTorch
                        );
                    }
                }

                var idleDistance = 800;
                if(
                    NPC.Center.DistanceSQ(Target.Center) > idleDistance * idleDistance
                    || !Collision.CanHit(eyePosition, 1, 1, Target.Center, 1, 1)
                ) {
                    State = State.Idle;
                    NPC.netUpdate = true;
                }

                break;
        }

        if(Main.netMode != NetmodeID.MultiplayerClient) {
            Main.projectile[_ring].timeLeft = RingProjectile.DisapearFrames;
            Main.projectile[_tendonA].timeLeft = TendonProjectile.DisapearFrames;
            Main.projectile[_tendonB].timeLeft = TendonProjectile.DisapearFrames;
            Main.projectile[_tendonC].timeLeft = TendonProjectile.DisapearFrames;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        var whitesTexture = Assets.Assets.Textures.NPCs.Crimson.MarrowEye.MarrowEyeWhites.Value;
        var irisTexture = Assets.Assets.Textures.NPCs.Crimson.MarrowEye.MarrowEyeIris.Value;

        var position = NPC.Center + new Vector2(-4f, -38f);
        var origin = new Vector2(34, 8);

        spriteBatch.Draw(
            whitesTexture,
            position - screenPos,
            null,
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            irisTexture,
            position - screenPos + _lookDirection * 7f,
            null,
            drawColor,
            NPC.rotation,
            origin + new Vector2(-30, -35),
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            texture,
            position - screenPos,
            new(0, (int)NPC.frameCounter * 82, 72, 82),
            drawColor,
            NPC.rotation,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );
        return false;
    }
}
