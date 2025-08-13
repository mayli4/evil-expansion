using EvilExpansionMod.Content.Biomes;
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

    int LaserProjectile = -1;

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
        var randomDirection = (MathF.PI / 2f + Main.rand.NextFloatDirection() * MathF.PI / 8f).ToRotationVector2();

        var minLength = 50f;
        var hookPoint = NPC.Center - Vector2.UnitY * 42f;
        var positionA = hookPoint - randomDirection * minLength / 2f;
        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(positionA, 1, 1)) {
                break;
            }

            positionA -= randomDirection * 22.67f;
        }

        var positionB = hookPoint + randomDirection * minLength / 2f;
        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(positionB, 1, 1)) {
                break;
            }

            positionB += randomDirection * 22.67f;
        }

        if((positionB - positionA).LengthSquared() > 200) {
            var positionA2 = positionA + (positionB - positionA) * Main.rand.NextFloat();
            randomDirection = (
                randomDirection.ToRotation()
                + (Main.rand.NextBool() ? 1 : -1)
                * Main.rand.NextFloat(MathF.PI / 8f, MathF.PI / 4f)
            ).ToRotationVector2();

            var positionB2 = positionA2 + randomDirection * minLength;
            for(var i = 0; i < 50; i++) {
                if(Collision.SolidCollision(positionB2, 1, 1)) {
                    break;
                }

                positionB2 += randomDirection * 22.67f;
            }

            NewTendon(positionA2, positionB2);
        }

        NewTendon(positionA, positionB);
    }

    void NewTendon(Vector2 positionA, Vector2 positionB) {
        var tendon = Projectile.NewProjectileDirect(
            NPC.GetSource_FromThis(),
            (positionA + positionB) / 2f,
            Vector2.Zero,
            ModContent.ProjectileType<TendonProjectile>(),
            0,
            0f
        );

        var positionDelta = positionB - positionA;
        tendon.rotation = positionDelta.ToRotation() - MathF.PI / 2f;

        var tendonLength = positionDelta.Length();
        tendon.scale = tendonLength;
        (tendon.ModProjectile as TendonProjectile).AttachedEye = NPC.whoAmI;

        tendon.netUpdate = true;
    }

    public override void AI() {
        NPC.rotation = 0f;
        NPC.rotation = MathF.Sin(Main.GameUpdateCount * 0.03f + NPC.whoAmI * 574f) * 0.1f;

        var minDist = 400f;
        switch(State) {
            case State.Idle:
                _lookDirection *= 0.95f;
                NPC.frameCounter = Math.Max(NPC.frameCounter - 0.1, 0);

                NPC.TargetClosest();
                if(Target != null) {
                    if(NPC.Center.DistanceSQ(Target.Center) < minDist * minDist) {
                        State = State.Targeting;
                    }
                }

                break;
            case State.Targeting:
                if(Target == null || !Target.active) {
                    State = State.Idle;
                    break;
                }


                var directionToTarget = NPC.Center.DirectionTo(Target.Center);
                _lookDirection = Vector2.Lerp(_lookDirection, directionToTarget, 0.04f);
                NPC.frameCounter = Math.Min(NPC.frameCounter + 0.1, 2);

                if(NPC.frameCounter == 2) {
                    if(LaserProjectile == -1) {
                        LaserProjectile = Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            Vector2.Zero,
                            directionToTarget,
                            ModContent.ProjectileType<MarrowLazerProjectile>(),
                            NPC.damage,
                            0.1f
                        );
                    }

                    var laser = Main.projectile[LaserProjectile];

                    var offset = new Vector2(-4f, -38f);
                    laser.position = NPC.Center + offset - offset.RotatedBy(NPC.rotation) + _lookDirection * 4f;
                    laser.velocity = _lookDirection;
                    laser.timeLeft = Math.Max(MarrowLazerProjectile.DisapearFrames, laser.timeLeft);
                }

                if(NPC.Center.DistanceSQ(Target.Center) > minDist * minDist) {
                    LaserProjectile = -1;
                    State = State.Idle;
                }
                break;
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
