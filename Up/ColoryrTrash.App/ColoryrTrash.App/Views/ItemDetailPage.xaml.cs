using ColoryrTrash.App.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace ColoryrTrash.App.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}