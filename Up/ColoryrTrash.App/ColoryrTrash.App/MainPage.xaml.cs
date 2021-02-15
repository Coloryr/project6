using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : FlyoutPage
    {
        private string NowSel;
        public MainPage()
        {
            InitializeComponent();
            flyoutPage.listView.ItemSelected += OnItemSelected;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutPageItem;
            if (item != null)
            {
                if (NowSel != item.Name)
                    Switch(item.Name);
            }
            flyoutPage.listView.SelectedItem = null;
            IsPresented = false;
        }
        public void Switch(string name)
        {
            switch (name)
            {
                case "Main":
                    Detail = App.helloPage_;
                    break;
                case "Info":
                    Detail = App.infoPage_;
                    break;
                case "Login":
                    Detail = App.loginPage_;
                    break;
                case "Map":
                    Detail = App.mapPage_;
                    break;
            }
            NowSel = name;
        }

        internal void SetName(string v)
        {
            flyoutPage.SetName(v);
        }

        public void ClearName()
        {
            flyoutPage.ClearName();
        }
    }
}