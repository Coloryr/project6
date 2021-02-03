using System.Collections.Generic;

namespace Server
{
    public record ConfigObj
    {
        public int ThreadNumber { get; set; }
        public Dictionary<string, string> User { get; set; }
        public Mysql Mysql { get; set; }
        public MQTTConfig MQTT { get; set; }
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
    public record MQTTConfig
    { 
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Topic { get; set; }
    }
}
