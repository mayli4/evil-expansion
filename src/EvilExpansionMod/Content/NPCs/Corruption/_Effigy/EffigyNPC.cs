using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

public sealed class EffigyNPC : ModNPC {
    byte _spawnedSprits = 0;

    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Effigy.KEY_EffigyNPC;

    private bool _dead;
    private int _deadTimer;
    private int _animCounter;
    
    public int DEATH_TIME = 5 * 60;

    private int _spawnTimer;

    private Color _glowColor = new Color(230, 254, 6);
    
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 18;
    }
    
    public override void SetDefaults() {
        NPC.width = 60;
        NPC.height = 90;
        
        NPC.lifeMax = 640;
        NPC.value = 250f;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.knockBackResist = 0f;
        NPC.damage = 0;
        NPC.friendly = false;

        NPC.HitSound = SoundID.NPCHit23;

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;
    }

    public override void AI() {
        if(_dead) {
            _deadTimer++;
            _animCounter++;
            Lighting.AddLight(NPC.Center, _glowColor.ToVector3());
            
            if(_deadTimer >= DEATH_TIME) {
                NPC.life = 0;
                NPC.active = false;
            }
        }

        if(_spawnedSprits >= 3) {
            _dead = true;
        }

        // _spawnTimer++;
        //
        // if(_spawnTimer >= 100) {
        //     if(Main.rand.NextBool(50)) {
        //         SpawnSpirit(NPC);
        //
        //         _spawnTimer = 0;
        //     }   
        // }
    }

    void SpawnSpirit(Entity attacker) {
        var position = NPC.position + Vector2.UnitX * NPC.width / 2f;
        NPC.NewNPC(NPC.GetSource_OnHurt(attacker), (int)position.X, (int)position.Y, ModContent.NPCType<CursedSpiritNPC>());

        _spawnedSprits++;
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
        var texture = TextureAssets.Npc[Type].Value;
        var glowTex = Assets.Assets.Textures.NPCs.Corruption.Effigy.EffigyNPC_Glow.Value;
        
        var offset = new Vector2(0, -77); //cause frame very big! yes

        var shader = Assets.Assets.Effects.Compiled.Pixel.EffigyDecay.Value;

        float progValue = 1.5f;
        
        if (NPC.frameCounter == 17) {
            if(_animCounter >= 1.5f * 60) {
                float deathProgress = MathHelper.Clamp((float)_deadTimer / DEATH_TIME, 0f, 1f);
                progValue = MathHelper.Lerp(1.1f, 0.0f, deathProgress);
            }
        } else {
            progValue = 1.5f;
        }
        
        shader.Parameters["prog"].SetValue(progValue);
        shader.Parameters["edgeColor"].SetValue(Color.Black.ToVector3());
        shader.Parameters["ashColor"].SetValue(_glowColor.ToVector3());
        shader.Parameters["noisetex"].SetValue(Assets.Assets.Textures.Sample.DissolveNoise.Value);
        shader.Parameters["sampleColor"].SetValue(drawColor.ToVector4());
        
        var noiseTexture = Assets.Assets.Textures.Sample.DissolveNoise.Value;
        float noiseAspect = (float)noiseTexture.Width / noiseTexture.Height;
        float frameAspect = (float)NPC.frame.Width / NPC.frame.Height;

        shader.Parameters["noiseTexelAspect"].SetValue(noiseAspect + 200);
        shader.Parameters["frameTexelAspect"].SetValue(frameAspect + 2000);
        shader.Parameters["texSize"].SetValue(new Vector2(NPC.frame.Width, NPC.frame.Height));
        
        var shaderSnapshot = spriteBatch.CaptureEndBegin(new() { CustomEffect = shader });
        
        spriteBatch.Draw(
            texture,
            NPC.Center + offset - screenPos,
            NPC.frame,
            drawColor,
            0f,
            NPC.frame.Size() / 2f,
            NPC.scale,
            SpriteEffects.None,
            0
        );
        spriteBatch.EndBegin(shaderSnapshot);
        
        var snapshot = spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive, SamplerState = SamplerState.PointClamp});
        spriteBatch.Draw(
            glowTex,
            NPC.Center + offset - screenPos,
            new Rectangle(NPC.frame.X, NPC.frame.Y, NPC.frame.Width + 24, NPC.frame.Height),
            _glowColor,
            0f,
            new Rectangle(NPC.frame.X - 12, NPC.frame.Y, NPC.frame.Width + 24, NPC.frame.Height).Size() / 2f,
            1.0f,
            SpriteEffects.None,
            0
        );
        spriteBatch.EndBegin(snapshot);
        
        return false;
    }

    public override bool CheckDead() {
        if(_dead) return true;

        _dead = true;
        _deadTimer = 0;
        
        NPC.dontTakeDamage = true;
        NPC.life = 1;

        return false;
    }

    public override void FindFrame(int frameHeight) {
        if(_dead) {
            NPC.frameCounter += 0.20f;
            if (NPC.frameCounter >= 17)
                NPC.frameCounter = 17;
        }
        else {
            NPC.frameCounter += 0.15f;
            if (NPC.frameCounter >= 3)
                NPC.frameCounter = 0;
        }
        NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
    }
}
