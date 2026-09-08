using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class MatteBrick : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickTile.KEY;

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

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }

}
public class MatteBrickItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<MatteBrick>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 5;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.StoneBlock, 1)
            .AddIngredient(ModContent.ItemType<CrimsonAshItem>(), 1)
            .AddTile(TileID.Furnaces)
            .Register();
    }
}