using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;


public class PusBottleNPC : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_PusBottle;

    private int ParentNPCID => (int)NPC.ai[0];
    private ref float IsDetached => ref NPC.ai[1];

    private const int spew_interval = 60 * 2;
    private const int detached_lifetime = 60 * 10;

    public override void SetDefaults() {
        NPC.width = 16;
        NPC.height = 16;
        NPC.aiStyle = -1;
        NPC.friendly = false;
        NPC.damage = 0;
        NPC.lifeMax = 100;
        NPC.knockBackResist = 0f;
        NPC.value = 0f;

        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.HitSound = SoundID.NPCHit3;
        NPC.DeathSound = SoundID.Shatter;
    }

    public override void AI() {
        NPC parentNPC = Main.npc[ParentNPCID];

        if (IsDetached == 0) {
            if (parentNPC.active && parentNPC.type == ModContent.NPCType<StinkgrubNPC>())
            {
                StinkgrubNPC grub = (StinkgrubNPC)parentNPC.ModNPC;
                if (grub.IsPusCarrier) {
                    NPC.Center = parentNPC.Center + new Vector2(parentNPC.spriteDirection * -5 * parentNPC.scale, -parentNPC.height / 2 * parentNPC.scale);
                    NPC.velocity = parentNPC.velocity;
                    NPC.gfxOffY = parentNPC.gfxOffY;

                    NPC.ai[2]++;
                    if (NPC.ai[2] >= spew_interval) {
                        FirePus();
                        NPC.ai[2] = 0;
                    }
                    NPC.timeLeft = 2;
                }
                else {
                    IsDetached = 1;
                    NPC.timeLeft = detached_lifetime;
                    NPC.netUpdate = true;
                }
            }
            else {
                IsDetached = 1;
                NPC.timeLeft = detached_lifetime;
                NPC.netUpdate = true;
            }
        }
        else {
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.velocity.Y += 0.2f;
            if (NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;

            NPC.velocity.X *= 0.98f;
            
            if (NPC.collideX || NPC.collideY) {
                NPC.StrikeInstantKill();
            }
        }
    }

    private void FirePus() {
        var amount = Main.rand.Next(2, 4);

        for(int i = 0; i < amount; i++) {
            float speed = Main.rand.NextFloat(4f, 7f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -1f).SafeNormalize(Vector2.UnitY) * speed; 
            
            Projectile.NewProjectile(
                NPC.GetSource_FromThis(),
                NPC.Center - new Vector2(0, 130),
                velocity,
                ModContent.ProjectileType<PusGlob>(),
                (int)(ParentNPCID != -1 && Main.npc[ParentNPCID].active ? Main.npc[ParentNPCID].damage * 0.75f : 10),
                0.5f,
                Main.myPlayer
            );
        }
        
        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = Main.rand.NextFloat(0.5f, 0.8f) }, NPC.Center);
    }

    public override void OnKill() {
        if (Main.netMode == NetmodeID.Server) return;

        int pusImpCount = Main.rand.Next(2, 4);
        for (int i = 0; i < pusImpCount; i++) {
            NPC.NewNPC(
                NPC.GetSource_FromThis(),
                (int)NPC.Center.X + Main.rand.Next(-10, 10),
                (int)NPC.Center.Y + Main.rand.Next(-10, 10),
                ModContent.NPCType<PusImpNPC>()
            );
        }

        for (int i = 0; i < 15; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Glass, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 0, default, 1.2f);
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ichor, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 0, default, 0.8f);
        }
        SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
    }
}