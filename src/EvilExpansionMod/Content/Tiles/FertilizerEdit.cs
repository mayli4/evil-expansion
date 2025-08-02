using EvilExpansionMod.Content.Tiles.Corruption;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles;

public class FertilizerEdit : GlobalProjectile {
    public override void AI(Projectile projectile) {
        if (projectile.aiStyle == 6) {
            bool flag23 = projectile.type == 1019;
            bool flag34 = Main.myPlayer == projectile.owner;
            if (flag23) {
                flag34 = Main.netMode != 1;
            }
            if (flag34 && (flag23)) {
                int num988 = (int)(projectile.position.X / 16f) - 1;
                int num999 = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
                int num1010 = (int)(projectile.position.Y / 16f) - 1;
                int num1021 = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;
                if (num988 < 0) {
                    num988 = 0;
                }
                if (num999 > Main.maxTilesX) {
                    num999 = Main.maxTilesX;
                }
                if (num1010 < 0) {
                    num1010 = 0;
                }
                if (num1021 > Main.maxTilesY) {
                    num1021 = Main.maxTilesY;
                }
                Vector2 vector57 = default(Vector2);
                for (int num1032 = num988; num1032 < num999; num1032++) {
                    for (int num1043 = num1010; num1043 < num1021; num1043++) {
                        vector57.X = num1032 * 16;
                        vector57.Y = num1043 * 16;
                        if (!(projectile.position.X + (float)projectile.width > vector57.X) || !(projectile.position.X < vector57.X + 16f) || !(projectile.position.Y + (float)projectile.height > vector57.Y) || !(projectile.position.Y < vector57.Y + 16f) || !Main.tile[num1032, num1043].HasTile) {
                            continue;
                        }
                        Tile tile = Main.tile[num1032, num1043];
                        if (tile.TileType == ModContent.TileType<HellEbontreeSapling>()) {
                            if (Main.remixWorld && num1043 >= (int)Main.worldSurface - 1 && num1043 < Main.maxTilesY - 20) {
                                HellEbontreeSapling.AttemptToGrowTreeFromSapling(num1032, num1043);
                            }
                            HellEbontreeSapling.AttemptToGrowTreeFromSapling(num1032, num1043);
                        }
                    }
                }
            }
        }
    }
}