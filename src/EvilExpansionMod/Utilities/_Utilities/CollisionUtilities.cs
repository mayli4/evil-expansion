using Microsoft.Xna.Framework;
using Terraria;

namespace EvilExpansionMod.Utilities;

public static class CollisionUtilities {
    public static Vector2 NoSlopeCollision(Vector2 position, Vector2 velocity, int width, int height, bool fallThrough = false) {
        Collision.up = false;
        Collision.down = false;

        Vector2 result = velocity;

        int topLeft = (int)(position.X / 16f) - 1;
        int topRight = (int)((position.X + width) / 16f) + 2;
        int bottomLeft = (int)(position.Y / 16f) - 1;
        int bottomRight = (int)((position.Y + height) / 16f) + 2;

        int num1 = -1;
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;

        topLeft = Utils.Clamp(topLeft, 0, Main.maxTilesX - 1);
        topRight = Utils.Clamp(topRight, 0, Main.maxTilesX - 1);
        bottomLeft = Utils.Clamp(bottomLeft, 0, Main.maxTilesY - 1);
        bottomRight = Utils.Clamp(bottomRight, 0, Main.maxTilesY - 1);

        float num5 = (bottomRight + 3) * 16;

        for(int i = topLeft; i < topRight; i++) {
            for(int j = bottomLeft; j < bottomRight; j++) {
                var t = Main.tile[i, j];

                if(!WorldGen.SolidTile(i, j) && !Main.tileSolidTop[t.TileType] && !t.IsHalfBlock)
                    continue;

                Vector2 world = new Vector2(i, j) * 16;
                int num6 = 16;

                if(t.IsHalfBlock) {
                    world.Y += 8f;
                    num6 -= 8;
                }

                Vector2 displace = position + velocity;

                if(!(displace.X + width > world.X) || !(displace.X < world.X + 16f) || !(displace.Y + height > world.Y) || !(displace.Y < world.Y + num6))
                    continue;

                bool flag = false;
                bool flag2 = false;

                if(flag2)
                    continue;

                if(position.Y + height <= world.Y) //Move down
                {
                    Collision.down = true;
                    if((!(Main.tileSolidTop[t.TileType] && fallThrough) || !(velocity.Y <= 1f)) && num5 > world.Y) {
                        num3 = i;
                        num4 = j;

                        if(num6 < 16)
                            num4++;

                        if(num3 != num1 && !flag) {
                            result.Y = world.Y - (position.Y + height);
                            num5 = world.Y;
                        }
                    }
                }
                else if(position.X + width <= world.X && !Main.tileSolidTop[t.TileType]) //Move right
                {
                    num1 = i;
                    num2 = j;

                    if(num2 != num4)
                        result.X = world.X - (position.X + width);

                    if(num3 == num1)
                        result.Y = velocity.Y;
                }
                else if(position.X >= world.X + 16f && !Main.tileSolidTop[t.TileType]) //Move left
                {
                    num1 = i;
                    num2 = j;

                    if(num2 != num4)
                        result.X = world.X + 16f - position.X;

                    if(num3 == num1)
                        result.Y = velocity.Y;
                }
                else if(position.Y >= world.Y + num6 && !Main.tileSolidTop[t.TileType]) //Move up
                {
                    Collision.up = true;
                    num3 = i;
                    num4 = j;
                    result.Y = world.Y + num6 - position.Y;

                    if(num4 == num2)
                        result.X = velocity.X;
                }
            }
        }

        return result;
    }
}