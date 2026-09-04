using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
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
    int _chain0;
    int _chain1;
    int _chain2 = -1;

    public override void SetDefaults() {
        NPC.width = 50;
        NPC.height = 50;
        NPC.lifeMax = 333;
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
        var ringCenter = NPC.Center - new Vector2(2f, 51f);

        var randomRotation = MathF.PI / 2f + Main.rand.NextFloatDirection() * MathF.PI / 6f;

        var foundEnd0 = false;
        var direction0 = randomRotation.ToRotationVector2();

        var endPosition0 = ringCenter;

        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endPosition0, 1, 1)) {
                foundEnd0 = true;
                break;
            }

            endPosition0 += direction0 * 16f;
        }

        if(!foundEnd0) {
            NPC.active = false;
            return;
        }

        var foundEnd1 = false;
        var direction1 = (randomRotation + MathHelper.Pi).ToRotationVector2();

        var endPosition1 = ringCenter;

        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endPosition1, 1, 1)) {
                foundEnd1 = true;
                break;
            }

            endPosition1 += direction1 * 16f;
        }

        if(!foundEnd1) {
            NPC.active = false;
            return;
        }

        var chainProjectile0 = Projectile.NewProjectileDirect(
            NPC.GetSource_FromThis(),
            ringCenter,
            Vector2.Zero,
            ModContent.ProjectileType<MarrowEyeChainProjectile>(),
            0,
            0f);

        var delta0 = endPosition0 - ringCenter;
        var distance0 = delta0.Length();

        chainProjectile0.scale = distance0;
        chainProjectile0.rotation = (delta0 / distance0).ToRotation();

        _chain0 = chainProjectile0.whoAmI;

        var chainProjectile1 = Projectile.NewProjectileDirect(
            NPC.GetSource_FromThis(),
            ringCenter,
            Vector2.Zero,
            ModContent.ProjectileType<MarrowEyeChainProjectile>(),
            0,
            0f);

        var delta1 = endPosition1 - ringCenter;
        var distance1 = delta1.Length();

        chainProjectile1.scale = distance1;
        chainProjectile1.rotation = (delta1 / distance1).ToRotation();

        _chain1 = chainProjectile1.whoAmI;

        var direction2 = (randomRotation
            + (Main.rand.NextBool() ? 1 : -1)
            * Main.rand.NextFloat(MathF.PI / 4f, MathF.PI * 3f / 4f)).ToRotationVector2();

        var foundEnd2 = false;
        var endPosition2 = ringCenter;

        for(var i = 0; i < 50; i++) {
            if(Collision.SolidCollision(endPosition2, 1, 1)) {
                foundEnd2 = true;
                break;
            }

            endPosition2 += direction2 * 16f;
        }

        if(foundEnd2) {
            var chainProjectile2 = Projectile.NewProjectileDirect(
                NPC.GetSource_FromThis(),
                ringCenter,
                Vector2.Zero,
                ModContent.ProjectileType<MarrowEyeChainProjectile>(),
                0,
                0f);

            var delta2 = endPosition2 - ringCenter;
            var distance2 = delta2.Length();

            chainProjectile2.scale = distance2;
            chainProjectile2.rotation = (delta2 / distance2).ToRotation();

            _chain2 = chainProjectile2.whoAmI;
        }

        _ring = Projectile.NewProjectile(
            NPC.GetSource_FromThis(),
            ringCenter,
            Vector2.Zero,
            ModContent.ProjectileType<MarrowEyeRingProjectile>(),
            0,
            0f);
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
        if(NPC.frameCounter == 0) { // eye closed
            modifiers.FinalDamage *= 0.001f;
        }
    }

    public override void AI() {
        NPC.rotation = MathF.Sin(Main.GameUpdateCount * 0.01f + NPC.whoAmI * 574f) * 0.05f;

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

                if(IsTargetValid(400f)) {
                    State = State.Waking;
                    SoundEngine.PlaySound(Assets.Sounds.MarrowEye.MarrowEyeChargeup.Asset, NPC.Center);
                }
                break;
            case State.Waking:
                if(Target == null || !Target.active) {
                    State = State.Idle;
                    break;
                }

                _lookRotation = _lookRotation.AngleLerp(_directionToTarget.ToRotation(), 0.4f);
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
                        MarrowLazerProjectile.DisappearFrames,
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

        var disapearOffset = 240;
        Main.projectile[_ring].timeLeft = MarrowEyeRingProjectile.DisapearFrames + disapearOffset;
        Main.projectile[_chain0].timeLeft = MarrowEyeChainProjectile.DisapearFrames + disapearOffset;
        Main.projectile[_chain1].timeLeft = MarrowEyeChainProjectile.DisapearFrames + disapearOffset;
        if(_chain2 != -1) Main.projectile[_chain2].timeLeft = MarrowEyeChainProjectile.DisapearFrames + disapearOffset;
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
