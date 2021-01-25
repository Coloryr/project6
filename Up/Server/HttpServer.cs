using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class HttpServer
    {
        public static HttpListener Listener;//http服务器
        private static Thread[] Workers;                             // 工作线程组
        public static ConcurrentBag<HttpListenerContext> Queue;   // 请求队列
        public static bool IsActive { get; set; }//是否在运行

        private static void Init()
        {
            ServerMain.LogOut("Http服务器正在启动");
            Listener = new();
            Workers = new Thread[ServerMain.Config.Http.ThreadNumber];
            Queue = new();
            IsActive = false;

            Listener.Prefixes.Add("http://" + ServerMain.Config.Http.IP + ":" +
                ServerMain.Config.Http.Port + "/");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Listener.TimeoutManager.EntityBody = TimeSpan.FromSeconds(30);
                Listener.TimeoutManager.RequestQueue = TimeSpan.FromSeconds(30);
            }
        }

        public static void Start()
        {
            try
            {
                Init();
                // 启动工作线程
                for (int i = 0; i < Workers.Length; i++)
                {
                    Workers[i] = new Thread(HttpWork.Worker);
                    Workers[i].Start();
                }
                IsActive = true;
                Listener.Start();
                Listener.BeginGetContext(ContextReady, null);
                ServerMain.LogOut("Http服务器已启动");
            }
            catch (Exception e)
            {
                ServerMain.LogError(e);
            }
        }

        public static void StartPipe()
        {
            try
            {
                Init();
                // 启动工作线程
                for (int i = 0; i < Workers.Length; i++)
                {
                    Workers[i] = new Thread(HttpWork.PipeWorker);
                    Workers[i].Start();
                }
                IsActive = true;
                Listener.Start();
                Listener.BeginGetContext(ContextReady, null);
                ServerMain.LogOut("Http服务器已启动");
            }
            catch (Exception e)
            {
                ServerMain.LogError(e);
            }
        }

        private static void ContextReady(IAsyncResult ar)
        {
            if (IsActive)
            {
                Listener.BeginGetContext(ContextReady, null);
                Queue.Add(Listener.EndGetContext(ar));
            }
        }

        public static void Stop()
        {
            if (IsActive)
            {
                ServerMain.LogOut("Http服务器正在关闭");
                IsActive = false;
                Listener.Stop();
                ServerMain.LogOut("Http服务器已关闭");
            }
        }
    }

    class HttpWork
    {
        // 处理一个任务
        public static void Worker()
        {
            while (!HttpServer.IsActive)
            {
                Thread.Sleep(200);
            }
            while (HttpServer.IsActive)
            {
                if (HttpServer.Queue.TryTake(out HttpListenerContext Context))
                {
                    try
                    {
                        HttpListenerRequest Request = Context.Request;
                        HttpListenerResponse Response = Context.Response;
                        HttpReturn httpReturn;
                        switch (Request.HttpMethod)
                        {
                            case "POST":
                                StreamReader Reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8);
                                MyContentType type;
                                if (Context.Request.ContentType == "application/x-www-form-urlencoded")
                                {
                                    type = MyContentType.Form;
                                }
                                else if (Context.Request.ContentType == "application/json")
                                {
                                    type = MyContentType.Json;
                                }
                                else
                                {
                                    type = MyContentType.Other;
                                }
                                httpReturn = HttpProcessor.HttpPOST(Reader, Request.RawUrl, Request.Headers, type);
                                Response.ContentType = httpReturn.ContentType;
                                Response.ContentEncoding = httpReturn.Encoding;
                                if (httpReturn.Head != null)
                                    foreach (var Item in httpReturn.Head)
                                    {
                                        Response.AddHeader(Item.Key, Item.Value);
                                    }
                                if (httpReturn.Cookie != null)
                                    Response.AppendCookie(new Cookie("cs", httpReturn.Cookie));
                                if (httpReturn.Data1 == null)
                                    Response.OutputStream.Write(httpReturn.Data);
                                else
                                {
                                    httpReturn.Data1.Seek(0, SeekOrigin.Begin);
                                    httpReturn.Data1.CopyTo(Response.OutputStream);
                                }
                                Response.OutputStream.Flush();
                                Response.StatusCode = httpReturn.ReCode;
                                Response.Close();
                                break;
                            case "GET":
                                httpReturn = HttpProcessor.HttpGET(Request.RawUrl, Request.Headers, Request.QueryString);
                                Response.ContentType = httpReturn.ContentType;
                                Response.ContentEncoding = httpReturn.Encoding;
                                if (httpReturn.Head != null)
                                    foreach (var Item in httpReturn.Head)
                                    {
                                        Response.AddHeader(Item.Key, Item.Value);
                                    }
                                if (httpReturn.Cookie != null)
                                    Response.AppendCookie(new Cookie("cs", httpReturn.Cookie));
                                if (httpReturn.Data1 == null)
                                    Response.OutputStream.Write(httpReturn.Data);
                                else
                                {
                                    httpReturn.Data1.Seek(0, SeekOrigin.Begin);
                                    httpReturn.Data1.CopyTo(Response.OutputStream);
                                }
                                Response.OutputStream.Flush();
                                Response.StatusCode = httpReturn.ReCode;
                                Response.Close();
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        ServerMain.LogError(e);
                    }
                }
                Thread.Sleep(1);
            }
        }
        public static void PipeWorker()
        {
            while (!HttpServer.IsActive)
            {
                Thread.Sleep(200);
            }
            while (HttpServer.IsActive)
            {
                if (HttpServer.Queue.TryTake(out HttpListenerContext Context))
                {
                    try
                    {
                        HttpListenerRequest Request = Context.Request;
                        HttpListenerResponse Response = Context.Response;
                        switch (Request.HttpMethod)
                        {
                            case "POST":
                                StreamReader Reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8);
                                MyContentType type;
                                if (Context.Request.ContentType == "application/x-www-form-urlencoded")
                                {
                                    type = MyContentType.Form;
                                }
                                else if (Context.Request.ContentType == "application/json")
                                {
                                    type = MyContentType.Json;
                                }
                                else
                                {
                                    Response.OutputStream.Write(Encoding.UTF8.GetBytes($"不支持{Context.Request.ContentType}"));
                                    Response.OutputStream.Flush();
                                    Response.StatusCode = 400;
                                    Response.Close();
                                    break;
                                }
                                dynamic httpReturn = HttpProcessor.PipeHttpPOST(Reader, Request.RawUrl, Request.Headers, type);
                                if (httpReturn is HttpReturn)
                                {
                                    Response.ContentType = httpReturn.ContentType;
                                    Response.ContentEncoding = httpReturn.Encoding;
                                    Response.OutputStream.Write(httpReturn.Data);
                                    Response.OutputStream.Flush();
                                    Response.StatusCode = httpReturn.ReCode;
                                    Response.Close();
                                    break;
                                }
                                else
                                {
                                    PipeClient.Http(httpReturn, Response);
                                }
                                break;
                            case "GET":
                                httpReturn = HttpProcessor.PipeHttpGET(Request.RawUrl, Request.Headers, Request.QueryString);
                                PipeClient.Http(httpReturn, Response);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        ServerMain.LogError(e);
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}
