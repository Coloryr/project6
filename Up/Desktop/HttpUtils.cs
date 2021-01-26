using Lib;
using System.Net.Http;

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
