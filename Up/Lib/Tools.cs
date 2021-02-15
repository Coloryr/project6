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
    }
}
