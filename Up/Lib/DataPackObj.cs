namespace Lib
{
    public enum DataType
    {
        Login, GetList, GetInfo, Move
    }
    public record DataPackObj
    {
        public DataType Type { get; set; }
        public object Data { get; set; }
        public object Data1 { get; set; }
    }
}
