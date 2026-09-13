using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption.Items;

[Autoload(Side = ModSide.Client)]
internal class CurseknightFireParticlePlayer : ModPlayer {
    public List<CurseknightFireParticle> Particles { get; } = [];

    public override void PostUpdate() {
        for(var i = 0; i < Particles.Count; i++) {
            var particle = Particles[i];

            particle.TimeLeft--;
            if(particle.TimeLeft <= 0) {
                Particles.RemoveAt(i--);
                continue;
            }

            particle.Velocity.Y -= 0.135f;
            particle.Velocity *= 0.96f;

            particle.Position += particle.Velocity;
            Particles[i] = particle;
        }

        if(CurseknightsHelm.HelmExploded) {
            for(var i = 0; i < 2; i++) {
                Particles.Add(new()
                {
                    Position = Player.Center - Vector2.UnitY * 11f + Main.rand.NextVector2Unit() * 7.5f,
                    Velocity = Player.velocity * 0.5f,
                    Scale = Main.rand.NextFloat(0.75f, 1.5f),
                    Alpha = Main.rand.NextFloat(0.4f, 0.6f),
                    Rotation = Main.rand.NextFloat(MathHelper.Pi),
                    TimeLeft = CurseknightFireParticle.MaxTimeLeft,
                });
            }
        }
    }
}

public struct CurseknightFireParticle {
    public const int MaxTimeLeft = 35;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float Alpha;
    public float Scale;
    public int TimeLeft;
}
