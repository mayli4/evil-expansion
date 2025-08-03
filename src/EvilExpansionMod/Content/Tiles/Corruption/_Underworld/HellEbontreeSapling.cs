using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Tiles.Corruption;

public class HellEbontreeSapling : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptHellTreeSapling;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<OvergrownCorruptAsh>() };
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(106, 103, 126), Language.GetText("MapObject.Sapling"));
        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        AdjTiles = new int[] { TileID.Saplings };
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = fail ? 1 : 3;
    }

    public override void RandomUpdate(int i, int j) {
        AttemptToGrowTreeFromSapling(i, j);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
        if(i % 2 == 1) {
            effects = SpriteEffects.FlipHorizontally;
        }
    }

    public static bool AttemptToGrowTreeFromSapling(int x, int y) {
        if(Main.netMode == NetmodeID.MultiplayerClient) {
            return false;
        }
        if(!WorldGen.InWorld(x, y, 2)) {
            return false;
        }
        Tile tile = Main.tile[x, y];
        if(tile == null || !tile.HasTile) {
            return false;
        }
        bool flag = HellEbontree.GrowModdedTreeWithSettings(x, y, HellEbontree.Tree_Cream);
        if(flag && WorldGen.PlayerLOS(x, y)) {
            GrowHellEbonTreeFXCheck(x, y);
        }
        return flag;
    }

    public static void GrowHellEbonTreeFXCheck(int x, int y) {
        int treeHeight = 1;
        for(int num = -1; num > -100; num--) {
            Tile tile = Main.tile[x, y + num];
            if(!tile.HasTile || !TileID.Sets.GetsCheckedForLeaves[tile.TileType]) {
                break;
            }
            treeHeight++;
        }
        for(int i = 1; i < 5; i++) {
            Tile tile2 = Main.tile[x, y + i];
            if(tile2.HasTile && TileID.Sets.GetsCheckedForLeaves[tile2.TileType]) {
                treeHeight++;
                continue;
            }
            break;
        }
        if(treeHeight > 0) {
            if(Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.SpecialFX, -1, -1, null, 1, x, y, treeHeight, 1);
            }
            if(Main.netMode == NetmodeID.SinglePlayer) {
                //WorldGen.TreeGrowFX(x, y, treeHeight, 1);
            }
        }
    }
}