using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace VacuumChest.Tiles
{
    public class VacuumChest : ModTile
    {
        
        public override void SetDefaults()
        {
            Main.tileSpelunker[Type] = true;
            Main.tileContainer[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileValue[Type] = 500;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.HookCheck = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
            TileObjectData.newTile.AnchorInvalidTiles = new int[] { 127 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("VacuumChest");
            AddMapEntry(new Color(200, 200, 200), name, MapChestName);
            dustType = mod.DustType("Sparkle");
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.Containers };
            chest = "VacuumChest";
            chestDrop = mod.ItemType("VacuumChest");
        }

        public override bool HasSmartInteract()
        {
            return true;
        }

        private string MapChestName(string name, int i, int j)
        {
            int left = i;
            int top = j;
            Tile tile = Main.tile[i, j];
            if (tile.frameX % 36 != 0)
            {
                left--;
            }
            if (tile.frameY != 0)
            {
                top--;
            }
            int chest = Chest.FindChest(left, top);
            if (Main.chest[chest].name == "")
            {
                return name;
            }
            else
            {
                return name + ": " + Main.chest[chest].name;
            }
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 1;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 32, 32, chestDrop);
            Chest.DestroyChest(i, j);
        }

        public override void RightClick(int i, int j)
        {           
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.frameX % 36 != 0)
            {
                left--;
            }
            if (tile.frameY != 0)
            {
                top--;
            }
            if (player.sign >= 0)
            {
                Main.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = "";
            }
            if (Main.editChest)
            {
                Main.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = "";
            }
            if (player.editedChestName)
            {
                NetMessage.SendData(33, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
                player.editedChestName = false;
            }
            if (Main.netMode == 1)
            {
                if (left == player.chestX && top == player.chestY && player.chest >= 0)
                {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    Main.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    NetMessage.SendData(31, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
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
                        Main.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        player.chest = chest;
                        Main.playerInventory = true;
                        Main.recBigList = false;
                        player.chestX = left;
                        player.chestY = top;
                        Main.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                    }
                    Recipe.FindRecipes();
                }
            }
           
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            if (tile.frameX % 36 != 0)
            {
                left--;
            }
            if (tile.frameY != 0)
            {
                top--;
            }
            int chest = Chest.FindChest(left, top);
            player.showItemIcon2 = -1;
            if (chest < 0)
            {
                player.showItemIconText = Language.GetTextValue("LegacyChestType.0");
            }
            else
            {
                player.showItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : "Vacuum Chest";
                if (player.showItemIconText == "Vacuum Chest")
                {
                    player.showItemIcon2 = mod.ItemType("VacuumChest");
                }
            }
            player.noThrow = 2;
            player.showItemIcon = true;
        }

        public override void MouseOverFar(int i, int j)
        {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.showItemIconText == "")
            {
                player.showItemIcon = false;
                player.showItemIcon2 = 0;
            }
        }

        public override void HitWire(int x, int y)
        {
            TriggerVacuum(x, y);
        }

        public void TriggerVacuum(int x, int y)
        {
            if (Main.netMode != 1)
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
                                if (Main.netMode != 1)
                                {
                                    Main.item[i] = new Item();
                                }

                                if (Main.netMode == 2)
                                {
                                    // Notify players about item change
                                    var netMessage = global::VacuumChest.VacuumChest.instance.GetPacket();
                                    netMessage.Write((byte) global::VacuumChest.VacuumChest.VacuumChestMessageType.VacuumItem);
                                    netMessage.Write(i);
                                    netMessage.Write(chestID);
                                    netMessage.Write(insertedIndex);
                                    netMessage.WriteItem(Main.chest[chestID].item[insertedIndex]);
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