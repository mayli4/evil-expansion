using EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvilExpansionMod.Content.Misc.Furniture.CorruptAshwoodFurniture;


public class CorruptAshwoodClosedDoorTile : ClosedDoorTile<CorruptAshwoodOpenDoorTile, CorruptAshwoodDoor> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodDoorTile_Closed.KEY;
};
public class CorruptAshwoodOpenDoorTile : OpenDoorTile<CorruptAshwoodClosedDoorTile, CorruptAshwoodDoor> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodDoorTile_Open.KEY;
};

public class CorruptAshwoodDoor : BathtubItem<CorruptAshwoodClosedDoorTile> {
    public override string Texture => Assets.Images.Furniture.CorruptAshwoodFurniture.CorruptAshwoodDoorItem.KEY;
};