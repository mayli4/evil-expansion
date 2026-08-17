using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common;

internal sealed class TileTextureReplacements : ILoadable {
    public void Load(Mod mod) {
        Main.instance.LoadTiles(TileID.CorruptJungleGrass);
        TextureAssets.Tile[TileID.CorruptJungleGrass] = Assets.Images.Corruption.Tiles.Jungle.CorruptJungleGrass.Asset;

        Main.instance.LoadTiles(TileID.CrimsonJungleGrass);
        TextureAssets.Tile[TileID.CrimsonJungleGrass] = Assets.Images.Crimson.Tiles.Jungle.CrimsonJungleGrass.Asset;
    }

    public void Unload() {
        TextureAssets.Tile[TileID.CorruptJungleGrass] = Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.CorruptJungleGrass}");
        TextureAssets.Tile[TileID.CrimsonJungleGrass] = Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.CrimsonJungleGrass}");
    }
}