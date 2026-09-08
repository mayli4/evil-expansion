using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CoarseBrick : ModTile {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        TileID.Sets.BlockMergesWithMergeAllBlockOverride[Type] = true;
        Main.tileMerge[Type][TileID.Ash] = true;
        Main.tileMerge[TileID.Ash][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CrimsonAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAsh>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CorruptAsh>()][Type] = true;
        Main.tileMerge[Type][TileID.ObsidianBrick] = true;
        Main.tileMerge[TileID.ObsidianBrick][Type] = true;
        Main.tileMerge[Type][TileID.HellstoneBrick] = true;
        Main.tileMerge[TileID.HellstoneBrick][Type] = true;
        Main.tileMerge[Type][TileID.Dirt] = true;
        Main.tileMerge[TileID.Dirt][Type] = true;
        Main.tileMerge[Type][TileID.Grass] = true;
        Main.tileMerge[TileID.Grass][Type] = true;

        AddMapEntry(new Color(100, 37, 62));
    }
}
public class CoarseBrickItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CoarseBrick>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 5;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.ResearchUnlockCount = 100;
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.StoneBlock, 1)
            .AddIngredient(ModContent.ItemType<CorruptAshItem>(), 1)
            .AddTile(TileID.Furnaces)
            .Register();
    }
}