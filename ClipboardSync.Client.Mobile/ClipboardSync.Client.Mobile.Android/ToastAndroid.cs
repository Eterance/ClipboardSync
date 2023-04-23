using Android.Widget;
using System.Runtime.Remoting.Messaging;
using ClipboardSync.Client.Mobile.Services;
using ClipboardSync.Client.Mobile;
using Android.App;
using ClipboardSync.Client.Mobile.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace ClipboardSync.Client.Mobile.Droid
{
    public class ToastAndroid : IToast
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}