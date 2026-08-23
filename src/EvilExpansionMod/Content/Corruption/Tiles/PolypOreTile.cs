using EvilExpansionMod.Content.Crimson;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class PolypOreTile : ModTile {
    public override bool CanExplode(int i, int j) {
        return false;
    }
    public override string Texture => Assets.Images.Corruption.Tiles.PolypOre.KEY;

    public override void SetStaticDefaults() {
        Main.tileOreFinderPriority[Type] = 450;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoSunLight[Type] = false;

        TileID.Sets.Ore[Type] = true;

        DustType = DustID.PurpleCrystalShard;
        HitSound = SoundID.Tink;
        Main.tileSpelunker[Type] = true;

        MineResist = 5f;
        MinPick = 110;
        
        Main.tileMerge[Type][TileID.Hellstone] = true;
        Main.tileMerge[TileID.Hellstone][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CartilageOreTile>()] = true;
        Main.tileMerge[ModContent.TileType<CartilageOreTile>()][Type] = true;

        Main.tileMerge[Type][TileID.Ash] = true;
        Main.tileMerge[TileID.Ash][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CrimsonAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAsh>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CrimsonAshGrass>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAshGrass>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CorruptAsh>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<OvergrownCorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<OvergrownCorruptAsh>()][Type] = true;
        
        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(147, 88, 201), CreateMapEntryName());

        TileLoader.RegisterConversion(TileID.Hellstone, BiomeConversionID.Corruption, ConvertToCorruption);
    }
    
    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        => WorldGen.TileMergeAttempt(-2, ModContent.TileType<CorruptAsh>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        var tile = Main.tile[i, j];
        if (fail == false && j > 700) {
            tile.LiquidType = LiquidID.Lava;
            tile.LiquidAmount = 255;
        }
        Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.CursedTorch);
    }

    public override void FloorVisuals(Player player) {
        if(!player.fireWalk) {
            player.AddBuff(BuffID.Burning, 10, false);
        }
        player.AddBuff(BuffID.CursedInferno, 10, false);
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
    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
        if (Main.rand.NextBool(200)){
            Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.Demonite);
            Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.CursedTorch);
        }
    }
}

public class PolypOreItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.PolypOreItem.KEY;

    public override void SetStaticDefaults() {
        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ModContent.ItemType<CartilageOreItem>(), 1);
    }
    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.YellowGreen.ToVector3() * 0.3f * Main.essScale);
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