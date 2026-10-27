using EvilExpansionMod.Common;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class HeadPounderHeldProjectile : ModProjectile {
    Player Owner => Main.player[Projectile.owner];

    readonly static int PostChargeFrames = 15;
    readonly static int MaxCharge = 60;

    float _charge;
    bool _hitCheck;
    bool _hit;
    float _outlineAlpha;

    float ChargeProgress => MathF.Min((float)_charge / MaxCharge, 1f);
    float Overcharge = 0;
    float spikeMultiplier = 0;

    private bool hasCreatedOverchargeFlash1 = false;
    private bool hasCreatedOverchargeFlash2 = false;

    private SlotId chargeSoundSlot = SlotId.Invalid;

    Vector2 RotationVector => (Projectile.rotation - MathF.PI / 4f).ToRotationVector2() * new Vector2(Owner.direction, 0.5f);
    Vector2[] _trailPositions;

    SlotId _Hitsound = SlotId.Invalid;

    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.HeadPounderItem.KEY;
    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = PostChargeFrames;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 999;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanCutTiles() {
        return true;
    }
    public override void CutTiles() {       // As you can tell... I copied these from an LLM.........hope the code doesn't result in lag spikes or memory leaks or breaks anything else
        // Enforce the attack context for breaking tiles
        DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;

        // 1. CLEAR THE SWEEPING SWING HITBOX
        // Replicates the line geometry used in your Colliding hook (outwards from the player along the weapon's head vector)
        Vector2 startPos = Projectile.position;
        Vector2 endPos = Projectile.position + RotationVector * 90f; // Extends slightly past the weapon center
        float swingThickness = 60f;

        Utils.PlotTileLine(startPos, endPos, swingThickness, DelegateMethods.CutTiles);

        // 2. CLEAR THE IMPACT SHOCKWAVE EXPLOSION (Only triggers precisely upon ground contact)
        if(_hitCheck && _hit) {
            // Reconstruct the exact hitCenter calculated in your AI method
            Vector2 hitCenter = Projectile.position
                + RotationVector * 65f
                + Owner.direction * RotationVector.RotatedBy(MathF.PI / 2f) * 25f;

            // Define the radius of your ground shockwave impact zone (Matches your ForEachNPCInRange 80f radius)
            float shockwaveRadius = 100f;

            // Draw a horizontal line cutting directly across the ground surface where the slam landed
            Vector2 blastLeft = hitCenter - Vector2.UnitX * shockwaveRadius;
            Vector2 blastRight = hitCenter + Vector2.UnitX * shockwaveRadius;

            // Plot the explosive shockwave cut line
            Utils.PlotTileLine(blastLeft, blastRight, 32f, DelegateMethods.CutTiles);
        }
    }
    public override bool PreAI() {
        Owner.itemAnimation = 1;
        if(Owner.HeldItem.type != ModContent.ItemType<HeadPounderItem>()) {
            Projectile.Kill();
            return false;
        }

        return true;
    }

    public override void AI() {
        Owner.heldProj = Projectile.whoAmI;
        if(Owner.channel) {
            Projectile.timeLeft = PostChargeFrames;
            Projectile.rotation = -3.5f * MathF.PI / 4f - ChargeProgress * MathF.PI / 4f;

            _charge += 1 * ((Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Melee)) + (Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Generic)) - 1);
            //Main.NewText("Projectile.speed" + ((Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Melee)) + (Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Generic)) - 1 ), Microsoft.Xna.Framework.Color.White);
            _outlineAlpha = MathF.Pow(ChargeProgress, 2);

            if(_charge >= MaxCharge) {
                if(Overcharge == 0) {
                    SoundEngine.PlaySound(SoundID.Tink);
                }
            }
        }
        else {
            var charged = ChargeProgress >= 1f;
            if(charged) _outlineAlpha = 1f;

            var progress = 1f - (float)Projectile.timeLeft / PostChargeFrames;
            var progressHit = 0.4f;

            Projectile.rotation = -3.5f * MathF.PI / 4f * (1f - MathF.Pow(progress / progressHit, 2));
            if(progress >= progressHit) {
                if(charged && !_hitCheck) {
                    if(_hitCheck == true) ;

                    _hitCheck = true;

                    var hitCenter = Projectile.position
                        + RotationVector * 65f
                        + Owner.direction * RotationVector.RotatedBy(MathF.PI / 2f) * 25f;
                    var hitSize = 40;

                    var hitPosition = hitCenter - Vector2.One * hitSize / 2f;
                    if(Collision.SolidTiles(hitPosition, hitSize, hitSize)) {
                        if(SoundEngine.TryGetActiveSound(_Hitsound, out var sound)) {
                            sound.Stop();
                        }
                        SoundEngine.PlaySound(
                            Assets.Sounds.Cursehound.MaceSlam.Asset with
                            {
                                Pitch = Main.rand.NextFloatDirection() * 0.1f,
                            },
                            Projectile.position
                        );
                        hitCenter.Y += 8f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            hitCenter - Vector2.UnitY * 5, // Does the "- Vector2.UnitY * 5" bit even do anything......
                            Vector2.Zero,
                            ModContent.ProjectileType<MaceCrack>(),
                            0,
                            0
                        );

                        for(int i = 0; i < 6; i++) {
                            var dustPos = hitCenter + Main.rand.NextVector2Circular(120f, 20f)
                                + Vector2.UnitY * Projectile.height * 1.5f;
                            var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 6f)
                                + hitCenter.DirectionTo(dustPos) * 5f;

                            var dustColorStart = new Color(133, 122, 94);
                            var dustColorFade = dustColorStart * 0.4f;

                            var newDustData = new Smoke.Data()
                            {
                                InitialLifetime = 40,
                                ElapsedFrames = 0,
                                InitialOpacity = 0.8f,
                                ColorStart = dustColorStart,
                                ColorFade = dustColorFade,
                                Spin = 0.03f,
                                InitialScale = Main.rand.NextFloat(0.75f, 1.35f)
                            };

                            var newDust = Dust.NewDustPerfect(
                                dustPos,
                                ModContent.DustType<Smoke>(),
                                dustVelocity,
                                0,
                                newColor: Color.White,
                                newDustData.InitialScale
                            );

                            newDust.customData = newDustData;
                        }

                        for(int i = 0; i < 5 * Overcharge; i++) {
                            Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -0.5f)); // More charge = crazier dust burst
                            int ds = Dust.NewDust(hitPosition, hitSize, hitSize, DustID.Stone);
                            Main.dust[ds].velocity = Vector2.Max(burstVelocity * Overcharge * 0.3f, burstVelocity);
                            Main.dust[ds].scale = Math.Max(Main.rand.NextFloat(0.05f * Overcharge, 0.3f * Overcharge), 0.6f);
                        }
                        for(int i = 0; i < Math.Max(4 * Overcharge, 7); i++) {
                            Vector2 burstVelocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-8f, -4f)); // Likewise
                            int dc = Dust.NewDust(hitPosition, hitSize, hitSize, DustID.CursedTorch);
                            Main.dust[dc].velocity = Vector2.Max(burstVelocity * Overcharge * 0.3f, burstVelocity);
                            Main.dust[dc].scale = Math.Max(Main.rand.NextFloat(0.05f * Overcharge, 0.3f * Overcharge), 0.6f);
                            //Main.dust[d].noGravity = true;
                        }

                        Helper.ForEachNPCInRange(hitCenter, 80f, npc =>
                        {
                            if(npc.dontTakeDamage || npc.friendly) return;
                            var modifiers = npc.GetIncomingStrikeModifiers(
                                Projectile.DamageType,
                                Math.Sign(npc.Center.X) - Math.Sign(Owner.Center.X)
                            );
                            Owner.StrikeNPCDirect(npc, modifiers.ToHitInfo(Projectile.damage * 2, false, Projectile.knockBack * 5f));
                        });

                        Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(13f, 0.6f));

                        if(Overcharge >= 5) Overcharge = 20;
                        int Spikemultiplier = (int)Overcharge * 3;

                        CoroutineSystem.Start(SpawnSpikesRoutine(
                            Projectile.GetSource_FromAI(),
                            hitCenter,
                            Owner.direction,
                            Projectile.damage,
                            Projectile.knockBack * 2f,
                            Projectile.owner,
                            Spikemultiplier
                         ));
                        _hit = true;
                    }
                }
                else {
                    if(!SoundEngine.TryGetActiveSound(_Hitsound, out var sound) || !sound.IsPlaying) {
                        _Hitsound = SoundEngine.PlaySound(SoundID.NPCHit52 with { Volume = 1f });
                    }

                }

                Projectile.rotation = _hit ? 0.25f // End rotation I think... 0 is just 0 degrees, but higher numbers rotate it clockwise,
                    : (1f - MathF.Pow(progress - (1f + progressHit), 2)) * MathF.PI / 2f;
            }
        }

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.direction * (Projectile.rotation - MathHelper.PiOver2));
        Projectile.position = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(-4 * Owner.direction, -2);

        var spriteRotationVector = Projectile.rotation.ToRotationVector2() * new Vector2(Owner.direction, 1f);
        var trailLastPosition = spriteRotationVector * 65f;
        _trailPositions ??= [.. Enumerable.Repeat(trailLastPosition, 4)];

        for(var i = _trailPositions.Length - 1; i > 0; i--) {
            _trailPositions[i] = _trailPositions[i - 1]
                + new Vector2(MathF.Sin(i * 0.35f + Main.GameUpdateCount * 0.1f) * 3.2f, -4f);
        }
        _trailPositions[0] = trailLastPosition;
    }
    public override void PostAI() { // Reserving PostAI for overcharge visuals now
        if(ChargeProgress >= 1 && Owner.channel) {
            // Check if the sound is NOT already playing to prevent overlapping!
            if(chargeSoundSlot == SlotId.Invalid || !SoundEngine.TryGetActiveSound(chargeSoundSlot, out _)) {

                // Configure your audio style to loop infinitely
                SoundStyle loopingAudio = SoundID.Item15;

                loopingAudio.Volume = 0.4f + Overcharge * 0.2f;
                loopingAudio.Pitch = -0.5f + Overcharge * 0.2f;

                // Play the sound and remember WHICH slot it occupies
                chargeSoundSlot = SoundEngine.PlaySound(loopingAudio, Projectile.Center);

                SoundStyle loopingAudio2 = SoundID.Item24;

                loopingAudio2.Volume = 0.6f + Overcharge * 0.2f;
                loopingAudio2.Pitch = 0.0f + Overcharge * 0.4f;

                // Play the sound and remember WHICH slot it occupies
                chargeSoundSlot = SoundEngine.PlaySound(loopingAudio2, Projectile.Center);

                if(Overcharge < 5) {
                    Overcharge += 0.5f * ((Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Melee)) + (Main.player[Projectile.owner].GetAttackSpeed(DamageClass.Generic)) - 1); // Charge speed
                    if(Overcharge <= 4.5) {
                        Projectile.NewProjectile(
                            Main.player[Projectile.owner].GetSource_FromAI(),
                            Projectile.Center + Main.rand.NextVector2Circular(2f, 2f) + Vector2.UnitY * 20.7f + Vector2.UnitX * -59.5f * new Vector2(Owner.direction, 1f),
                            new Vector2(0f, 0f),
                            ModContent.ProjectileType<HeadPounderCharge>(),
                            0,
                            0,
                            Main.myPlayer,
                            ai0: Main.rand.NextFloat(1f, 1.3f),
                            ai1: 0.01f
                        );
                    }
                    if(Overcharge >= 1) {
                        if(!hasCreatedOverchargeFlash1) {
                            Projectile.NewProjectile(
                                Main.player[Projectile.owner].GetSource_FromAI(),
                                Projectile.Center + Vector2.UnitY * 20.7f + Vector2.UnitX * -59.5f * new Vector2(Owner.direction, 1f),
                                new Vector2(0f, 0f),
                                ModContent.ProjectileType<HeadPounderChargeShine>(),
                                0,
                                0,
                                Main.myPlayer,
                                ai0: 0f,
                                ai1: 0.1f,
                                ai2: 0
                            );
                            hasCreatedOverchargeFlash1 = true;
                        }
                    }
                }
                else { // Smoke
                    var dustPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 22f)
                        + Vector2.UnitY * 9f + Vector2.UnitX * -70f * new Vector2(Owner.direction, 1f);

                    var dustColorStart = new Color(133, 122, 94);
                    var dustColorFade = dustColorStart * 0.4f;

                    var newDustData = new Smoke.Data()
                    {
                        InitialLifetime = 40,
                        ElapsedFrames = 0,
                        InitialOpacity = Main.rand.NextFloat(0.2f, 0.4f),
                        ColorStart = dustColorStart,
                        ColorFade = dustColorFade,
                        Spin = Main.rand.NextFloat(0.00f, 0.01f),
                        InitialScale = Main.rand.NextFloat(0.2f, 0.8f)
                    };

                    var newDust = Dust.NewDustPerfect(
                        dustPos,
                        ModContent.DustType<Smoke>(),
                        new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -0.5f)),
                        0,
                        newColor: Color.White,
                        newDustData.InitialScale
                    );

                    newDust.customData = newDustData;

                    //Shine spawn and sfx etc.
                    if(!hasCreatedOverchargeFlash2) {
                        Projectile.NewProjectile(
                            Main.player[Projectile.owner].GetSource_FromAI(),
                            Projectile.Center + Vector2.UnitY * 20.7f + Vector2.UnitX * -59.5f * new Vector2(Owner.direction, 1f),
                            new Vector2(0f, 0f),
                            ModContent.ProjectileType<HeadPounderChargeShine>(),
                            0,
                            0,
                            Main.myPlayer,
                            ai0: 1.4f,
                            ai1: 0.1f,
                            ai2: 1
                        );
                        hasCreatedOverchargeFlash2 = true;
                    }
                }
            }
            else {
                // OPTIONAL: Update the sound's position so it moves smoothly with the player
                if(SoundEngine.TryGetActiveSound(chargeSoundSlot, out var activeSound)) {
                    activeSound.Position = Projectile.Center;
                }
            }
        }
        else {
            // If they are no longer charging, stop the loop safely
            StopChargeSound();
        }
    }

    private void StopChargeSound() {
        // If we have a valid slot, locate it and shut it down cleanly
        if(chargeSoundSlot != SlotId.Invalid) {
            if(SoundEngine.TryGetActiveSound(chargeSoundSlot, out var activeSound)) {
                activeSound.Stop();
            }
            // Reset the tracker variable
            chargeSoundSlot = SlotId.Invalid;
        }
    }

    static IEnumerator SpawnSpikesRoutine(
        IEntitySource source,
        Vector2 position,
        int direction,
        int damage,
        float knockback,
        int owner,
        int spikeMultiplier
    ) {
        var x = position.X;
        var tileY = (int)position.Y >> 4;
        for(var i = 0; i < 6 + spikeMultiplier; i++) {
            x += Main.rand.NextFloat(40f, 60f) * direction;
            if(Collision.SolidTiles(new(x, tileY * 16), 1, 1)) {
                for(var j = 1; j < 100; j++) {
                    if(!Collision.SolidTiles(new(x, (tileY - j) * 16), 1, 1)) {
                        tileY -= (j - 1);
                        break;
                    }
                }
            }
            else {
                for(var j = 1; j < 100; j++) {
                    if(Collision.SolidTiles(new(x, (tileY + j) * 16), 1, 1)) {
                        tileY += (j + 1);
                        break;
                    }
                }
            }

            var projectile = Projectile.NewProjectileDirect(
                source,
                new(x, tileY * 16f + 8f - 2f),
                Vector2.Zero,
                ModContent.ProjectileType<PounderSpike>(),
                damage,
                knockback,
                owner,
                Main.rand.Next(9)
            );
            projectile.rotation = Main.rand.NextFloatDirection() * 0.3f;

            for(var j = 0; j < 2; j++) yield return null;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Owner.channel) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.position,
            Projectile.position + RotationVector * 80f,
            40,
            ref _
        );
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        var modifier = MathF.Pow(ChargeProgress, 2);
        if(modifier < 0.1) modifier = 0.1f;
        modifiers.Knockback *= modifier;
        modifiers.SourceDamage *= modifier;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;
        var offset = -6;
        var rotation = (Projectile.rotation + MathF.PI / 4f) * Owner.direction;
        Vector2 origin = new(offset - 5f, texture.Height - offset);

        var trailColor = new Color(96, 91, 206) * _outlineAlpha * 0.4f;

        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetTexture(TextureAssets.MagicPixel.Value)
            .DrawTrail(
                _trailPositions.Select(p => p + Projectile.position).ToArray(),
                static t => (1.25f - t) * 20f,
                _ => trailColor
            )
            .SetSamplerState(SamplerState.PointClamp)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = Projectile.position,
                Color = lightColor,
                Rotation = rotation,
                Origin = Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
                Scale = Vector2.One * Projectile.scale,
                SpriteEffects = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            })
            .ApplyOutline(trailColor)
            .ApplyOutline(new Color(230, 255, 5) * _outlineAlpha)
            .End();

        var tintFlashFrames = 15;
        var tintColor = Color.Transparent;
        if(Owner.channel) {
            tintColor = Color.Yellow
                * MathF.Max(1f - MathF.Pow(2f * MathF.Max(_charge - MaxCharge + tintFlashFrames / 2, 0) / tintFlashFrames - 1f, 2), 0f);
        }

        Graphics.Begin(Graphics.WorldTransformMatrix)
            .DrawTexture(new()
            {
                Texture = texture,
                Position = Projectile.position,
                Color = lightColor,
                Rotation = (Projectile.rotation + MathF.PI / 4f) * Owner.direction,
                Origin = Owner.direction == -1 ? new Vector2(texture.Width - origin.X, origin.Y) : origin,
                Scale = Vector2.One * Projectile.scale,
                SpriteEffects = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            })
            .ApplyTint(tintColor)
            .End();

        return false;
    }
}
