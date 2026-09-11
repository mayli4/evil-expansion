using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.Tiles;

namespace EvilExpansionMod.Content.Corruption.Tiles;

internal class Coarsebrickwall : ModWall {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickWall.KEY;

    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;

    }
}

internal sealed class CoarsebrickwallItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickWallItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<Coarsebrickwall>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 5;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.ResearchUnlockCount = 400;
    }
    public override void AddRecipes() {
        CreateRecipe(4)
            .AddIngredient(ModContent.ItemType<CoarseBrickItem>(), 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}