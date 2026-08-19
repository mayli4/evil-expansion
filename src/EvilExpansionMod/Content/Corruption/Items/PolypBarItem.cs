using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Corruption;

public class PolypBarItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.PolypBarItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
        ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<PolypBarTile>());
        Item.width = 20;
        Item.height = 20;
        Item.value = 30000;

        Item.maxStack = Item.CommonMaxStack;

        Item.rare = ItemRarityID.Orange;
    }
    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.YellowGreen.ToVector3() * 0.5f * Main.essScale);
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<PolypOreItem>(3)
            .AddIngredient<RawShadowScalesItem>()
            .AddTile(TileID.Furnaces)
            .Register();
    }
}

internal sealed class PolypBarTile : ModTile {
    public override string Texture => Assets.Images.Corruption.Tiles.PolypBarTile.KEY;
    public override void SetStaticDefaults() {
        RegisterItemDrop(ModContent.ItemType<PolypBarItem>());

        Main.tileShine[Type] = 1100;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.addTile(Type);
        
        DustType = DustID.PurpleTorch;

        AddMapEntry(new Color(149, 59, 185), Language.GetText("MapObject.MetalBar"));
    }
}
