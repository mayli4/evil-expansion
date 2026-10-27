using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Corruption;

public class CharredBloodwoodBeam : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.CharredBloodwoodBeamTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = false;
        TileID.Sets.IsBeam[Type] = true;

        AddMapEntry(new Color(97, 14, 14));
    }
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        height = 18;
    }
}
public class CharredBloodwoodBeamItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.CharredBloodwoodBeamItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CharredBloodwoodBeam>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 0;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.ResearchUnlockCount = 50;
    }
    public override void AddRecipes() {
        CreateRecipe(2)
            .AddIngredient(ModContent.ItemType<CharredBloodwoodItem>(), 1)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}