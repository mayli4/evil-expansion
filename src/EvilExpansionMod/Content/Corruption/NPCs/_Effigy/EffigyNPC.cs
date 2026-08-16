using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class EffigyNPC : ModNPC {
    public override string Texture => Assets.Textures.NPCs.Corruption.Effigy.EffigyNPC.KEY;

    private bool dead;
    private int deadTimer;
    private byte spawnedSprits;
    
    public const int DEATH_TIME = 5 * 60;

    private Color glowColor = new Color(230, 254, 6);
    
    private Vector2 squashStretch = Vector2.One;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 21;
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
        NPC.hide = true;
        NPC.behindTiles = true;

        NPC.HitSound = Assets.Sounds.EffigyHit.Asset
            .WithPitchVariance(0.5f)
            .WithPitchOffset(-0.3f);

        SpawnModBiomes = [ModContent.GetInstance<UnderworldCorruptionBiome>().Type];

        NPC.buffImmune[BuffID.CursedInferno] = true;
        NPC.buffImmune[BuffID.OnFire] = true;
        NPC.lavaImmune = true;

        Banner = NPC.type;
        BannerItem = ModContent.ItemType<EffigyBannerItem>();
    }

    public override void DrawBehind(int index) {
        Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return spawnInfo.Player.InModBiome<UnderworldCorruptionBiome>() ? 0.1f : 0f;
    }

    public override void Load() {
        for(int j = 1; j <= 5; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Textures/Gores/EffigyGore" + j);
    }

    public override void AI() {
        squashStretch = Vector2.Lerp(squashStretch, Vector2.One, 0.1f);
        
        if(dead) {
            deadTimer++;
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

    public override void HitEffect(NPC.HitInfo hit) {
        if(Main.netMode == NetmodeID.Server || NPC.life > 0) {
            return;
        }
        
        for(int i = 1; i <= 5; i++) {
            //Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("EffigyGore" + i).Type);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var texture = TextureAssets.Npc[Type].Value;
        var glowTex = Assets.Textures.NPCs.Corruption.Effigy.EffigyNPC_Glow.Asset.Value;

        var offset = new Vector2(0, -30); //cause frame very big! yes

        var shader = Assets.Effects.Pixel.EffigyDecay.Asset.Value;

        float progValue = 1.5f;

        shader.Parameters["prog"].SetValue(progValue);
        shader.Parameters["edgeColor"].SetValue(Color.Black.ToVector3());
        shader.Parameters["ashColor"].SetValue(glowColor.ToVector3());
        shader.Parameters["noisetex"].SetValue(Assets.Textures.Sample.DissolveNoise.Asset.Value);
        shader.Parameters["sampleColor"].SetValue(drawColor.ToVector4());

        var noiseTexture = Assets.Textures.Sample.DissolveNoise.Asset.Value;
        float noiseAspect = (float)noiseTexture.Width / noiseTexture.Height;
        float frameAspect = (float)NPC.frame.Width / NPC.frame.Height;

        shader.Parameters["noiseTexelAspect"].SetValue(noiseAspect + 200);
        shader.Parameters["frameTexelAspect"].SetValue(frameAspect + 2000);
        shader.Parameters["texSize"].SetValue(new Vector2(NPC.frame.Width, NPC.frame.Height));

        //var shaderSnapshot = spriteBatch.CaptureEndBegin(new() { CustomEffect = shader });

        Vector2 drawOrigin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height);
        Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.position.Y + NPC.height) - screenPos - offset;
        Vector2 drawScale = new Vector2(NPC.scale) * squashStretch;
        
        spriteBatch.Draw(
            texture,
            drawPosition,
            NPC.frame,
            drawColor,
            0f,
            drawOrigin,
            drawScale,
            SpriteEffects.None,
            0
        );
        //spriteBatch.EndBegin(shaderSnapshot);

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