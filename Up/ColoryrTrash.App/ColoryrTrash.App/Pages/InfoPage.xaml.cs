using Lib;
using System;
using System.Collections.Generic;
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
        private List<TrashSaveObj> NowList = new List<TrashSaveObj>();
        public InfoPage()
        {
            InitializeComponent();
            NowList.Add(new TrashSaveObj
            {
                UUID = "UUID",
                Nick = "智能垃圾桶",
                Capacity = 10,
                State = ItemState.Ok
            });
        }
        public void SetList(List<TrashSaveObj> list)
        {
            this.list = list;
        }

        private void Re_Refreshing(object sender, EventArgs e)
        {
            List.ItemsSource = NowList;
            Re.IsRefreshing = false;
        }

        private string GetString(TrashSaveObj item)
        {
            return $"垃圾桶别称:{item.Nick}\n垃圾桶坐标:{(double)item.X / 1000000}, {(double)item.Y / 1000000}" +
                $"\n垃圾桶容量:{item.Capacity}\n垃圾桶是否打开:{item.Open}" +
                $"\n垃圾桶状态:{item.State}\n垃圾桶上线时间:{item.Time}";
        }

        private async void List_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (List.SelectedItem == null)
                return;
            var obj = List.SelectedItem as TrashSaveObj;
            List.SelectedItem = null;
            if (obj != null)
                await DisplayAlert($"UUID:{obj.UUID}", GetString(obj), "OK");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }
    }
}