using EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Furniture.CharredBloodwoodFurniture;

public class CharredBloodwoodCandle : CandleTile {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodCandleTile.KEY;
    public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Glow");
};

public class CharredBloodwoodCandleItem : CandleItem<CharredBloodwoodCandle> {
    public override string Texture => Assets.Images.Furniture.CharredBloodwoodFurniture.CharredBloodwoodCandleItem.KEY;
};