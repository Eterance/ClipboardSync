using ClipboardSync_Client_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ClipboardSync_Client_Mobile.Localization;
using ClipboardSync_Client_Mobile.Services;

namespace ClipboardSync_Client_Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PinnedPage : ContentPage
	{
		public PinnedPage ()
		{
			InitializeComponent ();
            BindingContext = App.ClipboardManagementViewModel;
        }

        private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                // Navigate to the NoteEntryPage, passing the filename as a query parameter.
                string message = (string)e.CurrentSelection.FirstOrDefault();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Code to run on the main thread
                    await Clipboard.SetTextAsync(message);
                });
                DependencyService.Get<IToast>().ShortAlert(Localization.Resources.CopyComplete);
                /*string action = await DisplayActionSheet(message,
                    Localization.Resources.Cancel,
                    Localization.Resources.Delete,
                    Localization.Resources.Copy,
                    Localization.Resources.Unpin,
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
                    App.ViewModel.PinnedList.Remove(message);
                }
                else if (action == Localization.Resources.Unpin)
                {
                    App.ViewModel.Unpin(message);
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

        private void SwipeItem_Invoked_Unpin(object sender, EventArgs e)
        {
            var swipeview = sender as SwipeItem;
            string message = swipeview.CommandParameter as string;
            App.ClipboardManagementViewModel.Unpin(message);
        }

        private void SwipeItem_Invoked_Delete(object sender, EventArgs e)
        {
            var swipeview = sender as SwipeItem;
            string message = swipeview.CommandParameter as string;
            App.ClipboardManagementViewModel.PinnedList.Remove(message);
        }
    }
}