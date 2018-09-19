using System.Diagnostics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VacuumChest
{
	class VacuumChest : Mod
	{
		internal static VacuumChest instance;
		
		public VacuumChest()
		{
			
		}

		public override void Load()
		{
			instance = this;
		}
		
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			VacuumChestMessageType msgType = (VacuumChestMessageType)reader.ReadByte();

			switch (msgType)
			{
				case VacuumChestMessageType.VacuumItem:
					if (Main.netMode == 1)
					{
						int itemID = reader.ReadInt32();
						int chestID = reader.ReadInt32();
						int chestIndex = reader.ReadInt32();
						Item chestItem = reader.ReadItem();
						int stackSize = reader.ReadInt32();
						
						Main.item[itemID] = new Item();

						if (Main.LocalPlayer.chest.Equals(chestID))
						{
							Main.chest[chestID].item[chestIndex] = chestItem;
							Main.chest[chestID].item[chestIndex].stack = stackSize;
							Recipe.FindRecipes();
						}
					}
					break;
				default:
					ErrorLogger.Log("VacuumChest: Unknown Message type: " + msgType);
					break;
			}
		}
		
		internal enum VacuumChestMessageType : byte
		{
			VacuumItem
		}

	}
}
