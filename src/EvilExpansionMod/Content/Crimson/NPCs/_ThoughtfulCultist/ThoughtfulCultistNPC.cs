using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace EvilExpansionMod.Content.Crimson;

enum CultistState {
    FlyToTarget,
    SpearAttack,
    EyeAttack,
    BloodAttack,
}

public class ThoughtfulCultistNPC : ModNPC {
    public override string Texture => Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistBrain.KEY;
    Player Target => Main.player[NPC.target];
    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;
    int CurrentAttack = 1;
    CultistState State => Unsafe.BitCast<float, CultistState>(NPC.ai[0]);

    float _timer;
    float _robeOffset;
    float _portalRotation;

    void ChangeState(CultistState state) {
        NPC.ai[0] = Unsafe.BitCast<CultistState, float>(state);
        //Main.NewText("NPC.ai[0]" + NPC.ai[0], Microsoft.Xna.Framework.Color.White);
        if (NPC.ai[0] != 0) _timer = 0; // Timer starts from 0 every time ATTACK IS INITIATED (previously, when state was changed)
        NPC.netUpdate = true;
    }
    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            CustomTexturePath = Assets.Images.Crimson.NPCs.ThoughtfulCultist.ThoughtfulCultist_Bestiary.KEY, //Custom texture for the Bestiary
            Position = new Vector2(1f, 50f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 40f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        ContentSamples.NpcBestiaryRarityStars[Type] = 4;

        ItemID.Sets.KillsToBanner[BannerItem] = 25;
    }
    public override void SetDefaults() {
        NPC.width = 38;
        NPC.height = 38;
        NPC.lifeMax = 2430;
        NPC.defense = Main.expertMode ? (int)15 : (int)10;
        NPC.value = Item.buyPrice(gold: 1, silver: 50);
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;

        NPC.buffImmune[BuffID.Ichor] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<ThoughtfulCultistBannerItem>();

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];
    }
   
    public override float SpawnChance(NPCSpawnInfo spawnInfo) => Main.hardMode && spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>() ? 0.05f : 0;

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PusClumpItem>(), 1, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoneSlicesItem>(), 1, 3, 5));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThoughtfulPendantItem>(), 10, 1, 1));
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.ThoughtfulCultistNPCBestiary.KEY),
        });
    }
    public override void OnSpawn(IEntitySource source) {
        ChangeState(CultistState.FlyToTarget);
        CurrentAttack = Main.rand.Next(2, 4);
    }
    public override void AI() {
        NPC.TargetClosest();
        var directionToTarget = Vector2.Zero;
        var distanceToTarget = float.MaxValue;
        if(Target != null) {
            var targetDelta = Target.Center - Vector2.UnitY * 80f - NPC.Center;
            distanceToTarget = targetDelta.Length();
            directionToTarget = targetDelta / distanceToTarget;
        }

        switch(State) {
            case CultistState.FlyToTarget:
                if(distanceToTarget > 400) {
                    NPC.velocity += directionToTarget * 0.2f;
                    NPC.velocity *= 0.97f;
                }
                else if(Main.netMode != NetmodeID.MultiplayerClient && _timer > 240) { // Start attack when in range and timer > 240
                    if(CurrentAttack == 1) {
                        _portalRotation = Main.rand.NextFloat(0, -MathF.PI);
                        ChangeState(CultistState.SpearAttack);
                        CurrentAttack = 2;
                    }
                    else if(CurrentAttack == 2) {
                        _portalRotation = Main.rand.NextFloat(0, 2 * MathF.PI);
                        ChangeState(CultistState.EyeAttack);
                        CurrentAttack = 3;
                    }
                    else {
                        _portalRotation = Main.rand.NextFloat(0, 4 * MathF.PI);
                        ChangeState(CultistState.BloodAttack);
                        CurrentAttack = 1;
                    }

                    SoundEngine.PlaySound(Assets.Sounds.ThoughtfulCultist.CultistPendantActivate.Asset with
                    {
                        //Pitch = Main.rand.NextFloatDirection() * 0.5f,
                        PitchRange = (-0.2f, 0.2f),
                        Volume = 2f,
                    },
                    NPC.Center);

                    SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with
                    {
                        //Pitch = Main.rand.NextFloatDirection() * 0.5f,
                        PitchRange = (-1.2f, -0.2f),
                        Volume = 2f,
                    },
                    NPC.Center);
                }
                //ChangeState(CultistState.SpearAttack); // For testing purposes
                break;
            case CultistState.SpearAttack:
                NPC.velocity *= 0.98f;

                if(_timer > 60 / DifficultyScaler && (int)_timer % (Main.expertMode ? 6f : 12f) == 0) { // If timer is above 60 frames / difficultyscaler, every 12 frames, or 6 frames on expert+
                    Vector2 vel = Target.velocity;
                    var rotationOffset = (float)Math.Pow(DifficultyScaler, 1) * Main.rand.Next(0, 50);
                    var position = Target.Center - (105f + (float)(rotationOffset)) * _portalRotation.ToRotationVector2() + (vel);
                    var direction = position.DirectionTo(Target.Center);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        position,
                        direction,
                        ModContent.ProjectileType<CultistPortal>(),
                        35, // 70, 140, 210
                        0.2f,
                        ai0: (float)PortalType.Spear,
                        ai1: 112
                    );
                    _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                }

                if(Target == null || _timer > 150) { // Stop attack when timer is above 150
                    ChangeState(CultistState.FlyToTarget);
                    break;
                }

                //if(_timer > 60 / DifficultyScaler && (int)_timer % (30f) == 0) { // If timer is above 60 frames / difficultyscaler, every 30 frames
                //    Vector2 vel = Target.velocity;
                //    var rotationOffset = (float)Math.Pow(DifficultyScaler, 1) * Main.rand.Next(0, 50);
                //    for(var i = 1; i < DifficultyScaler + 1; i++) {
                //        var position = Target.Center - (105f + (float)(rotationOffset)) * _portalRotation.ToRotationVector2() + (vel);
                //        var direction = position.DirectionTo(Target.Center);
                //        Projectile.NewProjectile(
                //            NPC.GetSource_FromAI(),
                //            position,
                //            direction,
                //            ModContent.ProjectileType<CultistPortal>(),
                //            35, // 70, 140, 210
                //            0.2f,
                //            ai0: (float)PortalType.Spear,
                //            ai1: 112
                //        );
                //        _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                //    }
                //}

                var moveDirection = NPC.DirectionTo(Target.Center).RotatedBy(MathF.PI / 4f);
                NPC.velocity += moveDirection * 0.075f;

                break;
            case CultistState.BloodAttack:
                NPC.velocity *= 0.98f;

                if(_timer > 60 / DifficultyScaler && (int)_timer % 30 == 0) { // If timer is above 60 frames / difficultyscaler, every 30 frames
                    var position = Target.Center + _portalRotation.ToRotationVector2() * Main.rand.NextFloat(300, 400);
                    var direction = position.DirectionTo(Target.Center);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        position,
                        direction,
                        ModContent.ProjectileType<CultistPortal>(),
                        25,
                        0.2f,
                        ai0: (float)PortalType.Blood,
                        ai1: 336
                    );
                    _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                }

                if(_timer > 120) { // Stop attack when timer is above 120
                    ChangeState(CultistState.FlyToTarget);
                }
                break;
            case CultistState.EyeAttack:
                NPC.velocity *= 0.98f;

                if(_timer > 60 / DifficultyScaler && (int)_timer % 30 == 0) { // If timer is above 60 frames / difficultyscaler, every 30 frames
                    var position = Target.Center + _portalRotation.ToRotationVector2() * Main.rand.NextFloat(300, 400);
                    var direction = position.DirectionTo(Target.Center);
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        position,
                        direction,
                        ModContent.ProjectileType<CultistPortal>(),
                        25,
                        0.2f,
                        ai0: (float)PortalType.Eyes,
                        ai1: 224
                    );
                    _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                }

                if(_timer > 120) { // Stop attack when timer is above 120
                    ChangeState(CultistState.FlyToTarget);
                }
                break;
        }

        _timer += 1;

        var offsetMax = 12f;
        _robeOffset = Math.Clamp(_robeOffset + NPC.velocity.X * 0.1f, -offsetMax, offsetMax);
        _robeOffset *= 0.98f;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        for(var i = 1; i < 4; i++) Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center + Main.rand.NextVector2Unit() * 5f - Vector2.UnitY * 30f,
            Main.rand.NextVector2Unit(rotationRange: -MathF.PI) * 3f,
            Mod.Find<ModGore>($"CultistBrainGore{i}").Type
        );

        for(var i = 0; i < 5; i++) {
            Gore.NewGoreDirect(
                NPC.GetSource_Death(),
                NPC.Center + Main.rand.NextVector2Unit() * 40f + Vector2.UnitY * 30f,
                Vector2.Zero,
                Mod.Find<ModGore>($"CultistRobeGore{Main.rand.Next(1, 4)}").Type
            );
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var brainTexture = TextureAssets.Npc[Type].Value;

        var whiskerTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistWhisker.Asset.Value;
        var robeTextureBack = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistRobeBack.Asset.Value;
        var robeTextureFront = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistRobeFront.Asset.Value;
        var pendantTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPendant.Asset.Value;
        var pendantGlowmaskTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPendantGlowmask.Asset.Value;
        var chainTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistChain.Asset.Value;
        var pendantPulseTexture = Assets.Images.Crimson.NPCs.ThoughtfulCultist.CultistPulse.Asset.Value;

        if(NPC.IsABestiaryIconDummy)
            return true;

        Span<Vector2> robeTrailPositions = new Vector2[7];
        robeTrailPositions[0] = NPC.Center - Vector2.UnitY * 7f;

        for(var i = 1; i < robeTrailPositions.Length; i++) {
            robeTrailPositions[i] = robeTrailPositions[i - 1];
            robeTrailPositions[i].Y += 29;
            robeTrailPositions[i].X -=
                (float)i / robeTrailPositions.Length
                * _robeOffset
                + 1.25f * MathF.Sin(NPC.whoAmI * 23.2f + Main.GameUpdateCount * 0.03f);
        }

        var center = NPC.Center + Vector2.UnitY * 92;

        var offsetX = 20;
        var offsetY = 80;
        var bezierRight = center + new Vector2(offsetX, -offsetY);
        var bezierLeft = center + new Vector2(-offsetX, -offsetY);
        var bezierCenter = center - Vector2.UnitX * _robeOffset * 2f;

        var bezier = new BezierCurve(bezierLeft, bezierCenter, bezierRight);
        var chainPoints = bezier.GetPoints(13).ToArray();

        var pendantOutlineColor = Color.Transparent;
        var pendantPosition = chainPoints[chainPoints.Length / 2];

        var pulseFrameHeight = 408;
        var pulseSource = new Rectangle(0, pulseFrameHeight, pendantPulseTexture.Width, pulseFrameHeight);

        float pulseProgress = _timer / 60;
        float pulseScale = (float)( pulseProgress < 0.8 ? 1f + ( 0.8f - pulseProgress) / 10f : 1f + (pulseProgress-0.8) * 2f ); //(float)( pulseProgress < 0.8 ? 1f + ( 0.8f - pulseProgress) / 10f : 1f + (pulseProgress-0.8) * 2f )
        float pulseOpacity = (float) ( pulseProgress < 0.7? (0.3f + pulseProgress): (1f - ((pulseProgress - 0.7f) / 0.3f)) );
        Vector2 pulseOrigin = pulseSource.Size() / 2f;

        if(_timer < 60) {
            var t = MathF.Sin(MathHelper.Pi * _timer / 60f);

            pendantPosition += 5f * Main.rand.NextVector2Unit() * t;

            switch(State) {
                case CultistState.SpearAttack:
                    pendantOutlineColor = Color.Lerp(
                        pendantOutlineColor,
                        Color.Orange,
                        t);
                    pulseSource.Y = pulseFrameHeight * 0;
                    break;
                case CultistState.EyeAttack:
                    pendantOutlineColor = Color.Lerp(
                        pendantOutlineColor,
                        new Color(234, 90, 255),
                        t);
                    pulseSource.Y = pulseFrameHeight * 1;
                    break;
                case CultistState.BloodAttack:
                    pendantOutlineColor = Color.Lerp(
                        pendantOutlineColor,
                        Color.Red,
                        t);
                    pulseSource.Y = pulseFrameHeight * 2;
                    break;
            }
            //Main.NewText("pulseSource" + pulseSource, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("pulseSource.Y" + pulseSource.Y, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("pulseProgress" + pulseProgress, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("pulseScale" + pulseScale, Microsoft.Xna.Framework.Color.White);
            //Main.NewText("pulseOpacity" + pulseOpacity, Microsoft.Xna.Framework.Color.White);
        }

        Graphics.Begin(Graphics.WorldTransformMatrix)
            .SetSamplerState(0, SamplerState.PointWrap)
            .SetTexture(robeTextureBack)
            .DrawTrail(robeTrailPositions, static _ => 88, _ => drawColor, spriteRotation: 1)
            .SetTexture(chainTexture)
            .DrawTrail(chainPoints, static _ => 6, _ => drawColor)
            .End();

        Graphics.Begin(Graphics.WorldTransformMatrix)
            .DrawTexture(new()
            {
                Texture = pendantTexture,
                Position = pendantPosition,
                Color = drawColor,
                Rotation = 0f,
                Origin = pendantTexture.Size() / 2f,
            })
            .DrawTexture(new()
            {
                Texture = pendantGlowmaskTexture,
                Position = pendantPosition,
                Color = pendantOutlineColor,
                Rotation = 0f,
                Origin = pendantGlowmaskTexture.Size() / 2f,
            })
            .ApplyOutline(pendantOutlineColor)
            .End();

        Graphics.Begin(Graphics.WorldTransformMatrix)
            .SetTexture(robeTextureFront, SamplerState.PointWrap)
            .DrawTrail(robeTrailPositions, static _ => 88, _ => drawColor, spriteRotation: 1)
            .End();

        var brainFrameHeight = 78;
        var brainSource = new Rectangle(0, brainFrameHeight, brainTexture.Width, brainFrameHeight);
        if(MathF.Abs(NPC.velocity.X) > 1.5f) {
            brainSource.Y = NPC.velocity.X > 0 ? brainFrameHeight * 2 : 0;
        }

        spriteBatch.Draw(
            whiskerTexture,
            NPC.Center + new Vector2(4f, 6f) - screenPos,
            null,
            drawColor,
            -0.1f - 0.1f * MathF.Sin(2.34f + Main.GameUpdateCount * 0.025f + NPC.whoAmI),
            new Vector2(3f, 0f),
            1f,
            SpriteEffects.None,
            0f);

        spriteBatch.Draw(
            whiskerTexture,
            NPC.Center + new Vector2(-4f, 6f) - screenPos,
            null,
            drawColor,
            0.1f + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.02f + NPC.whoAmI),
            new Vector2(13f, 0f),
            1f,
            SpriteEffects.FlipHorizontally,
            0f);

        spriteBatch.Draw(brainTexture, NPC.Center - screenPos, brainSource, drawColor, 0f, new Vector2(53, 55), 1f, SpriteEffects.None, 0f);

        if(_timer < 60) {
           spriteBatch.Draw(
               pendantPulseTexture,
               chainPoints[chainPoints.Length / 2] - screenPos + new Vector2() * Main.rand.NextFloat(-50f,50f) * pulseOpacity,
               pulseSource,
               new Color(255, 255, 255, 255 * (pulseProgress > 0.7? 1f - pulseProgress : pulseProgress + 0.3f) ) * pulseOpacity,
               pulseProgress,
               pulseOrigin,
               pulseScale,
               SpriteEffects.None,
               0f
           );
        }
        return false;
    }
}