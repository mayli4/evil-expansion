using EvilExpansionMod.Common.PrimitiveDrawing;
using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.NPCs.Corruption._Effigy;
using EvilExpansionMod.Content.Projectiles;
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
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

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
    const float ExploderExplosionTime = 120f;
    const float SplitterSplitTime = 90f;
    const float SplitterMaxDepth = 1;
    const int MaxLife = 50;

    SpiritType SpiritType {
        get => Unsafe.BitCast<float, SpiritType>(NPC.ai[0]);
        set => NPC.ai[0] = Unsafe.BitCast<SpiritType, float>(value);
    }
    SpiritData _data;

    ref float Timer => ref NPC.ai[1];

    PrimitiveTrail trail;

    float _lookOffset;
    Vector2 _lookDirection;
    Player Target => Main.player[NPC.target];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T State<T>() where T : struct => Unsafe.BitCast<float, T>(NPC.ai[2]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetState<T>(T state) where T : struct {
        NPC.ai[2] = Unsafe.BitCast<T, float>(state);
        NPC.netUpdate = true;
        Timer = 0;
    }

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_CursedSpiritMasks;

    public readonly static Color GhostColor1 = new(214, 237, 5);
    public readonly static Color GhostColor2 = new(181, 200, 4);

    public override void SetDefaults() {
        NPC.width = 38;
        NPC.height = 38;
        NPC.lifeMax = MaxLife;
        NPC.value = 250f;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.05f;
        NPC.friendly = false;
        NPC.damage = 20;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void OnSpawn(IEntitySource source) {
        SpiritType = (SpiritType)Main.rand.Next(0, 3);
        // SpiritType = SpiritType.Exploder;
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

        const float TrailSize = 55;
        trail ??= new(
            [.. Enumerable.Repeat(NPC.Center, 12)],
            static t => TrailSize,
            static t => Color.Lerp(GhostColor1, GhostColor2, t + 0.7f)
        );

        var i = trail.Positions.Length - 1;
        while(i > 0) {
            trail.Positions[i] = trail.Positions[i - 1];
            i -= 1;
        }
        trail.Positions[0] = NPC.Center;

        if(!Main.dedServ) {
            if(Main.rand.NextBool(7)) Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Pixie,
                newColor: Main.rand.NextFromList(GhostColor1, GhostColor2)
            );

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
                    const float ExplosionRange = 200;
                    if(Main.netMode != NetmodeID.MultiplayerClient) {
                        MathUtilities.ForEachPlayerInRange(
                            NPC.Center,
                            ExplosionRange,
                            player => player.Hurt(
                                PlayerDeathReason.ByNPC(NPC.whoAmI),
                                40,
                                MathF.Sign(player.Center.X - NPC.Center.X),
                                knockback: 8f
                            )
                        );

                        ExplosionVFXProjectile.Spawn(
                            NPC.GetSource_Death(),
                            NPC.Center,
                            Color.Yellow,
                            Color.Orange,
                            t => Color.Lerp(GhostColor1, Color.Black, t),
                            400,
                            80
                        );

                        NPC.StrikeInstantKill();
                    }

                    Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.96f));
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

                NPC.velocity *= 0.99f;
                if(Timer > 60 * 1.5f && Main.netMode != NetmodeID.MultiplayerClient) {
                    _data.Ram.DashDirection = directionToTarget;
                    NPC.velocity = _data.Ram.DashDirection * 0.8f;
                    SetState(RamState.Dash);
                }

                break;
            case RamState.Dash:
                UpdateLookDirection(moveDirection);
                _lookOffset = MathF.Min(moveSpeed * 0.25f, 1f);

                NPC.velocity += _data.Ram.DashDirection * 0.7f;
                NPC.velocity *= 0.97f;

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

        fireballTimer += 1;
        if(fireballTimer > 90 && Target != null) {
            fireballTimer = 0;

            var position = NPC.Center;
            var velocity = MathUtilities.InitialVelocityRequiredToHitPosition(
                position,
                Target.Center + Target.velocity * 70f,
                SpiritFireball.Gravity,
                12f
            );

            for(var i = -1; i < 2; i++) {
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity.RotatedBy(Math.PI * 0.025f * i),
                    ModContent.ProjectileType<SpiritFireball>(),
                    20,
                    0.3f
                );
            }

            var groan1 = new SoundStyle($"{nameof(EvilExpansionMod)}/Assets/Sounds/CursedSpiritGroan1");
            var groan2 = new SoundStyle($"{nameof(EvilExpansionMod)}/Assets/Sounds/CursedSpiritGroan2");

            SoundEngine.PlaySound(Main.rand.NextFromList(groan1, groan2), NPC.Center);
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
                if(_data.Splitter.Depth < SplitterMaxDepth) return;
                break;
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

        for(var i = 0; i < 10; i += 1) Dust.NewDust(
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
                SetState(RamState.Concussion);
                break;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(!NPC.IsABestiaryIconDummy) {
            var trailEffect = Assets.Assets.Effects.Compiled.Trail.CursedSpiritFire.Value;
            trailEffect.Parameters["time"].SetValue(0.025f * Main.GameUpdateCount);
            trailEffect.Parameters["mat"].SetValue(MathUtilities.WorldTransformationMatrix);
            trailEffect.Parameters["stepY"].SetValue(0.25f);
            trailEffect.Parameters["scale"].SetValue(0.8f);
            trailEffect.Parameters["texture1"].SetValue(Assets.Assets.Textures.Sample.Pebbles.Value);
            trailEffect.Parameters["texture2"].SetValue(Assets.Assets.Textures.Sample.Noise2.Value);
            trail?.Draw(trailEffect);
        }

        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
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


        var snapshot = spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos - _lookDirection * _lookOffset * 12f,
            null,
            bigGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.45f * glowScale,
            SpriteEffects.None,
            0
        );

        spriteBatch.Draw(
            glowTexture,
            NPC.Center - screenPos,
            null,
            smallGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.2f * glowScale,
            SpriteEffects.None,
            0
        );
        spriteBatch.EndBegin(snapshot);

        var maskShake = 0f;
        var maskRotation = NPC.direction == 1 ? NPC.rotation : NPC.rotation + MathF.PI;
        switch(SpiritType) {
            case SpiritType.Ram:
                if(State<RamState>() == RamState.Charge) maskShake += Timer * 0.01f;
                break;
            case SpiritType.Exploder:
                if(State<ExploderState>() == ExploderState.Exploding) {
                    maskShake += (Main.GameUpdateCount % 4 == 0 ? 1f : 0f) * Timer * 0.015f;
                    maskRotation += Main.rand.NextFloat(-0.001f, 0.001f) * Timer;
                }
                break;
        }

        var maskPositionOffset = _lookDirection * _lookOffset * 10f + Main.rand.NextVector2Unit() * maskShake;

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
                spriteBatch.EndBegin(snapshot);
                break;
            case SpiritType.Exploder:
                break;
        }

        return false;
    }
}
