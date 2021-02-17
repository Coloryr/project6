using Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoPage : ContentPage
    {
        private List<TrashSaveObj> list = new List<TrashSaveObj>();
        private ObservableCollection<TrashSaveObj> NowList = new ObservableCollection<TrashSaveObj>();
        public InfoPage()
        {
            InitializeComponent();
            List.ItemsSource = NowList;
        }
        public void SetList(List<TrashSaveObj> list)
        {
            if (list == null)
            {
                App.Show("垃圾桶组", "垃圾桶组获取失败");
            }
            else
            {
                Clear();
                this.list.AddRange(list);
                Update();
                Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    Re.IsRefreshing = false;
                });
            }
        }
        public void Clear()
        {
            list.Clear();
            NowList.Clear();
        }

        private void Update()
        {
            IEnumerable<TrashSaveObj> query = null;
            query = from items in list orderby items.Capacity descending select items;
            foreach (var item in query)
            {
                NowList.Add(item);
            }
        }

        private void Re_Refreshing(object sender, EventArgs e)
        {
            if (App.IsLogin)
            {
                App.MqttUtils.GetItems();
            }
            else
            {
                App.Show("登录", "未登录");
                Re.IsRefreshing = false;
                App.mainPage.Switch(PageName.LoginPage);
            }
        }

        private string GetString(TrashSaveObj item)
        {
            return $"别称:{item.Nick}\n坐标:{item.X}, {item.Y}" +
                $"\n容量:{item.Capacity}\n是否打开:{item.Open}" +
                $"\n状态:{item.State}\n上线时间:{item.Time}" +
                $"\nSIM卡号:{item.SIM}\n电量:{item.Battery}";
        }

        private async void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as TrashSaveObj;
            List.SelectedItem = null;
            if (obj != null)
            {
                var res = await DisplayAlert($"垃圾桶:{obj.UUID}", GetString(obj), "更新", "OK");
                if (res)
                {
                    App.MqttUtils.Updata(obj.UUID);
                }
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;
            IEnumerable<TrashSaveObj> query = null;
            string uuid = button.CommandParameter as string;
            query = from items in list where items.UUID == uuid select items;
            var item = query.FirstOrDefault();
            App.mapPage.AddPoint(item);
            App.mapPage.TurnTo(uuid);
            App.mainPage.Switch(PageName.MapPage);
        }

        internal void Update(TrashSaveObj obj)
        {
            IEnumerable<TrashSaveObj> query = null;
            query = from items in list where items.UUID == obj.UUID select items;
            var item = query.FirstOrDefault();
            list.Remove(item);
            list.Add(obj);
            Update();
        }
    }
}