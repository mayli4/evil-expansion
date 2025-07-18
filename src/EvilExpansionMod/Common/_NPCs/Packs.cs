using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace EvilExpansionMod.Common;

//from spirit reforged

[AttributeUsage(AttributeTargets.Class)]
public class SpawnPackAttribute : Attribute {
	public int MinSize { get; private set; }
	public int MaxSize { get; private set; }

	public SpawnPackAttribute(int size) => (MinSize, MaxSize) = (size, size);
	public SpawnPackAttribute(int minSize, int maxSize) => (MinSize, MaxSize) = (minSize, maxSize);
}

internal class PackGlobalNPC : GlobalNPC {
	private static SpawnPackAttribute Tag(int type) {
		if (NPCLoader.GetNPC(type) is ModNPC mNPC)
			return (SpawnPackAttribute)Attribute.GetCustomAttribute(mNPC.GetType(), typeof(SpawnPackAttribute));

		return null;
	}

	public override void OnSpawn(NPC npc, IEntitySource source) {
		const int spawnRange = 5;

		if (Main.netMode == NetmodeID.MultiplayerClient || source is EntitySource_Parent || Tag(npc.type) is not SpawnPackAttribute atr)
			return;

		int packSize = Main.rand.Next(atr.MinSize, atr.MaxSize + 1) - 1;
		for (int i = 0; i < packSize; i++) {
			for (int a = 0; a < 20; a++) {
				var randomPos = npc.Center.ToTileCoordinates() + new Point((int)(spawnRange * Main.rand.NextFloat(-1f, 1f)), 0);
				int originalY = randomPos.Y;

				if (!WorldGen.PlayerLOS(randomPos.X, randomPos.Y) && Math.Abs(originalY - randomPos.Y) <= 5 && NPC.IsValidSpawningGroundTile(randomPos.X, randomPos.Y)) {
					NPC.NewNPCDirect(new EntitySource_Parent(npc), randomPos.ToWorldCoordinates() - new Vector2(0, npc.height / 2), npc.type);
					break;
				}
			}
		}
	}
}