using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public record ConfigObj
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public int ThreadNumber { get; set; }
        public Dictionary<string, string> User { get; set; }
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
    }
}
