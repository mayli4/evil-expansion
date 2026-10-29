using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CrytapeteItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteItem.KEY;

    public override void SetStaticDefaults() {
        ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
    }

    public override void SetDefaults() {
        Item.damage = 28;
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
        Item.rare = ItemRarityID.LightRed;
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
        SoundEngine.PlaySound(SoundID.Item44, player.Center);
        return false;
    }

    public override void AddRecipes()
        => CreateRecipe()
            .AddIngredient(ModContent.ItemType<PolypBarItem>(), 12)
            .AddIngredient(ModContent.ItemType<RawShadowScalesItem>(), 8)
            .AddIngredient(ModContent.ItemType<ImputedFlameItem>(), 14)
            .AddTile(TileID.DemonAltar)
            .Register();
}

public class CrytapeteBuff : ModBuff {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteBuff.KEY;

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
public class CrytapeteTear : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteTear.KEY;

    public static float Gravity = 0.2f;
    public override void SetDefaults() {
        Projectile.width = 18;
        Projectile.height = 18;
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

        //Projectile.usesLocalNPCImmunity = true;
        //Projectile.localNPCHitCooldown = -1;
    }
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 1;
    }
    public override void OnSpawn(IEntitySource source) {
        Projectile.velocity *= Main.rand.NextFloat(0.8f, 1.2f);
        Projectile.netUpdate = true;

        Projectile.frame = Main.rand.Next(2);
        //Sync the frame in multiplayer?
        if(Main.netMode == Terraria.ID.NetmodeID.Server) {
            NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
        }
    }

    public override void AI() {
        Projectile.velocity.Y += Gravity * 1.1f;
        if (Projectile.velocity.Y > 0) { Projectile.velocity.Y *= 1.03f; }

        Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.02f, 0.02f));
        Projectile.rotation = Projectile.velocity.ToRotation();

        if(Main.rand.NextBool(5)) {
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 4f, Projectile.height / 4f), ModContent.DustType<TinyCrytapeteTear>(), Vector2.Zero, 0, Color.LightBlue, 0.5f);
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 4f, Projectile.height / 4f), ModContent.DustType<SmallCrytapeteTear>(), Vector2.Zero, 0, Color.LightBlue, 0.5f);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        int frameWidth = texture.Width / 2;
        Rectangle sourceRectangle = new Rectangle(Projectile.frame * frameWidth, 0, frameWidth, texture.Height);
        Vector2 drawOrigin = sourceRectangle.Size() / 2f;
        Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);

        var stretch = MathF.Min(Projectile.velocity.Length() * 0.15f, 1f) - 0.5f;
        var strength = 0.5f;

        Main.spriteBatch.Draw(
            texture,
            drawPos,
            sourceRectangle,
            lightColor,
            Projectile.rotation,
            drawOrigin, //texture.Size() / 2f,
            new Vector2(1f + stretch * strength, 1f - stretch * strength),
            SpriteEffects.None,
            0f);

        return false;
    }

    public override void OnKill(int timeLeft) {
        for(int i = 0; i < 4; i++) {
            int dustIndex = Dust.NewDust(
                    Projectile.Center,
                    default,
                    default,
                    ModContent.DustType<SmallCrytapeteTear>(),
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    0,
                    default,
                    Main.rand.NextFloat(1f, 2f)
            );
            Main.dust[dustIndex].noGravity = false;
        }

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<TearDeath>(),
            0,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
        );

        SoundEngine.PlaySound(SoundID.Drip with {
            Pitch = Main.rand.NextFloat(0f, 1.5f), Volume = Main.rand.NextFloat(0.1f, 0.2f) },
            Projectile.position);
    }
}

public class CrytapeteMinion : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteMinion.KEY;

    public ref float AnimationTimer => ref Projectile.localAI[0];
    public ref float CryingTimer => ref Projectile.localAI[1];
    public ref float StackPosition => ref Projectile.localAI[2];

    public float BlinkTimer = 60;

    private const int frame_width = 34;
    private const int frame_height = 28;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 7;

        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        ProjectileID.Sets.MinionSacrificable[Type] = false;
    }

    public override void SetDefaults() {
        Projectile.width = frame_width;
        Projectile.height = frame_height;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.friendly = false;
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

        //Projectile.ai[2] = 1; //TO DEBUG

        if(Projectile.ai[2] == 0) {
            CryingTimer++;
            // After 5 seconds AND one out of five times , with the chance divided by (# of Crytapetes / ((max slots + 1) * 50%))
            // Minion slot => returned value: 1=>1 2=>1.5 3=>2 4=>2.5
            if(CryingTimer >= 60 * 5 && Main.rand.NextBool(500 * (int)(player.maxMinions + 1 * 0.5f) / player.ownedProjectileCounts[Projectile.type] ) ) {

                SoundEngine.PlaySound(SoundID.Item16 with
                { Pitch = Main.rand.NextFloat(-1.5f, 0.5f), Volume = Main.rand.NextFloat(0.2f, 0.4f) },
                Projectile.position
                );

                Projectile.ai[2] = 1;
                CryingTimer = 0;
                Projectile.netUpdate = true;
            }
        }
        else {
            CryingTimer++;
            if(CryingTimer >= Main.rand.Next(60 * 4, 60 * 12)) { //After 4 to 12 seconds, proj.AI is 0
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
            float directionToEnemy = targetNPC.Center.X - player.Center.X; // Enemy left or right?
            bool isFacingEnemy = (player.direction == 1 && directionToEnemy > 0) || (player.direction == -1 && directionToEnemy < 0);

            if(Projectile.ai[2] == 1 ) Shoot(targetNPC); // If crying, just shoot
            else if(isFacingEnemy) Shoot(targetNPC); // If normal AND facing enemy, shoot
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

        if(BlinkTimer > 0) {
            BlinkTimer--;
            if(BlinkTimer <= 0) {
                BlinkTimer = 60 * Main.rand.NextFloat(5f, 20f);
            }
        }

        if(Projectile.ai[2] == 0 && BlinkTimer > 10 && BlinkTimer < 20) { // Blink
            Projectile.frame = 3;
            SoundEngine.PlaySound(SoundID.DripSplash with { PitchRange = (2f, 3f), Volume = Main.rand.NextFloat(0.05f, 0.1f) }, Projectile.Center);
        }
        else if(Projectile.ai[0] == 0) {
            Projectile.frame = (Projectile.ai[2] == 0) ? 0 : 4; // Idle
        }
        else if(AnimationTimer <= 4) {
            Projectile.frame = (Projectile.ai[2] == 0) ? 2 : 6; // Shoot 1
        }
        else Projectile.frame = (Projectile.ai[2] == 0) ? 1 : 5; //Shoot 2
    }

    public override unsafe void PostAI() {
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
            (float)(playerVisualCenter.X + finalOffset.X + rotatedBobbingOffset.X),
            (float)(playerVisualCenter.Y + finalOffset.Y + rotatedBobbingOffset.Y));
        //var rounded = new Vector2(
        //    (float)Math.Round(playerVisualCenter.X + finalOffset.X + rotatedBobbingOffset.X),
        //    (float)Math.Round(playerVisualCenter.Y + finalOffset.Y + rotatedBobbingOffset.Y));
        // Is Math.Round causing the jittering?........

        Projectile.Center = rounded;

        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = player.fullRotation;

        if(Projectile.frame == 4) {
            var eyeOffset = new Vector2(player.direction * 11f, 4);
            if(Main.rand.NextBool(15)) {
                Dust.NewDustPerfect(playerVisualCenter + finalOffset + rotatedBobbingOffset + eyeOffset, ModContent.DustType<SmallCrytapeteTear>(), Vector2.Zero, 0, Color.White, 2f);
            }
        }
    }
    private void Shoot(NPC targetNPC) {
        Projectile.ai[1]++;
        if(Projectile.ai[1] >= Main.rand.Next(60 - 20, 60 + 20)) {
            FireCrytapeteProjectile(targetNPC, Projectile.ai[2] == 1);
            Projectile.ai[1] = 0;
            Projectile.ai[0] = 1;
            AnimationTimer = 12;
            BlinkTimer = 60 * Main.rand.NextFloat(5f, 60f);
        }
    }
    private void FireCrytapeteProjectile(NPC target, bool isCryingShot) {
        if(Main.myPlayer != Projectile.owner) return;

        Vector2 shootBaseOffset = new Vector2(Projectile.spriteDirection * (frame_width / 2f), -frame_height / 4f);
        Vector2 spawnPosition = Projectile.Center + shootBaseOffset.RotatedBy(Projectile.rotation);

        // Calculate direction pointing from the player to the NPC
        Vector2 directionAway = target.Center - Projectile.Center;
        // Normalize it and give it an incredibly small speed so it doesn't move, but still contain direction
        Vector2 customVelocity = directionAway.SafeNormalize(Vector2.UnitX) * 0.001f;

        if(isCryingShot) {
            int burstCount = Main.rand.Next(3, 7);
            for(int i = 0; i < burstCount; i++) {
                Vector2 velocity;
                float projectileSpeed = Main.rand.NextFloat(7f, 11f);
                float horizontalSpread = 80f;

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

                if(Main.rand.NextBool(2)) {
                    SoundEngine.PlaySound(Assets.Sounds.Crytapete.Isaacboss2bubbles1.Asset with { PitchVariance = 0.5f, Volume = Main.rand.NextFloat(0.2f, 0.4f) }, Projectile.Center);
                }
                else SoundEngine.PlaySound(Assets.Sounds.Crytapete.Isaacboss2bubbles2.Asset with { PitchVariance = 0.5f, Volume = Main.rand.NextFloat(0.2f, 0.4f) }, Projectile.Center);

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

            Projectile.NewProjectile(
                target.GetSource_FromAI(),
                spawnPosition + Vector2.UnitY * 0 + Vector2.UnitX * -2f * new Vector2(Main.player[Projectile.owner].direction, 1f),
                customVelocity.RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(12.5f), MathHelper.ToRadians(12.5f))),
                ModContent.ProjectileType<CrytapeteSplatter>(),
                0,
                0,
                Main.myPlayer,
                ai0: 0,
                ai1: 1
                );
        }
        else {
            float projectileSpeed = 12f;
            Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * projectileSpeed;
            velocity = velocity.RotatedBy(Main.rand.NextFloat(-0.07f, 0.07f));

            SoundEngine.PlaySound(SoundID.Item20 with { PitchVariance = 0.4f, Volume = 0.75f }, spawnPosition);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<TerrorBatSpit>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                ai0:1
            );

            Projectile.NewProjectile(
                target.GetSource_FromAI(),
                spawnPosition + Vector2.UnitY * 0 + Vector2.UnitX * -2f * new Vector2(Main.player[Projectile.owner].direction, 1f),
                customVelocity.RotatedBy(Main.rand.NextFloat(-MathHelper.ToRadians(12.5f), MathHelper.ToRadians(12.5f))),
                ModContent.ProjectileType<CrytapeteSplatter>(),
                0,
                0,
                Main.myPlayer,
                ai0: 0,
                ai1: 0
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
public class TearDeath : ModProjectile {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.TearDeath.KEY;
    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 30;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        CooldownSlot = 0;

        Projectile.aiStyle = -1;
        Main.projFrames[Projectile.type] = 5;
    }
    public override void OnSpawn(IEntitySource source) {
        if(Projectile.ai[0] == 1) {
            Projectile.hostile = true;
        }
        else {
            Projectile.hostile = false;
        }
        if(Projectile.ai[1] == 1) {
            Projectile.friendly = true;
        }
        else {
            Projectile.friendly = false;
        }
    }
    public override void AI() {
        AnimateProjectile();
    }
    private const int FrameCount = 5;
    private void AnimateProjectile() {
        int frameDuration = 2;
        Projectile.frameCounter++;
        if(Projectile.frameCounter >= frameDuration) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
            if(Projectile.frame >= FrameCount) {
                Projectile.active = false;
            }
        }
    }
}