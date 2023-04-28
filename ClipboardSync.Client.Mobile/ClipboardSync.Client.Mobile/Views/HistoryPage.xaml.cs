using ClipboardSync.Client.Mobile.Services;
using ClipboardSync.Client.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ClipboardSync.Client.Mobile.Localization;

namespace ClipboardSync.Client.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryPage : ContentPage
	{
		public HistoryPage ()
		{
			InitializeComponent ();
            BindingContext = App.ClipboardVM;
        }

        
        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                string message = (string)e.CurrentSelection.FirstOrDefault();
                MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Code to run on the main thread
                        await Clipboard.SetTextAsync(message);
                    });
                DependencyService.Get<IToast>().ShortAlert(Localization.Resources.CopyComplete);
/*                string action = await DisplayActionSheet(
                    message,
                    Localization.Resources.Cancel,
                    Localization.Resources.Delete,
                    Localization.Resources.Copy,
                    Localization.Resources.Pin,
                    Localization.Resources.Detail);
                if (action == Localization.Resources.Copy)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Code to run on the main thread
                        await Clipboard.SetTextAsync(message);
                    });
                    DependencyService.Get<IToast>().ShortAlert(Localization.Resources.CopyComplete);
                }
                else if (action == Localization.Resources.Delete)
                {
                    App.ViewModel.HistoryList.Remove(message);
                }
                else if (action == Localization.Resources.Pin)
                {
                    App.ViewModel.PinFromHistory(message);
                }
                else if (action == Localization.Resources.Detail)
                {
                    await DisplayAlert(Localization.Resources.Detail, message, Localization.Resources.Close);
                }
                (sender as CollectionView).SelectedItem = null;*/
            }
        }
        
        async private void SwipeItem_Invoked_Detail(object sender, EventArgs e)
        {
            var swipeview = sender as SwipeItem;
            string message = swipeview.CommandParameter as string;
            await DisplayAlert(Localization.Resources.Detail, message, Localization.Resources.Close);
        }

        private void SwipeItem_Invoked_Pin(object sender, EventArgs e)
        {
            var swipeview = sender as SwipeItem;
            string message = swipeview.CommandParameter as string;
            App.ClipboardVM.Pin(message);
        }

        private void SwipeItem_Invoked_Delete(object sender, EventArgs e)
        {
            var swipeview = sender as SwipeItem;
            string message = swipeview.CommandParameter as string;
            App.ClipboardVM.HistoryList.Remove(message);
        }
    }
}