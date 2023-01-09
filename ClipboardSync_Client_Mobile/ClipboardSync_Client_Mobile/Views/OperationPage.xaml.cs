using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClipboardSync_Client_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OperationPage : ContentPage
    {
        
        public OperationPage()
        {
            InitializeComponent();
        }

        async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}