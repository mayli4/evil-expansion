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
using static EvilExpansionMod.Core.AssetReferences.Assets.Images.Crimson.Tiles;

namespace EvilExpansionMod.Content.Crimson.Tiles;

internal class MatteBrickWall : ModWall {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickWall.KEY;
    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;

        Main.tileBlockLight[Type] = true;

        DustType = DustID.Blood;

        AddMapEntry(new Color(42, 17, 20));
    }
}

internal sealed class MatteBrickWallItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.MatteBrickWallItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<MatteBrickWall>());
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