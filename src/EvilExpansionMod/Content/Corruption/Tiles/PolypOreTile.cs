using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class PolypOreTile : ModTile {
    public override string Texture => Assets.Textures.Tiles.Corruption.PolypOre.KEY;

    public override void SetStaticDefaults() {
        Main.tileOreFinderPriority[Type] = 450;
        Main.tileBlockLight[Type] = false;
        Main.tileSolid[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoSunLight[Type] = false;

        TileID.Sets.Ore[Type] = true;

        DustType = DustID.Demonite;
        MinPick = 65;
        HitSound = SoundID.Tink;
        Main.tileSpelunker[Type] = true;

        MineResist = 2f;
        MinPick = 110;

        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CorruptAsh>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<OvergrownCorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<OvergrownCorruptAsh>()][Type] = true;

        AddMapEntry(new Color(147, 88, 201), CreateMapEntryName());

        TileLoader.RegisterConversion(TileID.Hellstone, BiomeConversionID.Corruption, ConvertToCorruption);
    }

    public override void FloorVisuals(Player player) {
        if(!player.fireWalk) {
            player.AddBuff(BuffID.Burning, 10, false);
        }
    }

    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.Hellstone);
                return;
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<PolypOreTile>());
                return;

        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        r = 0.24f;
        g = 0.246f;
        b = 0.42f;
    }

    public override void RandomUpdate(int i, int j) {
        Dust.NewDust(new Vector2(i, j), 5, 5, DustID.Demonite);
    }
}

public class PolypOreItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Corruption.PolypOreItem.KEY;

    public override void SetStaticDefaults() {
        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ModContent.ItemType<CartilageOreItem>(), 1);
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<PolypOreTile>());
        Item.width = 16;
        Item.height = 16;
        Item.value = Item.sellPrice(silver: 3);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.rare = ItemRarityID.Orange;
    }
}