using System.Collections.Generic;

namespace Lib
{
    public enum DataType
    {
        Login, GetGroups, GetGroupInfo, Move,
        Updata,
        AddGroup,
        MoveGroup,
        CheckLogin
    }
    public record PackBase
    { 
        public string Token { get; set; }
    }
    public record DataPackObj : PackBase
    {
        public DataType Type { get; set; }
        public bool Res { get; set; }
        public object Data { get; set; }
        public object Data1 { get; set; }
    }
    public record DataSaveObj
    {
        public Dictionary<string, ItemSaveObj> List { get; set; }
        public string Name { get; set; }
    }

    public enum ItemState
    {
        Ok, Error, LowBattery
    }

    public record ItemSaveObj
    {
        public string UUID { get; set; }
        public string Nick { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Capacity { get; set; }
        public string Time { get; set; }
        public bool Open { get; set; }
        public ItemState State { get; set; }
    }
}
