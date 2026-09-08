using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Crimson.Tiles;

namespace EvilExpansionMod.Content.Crimson.Tiles;

internal class Mattebrickwall : ModWall {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickWall.KEY;
}

internal sealed class MattebrickwallItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickWallItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<Mattebrickwall>());
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
            .AddIngredient(ModContent.ItemType<MatteBrickItem>(), 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}