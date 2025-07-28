using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Banners;

internal sealed class DevilOWarBannerTile : ModBannerTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_DevilOWarBannerTile;
}

internal sealed class TerrorbatBannerTile : ModBannerTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_TerrorbatBannerTile;
}

internal sealed class CursehoundBannerTile : ModBannerTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_CursehoundBannerTile;
}

internal sealed class CursedSpiritBannerTile : ModBannerTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_CursedSpiritBannerTile;
}

internal sealed class EffigyBannerTile : ModBannerTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_EffigyBannerTile;
}

public class DevilOWarBannerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_DevilOWarBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<DevilOWarBannerTile>();
        Item.width = 12;
        Item.height = 12;
    }
}

public class TerrorbatBannerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_TerrorbatBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<TerrorbatBannerTile>();
        Item.width = 12;
        Item.height = 12;
    }
}

public class CursehoundBannerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_CursehoundBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<CursehoundBannerTile>();
        Item.width = 12;
        Item.height = 12;
    }
}

public class CursedSpiritBannerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_CursedSpiritBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<CursedSpiritBannerTile>();
        Item.width = 12;
        Item.height = 12;
    }
}

public class EffigyBannerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Banners.KEY_EffigyBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<EffigyBannerTile>();
        Item.width = 12;
        Item.height = 12;
    }
}