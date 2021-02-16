using System;

namespace ColoryrTrash.Desktop
{
    public enum PackType
    {
        UUID, Set, IP, Sensor, Mqtt, None
    }
    class HardPack
    {
        public static readonly byte[] TestPack = new byte[] { 0x87, 0x32, 0xf5, 0xae, 0x1d, 0x91, 0x0f, 0x8e, 0x3f };
        public static readonly byte[] ResPack = new byte[] { 0x21, 0x00, 0x4f, 0x56, 0xae, 0xac, 0xe3, 0x76, 0x89 };
        public static readonly byte[] ReadPack = new byte[] { 0x52, 0x45, 0x41, 0x44, 0x3A };
        public static readonly byte[] SetPack = new byte[] { 0x53, 0x45, 0x54, 0x3A };
        public static readonly byte[] OKPack = new byte[] { 0x4F, 0x4B };

        public static bool CheckPack(byte[] data)
        {
            if (data.Length < 9)
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
            return (PackType)data[5];
        }
        public static byte[] MakeReadPack(PackType type)
        {
            var data = new byte[6];
            for (int a = 0; a < 5; a++)
            {
                data[a] = ReadPack[a];
            }
            data[5] = (byte)type;
            return data;
        }
        public static byte[] MakeSetPack(PackType type, byte[] data)
        {
            var temp = new byte[5 + data.Length];
            for (int a = 0; a < 4; a++)
            {
                temp[a] = SetPack[a];
            }
            temp[4] = (byte)type;
            Array.Copy(data, 0, temp, 5, data.Length);
            return temp;
        }
        public static bool CheckOK(byte[] data)
        {
            if (data.Length < 2)
                return false;
            return data[0] == OKPack[0] && data[1] == OKPack[1];
        }
        public static int ByteToInt(byte a, byte b)
        {
            return (a & 0xFF) << 8 | (b & 0xFF);
        }
        public static byte[] IntToByte(int a)
        {
            var bytes = new byte[2];
            bytes[0] = (byte)((a >> 8) & 0xFF);
            bytes[1] = (byte)(a & 0xFF);
            return bytes;
        }
    }
}
