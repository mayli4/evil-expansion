using Daybreak.Common.Features.Hooks;
using EvilExpansionMod.Content.Crimson;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.World;

public sealed class ClentaminatorConversions {
    [OnLoad]
    private static void Init() {
        IL_WorldGen.TileFrame += AddModdedPlantConversion;
        IL_WorldGen.PlantCheck += ConvertFoliage;
    }

    private static void AddModdedPlantConversion(ILContext il) {
        ILCursor c = new ILCursor(il);
        // move to the last tileFrameImportant access, right before the check
        while(c.TryGotoNext(MoveType.After, i => i.MatchLdsfld<Main>("tileFrameImportant"))) {

        }
        c.GotoNext(i => i.MatchBrfalse(out _));
        c.EmitLdarg(0);
        c.EmitLdarg(1);
        c.EmitDelegate((int x, int y) =>
        {
            Tile tile = Main.tile[x, y];
            if(y - 1 > 0) {
                Tile tileAbove = Main.tile[x, y - 1];
                int type = tile.TileType;
                if(tileAbove.HasTile)
                    WorldGen.PlantCheck(x, y);
            }
        });
    }

    private static void ConvertFoliage(MonoMod.Cil.ILContext il) {
        ILCursor c = new ILCursor(il);
        int tileIndex = -1;
        c.GotoNext(i => i.MatchCall<Tilemap>("get_Item"), i => i.MatchStloc(out tileIndex));
        if(tileIndex != -1) {
            while(c.TryGotoNext(i => i.MatchBeq(out _))) {

            }
            c.GotoPrev(MoveType.Before, i => i.MatchLdloca(tileIndex));
            c.EmitLdarg(0);
            c.EmitLdarg(1);
            c.EmitLdloc(tileIndex);
            c.EmitDelegate((int x, int y, Tile tile) =>
            {
                bool didAnything = false;
                if(y + 1 < Main.maxTilesY) {
                    Tile tileBelow = Main.tile[x, y + 1];
                    if(tileBelow.TileType == ModContent.TileType<CrimsonAshGrass>()) {
                        didAnything = true;
                        tile.ResetToType((ushort)ModContent.TileType<CrimsonFoliage>());
                    }
                    if(tileBelow.TileType == TileID.AshGrass) {
                        tile.ResetToType(TileID.AshPlants);
                        didAnything = true;
                    }
                }
                return didAnything;
            });
            ILLabel afterRetLabel = c.DefineLabel();
            c.EmitBrfalse(afterRetLabel);
            c.EmitPop();
            c.EmitRet();
            c.MarkLabel(afterRetLabel);
            // ret if our code did something, otherwise let it do its thing
        }
    }
}
