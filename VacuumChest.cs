using Newtonsoft.Json;
using System.IO;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VacuumChest
{
	class VacuumChest : Mod
	{

		
		public VacuumChest()
		{
			
		}

		public override void Load()
		{

		}
		
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			VacuumChestMessageType msgType = (VacuumChestMessageType)reader.ReadByte();

			switch (msgType)
			{
				case VacuumChestMessageType.VacuumItem:
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						int itemID = reader.ReadInt32();
						int chestID = reader.ReadInt32();
						int chestIndex = reader.ReadInt32();

						// Deserialize received item
						Item chestItem;
                        using (var memoryStream  = new MemoryStream())
                        using (var streamReader  = new StreamReader(stream: memoryStream, encoding: Encoding.UTF8, bufferSize: 4096, leaveOpen: true))
						using (var jsonReader    = new JsonTextReader(streamReader))
						{
							JsonSerializer jsonSerializer = new();
							Item deserializedItem = jsonSerializer.Deserialize(jsonReader) as Item;
							chestItem = deserializedItem as Item;
						}

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
					Logger.Error("VacuumChest: Unknown Message type: " + msgType);
					break;
			}
		}
		
		internal enum VacuumChestMessageType : byte
		{
			VacuumItem
		}

	}
}
