using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Crimson;
public class VeinOre : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Crimson.KEY_VeinOre;

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

        Main.tileMerge[Type][ModContent.TileType<CrimsonAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAsh>()][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CrimsonAshGrass>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAshGrass>()][Type] = true;

        AddMapEntry(new Color(140, 83, 14), CreateMapEntryName());

        TileLoader.RegisterConversion(TileID.Hellstone, BiomeConversionID.Crimson, (i, j, type, _) =>
        {
            WorldGen.ConvertTile(i, j, Type);
            return false;
        });
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.Hellstone);
                return;
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<VeinOre>());
                return;

        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        r = 0.24f;
        g = 0.146f;
        b = 0.22f;
    }

    public override void RandomUpdate(int i, int j) {
        Dust.NewDust(new Vector2(i, j), 5, 5, DustID.CrimtaneWeapons);
    }
}

public class VeinOreItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Tiles.Crimson.KEY_VeinOreItem;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<VeinOre>());
        Item.width = 17;
        Item.height = 19;
        Item.value = Item.sellPrice(0, 0, 3);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
    }
}
