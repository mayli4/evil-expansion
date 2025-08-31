using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Terraria;

namespace EvilExpansionMod.Utilities;
public static partial class Helper {
    public readonly static string PlaceholderTextureKey = "Terraria/Images/Item_0";

    public static Vector2 InitialVelocityRequiredToHitPosition(Vector2 initialPosition, Vector2 targetPosition, float gravity, float initialSpeed, bool secondAngle = false) {
        Vector2 localTargetPosition = targetPosition - initialPosition;
        localTargetPosition.X = MathF.Abs(localTargetPosition.X);
        float randomShit = MathF.Pow(initialSpeed, 4) - gravity * (gravity * MathF.Pow(localTargetPosition.X, 2) + 2f * localTargetPosition.Y * MathF.Pow(initialSpeed, 2));
        float angle = MathF.Atan(
            (MathF.Pow(initialSpeed, 2) + MathF.Sqrt(Math.Max(randomShit, 0f)) * (secondAngle ? -1 : 1))
            / (gravity * localTargetPosition.X)
        );

        Vector2 velocity = angle.ToRotationVector2() * initialSpeed;
        velocity.Y = -velocity.Y;
        velocity.X *= MathF.Sign(targetPosition.X - initialPosition.X);

        return velocity;
    }

    public static void ForEachNPCInRange(Vector2 position, float range, Action<NPC> action) {
        for(int i = 0; i < Main.maxNPCs; i++) {
            NPC npc = Main.npc[i];
            if(npc is null || !npc.active || !npc.Hitbox.Intersects(position, range)) {
                continue;
            }

            action(npc);
        }
    }

    public static bool HoleAtPosition(NPC npc, float xPosition) {
        int tileWidth = npc.width / 16;
        xPosition = (int)(xPosition / 16f) - tileWidth;
        if(npc.velocity.X > 0)
            xPosition += tileWidth;

        int tileY = (int)((npc.position.Y + npc.height) / 16f);
        for(int y = tileY; y < tileY + 2; y++) {
            for(int x = (int)xPosition; x < xPosition + tileWidth; x++) {
                if(Main.tile[x, y].HasTile)
                    return false;
            }
        }

        return true;
    }

    public static bool SAT(ReadOnlySpan<Vector2> p1, ReadOnlySpan<Vector2> p2) {
        if(!CheckFirst(p1, p2)) return false;
        return CheckFirst(p2, p1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CheckFirst(ReadOnlySpan<Vector2> p1, ReadOnlySpan<Vector2> p2) {
            for(var i = 0; i < p1.Length; i++) {
                var a = p1[i];
                var b = p1[(i + 1) % p1.Length];

                var normal = new Vector2(b.Y - a.Y, a.X - b.X);
                var (minP1, maxP1) = MinMaxProjection(p1, normal);
                var (minP2, maxP2) = MinMaxProjection(p2, normal);
                if(maxP1 < minP2 || maxP2 < minP1) return false;
            }

            return true;

            static (float, float) MinMaxProjection(ReadOnlySpan<Vector2> points, Vector2 normal) {
                var value0 = points[0].X * normal.X + points[0].Y * normal.Y;
                var min = value0;
                var max = value0;
                for(var i = 1; i < points.Length; i++) {
                    var value = points[i].X * normal.X + points[i].Y * normal.Y;
                    if(value < min) min = value;
                    if(value > max) max = value;
                }

                return (min, max);
            }
        }
    }
}
