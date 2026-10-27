using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using EvilExpansionMod.Content.Misc;

namespace EvilExpansionMod.Content.Crimson;

public class CutProjectile : ModProjectile, IPreDrawEverythingBulk<CutProjectile> {
    public override string Texture => Helper.PlaceholderTextureKey;

    public List<Vector2> TrailPositions = [];

    static int MaxTimeLeft = 120;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.DamageType = DamageClass.Melee;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 4;
        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = 8;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        for(var i = 0; i < TrailPositions.Count - 1; i++) {
            TrailPositions[i] += Vector2.UnitY * 0.25f * i / TrailPositions.Count;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        for(var i = 0; i < TrailPositions.Count - 1; i++) {
            var _ = 0f;
            if(Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                TrailPositions[i],
                TrailPositions[i + 1],
                30f,
                ref _
            )) {
                return true;
            }
        }

        return false;
    }

    public void PreDrawEverythingBulk(ReadOnlySpan<CutProjectile> entities) {
        using var pipeline = new RenderPipeline(Graphics.PreDrawNPCsQueue, 0.5f, Graphics.WorldTransformMatrix);
        foreach(var entity in entities) {
            entity.Draw(pipeline);
        }

        pipeline.ApplyOutline(Color.Lerp(Color.DarkRed, Color.Black, 0.5f));
    }

    private void Draw(RenderPipeline pipeline) {
        var progress = 1f - (float)Projectile.timeLeft / MaxTimeLeft;

        pipeline.SetTexture(TextureAssets.MagicPixel.Value);

        var buf = new Vector2[2];
        for(var i = 1; i < TrailPositions.Count - 2; i++) {
            buf[0] = TrailPositions[i];
            buf[1] = buf[0]
                + Vector2.UnitY * 20f
                * MathF.Sin(progress * MathHelper.Pi)
                * ((MathF.Sin(4f * MathHelper.Pi * i / TrailPositions.Count) + 1f) / 2f);

            pipeline.DrawTrail(
                buf,
                t => MathF.Max((1f - t) * 3f, 2.1f),
                static _ => Color.DarkRed
            );
        }

        var axeCutEffect = Assets.Shaders.Trail.AxeCut.Asset.Value;

        var positions = CollectionsMarshal.AsSpan(TrailPositions);
        var texture = Assets.Images.Crimson.Items.MeatAxe.CutTexture.Asset.Value;

        pipeline
            .SetTexture(0, texture)
            .SetEffectParams(
                axeCutEffect,
                ("uImage0Size", texture.Size()))
            .DrawTrail(
                positions,
                t => MathF.Sin(t * MathHelper.Pi) * 30f * (1f + 0.2f * MathF.Sin(t * MathHelper.Pi * 6f))
                    * MathF.Pow(MathF.Sin(MathHelper.PiOver2 * (1f - progress)), 2),
                static _ => Color.Red,
                axeCutEffect);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {

        // Calculate direction pointing from the player to the NPC
        Vector2 directionAway = target.Center - Projectile.Center;
        // Normalize it and give it an incredibly small speed so it doesn't move, but still contain direction
        Vector2 customVelocity = directionAway.SafeNormalize(Vector2.UnitX) * 0.001f;
        Projectile.NewProjectile(
                target.GetSource_FromAI(),
                target.Center + new Vector2(
                    target.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f),
                    target.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f)
                ),
                customVelocity.RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(22.5f), MathHelper.ToRadians(22.5f))),
                ModContent.ProjectileType<MeatSplatter>(),
                0,
                0,
                Main.myPlayer,
                ai0: 0,
                ai1: 1
                );
        //SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.5f, PitchRange = (1.1f, 1.6f) }, target.Center);
        //SoundEngine.PlaySound(SoundID.NPCHit29 with { Volume = 0.3f, PitchRange = (-0.8f, -0.3f) }, target.Center);

        for(int i = 0; i < Main.rand.NextFloat(15f, 25f); i++) { // You can have a little blood dust on-hit as a treat
            Dust dust = Dust.NewDustPerfect(
                target.Center + new Vector2(
                    target.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-18f, 18f),
                    target.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-18f, 18f)
                ),
                DustID.Blood,
                (directionAway.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 10f)).RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(22.5f), MathHelper.ToRadians(22.5f))),
                default,
                default,
                Main.rand.NextFloat(0.7f, 1.2f)
            );
        }

        int duration = 60 * 10;
        target.AddBuff(ModContent.BuffType<MeatAxeDebuff>(), duration);
    }
}
public class MeatAxeDebuff : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.MeatAxe.MeatAxeDebuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true; // Makes it a debuff
    }

    public override void Update(Player player, ref int buffIndex) {
        player.statDefense -= 20;

        if(Main.rand.NextBool(2)) {
            Dust.NewDust(player.position + new Vector2(
                player.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f),
                player.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f)),
                player.width, player.height, DustID.Blood);
            Dust.NewDust(player.position + new Vector2(
                player.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f),
                player.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f)),
                player.width, player.height, DustID.BloodWater);
        }
    }
    public override void Update(NPC npc, ref int buffIndex) {

        if(Main.rand.NextBool(1)) {
            Dust.NewDust(npc.position + new Vector2(
                npc.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f),
                npc.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f)),
                npc.width, npc.height, DustID.Blood);
            Dust.NewDust(npc.position + new Vector2(
                npc.width * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f),
                npc.height * Main.rand.NextFloat(-0.3f, 0.3f) + Main.rand.NextFloat(-10f, 10f)),
                npc.width, npc.height, DustID.BloodWater);
        }
    }
}
public class MeatAxeGlobalNPC : GlobalNPC {
    // This hook runs every time the NPC is about to take damage from any source
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
        // Check if the NPC currently has your custom debuff active
        if(npc.HasBuff(ModContent.BuffType<MeatAxeDebuff>())) {
            // Safely reduces their effective defense by exactly 20 for this calculation
            modifiers.ArmorPenetration += 20;
        }
    }
}