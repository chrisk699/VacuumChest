using System;
using System.IO;
using System.Text;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VacuumChest.Tiles
{
    public class VacuumChest : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSpelunker[Type] = true;
            Main.tileContainer[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileOreFinderPriority[Type] = 500;            
            TileID.Sets.BasicChest[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
            TileObjectData.newTile.AnchorInvalidTiles = new int[] { TileID.MagicalIceBlock };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("VacuumChest");
            AddMapEntry(new Color(200, 200, 200), name, MapChestName);
            AdjTiles = new int[] { TileID.Containers };
            ContainerName.SetDefault("VacuumChest");
            ChestDrop = ModContent.ItemType<global::VacuumChest.Items.VacuumChest>();
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }
        public static string MapChestName(string name, int i, int j)
        {
            int left = i;
            int top = j;
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }

            if (tile.TileFrameY != 0)
            {
                top--;
            }

            int chest = Chest.FindChest(left, top);
            if (chest < 0)
            {
                return Language.GetTextValue("LegacyChestType.0");
            }

            if (Main.chest[chest].name == "")
            {
                return name;
            }

            return name + ": " + Main.chest[chest].name;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 1;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            IEntitySource source = new EntitySource_TileBreak(i, j);
            Item.NewItem(source, i * 16, j * 16, 32, 32, ChestDrop);
            Chest.DestroyChest(i, j);
        }

        public override bool RightClick(int i, int j)
        {           
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY != 0)
            {
                top--;
            }
            if (player.sign >= 0)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = "";
            }
            if (Main.editChest)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = "";
            }
            if (player.editedChestName)
            {
                NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
                player.editedChestName = false;
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (left == player.chestX && top == player.chestY && player.chest >= 0)
                {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
                    Main.stackSplit = 600;
                }
            }
            else
            {
                int chest = Chest.FindChest(left, top);
                if (chest >= 0)
                {
                    Main.stackSplit = 600;
                    if (chest == player.chest)
                    {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        player.chest = chest;
                        Main.playerInventory = true;
                        Main.recBigList = false;
                        player.chestX = left;
                        player.chestY = top;
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                    }
                    Recipe.FindRecipes();
                }
            }

            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY != 0)
            {
                top--;
            }
            int chest = Chest.FindChest(left, top);
            player.cursorItemIconID = -1;
            if (chest < 0)
            {
                player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
            }
            else
            {
                player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : "Vacuum Chest";
                if (player.cursorItemIconText == "Vacuum Chest")
                {
                    player.cursorItemIconID = Mod.Find<ModItem>("VacuumChest").Type;
                }
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }

        public override void MouseOverFar(int i, int j)
        {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.cursorItemIconText == "")
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }

        public override void HitWire(int x, int y)
        {
            TriggerVacuum(x, y);
        }

        public void TriggerVacuum(int x, int y)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                DoVacuum(x, y);
            }
        }

        private void DoVacuum(int x, int y)
        {
            float range = 16 * 10f;
            Vector2 chestPos = new Vector2(x * 16, y * 16);

            for (int i = 0; i < Main.item.Length; i++)
            {
                if (Main.item[i].active)
                {
                    if (Vector2.Distance(chestPos, Main.item[i].position) <= range)
                    {

                        int chestID = Chest.FindChest(x, y);

                        if (chestID >= 0)
                        {

                            int insertedIndex = InsertItem(Main.chest[chestID], Main.item[i]); 
                            if (insertedIndex != -1)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Main.item[i] = new Item();
                                }

                                if (Main.netMode == NetmodeID.Server)
                                {
                                    JsonSerializer js = new();

                                    // Notify players about item change
                                    var netMessage = ModContent.GetInstance<global::VacuumChest.VacuumChest>().GetPacket();
                                    netMessage.Write((byte) global::VacuumChest.VacuumChest.VacuumChestMessageType.VacuumItem);
                                    netMessage.Write(i);
                                    netMessage.Write(chestID);
                                    netMessage.Write(insertedIndex);

                                    // Serialize item for sending
                                    using (var memoryStream = new MemoryStream())
                                    using (var streamWriter = new StreamWriter(stream: memoryStream, encoding: Encoding.UTF8, bufferSize: 4096, leaveOpen: true))
                                    {
                                        JsonSerializer jsonSerializer = new();
                                        Item insertedItem = Main.chest[chestID].item[insertedIndex];
                                        jsonSerializer.Serialize(streamWriter, insertedItem);
                                        byte[] serializedItem = memoryStream.ToArray();
                                        netMessage.Write(serializedItem);
                                    }
                                    
                                    netMessage.Write(Main.chest[chestID].item[insertedIndex].stack);
                                    netMessage.Send();
                                }
                            }

                        }

                    }
                }
            }
        }

        private int InsertItem(Chest chest, Item item)
        {

            for (int curIndex = 0; curIndex < chest.item.Length; curIndex++)
            {
                Item curItem = chest.item[curIndex];

                if (String.IsNullOrEmpty(curItem.Name) && curItem.netID.Equals(0))
                {
                    chest.item[curIndex] = item;
                    return curIndex;
                } else if (curItem.Name.Equals(item.Name) && (curItem.netID.Equals(item.netID)) && curItem.stack + 1 <= curItem.maxStack)
                {
                    chest.item[curIndex].stack += item.stack;
                    return curIndex;
                }
                            
            }

            return -1;
        }

    }
}