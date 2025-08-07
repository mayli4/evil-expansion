using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles;

public class BambooSystem : ILoadable {
    public static bool[] CanGrowBamboo = TileID.Sets.Factory.CreateBoolSet(false, TileID.JungleGrass);
    
    public void Load(Mod mod) {
        IL_WorldGen.CheckBamboo += WorldGen_CheckBamboo;
        IL_WorldGen.PlaceBamboo += WorldGen_PlaceBamboo;
        IL_WorldGen.UpdateWorld_OvergroundTile += WorldGen_UpdateWorld_OvergroundTile;
    }

    public void Unload() { }

    private void WorldGen_CheckBamboo(ILContext il) {
        ILCursor c = new ILCursor(il);

        DoSwap(c);
    }

    private void WorldGen_PlaceBamboo(ILContext il) {
        ILCursor c = new ILCursor(il);

        DoSwap(c);
    }

    private void WorldGen_UpdateWorld_OvergroundTile(ILContext il) {
        ILCursor c = new ILCursor(il);

        DoSwap(c);

        if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(TileID.JungleGrass))) {
            return;
        }

        c.EmitDelegate<Func<int, int>>(tileType => CanGrowBamboo[tileType] ? tileType : TileID.JungleGrass);
    }

    private void DoSwap(ILCursor c) {
        if (!c.TryGotoNext(i => i.MatchCall<Tile>("get_type")) || !c.TryGotoNext(i => i.MatchLdcI4(TileID.JungleGrass))) {
            return;
        }

        c.EmitDelegate<Func<int, int>>(SwapDelegate);
    }

    private int SwapDelegate(int tileType) => CanGrowBamboo[tileType] ? TileID.JungleGrass : tileType;
}

internal sealed class BambooGlobalTile : GlobalTile {
    public override void SetStaticDefaults() {
        for(int i = 0; i < TileLoader.TileCount; i++) {
            if(i == TileID.CorruptJungleGrass || i == TileID.CrimsonJungleGrass) {
                BambooSystem.CanGrowBamboo[i] = true;
            }
        }
    }
}