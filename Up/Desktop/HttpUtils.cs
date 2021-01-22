using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lib;

namespace Desktop
{
    public class HttpUtils
    {
        private HttpClient client;
        public HttpUtils()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add(LoginArg.Key, LoginArg.Value);
        }


    }
}
