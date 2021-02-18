using Lib;
using System;
using System.Text;

namespace ColoryrTrash.Server
{
    class SocketPackUtils
    {
        private static readonly byte[] PackHand = new byte[6] { 0x00, 0x0a, 0x0f, 0xf4, 0x5f, 0x23 };

        public static bool CheckPack(byte[] data)
        {
            if (data.Length < 6)
            {
                return false;
            }
            for (int a = 0; a < 6; a++)
            {
                if (data[a] != PackHand[a])
                    return false;
            }
            return true;
        }

        public static void ReadTcpPack(int port, byte[] data)
        {
            if (data.Length == 16)
            {
                byte[] temp = new byte[16];
                Array.Copy(data, temp, 16);
                string uuid = Encoding.UTF8.GetString(temp);
                ServerMain.SaveData.AddItem(uuid);
            }
            else if (data.Length == 23)
            {
                byte[] temp = new byte[16];
                Array.Copy(data, temp, 16);
                string uuid = Encoding.UTF8.GetString(temp);
                int x = (data[17] & 0xFF) << 8 | data[16];
                int y = (data[19] & 0xFF) << 8 | data[18];
                int capacity = data[20];
                bool open = data[21] == 0x01;
                ItemState state = data[22] switch
                {
                    0 => ItemState.正常,
                    1 => ItemState.初始化,
                    2 => ItemState.初始化完成,
                    3 => ItemState.传感器错误,
                    4 => ItemState.Null,
                    5 => ItemState.快满了
                };
                ServerMain.SaveData.UpData(uuid, x, y, capacity, open, state, 0, "");
            }
        }

        public static void ReadUdpPack(int port, byte[] data)
        {
            if (CheckPack(data))
            {
                if (data.Length == 22)
                {
                    byte[] temp = new byte[16];
                    Array.Copy(data, 6, temp, 0, 16);
                    string uuid = Encoding.UTF8.GetString(temp);
                    ServerMain.SaveData.AddItem(uuid);
                }
                else if (data.Length == 29)
                {
                    byte[] temp = new byte[16];
                    Array.Copy(data, 6, temp, 0, 16);
                    string uuid = Encoding.UTF8.GetString(temp);
                    int x = (data[23] & 0xFF) << 8 | data[22];
                    int y = (data[25] & 0xFF) << 8 | data[24];
                    int capacity = data[26];
                    bool open = data[27] == 0x01;
                    ItemState state = data[28] switch
                    {
                        0 => ItemState.正常,
                        1 => ItemState.初始化,
                        2 => ItemState.初始化完成,
                        3 => ItemState.传感器错误,
                        4 => ItemState.Null,
                        5 => ItemState.快满了
                    };
                    ServerMain.SaveData.UpData(uuid, x, y, capacity, open, state, 0, "");
                }
            }
        }
    }
}
