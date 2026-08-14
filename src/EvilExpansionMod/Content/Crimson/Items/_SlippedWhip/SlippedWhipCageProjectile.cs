using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class SlippedWhipCageProjectile : ModProjectile {
    public override string Texture => Assets.Textures.Items.Crimson.SlippedWhip.SlippedWhipRibcageMain.KEY;

    public readonly static int MaxTimeLeft = 240;
    public readonly static int LockFrames = 15;

    NPC _target;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnSpawn(IEntitySource source) {
        _target = Main.npc[(int)Projectile.ai[0]];
        Projectile.rotation = Main.rand.NextFloatDirection() * 0.75f;
    }

    public override void AI() {
        if(_target == null || !_target.active) {
            Projectile.Kill();
            return;
        }

        if(Projectile.timeLeft > MaxTimeLeft - LockFrames) {
            Projectile.Center = _target.Center;
        }
        else if(Projectile.timeLeft == MaxTimeLeft - LockFrames) {
            _target.AddBuff(ModContent.BuffType<RibcagedNPCDebuff>(), MaxTimeLeft - LockFrames);

            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(3f, 0.35f));
            for(var i = 0; i < 10; i++) {
                var size = 80;
                Dust.NewDust(
                    Projectile.Center - Vector2.One * size / 2f,
                    size,
                    size,
                    DustID.Bone
                );
            }

            SoundEngine.PlaySound(SoundID.Research, Projectile.Center);
        }
        else {
            _target.Center = Projectile.Center;
            _target.velocity = Vector2.Zero;
        }
    }

    public override bool PreKill(int timeLeft) {
        if(timeLeft > MaxTimeLeft - LockFrames) return true;

        var gore0 = Gore.NewGoreDirect(
            Projectile.GetSource_Death(),
            Projectile.Center + new Vector2(20, 0),
            new Vector2(1, -1) * 2f,
            Mod.Find<ModGore>($"RibcageGore0").Type
        );
        gore0.position -= new Vector2(gore0.Width, gore0.Height) / 2f;

        var gore1 = Gore.NewGoreDirect(
            Projectile.GetSource_Death(),
            Projectile.Center - new Vector2(20, 0),
            new Vector2(-1, -1) * 2f,
            Mod.Find<ModGore>($"RibcageGore1").Type
        );
        gore1.position -= new Vector2(gore1.Width, gore1.Height) / 2f;

        for(var i = 0; i < 10; i++) {
            var size = 60;
            Dust.NewDust(
                Projectile.Center - Vector2.One * size / 2f,
                size,
                size,
                DustID.Bone
            );
        }

        SoundEngine.PlaySound(SoundID.NPCHit2, Projectile.Center);
        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        var progress = (float)(MaxTimeLeft - Projectile.timeLeft) / LockFrames;
        var lockProgress = MathF.Min(progress, 1f);
        var visualProgress = lockProgress * lockProgress * lockProgress;

        var fadeOut = 0.5f;
        var flashAlpha = Math.Clamp(progress - 1f / fadeOut - 1f, -1f / fadeOut - 1f, 0f) * fadeOut;
        flashAlpha *= flashAlpha;

        var flashColor = Color.Red * flashAlpha * (int)visualProgress;

        var mainTexture = TextureAssets.Projectile[Type].Value;
        var partTexture = Assets.Textures.Items.Crimson.SlippedWhip.SlippedWhipRibcagePart.Asset.Value;

        var tintEffect = Assets.Effects.Pixel.Tint.Asset.Value;
        var outlineEffect = Assets.Effects.Pixel.Outline.Asset.Value;

        var scale = Vector2.One * (1f + 4f * (1f - visualProgress) + flashAlpha * 0.3f);
        Renderer.BeginPipeline(1f)
            .DrawTexture(new()
            {
                Texture = mainTexture,
                Position = Projectile.Center - Main.screenPosition,
                Color = lightColor * visualProgress,
                Rotation = Projectile.rotation,
                Origin = new Vector2(17 - 30 * (1f - visualProgress), 29),
                Scale = scale,
                SpriteEffects = SpriteEffects.None,
            })
            .DrawTexture(new()
            {
                Texture = partTexture,
                Position = Projectile.Center - Main.screenPosition,
                Color = lightColor * visualProgress,
                Rotation = Projectile.rotation,
                Origin = new Vector2(28 + 30 * (1f - visualProgress), 19),
                Scale = scale,
                SpriteEffects = SpriteEffects.None,
            })
            .ApplyEffect(tintEffect, ("uColor", Color.Purple * flashAlpha * 0.2f))
            .ApplyEffect(outlineEffect, ("uColor", flashColor))
            .ApplyEffect(outlineEffect, ("uColor", flashColor))
            .End();

        return false;
    }
}

public class RibcagedNPCDebuff : ModBuff {
    public override string Texture => Helper.PlaceholderTextureKey;
    public override void SetStaticDefaults() {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}

public class RibcagedNPC : GlobalNPC {
    public override bool PreAI(NPC npc) => !npc.HasBuff<RibcagedNPCDebuff>();

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        if(!npc.HasBuff<RibcagedNPCDebuff>() && !(projectile.npcProj || projectile.trap || projectile.IsMinionOrSentryRelated)) return;

        var tagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
        modifiers.ScalingBonusDamage += SlippedWhipItem.CageMinionDamageMultiplier * tagMultiplier;
    }
}
