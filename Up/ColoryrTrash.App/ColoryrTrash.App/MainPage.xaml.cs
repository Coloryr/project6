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
        public MainPage()
        {
            InitializeComponent();
            flyoutPage.listView.ItemSelected += OnItemSelected;
            if (!App.IsLogin)
            {
                Switch("Login");
            }
        }
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutPageItem;
            if (item != null)
            {
                Switch(item);
            }
        }
        public void Switch(string name)
        {
            foreach (var item in flyoutPage.listView.ItemsSource)
            {
                var temp = item as FlyoutPageItem;
                if (temp != null)
                {
                    if (temp.Name == name)
                    {
                        Switch(temp);
                        return;
                    }
                }
            }
        }

        public void Switch(FlyoutPageItem item)
        {
            Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
            flyoutPage.listView.SelectedItem = null;
            IsPresented = false;
        }
    }
}