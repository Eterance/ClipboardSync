using ClipboardSync_Client_Mobile.Services;
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

namespace ClipboardSync_Client_Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HistoryPage : ContentPage
	{
		public HistoryPage ()
		{
			InitializeComponent ();
            BindingContext = App.ViewModel;
        }

        
        async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                // Navigate to the NoteEntryPage, passing the filename as a query parameter.
                string message = (string)e.CurrentSelection.FirstOrDefault();
                string action = await DisplayActionSheet(
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
                (sender as CollectionView).SelectedItem = null;
            }
        }
    }
}