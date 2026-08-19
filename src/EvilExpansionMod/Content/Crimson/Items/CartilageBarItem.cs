using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Crimson;

public class CartilageBarItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CartilageBarItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
        ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CartilageBarTile>());
        Item.width = 20;
        Item.height = 20;
        Item.value = 30000;

        Item.maxStack = Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.Red.ToVector3() * 0.3f * Main.essScale);
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CartilageOreItem>(3)
            .AddIngredient<PusClumpItem>()
            .AddTile(TileID.Furnaces)
            .Register();
    }
}

internal sealed class CartilageBarTile : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.CartilageBarTile.KEY;
    public override void SetStaticDefaults() {
        RegisterItemDrop(ModContent.ItemType<CartilageBarItem>());

        Main.tileShine[Type] = 1100;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);
        
        DustType = DustID.IchorTorch;

        AddMapEntry(new Color(203, 10, 26), Language.GetText("MapObject.MetalBar"));
    }
}

