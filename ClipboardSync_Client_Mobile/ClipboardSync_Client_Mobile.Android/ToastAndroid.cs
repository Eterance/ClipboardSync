using Android.Widget;
using System.Runtime.Remoting.Messaging;
using ClipboardSync_Client_Mobile.Services;
using ClipboardSync_Client_Mobile;
using Android.App;
using ClipboardSync_Client_Mobile.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace ClipboardSync_Client_Mobile.Droid
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