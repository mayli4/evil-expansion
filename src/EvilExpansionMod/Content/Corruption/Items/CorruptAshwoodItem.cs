using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.Tiles;

namespace EvilExpansionMod.Content.Corruption;

public class CorruptAshwoodItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 100;
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(ModContent.ItemType<CorruptAshwoodItem>());
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CorruptAshwoodTile>());
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
    }
}
internal sealed class CorruptAshwoodTile : ModTile {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshwoodTile.KEY;
    public override void SetStaticDefaults() {

        Main.tileSolid[Type] = true;
        Main.tileBrick[Type] = true;

        DustType = DustID.Blood;

        AddMapEntry(new Color(118, 18, 15));
    }
}