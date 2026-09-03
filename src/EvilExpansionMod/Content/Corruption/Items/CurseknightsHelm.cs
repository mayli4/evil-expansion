using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

internal sealed class CurseknightsHelm : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmItem.KEY;
    public static int HelmOn;
    public static int HelmOff;
    public static bool HelmExploded;
    
    public static float DifficultylessDebuff => Main.expertMode ? (Main.masterMode ? 0.4f : 0.5f) : 1f; // Expert and master mode multiply debuff time... need to counteract this

    public override void Load() {
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_Head.KEY, EquipType.Head, this, name: "OnCurseknightsHelm");
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_Head.KEY, EquipType.Head, this, name: "OffCurseknightsHelm");
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1);
        Item.defense = 10;
        HelmOn = EquipLoader.GetEquipSlot(Mod, "OnCurseknightsHelm", EquipType.Head);
        HelmOff = EquipLoader.GetEquipSlot(Mod, "OffCurseknightsHelm", EquipType.Head);
        HelmExploded = false;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<CurseknightsHelmPlayer>();
        modPlayer.IsWearingHelm = true; // this keeps setting the accessory being equipped... a little inefficient, oh well
        modPlayer.HideVisual = hideVisual;

        var healthThreshold = player.statLifeMax2 / 2;
        modPlayer.IsBelowThreshold = player.statLife < healthThreshold;

        if(!Main.mouseLeft) { // if HP >50%
            if (HelmExploded && modPlayer.ActiveReformParticles <= 0) {
                //SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = -0.2f }, player.position);

                Vector2 headPos = player.MountedCenter + new Vector2(0f, -player.height * 0.3f) + player.headPosition;

                if (!Main.dedServ) {
                    for (int i = 0; i <= 4; i++) {
                        float angle = (MathHelper.TwoPi / 5f) * i + Main.rand.NextFloat(-0.3f, 0.3f);
                        float spawnDistance = Main.rand.NextFloat(80f, 120f);
                        Vector2 spawnPos = headPos + angle.ToRotationVector2() * spawnDistance;

                        Vector2 initialVelocity = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.8f);

                        var goreParticle = ShardGoricle.RequestNew(player, i, spawnPos, initialVelocity);
                        ParticleEngine.GORE_LAYER.Add(goreParticle);

                        modPlayer.ActiveReformParticles++;
                    }
                }
            }
        }
        else {
            if(player.HasBuff(BuffID.CursedInferno)) {
                player.AddBuff(ModContent.BuffType<CursedWrath>(), int.MaxValue, false);
            }

            if (!HelmExploded) {
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, player.position);

                Vector2 behindDirection = new(-player.direction, 0f);

                for (int i = 0; i <= 4; i++) {
                    float spreadAngle = Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f);
                    Vector2 blastVelocity = (behindDirection.RotatedBy(spreadAngle) * Main.rand.NextFloat(5f, 9f)) - new Vector2(0f, Main.rand.NextFloat(2f, 5f));

                    Gore.NewGoreDirect(
                        Entity.GetSource_FromThis(),
                        player.MountedCenter + new Vector2(0f, -player.height * 0.3f),
                        blastVelocity,
                        Mod.Find<ModGore>("CurseknightsHelmGore" + i).Type);

                    Vector2 headWorldPos = player.MountedCenter + new Vector2(0f, -player.height * 0.3f) + player.headPosition;

                    var ember = GlowEmberParticle.NewParticle(
                        headWorldPos,
                        blastVelocity * Main.rand.NextFloat(0.8f, 1.3f),
                        Main.rand.NextFloat(0.25f, 1.5f),
                        new Color(230, 254, 6),
                        Color.White);

                    ember.Randomness *= 2f;
                    ember.LossPerSecond *= 2f;
                    ParticleEngine.PARTICLES.Add(ember);
                    
                    var flame = DustFlameParticle.RequestNew(
                        headWorldPos, 
                        blastVelocity * Main.rand.NextFloat(0.8f, 1.3f), 
                        new Color(230, 254, 6), 
                        Color.White, 
                        1.5f, 
                        Main.rand.Next(18, 28)
                    );

                    flame.LossPerFrame = 0.12f; 
                    flame.Swirly = Main.rand.NextBool(); 
                    flame.ApplyLighting = false;

                    ParticleEngine.GORE_LAYER.Add(flame);
                }
                
                var modifier = new PunchCameraModifier(
                    player.Center, 
                    Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2(), 
                    strength: 6f, 
                    6f, 
                    10, 
                    100f)
                {
                    UniqueIdentity = "CurseknightsHelmScreenshake",
                };
                Main.instance.CameraModifiers.Add(modifier);

                HelmExploded = true;
            }
        }
    }
    
    internal sealed class DrawLayer : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
            if (drawInfo.drawPlayer.dead)
                return false;

            var modPlayer = drawInfo.drawPlayer.GetModPlayer<CurseknightsHelmPlayer>();
            return modPlayer.IsWearingHelm && !modPlayer.HideVisual;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            var drawPlayer = drawInfo.drawPlayer;

            var tex = HelmExploded 
                ? Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_HeadGlow.Asset
                : Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_HeadGlow.Asset;

            Vector2 position = new Vector2(
                (int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)
            ) + drawPlayer.headPosition + drawInfo.headVect;

            Rectangle bodyFrame = drawPlayer.bodyFrame;

            var drawData = new DrawData(
                tex.Value,
                position,
                bodyFrame,
                Color.White * ((255f - drawInfo.drawPlayer.immuneAlpha) / 255f),
                drawPlayer.headRotation,
                drawInfo.headVect,
                1f,
                drawInfo.playerEffect
            ) {
                shader = drawInfo.cHead,
            };

            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}

public class CursedWrath : ModBuff {
    public override string Texture => Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmBuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.GetDamage(DamageClass.Generic) += 0.5f;
        if(!player.HasBuff(BuffID.CursedInferno)) {
            player.ClearBuff(ModContent.BuffType<CursedWrath>());
        }
    }
}

public class CurseknightsHelmPlayer : ModPlayer {
    public bool IsWearingHelm; // UpdateAccessory uses this to tell Modplayer if the helmet is in the accessory slot
    public bool HideVisual; // UpdateAccessory uses this to tell Modplayer if the accessory is hidden
    public bool IsBelowThreshold;
    public int ReformTimer;
    public int ActiveReformParticles;
    
    public override void PostUpdate() {
        if (ReformTimer > 0) {
            ReformTimer--;
            if (ReformTimer <= 0) {
                CurseknightsHelm.HelmExploded = false;
            }
        }
    }

    public override void ResetEffects() {
        IsWearingHelm = false;
    }

    public override void FrameEffects() {
        if (IsWearingHelm && !HideVisual) {
            Player.head = (CurseknightsHelm.HelmExploded || ActiveReformParticles > 0)
                ? CurseknightsHelm.HelmOff 
                : CurseknightsHelm.HelmOn;
        }
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { // Inflictng +8s Cursed Inferno when above HP threshold
        if(IsWearingHelm && !IsBelowThreshold) {
            int buffIndex = Player.FindBuffIndex(BuffID.CursedInferno);
            if(buffIndex != -1) {
                int timeLeftInTicks = Player.buffTime[buffIndex];
                Player.AddBuff(BuffID.CursedInferno, (int)((8 * 60 + timeLeftInTicks) * CurseknightsHelm.DifficultylessDebuff), false);
            }
            else {
                Player.AddBuff(BuffID.CursedInferno, (int)(8 * 60 * CurseknightsHelm.DifficultylessDebuff), false);
            }

            for(int i = 0; i < 5; i++) { //On-hit VFX goes here
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.CursedTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.5f, 2f));
            }
        }
    }
}

[PoolCapacity(30)]
public class ShardGoricle : BaseParticle<ShardGoricle> {
    public Player TargetPlayer;
    public Texture2D GoreTexture;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float RotationalVelocity;
    public float Scale;
    public float Alpha;
    public int LifeTime;

    public static ShardGoricle RequestNew(Player player, int goreIndex, Vector2 spawnPosition, Vector2 initialVelocity) {
        var particle = Pool.RequestParticle();
        particle.TargetPlayer = player;
        particle.Position = spawnPosition;
        particle.Velocity = initialVelocity;
        particle.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        particle.RotationalVelocity = Main.rand.NextFloat(-0.2f, 0.2f);
        particle.Scale = 1f;
        particle.Alpha = 0f;
        particle.LifeTime = 0;

        string path = $"EvilExpansionMod/Assets/Images/Gores/CurseknightsHelmGore{goreIndex}";
        particle.GoreTexture = ModContent.Request<Texture2D>(path).Value;

        return particle;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (!TargetPlayer.active) {
            ShouldBeRemovedFromRenderer = true;
            return;
        }

        LifeTime++;

        if (Alpha < 1f) {
            Alpha = MathHelper.Clamp(Alpha + 0.1f, 0f, 1f);
        }

        Vector2 targetPos = TargetPlayer.MountedCenter + new Vector2(0f, -TargetPlayer.height * 0.3f) + TargetPlayer.headPosition;
        Vector2 directionToHead = targetPos - Position;
        float distance = directionToHead.Length();

        if (distance < 16f || LifeTime >= 120) {
            SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.35f, Pitch = 0.2f }, TargetPlayer.MountedCenter);
            float angle = (MathHelper.TwoPi / 5f) + Main.rand.NextFloat(-0.3f, 0.3f);

            var flame = DustFlameParticle.RequestNew(
                targetPos,
                -angle.ToRotationVector2() * 1.5f,
                new Color(230, 254, 6),
                Color.White,
                1.2f,
                28
            );
            flame.LossPerFrame = 0.06f;
            flame.ApplyLighting = false;
            flame.Swirly = true;
            ParticleEngine.BEHIND_PROJECTILES.Add(flame);

            if (TargetPlayer.TryGetModPlayer<CurseknightsHelmPlayer>(out var modPlayer)) {
                modPlayer.ActiveReformParticles--;

                if (modPlayer.ActiveReformParticles <= 0) {
                    modPlayer.ActiveReformParticles = 0;
                    CurseknightsHelm.HelmExploded = false;
                }
            }
            ShouldBeRemovedFromRenderer = true;
            return;
        }

        directionToHead.Normalize();

        float playerSpeed = TargetPlayer.velocity.Length();
        float baseSpeed = MathHelper.Clamp(6f - (distance * 0.01f), 3f, 7f);
        float targetSpeed = Math.Max(baseSpeed, playerSpeed + 4f);
        
        float lerpFactor = MathHelper.Clamp(0.06f + (LifeTime * 0.003f), 0.06f, 0.3f);
        Velocity = Vector2.Lerp(Velocity, directionToHead * targetSpeed, lerpFactor);

        Position += Velocity;
        Rotation += RotationalVelocity;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        var drawPos = Position + settings.AnchorPosition;
        var origin = GoreTexture.Size() * 0.5f;
    
        var color = Lighting.GetColor(Position.ToTileCoordinates()) * Alpha;

        spriteBatch.Draw(GoreTexture, drawPos, null, color, Rotation, origin, Scale, SpriteEffects.None, 0f);
    }
}