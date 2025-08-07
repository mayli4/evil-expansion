using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Utilities;

public static class ConversionUtilities {
	public static void RegisterConversions(int[] types, int conversionType, TileLoader.ConvertTile action)
	{
		foreach (int type in types)
			TileLoader.RegisterConversion(type, conversionType, action);
	}

	/// <summary> Based on <see cref="TileLoader.ConvertTile"/>.<br/>
	/// Converts all tiles within the specified area, then frames and sends the changes over the network. Necessary for multitiles. </summary>
	public static bool ConvertTiles(int i, int j, int width, int height, int newType, bool frameAndSend = true) {
		ushort oldType = Main.tile[i, j].TileType;

		if (oldType == newType)
			return false;

		for (int x = i; x < i + width; x++) {
			for (int y = j; y < j + height; y++) {
				var t = Main.tile[x, y];
				if (t.TileType != oldType)
					continue;

				t.TileType = (ushort)newType;
			}
		}

		if (frameAndSend) {
			WorldGen.RangeFrame(i, j, i + width - 1, j + height - 1);

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, i, j, width, height);
		}

		return true;
	}
}
