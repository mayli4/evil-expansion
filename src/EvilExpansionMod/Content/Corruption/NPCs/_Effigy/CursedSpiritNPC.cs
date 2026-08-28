using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Content.Projectiles;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Daybreak.Common.Rendering;

namespace EvilExpansionMod.Content.Corruption;
public enum SpiritType {
    Splitter,
    Exploder,
    Ram,
}

public enum SplitterState {
    FlyToTarget,
    Splitting
}

public enum ExploderState {
    FlyToTarget,
    Exploding
}

public enum RamState {
    FlyAround,
    Dash,
    Concussion,
    Charge
}

[StructLayout(LayoutKind.Explicit)]
public struct SpiritData {
    [FieldOffset(0)]
    public SplitterData Splitter;

    [FieldOffset(0)]
    public RamData Ram;

    [FieldOffset(0)]
    public ExploderData Exploder;

    public struct SplitterData {
        public float FireballTimer;
        public int Depth;
    }

    public struct RamData {
        public Vector2 DashDirection;
    }

    public struct ExploderData {
        public float FireballTimer;
    }
}

public sealed class CursedSpiritNPC : ModNPC {
    const float ExploderExplosionTime = 100f;
    const float SplitterSplitTime = 90f;
    const float SplitterMaxDepth = 1;
    const int MaxLife = 100;

    SpiritType SpiritType {
        get => Unsafe.BitCast<float, SpiritType>(NPC.ai[0]);
        set => NPC.ai[0] = Unsafe.BitCast<SpiritType, float>(value);
    }
    SpiritData _data;

    ref float Timer => ref NPC.ai[1];

    Vector2[] _trailPositions;

    float _lookOffset;
    Vector2 _lookDirection;
    Player Target => Main.player[NPC.target];
    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T State<T>() where T : struct => Unsafe.BitCast<float, T>(NPC.ai[2]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetState<T>(T state) where T : struct {
        NPC.ai[2] = Unsafe.BitCast<T, float>(state);
        NPC.netUpdate = true;
        Timer = 0;
    }

    public override string Texture => Assets.Images.Corruption.NPCs.Effigy.CursedSpiritMasks.KEY;

    public readonly static Color GhostColor1 = new(214, 237, 5);
    public readonly static Color GhostColor2 = new(181, 200, 4);

    public override void SetDefaults() {
        NPC.width = 38;
        NPC.height = 38;
        NPC.lifeMax = MaxLife;
        NPC.defense = 28;
        NPC.value = 150;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.damage = 40;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<CursedSpiritBannerItem>();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.25f : 0;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RawShadowScalesItem>(), 2, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ImputedFlameItem>(), 2, 1, 2));
    }

    public override void OnSpawn(IEntitySource source) {
        _trailPositions = new Vector2[12];
        for(int i = 0; i < _trailPositions.Length; i++) {
            _trailPositions[i] = NPC.Center;
        }

        SpiritType = (SpiritType)Main.rand.Next(0, 3);
        switch(SpiritType) {
            case SpiritType.Splitter:
                _data.Splitter = new()
                {
                    Depth = 0,
                    FireballTimer = 0,
                };
                break;
            case SpiritType.Exploder:
                _data.Exploder = new()
                {
                    FireballTimer = 0,
                };
                SetState(ExploderState.FlyToTarget);
                break;
            case SpiritType.Ram:
                _data.Ram = new()
                {
                    DashDirection = Vector2.Zero,
                };
                SetState(RamState.FlyAround);
                break;
        }
    }

    public override void AI() {
        NPC.TargetClosest();

        var directionToTarget = Vector2.Zero;
        var distanceToTarget = 999_999f;
        if(Target != null) {
            var targetDelta = Target.Center - NPC.Center;
            distanceToTarget = targetDelta.Length();
            directionToTarget = targetDelta / distanceToTarget;
        }

        var moveSpeed = NPC.velocity.Length();
        var moveDirection = NPC.velocity / moveSpeed;

        switch(SpiritType) {
            case SpiritType.Splitter:
                UpdateSplitter(moveDirection);
                break;
            case SpiritType.Exploder:
                UpdateExploder(moveDirection);
                break;
            case SpiritType.Ram:
                UpdateRam(moveDirection, distanceToTarget, directionToTarget, moveSpeed);
                break;
        }

        Timer += 1;

        _trailPositions ??= [.. Enumerable.Repeat(NPC.Center, 12)];

        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = NPC.Center + NPC.velocity;

        if(!Main.dedServ) {
            if(Main.rand.NextBool(7)) {
                Dust.NewDust(
                    NPC.position,
                    NPC.width,
                    NPC.height,
                    DustID.CursedTorch,
                    newColor: Main.rand.NextFromList(GhostColor1, GhostColor2)
                );
            }

            if (Main.rand.NextBool(12)) {
                var ember = GlowEmberParticle.NewParticle(
                    NPC.Center + Main.rand.NextVector2Circular(11, 11),
                    Main.rand.NextVector2Circular(11, 11),
                    Main.rand.NextFloat(0.5f, 1f),
                    GhostColor1 with { A = 0 }, Color.White with { A = 0 }
                    );

                ember.Randomness *= 2f;
                ember.LossPerSecond *= 2f;
                ParticleEngine.PARTICLES.Add(ember);


                var emitDirection = NPC.velocity.LengthSquared() > 0.1f
                    ? -Vector2.Normalize(NPC.velocity)
                    : -_lookDirection;

                float coneAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = Main.rand.NextFloat(3f, 7f);
                var flameVelocity = emitDirection.RotatedBy(coneAngle) * speed;

                var flame = DustFlameParticle.RequestNew(
                    NPC.Center + Main.rand.NextVector2Circular(6f, 6f),
                    flameVelocity,
                    GhostColor1,
                    GhostColor2,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    Main.rand.Next(18, 28)
                );

                flame.LossPerFrame = 0.12f;
                flame.Swirly = true;
                flame.ApplyLighting = true;

                ParticleEngine.PARTICLES.Add(flame);
            }

            Lighting.AddLight(NPC.Center, GhostColor1.ToVector3() * 0.75f);
        }
    }

    void UpdateSplitter(Vector2 moveDirection) {
        switch(State<SplitterState>()) {
            case SplitterState.FlyToTarget:
                FlyToTarget(moveDirection);
                break;
            case SplitterState.Splitting:
                UpdateLookDirection(_lookDirection);
                _lookOffset *= 0.95f;

                if(Timer > SplitterSplitTime && Main.netMode != NetmodeID.MultiplayerClient) {
                    _data.Splitter.Depth += 1;
                    NPC.life = NPC.lifeMax = (int)(MaxLife / (1f + _data.Splitter.Depth));
                    NPC.dontTakeDamage = false;

                    var splitNPC = NPC.NewNPCDirect(
                        NPC.GetSource_FromAI(),
                        (int)NPC.Center.X, (int)NPC.Center.Y,
                        ModContent.NPCType<CursedSpiritNPC>()
                    ).ModNPC as CursedSpiritNPC;

                    splitNPC.NPC.life = splitNPC.NPC.lifeMax = NPC.life;
                    splitNPC.SpiritType = SpiritType.Splitter;
                    splitNPC._data.Splitter.Depth = _data.Splitter.Depth;

                    const float SplitSpeed = 15f;
                    NPC.velocity -= Vector2.UnitX * SplitSpeed;
                    splitNPC.NPC.velocity += Vector2.UnitX * SplitSpeed;

                    SetState(SplitterState.FlyToTarget);
                    splitNPC.NPC.netUpdate = true;

                    if(_data.Splitter.Depth == 1) {
                        Gore.NewGoreDirect(
                            NPC.GetSource_Death(),
                            NPC.Center,
                            Main.rand.NextVector2Unit() * 5f,
                            Mod.Find<ModGore>($"CursedSpiritSplitterGore").Type
                        );
                    }
                }
                break;
        }
    }

    void UpdateExploder(Vector2 moveDirection) {
        switch(State<ExploderState>()) {
            case ExploderState.FlyToTarget:
                FlyToTarget(moveDirection);
                break;
            case ExploderState.Exploding:
                UpdateLookDirection(_lookDirection);
                _lookOffset *= 0.95f;

                if(Timer > ExploderExplosionTime) {
                    const int ExplosionRange = 300;
                    if(Main.netMode != NetmodeID.MultiplayerClient) {
                        ExplosionProjectile.New(
                            NPC.GetSource_Death(),
                            NPC.Center,
                            (int)(NPC.damage *1.5 / DifficultyScaler),
                            Color.Yellow,
                            Color.LightGoldenrodYellow,
                            size: ExplosionRange,
                            timeLeft: 35,
                            friendly: true,
                            hostile: true
                        );
                        for(int i = 0; i < 15; i++) {
                            var ember = GlowEmberParticle.NewParticle(NPC.Center + Main.rand.NextVector2Circular(15, 15), Main.rand.NextVector2Circular(11, 11), Main.rand.NextFloat(1f, 2f), GhostColor1 with { A = 0 }, Color.White with { A = 0 });
                            ember.Randomness *= 2f;
                            ember.LossPerSecond *= 2f;
                            ParticleEngine.PARTICLES.Add(ember);
                        }
                        NPC.StrikeInstantKill();
                    }

                    Lighting.AddLight(NPC.Center, GhostColor1.ToVector3() * 3.5f);
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);
                }
                break;
        }
    }

    void UpdateRam(Vector2 moveDirection, float distanceToTarget, Vector2 directionToTarget, float moveSpeed) {
        switch(State<RamState>()) {
            case RamState.FlyAround:
                UpdateLookDirection(directionToTarget);
                _lookOffset = MathF.Min(_lookOffset + 0.05f, 0.75f);

                const float CirclingRadius = 520;
                var targetPosition = PositionAroundTarget(CirclingRadius);
                NPC.velocity += NPC.Center.DirectionTo(targetPosition) * 0.3f;
                NPC.velocity *= 0.95f;

                if(Main.netMode != NetmodeID.MultiplayerClient && Timer > 60 * 2 && distanceToTarget < CirclingRadius + 50) {
                    SetState(RamState.Charge);
                }

                break;
            case RamState.Charge:
                UpdateLookDirection(directionToTarget);

                NPC.velocity *= 0.98f;

                Dust.NewDust(NPC.Center + Main.rand.NextVector2Circular(15, 15) + Main.rand.NextVector2Circular(15, 15), 2, 2, DustID.CursedTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, new Color(207, 255, 0));
                var particle = SmokeParticle.Pool.RequestParticle();

                Vector2 randomVelocity = new Vector2(
                    Main.rand.NextFloat(-1.5f, 1.5f),
                    Main.rand.NextFloat(-2f, -0.5f)
                );

                Color smokeColor = Color.Lerp(Color.White, Color.Black, Main.rand.NextFloat());
                float scale = Main.rand.NextFloat(0.1f, 1.2f);
                int lifetime = Main.rand.Next(40, 90);

                particle.Spawn(NPC.Center + Main.rand.NextVector2Circular(15, 45), randomVelocity, smokeColor, scale, lifetime);

                if(Timer > 60 * 1.5f && Main.netMode != NetmodeID.MultiplayerClient) {
                    _data.Ram.DashDirection = directionToTarget;
                    if(Main.expertMode) {
                        NPC.velocity = _data.Ram.DashDirection * 42f;
                    }
                    else {
                        NPC.velocity = _data.Ram.DashDirection * 30f;
                    }
                    float difficultyScaler = Main.expertMode ? (Main.masterMode ? 3f : 2f) : 1f;
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        new Microsoft.Xna.Framework.Vector2(0f, 0f),
                        ModContent.ProjectileType<SpiritContactExplosion>(),
                        (int)(NPC.damage / difficultyScaler),
                        //For the projectile damage, I have no idea why it deals double the value of the damage given by the above equation! Compensate in equation
                        0.5f,
                        Main.myPlayer,
                        ai0: 1,
                        ai1: 1
                        );
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, NPC.Center);

                    SetState(RamState.Dash);
                }

                break;
            case RamState.Dash:
                UpdateLookDirection(moveDirection);
                _lookOffset = MathF.Min(moveSpeed * 0.25f, 1f);

                NPC.velocity += _data.Ram.DashDirection * 0.7f;
                NPC.velocity *= 0.95f;

                if(Timer > 120) {
                    SetState(RamState.FlyAround);
                }

                break;
            case RamState.Concussion:
                NPC.rotation += 1.2f / (Timer * 0.1f + 1f);
                _lookOffset = 0f;

                NPC.velocity *= 0.97f;
                if(Timer > 120 && Main.netMode != NetmodeID.MultiplayerClient) {
                    SetState(RamState.FlyAround);
                }

                break;
        }
    }

    void FlyToTarget(Vector2 moveDirection) {

        UpdateLookDirection(moveDirection);
        _lookOffset = MathF.Min(_lookOffset + 0.05f, 0.75f);

        var targetPosition = PositionAroundTarget(100);
        NPC.velocity += NPC.Center.DirectionTo(targetPosition) * 0.1f;
        NPC.velocity *= 0.98f;

        ref float fireballTimer = ref _data.Splitter.FireballTimer;
        switch(SpiritType) {
            case SpiritType.Splitter: break;
            case SpiritType.Exploder:
                fireballTimer = ref _data.Exploder.FireballTimer;
                break;
            default: throw new Exception();
        }

        fireballTimer -= 1;
        if(fireballTimer <= 0 && Target != null) {
            fireballTimer = Main.rand.Next(220 / (int)DifficultyScaler, 300 / (int)DifficultyScaler);

            var position = NPC.Center;
            var velocity = Helper.InitialVelocityRequiredToHitPosition(
                position,
                Target.Center + Target.velocity * 40f + 40f * Vector2.UnitX * Main.rand.NextFloatDirection(),
                SpiritFireball.Gravity,
                12f
            );

            for(var i = -1; i < 1; i++) {
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity.RotatedBy(Math.PI * Main.rand.NextFloat(0.015f, 0.03f) * i),
                    ModContent.ProjectileType<SpiritFireball>(),
                    20,
                    0.3f
                );
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with
            {
                Pitch = 2.1f + Main.rand.NextFloatDirection() * 0.3f,
            }, NPC.Center);
        }
    }

    Vector2 PositionAroundTarget(float radius) =>
        Target.Center + (Main.GameUpdateCount * 0.04f + NPC.whoAmI).ToRotationVector2() * radius;

    void UpdateLookDirection(Vector2 direction) {
        _lookDirection = direction;

        NPC.direction = _lookDirection.X > 0 ? 1 : -1;
        NPC.rotation = _lookDirection.ToRotation();
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) return;
        switch(SpiritType) {
            case SpiritType.Splitter:
                return;
            case SpiritType.Exploder:
                if(State<ExploderState>() != ExploderState.Exploding) return;
                break;
        }

        var name = SpiritType switch
        {
            SpiritType.Splitter => "Splitter",
            SpiritType.Exploder => "Exploder",
            SpiritType.Ram => "Ram",
        };

        Gore.NewGoreDirect(
            NPC.GetSource_Death(),
            NPC.Center,
            Main.rand.NextVector2Unit() * 5f,
            Mod.Find<ModGore>($"CursedSpirit{name}Gore").Type
        );

        for(var i = 0; i < 20; i += 1) Dust.NewDust(
            NPC.position,
            NPC.width,
            NPC.height,
            DustID.Pixie,
            newColor: Main.rand.NextFromList(GhostColor1, GhostColor2)
        );
    }

    public override bool CheckDead() {
        switch(SpiritType) {
            case SpiritType.Splitter:
                if(_data.Splitter.Depth == SplitterMaxDepth) return true;
                if(State<SplitterState>() == SplitterState.Splitting) return false;

                NPC.dontTakeDamage = true;
                NPC.life = 1;
                SetState(SplitterState.Splitting);

                return false;
            case SpiritType.Exploder:
                if(State<ExploderState>() == ExploderState.Exploding) return false;

                NPC.dontTakeDamage = true;
                NPC.life = 1;
                SetState(ExploderState.Exploding);

                return false;
        }

        return true;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        unsafe {
            var ptr = Unsafe.AsPointer(ref _data);
            var span = new ReadOnlySpan<byte>(ptr, Unsafe.SizeOf<SpiritData>());
            writer.Write(span);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        var bytes = new byte[Unsafe.SizeOf<SpiritData>()];

        var len = reader.Read(bytes);
        if(len != bytes.Length) throw new Exception("Unexpected byte count..");

        unsafe {
            fixed(void* ptr = bytes) {
                _data = Unsafe.Read<SpiritData>(ptr);
            }
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
        switch(SpiritType) {
            case SpiritType.Ram:
                NPC.velocity = -NPC.velocity;
                if (Main.expertMode) {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        new Vector2(0f, 0f),
                        ModContent.ProjectileType<SpiritContactExplosion>(),
                        (int)(NPC.damage * 0.5 / DifficultyScaler),
                        //For the projectile damage, I have no idea why it deals double the value of the damage given by the above equation! Compensate in equation
                        0.5f,
                        Main.myPlayer,
                        ai0: 1,
                        ai1: 0);
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, NPC.Center);
                }
                else {
                    SoundEngine.PlaySound(SoundID.Item178 with { Volume = 1f } with { PitchRange = (-1.0f, -0.5f) }, NPC.Center);
                }
                SetState(RamState.Concussion);
                break;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var blinker = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * NPC.whoAmI) + MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var bigGlowColor = GhostColor2 * (0.3f + 0.3f * blinker);
        var smallGlowColor = GhostColor1;

        var glowScale = 1f;
        var maskScale = 1f;

        switch(SpiritType) {
            case SpiritType.Splitter:
                glowScale /= 1f + _data.Splitter.Depth;
                break;
            case SpiritType.Exploder:
                if(State<ExploderState>() == ExploderState.Exploding) {
                    var factor = Timer / ExploderExplosionTime;

                    bigGlowColor = Color.Lerp(bigGlowColor, Color.Red, factor * 0.7f);
                    smallGlowColor = Color.Lerp(smallGlowColor, Color.Red, factor * 0.6f);
                    glowScale = 1 + 0.75f * factor;
                    maskScale = 1 + 0.3f * MathF.Pow(factor, 2);
                }
                break;
        }

        var initialSnapshot = spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos - _lookDirection * _lookOffset * 12f,
            null,
            bigGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.35f * glowScale,
            SpriteEffects.None,
            0
        );

        spriteBatch.EndBegin(initialSnapshot);

        if(!NPC.IsABestiaryIconDummy) {
            var trailEffect = Assets.Shaders.Trail.CursedSpiritFire.Asset.Value;
            Renderer.BeginPixelated()
                .SetEffectParams(
                    trailEffect,
                    ("time", 0.025f * Main.GameUpdateCount + NPC.whoAmI * 3.432f),
                    ("mat", Graphics.WorldTransformMatrix),
                    ("stepY", 0.25f),
                    ("scale", 0.8f),
                    ("texture1", Assets.Images.Sample.Pebbles.Asset.Value),
                    ("texture2", Assets.Images.Sample.Noise2.Asset.Value))
                .DrawTrail(
                    _trailPositions,
                    static _ => 40,
                    static t => Color.Lerp(GhostColor1, GhostColor2, t + 0.7f),
                    trailEffect)
                .DrawTexture(new()
                {
                    Texture = Assets.Images.Misc.Circle.Asset.Value,
                    Position = NPC.Center - Main.screenPosition,
                    Color = smallGlowColor,
                    Origin = 16f * Vector2.One,
                    Scale = Vector2.One * 0.8f,
                })
                .ApplyOutline(GhostColor1)
                .End();
        }

        var maskShake = 0f;
        var maskRotation = NPC.direction == 1 ? NPC.rotation : NPC.rotation + MathF.PI;
        switch(SpiritType) {
            case SpiritType.Ram:
                if(State<RamState>() == RamState.Charge) maskShake += Timer * 0.04f;
                break;
            case SpiritType.Exploder:
                if(State<ExploderState>() == ExploderState.Exploding) {
                    maskShake += (Main.GameUpdateCount % 4 == 0 ? 1f : 0f) * Timer * 0.02f;
                    maskRotation += Main.rand.NextFloat(-0.001f, 0.001f) * Timer;
                }
                break;
            case SpiritType.Splitter:
                if(State<SplitterState>() == SplitterState.Splitting) {
                    maskShake += (Main.GameUpdateCount % 4 == 0 ? 1f : 0f) * Timer * 0.02f;
                    maskRotation += Main.rand.NextFloat(-0.001f, 0.001f) * Timer;
                }
                break;
        }

        var maskPositionOffset = _lookDirection * _lookOffset * 10f + Main.rand.NextVector2Unit() * maskShake;

        if(SpiritType == SpiritType.Ram && State<RamState>() == RamState.Dash) {
            var starTex = TextureAssets.Extra[ExtrasID.FallingStar].Value;
            float dashRotation = _data.Ram.DashDirection.ToRotation() + MathHelper.PiOver2;
    
            Vector2 drawPosition = NPC.Center - screenPos - (_data.Ram.DashDirection * 30f);

            float pulse = 1.5f + MathF.Sin(Main.GameUpdateCount * 0.35f) * 0.15f; 
            float finalScale = NPC.scale * maskScale * pulse - 0.2f;

            spriteBatch.End(out var ss);
            spriteBatch.Begin(ss with { BlendState = BlendState.Additive });
    
            float glowPulse = 0.4f + MathF.Sin(Main.GameUpdateCount * 0.5f) * 0.2f;
            spriteBatch.Draw(
                starTex,
                drawPosition,
                null,
                GhostColor1 * 0.5f,
                dashRotation,
                starTex.Size() * 0.5f,
                finalScale * 1f,
                SpriteEffects.None,
                0
            );
            
            spriteBatch.Draw(
                starTex,
                drawPosition - (_data.Ram.DashDirection * 10f),
                null,
                GhostColor1 * 0.5f,
                dashRotation,
                starTex.Size() * 0.5f,
                finalScale * 1.25f,
                SpriteEffects.None,
                0
            );
            
            spriteBatch.Draw(
                glowTexture,
                NPC.Center - screenPos + maskPositionOffset,
                null,
                GhostColor1 * 0.7f,
                0f,
                glowTexture.Size() * 0.5f,
                0.3f,
                SpriteEffects.None,
                0
            );

            spriteBatch.Restart(ss);
        }

        var maskTexture = TextureAssets.Npc[Type].Value;
        var maskSource = new Rectangle(
            SpiritType switch
            {
                SpiritType.Splitter => 0,
                SpiritType.Exploder => 44,
                _ => 100,
            },
            0,
            SpiritType switch
            {
                SpiritType.Splitter => 44,
                SpiritType.Exploder => 54,
                _ => 54,
            },
            44
        );

        var originOffset = SpiritType switch
        {
            SpiritType.Splitter => Vector2.UnitY * -2,
            SpiritType.Exploder => Vector2.UnitY * 3,
            _ => Vector2.Zero,
        };

        if(SpiritType != SpiritType.Splitter || _data.Splitter.Depth == 0) {
            Main.EntitySpriteDraw(
                maskTexture,
                NPC.Center - screenPos + maskPositionOffset,
                maskSource,
                drawColor,
                maskRotation,
                maskSource.Size() / 2f + originOffset,
                NPC.scale * new Vector2(1f - _lookOffset * 0.175f, 1) * maskScale,
                NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            );
        }

        switch(SpiritType) {
            case SpiritType.Splitter:
            case SpiritType.Ram:
                spriteBatch.EndBegin(new() { BlendState = BlendState.Additive });
                spriteBatch.Draw(
                    glowTexture,
                    NPC.Center - screenPos + maskPositionOffset,
                    null,
                    Color.White,
                    0f,
                    glowTexture.Size() * 0.5f,
                    0.05f,
                    SpriteEffects.None,
                    0
                );
                spriteBatch.EndBegin(initialSnapshot);
                break;
            case SpiritType.Exploder:
                break;
        }

        return false;
    }
}
public class SpiritContactExplosion : ModProjectile {
    public override string Texture => Assets.Images.Corruption.NPCs.Effigy.CursedSpiritExplode.KEY;
    public override void SetDefaults() {
        Projectile.width = 98;
        Projectile.height = 98;
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
        Main.projFrames[Projectile.type] = 7;
    }
    public override void AI() {
        if (Projectile.ai[0] == 1) {
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
        // Visuals: Create dust explosion effects here
        for(int i = 0; i < 5; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CursedTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 255, new Color(207, 255, 0));
        }
        Lighting.AddLight(Projectile.Center, 1.0f, 0.9f, 0.1f);
        AnimateProjectile();
    }
    private const int FrameCount = 7;
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