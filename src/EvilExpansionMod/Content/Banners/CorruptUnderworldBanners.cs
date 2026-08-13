using EvilExpansionMod.Content.Corruption;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Banners;

internal sealed class DevilOWarBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.DevilOWarBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<DevilOWarNPC>()] = true;
    }
}

internal sealed class TerrorbatBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.TerrorbatBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<TerrorBatNPC>()] = true;
    }
}

internal sealed class CursehoundBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.CursehoundBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<CursehoundNPC>()] = true;
    }
}

internal sealed class CursedSpiritBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.CursedSpiritBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<CursedSpiritNPC>()] = true;
    }
}

internal sealed class EffigyBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.EffigyBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<EffigyNPC>()] = true;
    }
}

public class DevilOWarBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.DevilOWarBannerItem.KEY;

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

        Item.rare = ItemRarityID.Blue;
    }
}

public class TerrorbatBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.TerrorbatBannerItem.KEY;

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

        Item.rare = ItemRarityID.Blue;
    }
}

public class CursehoundBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.CursehoundBannerItem.KEY;

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

        Item.rare = ItemRarityID.Blue;
    }
}

public class CursedSpiritBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.CursedSpiritBannerItem.KEY;

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

        Item.rare = ItemRarityID.Blue;
    }
}

public class EffigyBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.EffigyBannerItem.KEY;

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

        Item.rare = ItemRarityID.Blue;
    }
}