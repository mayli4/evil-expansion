using Microsoft.Xna.Framework;
using System.Linq;

namespace EvilExpansionMod.Common;

internal class VerletChain {
    public Vector2[] Positions { get; init; }

    public int Iterations { get; set; } = 4;
    public Vector2 Gravity { get; set; } = Vector2.UnitY * 0.4f;
    public float Damping { get; set; } = 0.95f;

    private readonly Vector2[] _oldPositions;
    private readonly float[] _distances;

    public VerletChain(Vector2 initialPosition, float[] distances) {
        Positions = [.. Enumerable.Repeat(initialPosition, distances.Length + 1)];
        _oldPositions = [.. Enumerable.Repeat(initialPosition, distances.Length + 1)];
        _distances = distances;
    }

    public void Update(Vector2? origin, Vector2? target) {
        for(var i = 0; i < Positions.Length; i++) {
            var velocity = Positions[i] - _oldPositions[i];

            _oldPositions[i] = Positions[i];
            Positions[i] += velocity * Damping + Gravity;
        }

        if(origin is Vector2 o) Positions[0] = o;
        if(target is Vector2 t) Positions[^1] = t;

        for(var it = 0; it < Iterations; it++) {
            for(var i = 0; i < Positions.Length - 1; i++) {
                var isAFixed = origin is not null && i == 0;
                var isBFixed = target is not null && i == Positions.Length - 2;

                if(isAFixed && isBFixed) continue;

                var delta = Positions[i + 1] - Positions[i];
                var distance = delta.Length();
                if(distance < 0.0001f) continue;

                var difference = (distance - _distances[i]) / distance;
                var offset = delta * difference;

                if(isAFixed) {
                    Positions[i + 1] -= offset;
                    continue;
                }

                if(isBFixed) {
                    Positions[i] += offset;
                    continue;
                }

                Positions[i] += offset * 0.5f;
                Positions[i + 1] -= offset * 0.5f;
            }
        }
    }
}
