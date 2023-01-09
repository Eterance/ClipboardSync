using System;
using System.Collections.Generic;
using System.Text;

namespace ClipboardSync_Client_Mobile.Services
{
    public interface IToast
    {
        // https://stackoverflow.com/questions/35279403/toast-equivalent-for-xamarin-forms
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
