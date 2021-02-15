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
            List.ItemsSource = NowList;
            Re.Command = new Command(Refresh);
            Show();
        }

        public void Refresh()
        {
            Show();
            Re.IsRefreshing = false;
        }

        public void Show()
        {
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
    }
}