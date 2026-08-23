using EvilExpansionMod.Content.Corruption;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class CartilageOreTile : ModTile {
    public override bool CanExplode(int i, int j) {
        return false;
    }
    public override string Texture => Assets.Images.Crimson.Tiles.CartilageOreTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileOreFinderPriority[Type] = 450;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoSunLight[Type] = false;

        TileID.Sets.Ore[Type] = true;

        DustType = DustID.CrimtaneWeapons;
        HitSound = SoundID.Tink;
        Main.tileSpelunker[Type] = true;

        MineResist = 5f;
        MinPick = 110;

        Main.tileMerge[Type][TileID.Hellstone] = true;
        Main.tileMerge[TileID.Hellstone][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<PolypOreTile>()] = true;
        Main.tileMerge[ModContent.TileType<PolypOreTile>()][Type] = true;

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

        AddMapEntry(new Color(140, 83, 14), CreateMapEntryName());

        TileLoader.RegisterConversion(TileID.Hellstone, BiomeConversionID.Crimson, (i, j, type, _) =>
        {
            WorldGen.ConvertTile(i, j, Type);
            return false;
        });
    }
    
    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        => WorldGen.TileMergeAttempt(-2, ModContent.TileType<CrimsonAsh>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        var tile = Main.tile[i, j];
        if(fail == false && j > 700) {
            tile.LiquidType = LiquidID.Lava;
            tile.LiquidAmount = 255;
        }
        Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.Blood);
        Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.BloodWater);
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.Hellstone);
                return;
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CartilageOreTile>());
                return;

        }
    }

    public override void FloorVisuals(Player player) {
        if(!player.fireWalk) {
            player.AddBuff(BuffID.Burning, 10, false);
        }
        player.AddBuff(BuffID.Ichor, 10, false);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        r = 0.124f;
        g = 0f;
        b = 0.22f;
    }
    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
        if(Main.rand.NextBool(200)) {
            Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.CrimtaneWeapons);
            Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.Blood);
            Dust.NewDust(new Vector2(i * 16, j * 16), 5, 5, DustID.BloodWater);
        }
    }

}
public class CartilageOreItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.CartilageOreItem.KEY;

    public override void SetStaticDefaults() {
        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ModContent.ItemType<PolypOreItem>(), 1);
    }
    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.Red.ToVector3() * 0.2f * Main.essScale);
    }
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CartilageOreTile>());
        Item.width = 17;
        Item.height = 19;
        Item.value = Item.sellPrice(0, 0, 3);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        Item.rare = ItemRarityID.Orange;
    }
}
