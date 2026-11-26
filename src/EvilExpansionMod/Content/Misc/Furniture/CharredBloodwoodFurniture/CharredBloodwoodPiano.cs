using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodPianoTile : PianoTile {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodPianoTile.KEY;
};

public class CharredBloodwoodPiano : PianoItem<CharredBloodwoodPianoTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodPianoItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(6)
            .AddIngredient(ItemID.Bone, 4)
            .AddIngredient(ItemID.Book, 1)
            .AddTile(TileID.Sawmill)
            .Register();
    }
};