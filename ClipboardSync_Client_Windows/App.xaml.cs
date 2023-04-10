using ClipboardSync_Client_Windows.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ClipboardSync_Client_Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static WindowsSettingsService WindowsSettingsService { get; set; } = new();

        public App()
        {
            InitializeComponent();
            Localization.Resources.Culture = new CultureInfo(WindowsSettingsService.Get("Localization", "en"));
            ClipboardSync.Common.Localization.Resources.Culture = new CultureInfo(WindowsSettingsService.Get("Localization", "en"));
        }

    }
}
