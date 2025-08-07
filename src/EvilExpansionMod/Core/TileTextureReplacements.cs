using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Core;

public class TileTextureReplacements : ILoadable {

    public void Load(Mod mod) {
        Main.instance.LoadTiles(TileID.CorruptJungleGrass);
        TextureAssets.Tile[TileID.CorruptJungleGrass] = Assets.Assets.Textures.Tiles.Corruption.Jungle.CorruptJungleGrass;
        
        Main.instance.LoadTiles(TileID.CrimsonJungleGrass);
        TextureAssets.Tile[TileID.CrimsonJungleGrass] = Assets.Assets.Textures.Tiles.Crimson.Jungle.CrimsonJungleGrass;
    }

    public void Unload() {
        TextureAssets.Tile[TileID.CorruptJungleGrass] = Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.CorruptJungleGrass}");
        TextureAssets.Tile[TileID.CrimsonJungleGrass] = Main.Assets.Request<Texture2D>($"Images/Tiles_{TileID.CrimsonJungleGrass}");
    }
}