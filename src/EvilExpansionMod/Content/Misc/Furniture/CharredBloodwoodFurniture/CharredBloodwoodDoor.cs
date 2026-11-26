using EvilExpansionMod.Content.Crimson.Items._CharredBloodwoodTiles;
using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodClosedDoorTile : ClosedDoorTile<CharredBloodwoodOpenDoorTile, CharredBloodwoodDoor> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodDoorTile_Closed.KEY;
};
public class CharredBloodwoodOpenDoorTile : OpenDoorTile<CharredBloodwoodClosedDoorTile, CharredBloodwoodDoor> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodDoorTile_Open.KEY;
};

public class CharredBloodwoodDoor : BathtubItem<CharredBloodwoodClosedDoorTile> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodDoorItem.KEY;
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(6)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
};