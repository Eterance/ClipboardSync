using ClipboardSync.Common.Helpers;
using ClipboardSync.Common.Services;
using ClipboardSync.Client.Windows.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Design;

namespace ClipboardSync.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static readonly string folderName = "ClipboardSync_Desktop";
        internal static WindowsSettingsService WindowsSettingsService { get; set; } = new(
            new LocalPinnedListFileHelper(folderName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName)
            );
        internal static AuthenticationService AuthenticationService { get; set; } = new(WindowsSettingsService);

        public App()
        {
            InitializeComponent();
            Localization.Resources.Culture = new CultureInfo(WindowsSettingsService.Get("Localization", "en"));
        }

    }
}
