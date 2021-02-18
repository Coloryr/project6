using Lib;
using MQTTnet;
using System;
using System.Text;

namespace ColoryrTrash.Server.Mqtt
{
    class TrashServer
    {
        /// <summary>
        /// 降度分秒格式经纬度转换为小数经纬度
        /// </summary>
        /// <param name="_Value">度分秒经纬度</param>
        /// <returns>小数经纬度</returns>
        private static double GPSTransforming(string _Value)
        {
            double Ret = 0.0;
            string[] TempStr = _Value.Split('.');
            string x = TempStr[0].Substring(0, TempStr[0].Length - 2);
            string y = TempStr[0].Substring(TempStr[0].Length - 2, 2);
            string z = TempStr[1].Substring(0, 4);
            Ret = Convert.ToDouble(x) + Convert.ToDouble(y) / 60 + Convert.ToDouble(z) / 600000;
            return Ret;
        }
        public static void TrashReceived(string ClientId, byte[] Payload)
        {
            string data = Encoding.UTF8.GetString(Payload);
            var temp = data.Split(',');
            if (temp.Length == 2)
            {
                string uuid = temp[0];
                string sim = temp[1];
                ServerMain.SaveData.UpSIM(uuid, sim);
            }
            else
            {
                try
                {
                    string uuid = temp[0];
                    string Y = temp[1];
                    string X = temp[2];
                    string YMD = temp[3];
                    string HMS = temp[4];
                    string Close = temp[5];
                    string Battery = temp[6];
                    string State = temp[7];
                    string Capacity = temp[8];
                    PointLatLng res = new PointLatLng(0, 0);
                    try
                    {
                        double x = GPSTransforming(X);
                        double y = GPSTransforming(Y);
                        res = ConvertGPS.Gps84_To_bd09(new PointLatLng(y, x));
                    }
                    catch
                    {

                    }
                    int day = int.Parse(YMD.Substring(0, 2));
                    int month = int.Parse(YMD.Substring(2, 2));
                    int year = int.Parse(YMD.Substring(4, 2));
                    year += 2000;
                    int hour = int.Parse(HMS.Substring(0, 2));
                    hour += 8;
                    string Time = $"{year}-{month}-{day}T{string.Format("{0:D2}", hour)}:" +
                        $"{HMS.Substring(2, 2)}:{HMS.Substring(4, 2)}";
                    int state = int.Parse(State);
                    ServerMain.SaveData.UpData(uuid, res.Lng, res.Lat,
                        int.Parse(Capacity), Close != "1",
                        (ItemState)state, int.Parse(Battery), Time);
                    Send(ClientId, "OK");
                    if (int.Parse(Capacity) < 10)
                    {
                        AppServer.Full(uuid);
                    }
                }
                catch
                {

                }
            }
        }

        public static async void Send(string uuid, string data)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = DataArg.TopicTrashServer + "/" + uuid,
                Payload = Encoding.UTF8.GetBytes(data)
            };
            await ThisMqttServer.PublishAsync(message);
        }
    }
}
