using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Tiles.Corruption;

public class CorruptHellTree : ModTree {

    public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetStaticDefaults() {
        GrowsOnTileId = [ModContent.TileType<OvergrownCorruptAsh>()];
    }

    public override Asset<Texture2D> GetTexture() {
        return Assets.Assets.Textures.Tiles.Corruption.CorruptHellTree;
    }

    public override int SaplingGrowthType(ref int style) {
        style = 0;
        return ModContent.TileType<CorruptHellTreeSapling>();
    }

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
        topTextureFrameWidth = 196;
        topTextureFrameHeight = 144;
        xoffset = 36;
        floorY = 2;
    }

    public override Asset<Texture2D> GetBranchTextures() => Assets.Assets.Textures.Tiles.Corruption.CorruptHellTreeBranches;

    public override Asset<Texture2D> GetTopTextures() => Assets.Assets.Textures.Tiles.Corruption.CorruptHellTreeTops;

    public override int DropWood() => ItemID.Shadewood;

    public override int TreeLeaf() => GoreID.TreeLeaf_Corruption;

    public override bool Shake(int x, int y, ref bool createLeaves) {
        Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ItemID.Shadewood);
        return false;
    }
}

public class CorruptHellTreeSapling : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_CorruptHellTreeSapling;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<OvergrownCorruptAsh>()];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(106, 103, 126), Language.GetText("MapObject.Sapling"));

        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = DustID.Shadewood;

        AdjTiles = [TileID.Saplings];
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        num = fail ? 1 : 3;
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.GrowTree(i, j);
        WorldGen.TreeGrowFXCheck(i, j);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
        if(i % 2 == 0) {
            effects = SpriteEffects.FlipHorizontally;
        }
    }
}