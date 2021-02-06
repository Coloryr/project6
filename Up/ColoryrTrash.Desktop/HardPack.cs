using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrTrash.Desktop
{
    public enum PackType
    {
        UUID, Size, Height, Low, IP, All, None
    }
    class HardPack
    {
        public static readonly byte[] TestPack = new byte[] { 0x87, 0x32, 0xf5, 0xae, 0x1d, 0x91, 0x0f, 0x8e, 0x3f };
        public static readonly byte[] ResPack = new byte[] { 0x21, 0x00, 0x4f, 0x56, 0xae, 0xac, 0xe3, 0x76, 0x89 };
        public static readonly byte[] ReadPack = new byte[] { 0x52, 0x45, 0x41, 0x44, 0x3A };

        public static bool CheckPack(byte[] data)
        {
            if (data.Length != 9)
                return false;
            for (int a = 0; a < 9; a++)
            {
                if (ResPack[a] != data[a])
                    return false;
            }
            return true;
        }
        public static PackType CheckType(byte[] data)
        {
            if (data.Length < 6)
                return PackType.None;
            for (int a = 0; a < 5; a++)
            {
                if (ReadPack[a] != data[a])
                    return PackType.None;
            }
            return (PackType)data[6];
        }
        public static byte[] MakePack(PackType type)
        {
            var data = new byte[6];
            for (int a = 0; a < 5; a++)
            {
                data[a] = ReadPack[a];
            }
            data[5] = (byte)type;
            return data;
        }
    }
}
