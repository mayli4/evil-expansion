using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.Tiles;

namespace EvilExpansionMod.Content.Corruption.Items._CorruptAshwoodTiles;

public class CorruptAshwoodPlatformItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodPlatformItem.KEY;

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 200;
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CorruptAshwoodPlatformTile>());
        Item.width = 20;
        Item.height = 20;
        Item.value = Item.sellPrice();

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes() {
        CreateRecipe(2)
            .AddIngredient<CorruptAshwoodItem>(1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
internal sealed class CorruptAshwoodPlatformTile : ModTile {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshwoodPlatform.KEY;
    public override void SetStaticDefaults() {
    // Properties
    Main.tileLighted[Type] = true;
    Main.tileFrameImportant[Type] = true;
    Main.tileSolidTop[Type] = true;
    Main.tileSolid[Type] = true;
    Main.tileNoAttach[Type] = true;
    Main.tileTable[Type] = true;
    Main.tileLavaDeath[Type] = false;
    TileID.Sets.Platforms[Type] = true;
    TileID.Sets.DisableSmartCursor[Type] = true;

    //TileID.Sets.RoomNeeds.CountsAsDoor[Type] = true; (This, does not even work)

    AddMapEntry(new Color(103, 104, 138));

    DustType = DustID.Corruption;
    AdjTiles = [TileID.Platforms];
    VanillaFallbackOnModDeletion = TileID.Platforms;

    // Placement
    TileObjectData.newTile.CoordinateHeights = [16];
    TileObjectData.newTile.CoordinateWidth = 16;
    TileObjectData.newTile.CoordinatePadding = 2;
    TileObjectData.newTile.StyleHorizontal = true;
    TileObjectData.newTile.StyleMultiplier = 27;
    TileObjectData.newTile.StyleWrapLimit = 27;
    TileObjectData.newTile.UsesCustomCanPlace = false;
    TileObjectData.newTile.LavaDeath = true;
    TileObjectData.addTile(Type);
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}