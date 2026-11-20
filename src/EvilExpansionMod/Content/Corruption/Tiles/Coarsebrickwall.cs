using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Corruption.Tiles;

namespace EvilExpansionMod.Content.Corruption.Tiles;

internal class CoarseBrickWall : ModWall {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickWall.KEY;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Demonite;

        AddMapEntry(new Color(10, 27, 34));
    }
}

internal sealed class CoarseBrickWallItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.CoarseBrickWallItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<CoarseBrickWall>());
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