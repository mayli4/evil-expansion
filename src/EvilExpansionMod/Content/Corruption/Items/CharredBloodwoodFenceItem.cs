using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.Tiles;

namespace EvilExpansionMod.Content.Corruption;

internal class CorruptAshwoodFence : ModWall {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshwoodFence.KEY;
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Blood;

        AddMapEntry(new Color(67, 21, 19));
    }
}
internal sealed class CorruptAshwoodFenceItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshwoodFenceItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 400;
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<CorruptAshwoodFence>());
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
            .AddIngredient<CorruptAshwoodItem>(1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}