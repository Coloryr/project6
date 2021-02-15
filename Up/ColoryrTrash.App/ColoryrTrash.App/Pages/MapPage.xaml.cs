using Lib;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private readonly Dictionary<string, TrashSaveObj> Points = new Dictionary<string, TrashSaveObj>();
        private readonly object Lock = new object();
        public MapPage()
        {
            InitializeComponent();
            Web.Navigated += Web_Navigated;
            Web.Source = $"http://localhost:{App.Config.HttpPort}";
        }

        private void Web_Navigated(object sender, WebNavigatedEventArgs e)
        {
            ClearPoint();
            lock (Lock)
            {
                foreach (var item in Points.Values)
                {
                    AddPoint(item.X, item.Y, item.UUID, GetString(item));
                }
            }
        }

        private string GetString(TrashSaveObj item)
        {
            return $"垃圾桶别称:{item.Nick}<br/>垃圾桶坐标:{(double)item.X / 1000000}, {(double)item.Y / 1000000}" +
                $"<br/>垃圾桶容量:{item.Capacity}<br/>垃圾桶是否打开:{item.Open}" +
                $"<br/>垃圾桶状态:{item.State}<br/>垃圾桶上线时间:{item.Time}";
        }

        public void AddPoint(TrashSaveObj item)
        {
            lock (Lock)
            {
                Points.Add(item.UUID, item);
            }
        }
        private void AddPoint(double x, double y, string title, string text)
        {
            double X = x / 1000000;
            double Y = y / 1000000;
            string temp = $"addpoint({X}, {Y},'{title}','{text}')";
            Web.EvaluateJavaScriptAsync(temp);
        }

        public void ClearPoint()
        {
            Web.EvaluateJavaScriptAsync("clearpoint()");
        }
        public void RemovePoint(string name)
        {
            Points.Remove(name);
            Web.EvaluateJavaScriptAsync($"removepoint({name})");
        }
    }
}