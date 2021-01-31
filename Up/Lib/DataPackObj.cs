namespace Lib
{
    public enum DataType
    {
        Login, GetGroups, GetGroupInfo, Move
    }
    public record DataPackObj
    {
        public DataType Type { get; set; }
        public bool Res { get; set; }
        public object Data { get; set; }
        public object Data1 { get; set; }
    }
}
