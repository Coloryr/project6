using Lib;
using System;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private readonly Dictionary<string, TrashSaveObj> Points = new Dictionary<string, TrashSaveObj>();
        private readonly object Lock = new object();
        private TrashSaveObj To;
        private Thread Time;
        public MapPage()
        {
            InitializeComponent();
            Web.Navigated += Web_Navigated;
            Web.Source = $"http://localhost:{App.Config.HttpPort}";
            Time = new Thread(TimeTask);
            Time.Start();
        }

        private void TimeTask()
        {
            while (true)
            {
                if (Auto.IsToggled)
                {
                    Local();
                }
                Thread.Sleep(1000);
            }
        }

        private async void Web_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Thread.Sleep(500);
            while (true)
            {
                var res = await Web.EvaluateJavaScriptAsync("isDone()");
                if (res == "true")
                {
                    break;
                }
                Thread.Sleep(500);
            }
            ClearPoint();
            lock (Lock)
            {
                foreach (var item in Points.Values)
                {
                    AddPoint(item.X, item.Y, item.UUID, GetString(item));
                }
                if (To != null)
                {
                    Thread.Sleep(500);
                    Turn(To.X, To.Y);
                }
            }
            Local();
        }

        private string GetString(TrashSaveObj item)
        {
            return $"垃圾桶别称:{item.Nick}<br/>垃圾桶坐标:{item.X}, {item.Y}" +
                $"<br/>垃圾桶容量:{item.Capacity}<br/>垃圾桶是否打开:{item.Open}" +
                $"<br/>垃圾桶状态:{item.State}<br/>垃圾桶上线时间:{item.Time}";
        }

        public void AddPoint(TrashSaveObj item)
        {
            lock (Lock)
            {
                if (Points.ContainsKey(item.UUID))
                    Points[item.UUID] = item;
                else
                    Points.Add(item.UUID, item);
            }
        }
        private void Disable()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                Auto.IsToggled = false;
            });
        }
        private async void Local(bool turn = false)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                CancellationTokenSource cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    var res = ConvertGPS.Gps84_To_bd09(new PointLatLng(location.Latitude, location.Longitude));
                    AddSelf(res.Lng, res.Lat);
                    if (turn)
                    {
                        Turn(res.Lng, res.Lat);
                    }
                }
            }
            catch (FeatureNotSupportedException)
            {
                Disable();
                App.Show("定位", "不支持定位");
            }
            catch (FeatureNotEnabledException)
            {
                Disable();
                App.Show("定位", "定位没有开启");
            }
            catch (PermissionException)
            {
                Disable();
                App.Show("定位", "定位没有权限");
            }
        }
        private void AddPoint(double x, double y, string title, string text)
        {
            Web.EvaluateJavaScriptAsync($"addpoint({x}, {y},'{title}','{text}')");
        }

        public void TurnTo(string uuid)
        {
            To = Points[uuid];
        }

        private void Turn(double x, double y)
        {
            Web.EvaluateJavaScriptAsync($"turnto({x}, {y})");
        }
        public void AddSelf(double x, double y)
        {
            Web.EvaluateJavaScriptAsync($"selfpoint({x}, {y})");
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

        private void Button_Clicked(object sender, EventArgs e)
        {
            Local(true);
        }

        internal void Clear()
        {
            Points.Clear();
            ClearPoint();
        }
    }
}