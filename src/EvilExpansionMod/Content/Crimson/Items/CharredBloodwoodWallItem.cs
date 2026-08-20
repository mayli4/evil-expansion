using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Crimson.Tiles;

namespace EvilExpansionMod.Content.Crimson;

internal class CharredBloodwoodWall : ModWall {
    public override string Texture => Assets.Images.Crimson.Tiles.CharredBloodwoodWall.KEY;
    public override void SetStaticDefaults() {

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Blood;

        AddMapEntry(new Color(48, 18, 15));
    }
}
internal sealed class CharredBloodwoodWallItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodWallItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 400;
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<CharredBloodwoodWall>());
        Item.width = 16;
        Item.height = 16;
        Item.value = Item.sellPrice();

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.rare = ItemRarityID.White;
    }
    public override void AddRecipes() {
        CreateRecipe(4)
            .AddIngredient<CharredBloodwoodItem>(1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}