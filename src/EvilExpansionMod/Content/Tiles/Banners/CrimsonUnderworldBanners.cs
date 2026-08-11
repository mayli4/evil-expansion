using EvilExpansionMod.Content.NPCs.Crimson;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Banners;

internal sealed class StinkgrubBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_StinkgrubBannerTile;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<StinkgrubNPC>()] = true;
    }
}

internal class StinkgrubBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_StinkgrubBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<StinkgrubBannerTile>();
        Item.width = 12;
        Item.height = 12;

        Item.rare = ItemRarityID.Blue;
    }
}

internal sealed class PusImpBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_PusImpBannerTile;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<PusImpNPC>()] = true;
    }
}

internal class PusImpBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_PusImpBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<PusImpBannerTile>();
        Item.width = 12;
        Item.height = 12;

        Item.rare = ItemRarityID.Blue;
    }
}

internal sealed class ThoughtfulCultistBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_ThoughtfulCultistBannerTile;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<ThoughtfulCultistNPC>()] = true;
    }
}

internal class ThoughtfulCultistBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_ThoughtfulCultistBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<ThoughtfulCultistBannerTile>();
        Item.width = 12;
        Item.height = 12;

        Item.rare = ItemRarityID.Blue;
    }
}

internal sealed class MarrowEyeBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_MarrowEyeBannerTile;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<MarrowEyeNPC>()] = true;
    }
}

internal class MarrowEyeBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_MarrowEyeBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<MarrowEyeBannerTile>();
        Item.width = 12;
        Item.height = 12;

        Item.rare = ItemRarityID.Blue;
    }
}

internal sealed class LanternBatBannerTile : ModBannerTile {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_LanternBatBannerTile;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<LanternBatNPC>()] = true;
    }
}

internal class LanternBatBannerItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Banners.KEY_LanternBatBannerItem;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<LanternBatBannerTile>();
        Item.width = 12;
        Item.height = 12;

        Item.rare = ItemRarityID.Blue;
    }
}