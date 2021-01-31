using System.Collections.Generic;

namespace Server
{
    public record ConfigObj
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public int ThreadNumber { get; set; }
        public Dictionary<string, string> User { get; set; }
        public Mysql Mysql { get; set; }
    }

    public record Mysql
    {
        public bool Enable { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }

    public record DataSaveObj
    {
        public Dictionary<string, ItemSaveObj> List { get; set; }
        public string Name { get; set; }
    }

    public record ItemSaveObj
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Capacity { get; set; }
        public string Time { get; set; }
        public bool Open { get; set; }
    }
}
