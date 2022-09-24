using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThatButtonAgain;
using Xamarin.Forms;

namespace MetaButton.Xamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            view.SetSketch(new Level());
        }
    }
}

