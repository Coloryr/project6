using System;
using System.Security.Cryptography;
using System.Text;

namespace Lib
{
    class Tools
    {
        public static string GenSHA1(string data, bool low = true)
        {
            string temp = BitConverter.ToString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "");
            if (low)
                temp = temp.ToLower();
            return temp;
        }
        public static string EnBase64(string data)
        {
            var temp = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(temp);
        }
        public static string DeBase64(string data)
        {
            var temp = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(temp);
        }
    }
}
