using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class TerrorBatNPC : ModNPC {
    public override string Texture => Assets.Images.Corruption.NPCs.TerrorBat.TerrorBatNPC.KEY;

    public enum State {
        IdleOnCeiling,
        WakingUp,
        Awake,
        DashTelegraph,
        Dashing,
        Spitting,
    }

    public State CurrentState {
        get => (State)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    public float StateTimer {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public float DashCooldown {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    public float SpitCooldown {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    private const float wake_up_detection_range = 300f;
    private const int waking_up_time = 2 * 60;
    private const float max_speed = 6f;
    private const float max_acceleration = 0.1f;

    private const float dash_speed = 20f;
    private const int dash_duration = 20;
    private const int dash_cooldown_time = 240;

    private const int spit_duration = 40;
    private const int spit_cooldown_time = 180;

    private const float rotation_factor = 0.08f;

    private int _sleepDustSpawnTimer;
    private int _currentSleepDustIndex;

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Position = new Vector2(0f, 0f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = -10f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        Main.npcFrameCount[Type] = 10;
    }

    public override void SetDefaults() {
        NPC.width = 40;
        NPC.height = 30;
        NPC.lifeMax = 152;
        NPC.damage = 40;
        NPC.defense = 10;
        NPC.value = 100 * 2;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.7f;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath2;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<TerrorbatBannerItem>();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.20f : 0;
    }
    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ImputedFlameItem>(), 3, 1, 3));
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.TerrorBatNPCBestiary.KEY),
        });
    }

    public override void OnSpawn(IEntitySource source) {

        var startX = (int)(NPC.Center.X / 16f);
        var startY = (int)(NPC.Center.Y / 16f);

        for(var j = 0; j < 40; j++) {
            var checkTopY = startY - j;
            var checkBottomY = startY + j;

            if(IsValidSpawnCoords(startX, checkTopY)) {
                AcceptSpawnCoords(this, startX, checkTopY);
                return;
            }
            else if(checkBottomY < Main.maxTilesY && IsValidSpawnCoords(startX, checkBottomY)) {
                AcceptSpawnCoords(this, startX, checkBottomY);
                return;
            }
        }

        //if the loop completes and doesnt find a valid ceiling, just despawn so its not floating awkwardly
        NPC.active = false;

        static bool IsValidSpawnCoords(int i, int j) {
            var ceilingTile = Main.tile[i, j];
            if(!ceilingTile.HasTile ||
                !Main.tileSolid[ceilingTile.TileType] ||
                TileID.Sets.Platforms[ceilingTile.TileType]) return false;

            for(var k = 1; k < 3; k++) {
                var underTile = Main.tile[i, j + k];
                if(underTile.HasTile || underTile.LiquidAmount > 0) return false;
            }

            return true;
        }

        void AcceptSpawnCoords(TerrorBatNPC bat, int i, int j) {
            var spawnPosition = new Vector2(i * 16f - 8f, (j + 2) * 16f);
            if(Main.tile[i, j].BlockType != BlockType.Solid) spawnPosition.Y -= 8f;

            NPC.position = spawnPosition;
            CurrentState = State.IdleOnCeiling;
            NPC.velocity = Vector2.Zero;
            NPC.rotation = 0f;

            _sleepDustSpawnTimer = Main.rand.Next(90, 150);

            if(source is not EntitySource_Parent) {
                var otherNpcCount = Main.rand.Next(1, 4);
                for(var k = 0; k < otherNpcCount; k++) {
                    NPC.NewNPCDirect(
                        new EntitySource_Parent(NPC),
                        spawnPosition + Vector2.UnitX * (1 + k) * Main.rand.Next(32, 48),
                        NPC.type);
                }
            }
        }
    }

    public override void Load() {
        for(int j = 0; j <= 3; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/TerrorBatGore" + j);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }

        for(int i = 0; i <= 3; i++) {
            Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("TerrorBatGore" + i).Type);
        }
    }

    public override void AI() {
        NPC.TargetClosest();
        Player targetPlayer = Main.player[NPC.target];

        if(!targetPlayer.active || targetPlayer.dead) {
            CurrentState = State.Awake; //todo make bat search for a place to perch after player is dead
            NPC.netUpdate = true;
            return;
        }

        if(DashCooldown > 0) {
            DashCooldown--;
        }
        if(SpitCooldown > 0) {
            SpitCooldown--;
        }

        switch(CurrentState) {
            case State.IdleOnCeiling:
                NPC.velocity = Vector2.Zero;
                NPC.rotation = 0f;
                NPC.noTileCollide = false;

                _sleepDustSpawnTimer--;
                if(_sleepDustSpawnTimer <= 0) {
                    Dust.NewDustPerfect(
                        NPC.Center + new Vector2(0f, -NPC.height / 2f - 4f - (_currentSleepDustIndex * 6f)),
                        ModContent.DustType<Sleep>(),
                        new Vector2(Main.rand.NextFloat(0, 0), Main.rand.NextFloat(0, -1))
                    ).fadeIn = 60;
                    _currentSleepDustIndex++;

                    if(_currentSleepDustIndex < 3) {
                        _sleepDustSpawnTimer = Main.rand.Next(15, 30);
                    }
                    else {
                        _sleepDustSpawnTimer = Main.rand.Next(90, 150);
                        _currentSleepDustIndex = 0;
                    }
                }

                if(Vector2.Distance(NPC.Center, targetPlayer.Center) < wake_up_detection_range) {
                    CurrentState = State.WakingUp;
                    StateTimer = waking_up_time;
                    NPC.netUpdate = true;
                }
                break;

            case State.WakingUp:
                WakeUp();
                break;

            case State.Awake:
                AwakeMovement(targetPlayer);
                break;

            case State.DashTelegraph:
                DashTelegraph(targetPlayer);
                break;

            case State.Dashing:
                Dash(targetPlayer);
                break;

            case State.Spitting:
                Spitting(targetPlayer);
                break;
        }
    }

    public void WakeUp() {
        NPC.velocity = Vector2.Zero;
        NPC.noTileCollide = false;

        if(StateTimer == waking_up_time) {
            SoundEngine.PlaySound(SoundID.DD2_WyvernScream, NPC.Center);
        }

        if(StateTimer == waking_up_time - 60) {
            //WAKE UP other bats
            for(int i = 0; i < Main.npc.Length; i++) {
                var otherNpc = Main.npc[i];
                if(otherNpc.active && otherNpc.type == Type && otherNpc.whoAmI != NPC.whoAmI) {
                    var otherBat = otherNpc.ModNPC as TerrorBatNPC;
                    if(otherBat != null && otherBat.CurrentState == State.IdleOnCeiling) {
                        otherBat.CurrentState = State.Awake;
                        otherNpc.netUpdate = true;
                    }
                }
            }
        }

        StateTimer--;
        if(StateTimer <= 0) {
            CurrentState = State.Awake;
            NPC.noTileCollide = true;
            NPC.netUpdate = true;
        }
    }

    public void AwakeMovement(Player targetPlayer) {
        NPC.noTileCollide = false;
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        Vector2 directionToPlayer = targetPlayer.Center - NPC.Center;
        directionToPlayer.Normalize();
        directionToPlayer *= max_acceleration;

        NPC.velocity += directionToPlayer;
        NPC.velocity = Vector2.Clamp(NPC.velocity, -Vector2.One * max_speed, Vector2.One * max_speed);

        NPC.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
        NPC.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

        NPC.rotation = NPC.velocity.X * rotation_factor;

        if(DashCooldown <= 0 && Vector2.Distance(NPC.Center, targetPlayer.Center) > 100f && Main.rand.NextBool(100)) {
            CurrentState = State.DashTelegraph;
            StateTimer = 25;
            NPC.netUpdate = true;
        }
        else if(SpitCooldown <= 0 && Vector2.Distance(NPC.Center, targetPlayer.Center) < 400f && Main.rand.NextBool(180)) {
            CurrentState = State.Spitting;
            StateTimer = spit_duration;
            NPC.netUpdate = true;
            SpitCooldown = spit_cooldown_time;
        }
    }

    public void DashTelegraph(Player targetPlayer) {
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        Vector2 directionAwayFromPlayer = Vector2.Normalize(NPC.Center - targetPlayer.Center);
        NPC.velocity = directionAwayFromPlayer * 4;

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if(StateTimer <= 0) {
            CurrentState = State.Dashing;
            StateTimer = dash_duration;
            NPC.netUpdate = true;
            DashCooldown = dash_cooldown_time;
        }
    }

    public void Dash(Player targetPlayer) {
        NPC.noTileCollide = true;

        if(StateTimer == dash_duration) {
            Vector2 dashDirection = Vector2.Normalize(targetPlayer.Center - NPC.Center);
            NPC.velocity = dashDirection * dash_speed;
        }

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if(StateTimer <= 0) {
            CurrentState = State.Awake;
            NPC.velocity *= 0.5f;
            NPC.netUpdate = true;
        }
    }

    public void Spitting(Player targetPlayer) {
        NPC.velocity *= 0.9f;
        NPC.noTileCollide = true;
        NPC.direction = NPC.spriteDirection = (targetPlayer.Center.X < NPC.Center.X) ? -1 : 1;

        if(StateTimer == (spit_duration / 2)) {
            var shootDirection = Vector2.Normalize(targetPlayer.Center - NPC.Center);
            float difficultyScaler = Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                shootDirection * 8f,
                ModContent.ProjectileType<TerrorBatSpit>(),
                (int)(NPC.damage * 0.125f / difficultyScaler),
                //For the projectile damage, I have no idea why it deals double the value of the damage given by the above equation! To compensate, it is divided by 8 instead of 4.
                0.5f,
                Main.myPlayer,
                ai0:0
            );

            NPC.velocity += shootDirection * -4;

            SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.75f }, NPC.Center);
        }

        NPC.rotation = NPC.velocity.X * rotation_factor;

        StateTimer--;
        if(StateTimer <= 0) {
            CurrentState = State.Awake;
            NPC.netUpdate = true;
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;

        NPC.frameCounter++;

        switch(CurrentState) {
            case State.IdleOnCeiling:
                NPC.frame.Y = 0 * frameHeight;
                break;

            case State.WakingUp:
                NPC.frame.Y = 1 * frameHeight;
                break;

            case State.Awake:
            case State.DashTelegraph:
            case State.Dashing:
                NPC.frame.Y = (int)(NPC.frameCounter / 5 % 4 + 2) * frameHeight;
                break;

            case State.Spitting:
                if(NPC.frameCounter < 10) {
                    NPC.frame.Y = 6 * frameHeight;
                }
                else if(NPC.frameCounter < 20) {
                    NPC.frame.Y = 7 * frameHeight;
                }
                else if(NPC.frameCounter < 30) {
                    NPC.frame.Y = 8 * frameHeight;
                }
                else if(NPC.frameCounter < 40) {
                    NPC.frame.Y = 9 * frameHeight;
                }
                else {
                    NPC.frameCounter = 0;
                }
                break;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(CurrentState == State.IdleOnCeiling || CurrentState == State.WakingUp) {
            var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically : (SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally);
            var texture = ModContent.Request<Texture2D>(Texture).Value;
            var frame = NPC.frame;
            var origin = new Vector2(frame.Width / 2f, frame.Height);
            var drawPos = new Vector2(NPC.Center.X, NPC.position.Y - 20) - screenPos;

            float sway = CurrentState == State.IdleOnCeiling ? (float)Math.Sin(Main.timeForVisualEffects / 30f + NPC.whoAmI) * 0.1f
                : CurrentState == State.WakingUp ? (float)Math.Sin(Main.timeForVisualEffects / 10f + NPC.whoAmI) * 0.1f : 0f;

            spriteBatch.Draw(
                texture,
                drawPos,
                frame,
                drawColor,
                NPC.rotation + MathHelper.Pi + sway,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }

        if(NPC.IsABestiaryIconDummy) {
            NPC.frame.Y = 2 * NPC.frame.Height;
            return true;
        }

        return true;
    }
}

public class TerrorBatSpit : ModProjectile {
    public override string Texture => "Terraria/Images/NPC_112";

    private PositionCache positionCache;
    private bool trailInit;

    public const int TRAIL_SIZE = 20;

    public static readonly int MaxTimeLeft = 300;

    float Trailscale = 1f;
    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);

    public readonly static Color GhostColor1 = new(214, 237, 5);
    public readonly static Color GhostColor2 = new(181, 200, 4);

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;

        Projectile.penetrate = 3;

        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 1;

        positionCache = new(30);

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void OnSpawn(IEntitySource source) {
        if(Projectile.ai[0] == 0) { 
            Projectile.hostile = true;
            Projectile.friendly = false;
        }
        else if (Projectile.ai[0] == 1) {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 1;
            Trailscale = 0.5f;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        base.OnHitPlayer(target, info);
        target.AddBuff(BuffID.CursedInferno, 125, false);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.CursedInferno, 3 * 60);
    }

    public override void AI() {
        Projectile.velocity.Y += 0.1f;
        if(Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation();

        var pos = Projectile.Center;

        if(!trailInit) {
            positionCache.SetAll(pos);
            Projectile.oldPos = Projectile.oldPos.Select(_ => Projectile.position).ToArray();
            trailInit = true;
        }

        Dust.NewDustDirect(Projectile.Center, (int)(10 * Trailscale), (int)(10 * Trailscale), DustID.CursedTorch, default, default, default, default, Main.rand.NextFloat(0.5f, 1.5f) * Trailscale);
        positionCache.Add(pos);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

        Projectile.penetrate--;

        if(Projectile.penetrate <= 0) {
            Projectile.Kill();
        }
        else {
            if(Projectile.velocity.X != oldVelocity.X) {
                Projectile.velocity.X = -oldVelocity.X * 0.85f;
            }
            if(Projectile.velocity.Y != oldVelocity.Y) {
                Projectile.velocity.Y = -oldVelocity.Y * 0.85f;
            }
        }
        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var cursedFireEffect = Assets.Shaders.Pixel.CursedSpiritFire.Asset.Value;
        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetTexture(0, Assets.Images.Sample.Pebbles.Asset.Value)
            .SetTexture(1, Assets.Images.Sample.Noise2.Asset.Value)
            .SetEffectParams(
                cursedFireEffect,
                ("uTime", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 3.432f),
                ("uStepY", 0.25f),
                ("uColor1", GhostColor1),
                ("uColor2", GhostColor2),
                ("uStepColor", 0.05f),
                ("uScale", 0.65f))
            .DrawTrail(
                positionCache.Positions,
                TRAIL_SIZE * Scale * Trailscale,
                Color.White,
                cursedFireEffect)
            .ApplyOutline(GhostColor1)
            .End();

        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;

        var fade = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * Projectile.whoAmI) + MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var glowColor = new Color(72, 96, 36, 255) * (0.3f + 0.3f * fade);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Projectile.velocity * 0.6f,
            null,
            glowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.3f * Scale,
            SpriteEffects.None,
            0
        );

        Main.spriteBatch.EndBegin(snapshot);

        return false;
    }

    public override void OnKill(int timeLeft) {
        //for(int i = 0; i < 35; i++) {
        //    Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
        //    int dust = Dust.NewDust(
        //        Projectile.position,
        //        Projectile.width,
        //        Projectile.height,
        //        DustID.CursedTorch,
        //        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
        //        255,
        //        new Color(207, 255, 0),
        //        Main.rand.NextFloat(0.5f, 1.5f));
        //    Main.dust[dust].velocity = burstVelocity;
        //    Main.dust[dust].noGravity = true;
        //}
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<TerrorBatSpitDeath>(),
            0,
            0.5f,
            Main.myPlayer,
            ai0: 1,
            ai1: 1
        );
    }
}

public class Sleep : ModDust {
    public override string Texture => Assets.Images.Corruption.NPCs.TerrorBat.TerrorBatSleepDust.KEY;

    public override bool Update(Dust dust) {

        float scaleReduction = Math.Clamp(dust.fadeIn / 60, 0, 1);

        dust.frame = new Rectangle(0, 0, 28, 30);

        dust.scale = scaleReduction * 0.9f + MathF.Pow(MathF.Sin((Main.GameUpdateCount) / 15f), 2) * 0.3f;
        dust.rotation = MathF.Sin((Main.GameUpdateCount) / 10f) * (MathF.PI / 180f) * 10;

        dust.velocity.X *= 0.975f;

        dust.position += dust.velocity + Vector2.UnitX * MathF.Sin(dust.fadeIn / 15f) * 0.6f;

        dust.fadeIn--;
        if(dust.fadeIn <= 0)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust) {
        var tex = Assets.Images.Corruption.NPCs.TerrorBat.TerrorBatSleepDust.Asset.Value;
        Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

        Main.EntitySpriteDraw(tex, dust.position - Main.screenPosition, dust.frame, Color.White, dust.rotation, drawOrigin, dust.scale, SpriteEffects.None);

        return false;
    }
}

public class TerrorBatSpitDeath : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.TerrorBat.BatSpitDeath.KEY;
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
    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        base.OnHitPlayer(target, info);
        target.AddBuff(BuffID.CursedInferno, 125, false);
    }
}