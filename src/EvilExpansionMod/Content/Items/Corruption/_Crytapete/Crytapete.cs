using EvilExpansionMod.Content.Items.Crimson;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

public class CrytapeteItem : ModItem {
    public override string Texture => Assets.Textures.Items.Corruption.Crytapete.CrytapeteItem.KEY;

    public override void SetStaticDefaults() {
        ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
    }

    public override void SetDefaults() {
        Item.damage = 15;
        Item.DamageType = DamageClass.Summon;
        Item.mana = 10;
        Item.width = 26;
        Item.height = 28;
        Item.useTime = 36;
        Item.useAnimation = 36;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(gold: 1, silver: 50);
        Item.rare = ItemRarityID.Green;
        Item.buffType = ModContent.BuffType<CrytapeteBuff>();
        Item.shoot = ModContent.ProjectileType<CrytapeteMinion>();
        Item.shootSpeed = 10f;
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool CanUseItem(Player player) {
        if(player.ownedProjectileCounts[Item.shoot] < player.maxMinions) {
            return true;
        }
        return false;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if(player.altFunctionUse == 2) {
            return false;
        }

        player.AddBuff(Item.buffType, 2);

        Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
        SoundEngine.PlaySound(SoundID.Item103, player.Center);
        return false;
    }

    public override void AddRecipes()
        => CreateRecipe()
            .AddIngredient(ModContent.ItemType<HellDemoniteBarItem>(), 12)
            .AddIngredient(ModContent.ItemType<RawShadowScalesItem>(), 8)
            .AddIngredient(ModContent.ItemType<ImputedFlameItem>(), 4)
            .Register();
}

public class CrytapeteBuff : ModBuff {
    public override string Texture => Assets.Textures.Items.Corruption.Crytapete.CrytapeteBuff.KEY;

    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if(player.ownedProjectileCounts[ModContent.ProjectileType<CrytapeteMinion>()] > 0) {
            player.buffTime[buffIndex] = 18000;
        }
        else {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}

public class CrytapeteFlame : ModProjectile {
    public override string Texture => "Terraria/Images/NPC_112";

    public static float Gravity = 0.2f;

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;
    }

    public override void AI() {
        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100, default, 0.8f);

        Projectile.velocity.Y += Gravity;
        Projectile.rotation = Projectile.velocity.ToRotation();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.CursedInferno, 180);
    }
}

public class CrytapeteTear : ModProjectile {
    public override string Texture => Assets.Textures.Items.Corruption.Crytapete.CrytapeteTear.KEY;

    public static float Gravity = 0.2f;

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;
    }

    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;
    }

    public override void AI() {
        Projectile.velocity.Y += Gravity;

        Projectile.rotation += Projectile.velocity.Length() * 0.05f * Projectile.direction;
        Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.02f, 0.02f));

        if(Main.rand.NextBool(5)) {
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 4f, Projectile.height / 4f), ModContent.DustType<TinyCrytapeteTear>(), Vector2.Zero, 0, Color.LightBlue, 0.5f);
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 4f, Projectile.height / 4f), ModContent.DustType<SmallCrytapeteTear>(), Vector2.Zero, 0, Color.LightBlue, 0.5f);
        }
    }
}

public class CrytapeteMinion : ModProjectile {
    public override string Texture => Assets.Textures.Items.Corruption.Crytapete.CrytapeteMinion.KEY;

    public ref float AnimationTimer => ref Projectile.localAI[0];
    public ref float CryingTimer => ref Projectile.localAI[1];
    public ref float StackPosition => ref Projectile.localAI[2];

    private const int frame_width = 30;
    private const int frame_height = 28;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;

        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = false;
    }

    public override void SetDefaults() {
        Projectile.width = frame_width;
        Projectile.height = frame_height;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 2;
        Projectile.minionSlots = 1f;
        Projectile.aiStyle = -1;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        => overPlayers.Add(index);

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        if(!player.active || player.dead || !player.HasBuff<CrytapeteBuff>()) {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;

        var ownedCrytapetes = Main.projectile.Where(p =>
            p.active && p.owner == Projectile.owner && p.type == Projectile.type
        ).OrderBy(p => p.whoAmI).ToList();

        int myIndex = ownedCrytapetes.FindIndex(p => p.whoAmI == Projectile.whoAmI);
        if(myIndex == -1) {
            Projectile.Kill();
            return;
        }
        StackPosition = myIndex;

        Vector2 playerVisualCenter = player.MountedCenter;

        Vector2 initialRelativeOffset = new Vector2(
            player.direction * 2f,
            -(player.height / 2f) - 6f
        );

        float bobbingOffset = 0f;
        int currentFrameIndex = player.bodyFrame.Y / 56;

        if(currentFrameIndex >= (int)PlayerFrames.Walk1 && currentFrameIndex <= (int)PlayerFrames.Walk14) {
            switch(currentFrameIndex) {
                case (int)PlayerFrames.Walk2: bobbingOffset = -2f; break;
                case (int)PlayerFrames.Walk3: bobbingOffset = -2f; break;
                case (int)PlayerFrames.Walk4: bobbingOffset = -2f; break;


                case (int)PlayerFrames.Walk9: bobbingOffset = -2f; break;
                case (int)PlayerFrames.Walk10: bobbingOffset = -2f; break;
                case (int)PlayerFrames.Walk11: bobbingOffset = -2f; break;
                default: bobbingOffset = 0f; break;
            }
        }

        var bobbingVector = new Vector2(0, bobbingOffset);
        var rotatedBobbingOffset = bobbingVector.RotatedBy(player.fullRotation);

        float offsetYPerCrytapete = frame_height * Projectile.scale * 0.5f;
        var finalOffset = (initialRelativeOffset + new Vector2(0, -StackPosition * offsetYPerCrytapete)).RotatedBy(player.fullRotation);

        var rounded = new Vector2(
            (float)Math.Round(playerVisualCenter.X + finalOffset.X + rotatedBobbingOffset.X),
            (float)Math.Round(playerVisualCenter.Y + finalOffset.Y + rotatedBobbingOffset.Y));

        Projectile.Center = rounded;

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = player.fullRotation;

        if(Projectile.ai[2] == 0) {
            CryingTimer++;
            if(CryingTimer >= 60 * 5 && Main.rand.NextBool(5) && player.ownedProjectileCounts[Projectile.type] < player.maxMinions * 0.75f) {
                Projectile.ai[2] = 1;
                CryingTimer = 0;
                Projectile.netUpdate = true;
            }
        }
        else {
            CryingTimer++;
            if(CryingTimer >= Main.rand.Next(60 * 3, 60 * 10)) {
                Projectile.ai[2] = 0;
                CryingTimer = 0;
                Projectile.netUpdate = true;
            }
        }

        NPC targetNPC = null;
        if(player.HasMinionAttackTargetNPC) {
            NPC potentialTarget = Main.npc[player.MinionAttackTargetNPC];
            if(potentialTarget.active
                && !potentialTarget.dontTakeDamage
                && !potentialTarget.friendly
                && potentialTarget.Distance(Projectile.Center) < 600f * 1.5f
                && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, potentialTarget.position, potentialTarget.width, potentialTarget.height)) {
                targetNPC = potentialTarget;
            }
        }

        if(targetNPC == null) {
            float maxDetectRange = 600f;
            for(int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if(npc.active
                    && !npc.dontTakeDamage
                    && !npc.friendly
                    && npc.lifeMax > 5
                    && !npc.immortal
                    && npc.Distance(Projectile.Center) < maxDetectRange
                    && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
                    maxDetectRange = npc.Distance(Projectile.Center);
                    targetNPC = npc;
                }
            }
        }

        if(targetNPC != null) {
            Projectile.spriteDirection = player.direction;
        }
        else {
            Projectile.spriteDirection = player.direction;
        }

        if(targetNPC != null && targetNPC.active && targetNPC.Distance(Projectile.Center) < 600f) {
            Projectile.ai[1]++;
            if(Projectile.ai[1] >= Main.rand.Next(60 - 15, 60 + 15)) {
                FireCrytapeteProjectile(targetNPC, Projectile.ai[2] == 1);
                Projectile.ai[1] = 0;
                Projectile.ai[0] = 1;
                AnimationTimer = 10;
            }
        }
        else {
            Projectile.ai[1] = Math.Max(0, Projectile.ai[1] - 1);
            Projectile.ai[0] = 0;
        }
        if(AnimationTimer > 0) {
            AnimationTimer--;
            if(AnimationTimer <= 0) {
                Projectile.ai[0] = 0;
            }
        }

        if(Projectile.ai[0] == 0) {
            Projectile.frame = (Projectile.ai[2] == 0) ? 0 : 2;
        }
        else {
            Projectile.frame = (Projectile.ai[2] == 0) ? 1 : 3;
        }

        if(Projectile.frame == 2) {
            var eyeOffset = new Vector2(player.direction * 11f, 4);

            if(Main.rand.NextBool(15)) {
                Dust.NewDustPerfect(playerVisualCenter + finalOffset + rotatedBobbingOffset + eyeOffset, ModContent.DustType<SmallCrytapeteTear>(), Vector2.Zero, 0, Color.White, 2f);
            }
        }
    }

    private void FireCrytapeteProjectile(NPC target, bool isCryingShot) {
        if(Main.myPlayer != Projectile.owner) return;

        Vector2 shootBaseOffset = new Vector2(Projectile.spriteDirection * (frame_width / 2f), -frame_height / 4f);
        Vector2 spawnPosition = Projectile.Center + shootBaseOffset.RotatedBy(Projectile.rotation);

        if(isCryingShot) {
            int burstCount = Main.rand.Next(2, 4);
            for(int i = 0; i < burstCount; i++) {
                Vector2 velocity;
                float projectileSpeed = Main.rand.NextFloat(5f, 8f);
                float horizontalSpread = 80f;

                if(Main.rand.NextBool(5)) {
                    velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                    velocity = velocity.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f));
                    SoundEngine.PlaySound(SoundID.Item20, spawnPosition);
                }
                else {
                    Vector2 targetLandingSpot = target.Center + new Vector2(Main.rand.NextFloat(-horizontalSpread, horizontalSpread), 0f);

                    float heightAdjustment = 50f;
                    Vector2 adjustedTargetForArc = targetLandingSpot - new Vector2(0, heightAdjustment);

                    velocity = Helper.InitialVelocityRequiredToHitPosition(
                        spawnPosition,
                        adjustedTargetForArc,
                        CrytapeteTear.Gravity,
                        projectileSpeed
                    );

                    if(velocity == Vector2.Zero || float.IsNaN(velocity.X) || float.IsNaN(velocity.Y)) {
                        velocity = (targetLandingSpot - spawnPosition).SafeNormalize(Vector2.UnitY) * projectileSpeed;
                        velocity.Y -= Main.rand.NextFloat(0f, 3f);
                    }

                    SoundEngine.PlaySound(SoundID.Splash, spawnPosition);
                }

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<CrytapeteTear>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
        else {
            float projectileSpeed = 12f;
            Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * projectileSpeed;
            velocity = velocity.RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f));
            SoundEngine.PlaySound(SoundID.Item20, spawnPosition);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<CrytapeteFlame>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        var sourceRectangle = new Rectangle(
            Projectile.frame * frame_width,
            0,
            frame_width,
            frame_height
        );

        var origin = sourceRectangle.Size() / 2f;
        var spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            sourceRectangle,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            spriteEffects
        );

        return false;
    }
}