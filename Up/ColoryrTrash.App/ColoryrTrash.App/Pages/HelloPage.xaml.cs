﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColoryrTrash.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelloPage : ContentPage
    {
        public HelloPage()
        {
            InitializeComponent();
        }

        public void ClearName()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                User.Text = "未登录";
            });
        }
    }
}