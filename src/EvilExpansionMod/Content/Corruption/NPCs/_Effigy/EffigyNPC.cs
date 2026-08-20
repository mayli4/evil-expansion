using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class EffigyNPC : ModNPC {
    public override string Texture => Assets.Images.Corruption.NPCs.Effigy.EffigyNPC.KEY;

    private bool dead;
    private int deadTimer;
    private int animCounter;
    private byte spawnedSprits;
    
    public const int DEATH_TIME = 5 * 60;

    private Color glowColor = new Color(230, 254, 6);
    
    private Vector2 squashStretch = Vector2.One;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 21;
        base.SetStaticDefaults();
        NPCID.Sets.NeedsExpertScaling[Type] = true;
    }
 
    public override void SetDefaults() {
        NPC.width = 80;
        NPC.height = 140;

        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0f;
        NPC.damage = 0;
        NPC.friendly = false;
        NPC.behindTiles = true;

        NPC.HitSound = Assets.Sounds.EffigyHit.Asset with {
            PitchVariance = 0.4f,
            Pitch = -0.3f,
        };

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<EffigyBannerItem>();
    }

    public override void DrawBehind(int index) {
        Main.instance.DrawCacheNPCsMoonMoon.Add(index);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.1f : 0f;
    }
    
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement(Mods.EvilExpansionMod.Bestiary.EffigyNPCBestiary.KEY),
        });
    }

    public override void Load() {
        for(int j = 1; j <= 5; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/EffigyGore" + j);
    }

    public override void AI() {
        squashStretch = Vector2.Lerp(squashStretch, Vector2.One, 0.1f);
        
        if(dead) {
            deadTimer++;
            animCounter++;
            Lighting.AddLight(NPC.Center, glowColor.ToVector3());
            
            if ((int)NPC.frameCounter == 6) {
        
                SoundEngine.PlaySound(
                    Assets.Sounds.EffigyBurn.Asset with
                    {
                        PitchVariance = 0.3f,
                    }, 
                    NPC.Center
                );
            }
            
            if ((int)NPC.frameCounter == 7) {
        
                for(int i = 0; i < Main.rand.Next(10, 15); i++) {
                    var ember = GlowEmberParticle.NewParticle(NPC.Center + Main.rand.NextVector2Circular(11, 11), Main.rand.NextVector2Circular(11, 11), Main.rand.NextFloat(0.5f, 1f), glowColor with { A = 0 }, Color.White with { A = 0 });
                    ember.Randomness *= 2f;
                    ember.LossPerSecond *= 2f;
                    ParticleEngine.PARTICLES.Add(ember);
                }
            }

            if ((int)NPC.frameCounter is 16 or 18) {
                var particle = SmokeParticle.Pool.RequestParticle();

                Vector2 randomVelocity = new Vector2(
                    Main.rand.NextFloat(-1.5f, 1.5f),
                    Main.rand.NextFloat(-2f, -0.5f)
                );

                Color smokeColor = Color.Lerp(Color.DarkGray, Color.DarkGray, Main.rand.NextFloat());
                float scale = Main.rand.NextFloat(0.1f, 1.2f);
                int lifetime = Main.rand.Next(40, 90);

                particle.Spawn(NPC.Center + Main.rand.NextVector2Circular(15, 45), randomVelocity, smokeColor, scale, lifetime);
            }
        
            if(deadTimer >= DEATH_TIME) {
                NPC.life = 0;
                NPC.active = false;
            }
        }
        
        if(spawnedSprits >= 3) {
            dead = true;
        }
    }

    void SpawnSpirit(Entity attacker) {
        var position = NPC.position + Vector2.UnitX * NPC.width / 2f;
        NPC.NewNPC(NPC.GetSource_OnHurt(attacker), (int)position.X, (int)position.Y, ModContent.NPCType<CursedSpiritNPC>());

        spawnedSprits++;
        
        squashStretch = new Vector2(0.75f, 1.35f);

        for(int i = 0; i < Main.rand.Next(10, 15); i++) {
            var ember = GlowEmberParticle.NewParticle(NPC.Center + Main.rand.NextVector2Circular(11, 11), Main.rand.NextVector2Circular(11, 11), Main.rand.NextFloat(0.5f, 1f), glowColor with { A = 0 }, Color.White with { A = 0 });
            ember.Randomness *= 2f;
            ember.LossPerSecond *= 2f;
            ParticleEngine.PARTICLES.Add(ember);
        }
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
        if(Main.rand.NextBool(5)) {
            SpawnSpirit(projectile);
        }
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
        if(Main.rand.NextBool(5)) {
            SpawnSpirit(player);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if(NPC.IsABestiaryIconDummy) {
            return false;
        }
            
        var texture = TextureAssets.Npc[Type].Value;
        var noiseTexture = Assets.Images.Sample.DissolveNoise.Asset.Value;
        var shader = Assets.Shaders.Pixel.EffigyDecay.CreateDecayPass();

        float fadeProgress = 0f;
        if (dead && (int)NPC.frameCounter >= 11) {
            fadeProgress = MathHelper.Clamp((deadTimer - 65f) / (DEATH_TIME - 65f), 0f, 1f);
        }

        var frameUvStart = new Vector2(0f, (float)NPC.frame.Y / texture.Height);
        var frameUvSize = new Vector2((float)NPC.frame.Width / texture.Width, (float)NPC.frame.Height / texture.Height);
        var drawPosition = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height) - screenPos - new Vector2(0, -44);

        shader.Parameters.fadeProgress = fadeProgress;
        shader.Parameters.sampleColor = drawColor.ToVector4();
        shader.Parameters.frameUVStart = frameUvStart;
        shader.Parameters.frameUVSize = frameUvSize;

        shader.Parameters.noiseStretch = Vector2.One;
        shader.Parameters.noiseOffset = new Vector2(-(float)Main.timeForVisualEffects * 0.006f, -(float)Main.timeForVisualEffects * 0.009f);
    
        shader.Parameters.framePixelSize = new Vector2(NPC.frame.Width, NPC.frame.Height);
        shader.Parameters.dissolvePixelSize = 2f;

        shader.Parameters.noise = new HlslSampler
        {
            Texture = noiseTexture,
            Sampler = SamplerState.LinearWrap,
        };

        spriteBatch.End(out var ss);
        spriteBatch.Begin(ss with { SortMode = SpriteSortMode.Immediate });
        {
            shader.Apply();
    
            spriteBatch.Draw(new DrawParameters(texture)
            {
                Position = drawPosition,
                Source = NPC.frame,
                Color = drawColor,
                Origin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height),
                Scale = NPC.scale * squashStretch,
            });   
        }
        spriteBatch.Restart(ss with { BlendState = BlendState.Additive });
        {
            spriteBatch.Draw(new DrawParameters(Assets.Images.Corruption.NPCs.Effigy.EffigyNPC_Glow.Asset)
            {
                Position = drawPosition,
                Source = NPC.frame,
                Color = Color.White * 0.4f,
                Origin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height),
                Scale = NPC.scale * squashStretch,
            });   
            
            spriteBatch.Draw(new DrawParameters(Assets.Images.Corruption.NPCs.Effigy.EffigyNPC_Bloom.Asset)
            {
                Position = drawPosition,
                Source = NPC.frame,
                Color = glowColor * 0.5f,
                Origin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height),
                Scale = NPC.scale * squashStretch,
            });   
        }
        spriteBatch.Restart(ss);
        
        return false;
    }

    public override bool CheckDead() {
        if(dead) return true;
        
        dead = true;
        deadTimer = 0; 
        
        NPC.dontTakeDamage = true;
        NPC.life = 1;
        
        return false;
    }

    public override void FindFrame(int frameHeight) {
        if(dead) {
            NPC.frameCounter += 0.16f;
            if(NPC.frameCounter >= 20)
                NPC.frameCounter = 20;
        }
        else {
            NPC.frameCounter += 0.15f;
            if(NPC.frameCounter >= 4)
                NPC.frameCounter = 0;
        }

        NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
    }
}