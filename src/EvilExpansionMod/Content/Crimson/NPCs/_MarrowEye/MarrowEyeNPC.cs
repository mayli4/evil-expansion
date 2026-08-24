using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

enum State {
    Idle,
    Targeting,
    Waking,
}

public class MarrowEyeNPC : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.MarrowEye.MarrowEyeNPC.KEY;

    Player Target => Main.player[NPC.target];
    State State {
        get => (State)NPC.ai[0];
        set {
            NPC.ai[0] = (float)value;
            NPC.netUpdate = true;
        }
    }

    MarrowLazerProjectile? LazerProjectile {
        get => NPC.ai[1] == -1 ? null : Main.projectile[(int)NPC.ai[1]].ModProjectile as MarrowLazerProjectile;
        set => NPC.ai[1] = value?.Projectile.whoAmI ?? -1;
    }

    float _lookRotation;
    Vector2 _lookDirection;
    Vector2 _eyePosition;

    float _distanceToTarget;
    Vector2 _directionToTarget;

    int _ring;
    int _tendonA;
    int _tendonB;
    int _tendonC = -1;

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

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<MarrowEyeBannerItem>();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.2f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSlicesItem>(), 1, 2, 4));
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;

        for(var i = 0; i < 4; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"MarroweyeGore{i}").Type
        );
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
        _eyePosition = NPC.Center + origin + _lookDirection * 7f - origin.RotatedBy(NPC.rotation) - Vector2.UnitY * 4f;

        _distanceToTarget = NPC.Center.Distance(Target.Center);
        _directionToTarget = (Target.Center - NPC.Center) / _distanceToTarget;

        _lookDirection = _lookRotation.ToRotationVector2();

        switch(State) {
            case State.Idle:
                LazerProjectile = null;

                NPC.frameCounter = Math.Max(NPC.frameCounter - 0.1, 0);

                NPC.TargetClosest();

                if(IsTargetValid(400f)) State = State.Waking;
                break;
            case State.Waking:
                if(Target == null || !Target.active) {
                    State = State.Idle;
                    break;
                }

                _lookRotation = Utils.AngleLerp(_lookRotation, _directionToTarget.ToRotation(), 0.4f);
                NPC.frameCounter = Math.Min(NPC.frameCounter + 0.1, 2d);

                if(NPC.frameCounter == 2d) State = State.Targeting;
                break;
            case State.Targeting:
                if(Target == null || !Target.active) {
                    State = State.Idle;
                    break;
                }

                _lookRotation = Utils.AngleTowards(
                    _lookRotation,
                    _directionToTarget.ToRotation(),
                    0.01f);

                if(LazerProjectile is MarrowLazerProjectile lazerProjectile) {
                    lazerProjectile.Projectile.position = _eyePosition;
                    lazerProjectile.Projectile.velocity = _lookDirection;
                    lazerProjectile.Projectile.timeLeft = Math.Max(
                        MarrowLazerProjectile.DisapearFrames,
                        lazerProjectile.Projectile.timeLeft);
                }
                else if(Main.netMode != NetmodeID.MultiplayerClient) {
                    var projectile = Projectile.NewProjectileDirect(
                        NPC.GetSource_FromAI(),
                        _eyePosition,
                        _directionToTarget,
                        ModContent.ProjectileType<MarrowLazerProjectile>(),
                        NPC.damage,
                        0.1f
                    );

                    LazerProjectile = (projectile.ModProjectile as MarrowLazerProjectile)!;
                }

                if(!IsTargetValid(800f)) State = State.Idle;
                break;
        }

        Main.projectile[_ring].timeLeft = RingProjectile.DisapearFrames;
        Main.projectile[_tendonA].timeLeft = TendonProjectile.DisapearFrames;
        Main.projectile[_tendonB].timeLeft = TendonProjectile.DisapearFrames;
        if(_tendonC != -1) Main.projectile[_tendonC].timeLeft = TendonProjectile.DisapearFrames;
    }

    bool IsTargetValid(float distance) {
        return _distanceToTarget < distance && Collision.CanHit(_eyePosition, 1, 1, Target.Center, 1, 1);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        var whitesTexture = Assets.Images.Crimson.NPCs.MarrowEye.MarrowEyeWhites.Asset.Value;
        var irisTexture = Assets.Images.Crimson.NPCs.MarrowEye.MarrowEyeIris.Asset.Value;

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
