using EvilExpansionMod.Content.Crimson;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Banners;

internal sealed class StinkgrubBannerTile : ModBannerTile {
    public override string Texture => Assets.Images.Banners.StinkgrubBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<StinkgrubNPC>()] = true;
    }
}

internal class StinkgrubBannerItem : ModItem {
    public override string Texture => Assets.Images.Banners.StinkgrubBannerItem.KEY;

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
    public override string Texture => Assets.Images.Banners.PusImpBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<PusImpNPC>()] = true;
    }
}

internal class PusImpBannerItem : ModItem {
    public override string Texture => Assets.Images.Banners.PusImpBannerItem.KEY;

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
    public override string Texture => Assets.Images.Banners.ThoughtfulCultistBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<ThoughtfulCultistNPC>()] = true;
    }
}

internal class ThoughtfulCultistBannerItem : ModItem {
    public override string Texture => Assets.Images.Banners.ThoughtfulCultistBannerItem.KEY;

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
    public override string Texture => Assets.Images.Banners.MarrowEyeBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<MarrowEyeNPC>()] = true;
    }
}

internal class MarrowEyeBannerItem : ModItem {
    public override string Texture => Assets.Images.Banners.MarrowEyeBannerItem.KEY;

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
    public override string Texture => Assets.Images.Banners.LanternBatBannerTile.KEY;

    public override void NearbyEffects(int i, int j, bool closer) {
        Main.SceneMetrics.hasBanner = true;
        Main.SceneMetrics.NPCBannerBuff[ModContent.NPCType<LanternBatNPC>()] = true;
    }
}

internal class LanternBatBannerItem : ModItem {
    public override string Texture => Assets.Images.Banners.LanternBatBannerItem.KEY;

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