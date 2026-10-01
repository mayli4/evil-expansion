using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles;

[UsedImplicitly]
internal sealed class BambooEdits {
    public static bool[] CanGrowBamboo = TileID.Sets.Factory.CreateBoolSet(false, TileID.JungleGrass);
    
    [OnLoad]
    public void Load(Mod mod) {
        IL_WorldGen.CheckBamboo += WorldGen_CheckBamboo;
        IL_WorldGen.PlaceBamboo += WorldGen_PlaceBamboo;
    }

    private void WorldGen_CheckBamboo(ILContext il) {
        ILCursor c = new ILCursor(il);
        DoSwap(c);
    }

    private void WorldGen_PlaceBamboo(ILContext il) {
        ILCursor c = new ILCursor(il);

        DoSwap(c);
    }

    private void DoSwap(ILCursor c) {
        if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(TileID.JungleGrass))) {
            return;
        }

        c.EmitDelegate<Func<int, int>>(SwapDelegate);
    }

    private static int SwapDelegate(int tileType) => CanGrowBamboo[tileType] ? TileID.JungleGrass : tileType;
    
    internal sealed class BambooGlobalTile : GlobalTile {
        public override void SetStaticDefaults() {
            for(int i = 0; i < TileLoader.TileCount; i++) {
                if(i == TileID.CorruptJungleGrass || i == TileID.CrimsonJungleGrass) {
                    CanGrowBamboo[i] = true;
                }
            }
        }
    }
}

internal abstract class BambooTile : ModTile {
    public abstract int BambooDust { get;} 
    public abstract Color MapColor { get;} 
    
    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileBlockLight[Type] = false;
        Main.tileLighted[Type] = false;

        DustType = BambooDust;
        HitSound = SoundID.Grass;

        AddMapEntry(MapColor);

        RegisterItemDrop(ItemID.BambooBlock);

        TileLoader.RegisterConversion(Type, BiomeConversionID.Purity, (i, j, t, c) => BambooSystem.ConvertSingle(i, j, TileID.Bamboo));
        TileLoader.RegisterConversion(Type, BiomeConversionID.PurificationPowder, (i, j, t, c) => BambooSystem.ConvertSingle(i, j, TileID.Bamboo));
        TileLoader.RegisterConversion(Type, BiomeConversionID.Corruption, (i, j, t, c) => BambooSystem.ConvertSingle(i, j, ModContent.TileType<CorruptBamboo>()));
        TileLoader.RegisterConversion(Type, BiomeConversionID.Crimson, (i, j, t, c) => BambooSystem.ConvertSingle(i, j, ModContent.TileType<CrimsonBamboo>()));
    }

    public static bool IsBamboo(int type)
        => type == TileID.Bamboo
        || type == ModContent.TileType<CorruptBamboo>()
        || type == ModContent.TileType<CrimsonBamboo>();

    public static bool IsValidGround(int type)
        => type == TileID.JungleGrass
        || type == TileID.CorruptJungleGrass
        || type == TileID.CrimsonJungleGrass
        || IsBamboo(type);

    internal static void FrameBamboo(int i, int j) {
        Tile below = Framing.GetTileSafely(i, j + 1);
        Tile above = Framing.GetTileSafely(i, j - 1);
        Tile self = Framing.GetTileSafely(i, j);

        if (!below.HasTile || !IsValidGround(below.TileType)) {
            WorldGen.KillTile(i, j);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, i, j);
            return;
        }

        bool hasAbove = above.HasTile && IsBamboo(above.TileType);
        bool hasBelow = below.HasTile && IsBamboo(below.TileType);
        int frame = self.TileFrameX / 18;
        self.TileFrameY = 0;

        if (hasAbove) {
            if (hasBelow) {
                if (frame < 5 || frame > 14)
                    self.TileFrameX = (short)(WorldGen.genRand.Next(5, 15) * 18);
            }
            else if (frame is < 0 or > 4)
                self.TileFrameX = (short)(WorldGen.genRand.Next(0, 5) * 18);
        }
        else if (hasBelow) {
            if (frame < 15 || frame > 19)
                self.TileFrameX = (short)(WorldGen.genRand.Next(15, 20) * 18);
        }
        else if (frame != 0)
            self.TileFrameX = 0;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        FrameBamboo(i, j);
        return false;
    }

    public override void RandomUpdate(int i, int j) {
        if (Main.tile[i, j - 1].HasTile)
            return;
        BambooSystem.TryGrowBamboo(i, j - 1, Type);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ItemID.BambooBlock);
    }

    public override void Convert(int i, int j, int conversionType) {
        switch (conversionType) {
            case BiomeConversionID.Purity:
            case BiomeConversionID.PurificationPowder:
                BambooSystem.ConvertSingle(i, j, TileID.Bamboo);
                break;
            case BiomeConversionID.Corruption:
                BambooSystem.ConvertSingle(i, j, ModContent.TileType<CorruptBamboo>());
                break;
            case BiomeConversionID.Crimson:
                BambooSystem.ConvertSingle(i, j, ModContent.TileType<CrimsonBamboo>());
                break;
        }
    }
}

internal sealed class CrimsonBamboo : BambooTile {
    public override string Texture => Assets.Images.Crimson.Tiles.Jungle.CrimsonBamboo.KEY;
    public override int BambooDust => DustID.CrimsonPlants;
    public override Color MapColor => Color.DarkRed;
}

internal sealed class CorruptBamboo : BambooTile {
    public override string Texture => Assets.Images.Corruption.Tiles.Jungle.CorruptBamboo.KEY;
    public override int BambooDust => DustID.CorruptionThorns;
    public override Color MapColor => Color.MediumPurple;
}

internal class BambooSystem : GlobalTile {
    public override void SetStaticDefaults() {
        TileLoader.RegisterConversion(TileID.Bamboo, BiomeConversionID.Corruption, (i, j, t, c) => ConvertColumn(i, j, ModContent.TileType<CorruptBamboo>()));
        TileLoader.RegisterConversion(TileID.Bamboo, BiomeConversionID.Crimson, (i, j, t, c) => ConvertColumn(i, j, ModContent.TileType<CrimsonBamboo>()));
        TileLoader.RegisterConversion(TileID.Bamboo, BiomeConversionID.Purity, (i, j, t, c) => ConvertColumn(i, j, TileID.Bamboo));
    }

    public override void RandomUpdate(int i, int j, int type) {
        if (type is TileID.CorruptJungleGrass or TileID.CrimsonJungleGrass
            && Main.tile[i, j - 1].LiquidAmount > 0
            && !Main.tile[i, j - 1].HasTile
            && WorldGen.genRand.NextBool(30)) {
            ushort bamboo = type == TileID.CorruptJungleGrass
                ? (ushort)ModContent.TileType<CorruptBamboo>()
                : (ushort)ModContent.TileType<CrimsonBamboo>();
            TryGrowBamboo(i, j - 1, bamboo);
        }
    }

    internal static bool ConvertSingle(int i, int j, int newType) {
        Tile t = Main.tile[i, j];
        if (!t.HasTile || t.TileType == newType)
            return false;
        short fx = t.TileFrameX;
        t.TileType = (ushort)newType;
        t.TileFrameX = fx;
        t.TileFrameY = 0;
        WorldGen.SquareTileFrame(i, j);
        
        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, i, j);
        return false;
    }

    private static bool ConvertColumn(int i, int j, int newType) {
        int top = j;
        while (top > 10 && Main.tile[i, top - 1].HasTile && BambooTile.IsBamboo(Main.tile[i, top - 1].TileType))
            top--;
        for (int y = top; y < Main.maxTilesY - 10; y++) {
            Tile t = Main.tile[i, y];
            if (!t.HasTile || !BambooTile.IsBamboo(t.TileType))
                break;
            if (t.TileType == newType)
                continue;
            short fx = t.TileFrameX;
            t.TileType = (ushort)newType;
            t.TileFrameX = fx;
            t.TileFrameY = 0;
            WorldGen.SquareTileFrame(i, y);
        }
        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, i, j);
        return false;
    }

    public static bool TryGrowBamboo(int x, int y, ushort bambooType) {
        if (!WorldGen.InWorld(x, y, 1))
            return false;
        Tile at = Main.tile[x, y];
        if (at.HasTile || (at.WallType > WallID.None && y <= Main.worldSurface))
            return false;
        Tile below = Main.tile[x, y + 1];
        if (!below.HasTile)
            return false;

        bool onSoil = below.TileType == TileID.JungleGrass
            || below.TileType == TileID.CorruptJungleGrass
            || below.TileType == TileID.CrimsonJungleGrass
            || BambooTile.IsBamboo(below.TileType);
        if (!onSoil)
            return false;

        if (GetWaterDepth(x, y) is < 2 or > 5)
            return false;

        int density = CountTiles(x, y, 5, bambooType) + CountTiles(x, y, 5, TileID.Bamboo);
        int height = 1;
        if (BambooTile.IsBamboo(below.TileType)) {
            for (; !WorldGen.SolidTile(x, y + height); height++) { }
            if (height + density / WorldGen.genRand.Next(1, 21) > WorldGen.genRand.Next(1, 21))
                return false;
        }
        else density += 25;
        if (density + height * 2 > WorldGen.genRand.Next(40, 61))
            return false;

        at.HasTile = true;
        at.TileType = bambooType;
        at.TileFrameX = 0;
        at.TileFrameY = 0;
        WorldGen.SquareTileFrame(x, y);
        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, x, y);
        return true;
    }

    private static int GetWaterDepth(int x, int y) {
        int solid = y;
        while (!WorldGen.SolidTile(x, solid)) {
            if (++solid > Main.maxTilesY - 1)
                return 0;
        }
        int top = solid;
        while (Main.tile[x, top].LiquidAmount > 0 && !WorldGen.SolidTile(x, top))
            top--;
        return solid - top;
    }

    private static int CountTiles(int x, int y, int range, int type) {
        int n = 0;
        for (int i = x - range; i <= x + range; i++)
            for (int k = y - range * 3; k <= y + range * 3; k++)
                if (WorldGen.InWorld(i, k) && Main.tile[i, k].HasTile && Main.tile[i, k].TileType == type)
                    n++;
        return n;
    }
}