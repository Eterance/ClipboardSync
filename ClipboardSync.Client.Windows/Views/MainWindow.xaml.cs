using ClipboardSync.Client.Windows.Services;
using ClipboardSync.Client.Windows.ViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.ViewModels;

namespace ClipboardSync.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClipboardViewModel clipboardVM;
        MainWindowViewModel mainViewModel;
        public static MainWindow? mainWindow;
        Grid? _InOperatingGrid_History;
        Grid? _InOperatingGrid_Pinned;
        string _lastClipBroadMessage = "";

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            ClipBroadChangedEvent += ClipBroadChanged;
            clipboardVM = new ClipboardViewModel(
                settingsService: App.WindowsSettingsService,
                signalrService: new(),
                authService: App.AuthenticationService,
                uiDispatcherInvoker: (act) =>
                    {
                        // https://stackoverflow.com/questions/18331723/this-type-of-collectionview-does-not-support-changes-to-its-sourcecollection-fro
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            act();
                        });
                    }                    
                );
            clipboardVM.NeedClipboardSetText += (sender, e) =>
            {
                // If no Dispatcher.Invoke:
                // System.Threading.ThreadStateException:“必须先将当前线程设置为单个线程单元(STA)模式方可进行 OLE 调用。”
                // System.Threading.ThreadStateException: "Current thread must be set to single thread apartment (STA) mode before OLE calls can be made. "
                Dispatcher.Invoke(() =>
                    Clipboard.SetText(e)
                );                
            };
            clipboardVM.Initialize();
            clipboardVM.LoginMethod = async (url, completionToken) =>
            {
                // https://learn.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/navigation/modal
                LoginWindow loginWindow = new(url, completionToken, App.AuthenticationService);
                loginWindow.Owner = this;
                loginWindow.ShowDialog();
            };


            mainViewModel = new MainWindowViewModel(clipboardVM, App.WindowsSettingsService);
            DataContext = mainViewModel;

            if (clipboardVM.IsConnected == false)
            {
                _ = clipboardVM.TryConnectAllUrlAsync();
            }
        }

        private void ErrorOcurred(object? sender, string message)
        {
            MessageBox.Show(message);
        }

        private void ListBox_History_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ListBox_Pinned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_InOperatingGrid_Pinned != null)
            {
                _InOperatingGrid_Pinned.Visibility = Visibility.Hidden;
            }
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (_InOperatingGrid_History != null)
            {
                _InOperatingGrid_History.Visibility = Visibility.Hidden;
            }

            if (_InOperatingGrid_Pinned != null)
            {
                _InOperatingGrid_Pinned.Visibility = Visibility.Hidden;
            }
            foreach (var child in ((Grid)sender).Children)
            {
                if ((child is Grid) && (((Grid)child).Name == "Control_Grid"))
                {
                    ((Grid)child).Visibility = Visibility.Visible;
                    if (ListBox_HistoryList.Visibility == Visibility.Visible)
                    {
                        _InOperatingGrid_History = (Grid)child;
                    }
                    else if (ListBox_PinnedList.Visibility == Visibility.Visible)
                    {
                        _InOperatingGrid_Pinned = (Grid)child;
                    }
                    break;
                }
            }
        }

        private void Grid_History_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //string history = ListBox_HistoryList.SelectedItem as string;
            string text = FindSubTextBlockUnderGrid((Grid)sender);
            Clipboard.SetText(text);
            Popup_ClipBroad.IsOpen = false;
            System.Windows.Forms.SendKeys.SendWait("^v");
        }

        private string FindSubTextBlockUnderGrid(Grid grid)
        {
            foreach (var child in ((Grid)grid).Children)
            {
                if ((child is TextBlock))
                {
                    return ((TextBlock)child).Text;
                }
            }
            return "";
        }
        
        private TResult? GetControlCommandParameter<TResult>(Button control) where TResult: class
        {
            TResult? message = control.CommandParameter as TResult;
            return message;
        }


        static bool IsTypeHasCommandParameter(Type type)
        {
            if ((type == typeof(ButtonBase)) || (type == typeof(Grid)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetChoosenItemText(object sender)
        {
            Grid grandpa = (Grid)((Grid)((Button)sender).Parent).Parent;
            List<TextBlock> result = FindChildControl.GetChildObjects<TextBlock>(grandpa, typeof(TextBlock));
            return result[0].Text;
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Grid parent = (Grid)((Button)sender).Parent;
            parent.Visibility = Visibility.Hidden;
        }

        private void Delete_History_Button_Click(object sender, RoutedEventArgs e)
        {
            clipboardVM.HistoryList.Remove(GetControlCommandParameter<string>(sender as Button));
        }
        
        private void Detail_Button_Click(object sender, RoutedEventArgs e)
        {
            DetailWindow detailWindow = new(GetControlCommandParameter<string>(sender as Button));
            detailWindow.ShowDialog();
        }

        private void Unpin_Button_Click(object sender, RoutedEventArgs e)
        {
            clipboardVM.Unpin(GetControlCommandParameter<string>(sender as Button));
        }
        
        private void Delete_Pinned_Button_Click(object sender, RoutedEventArgs e)
        {
            //string history = ListBox_PinnedList.SelectedItem as string;
            clipboardVM.Unpin(GetControlCommandParameter<string>(sender as Button));
        }

        private void Pin_Button_Click(object sender, RoutedEventArgs e)
        {
            clipboardVM.Pin(GetControlCommandParameter<string>(sender as Button));
        }

        // https://blog.csdn.net/gfg2007/article/details/108898788
        #region 消息钩子预定义参数
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x30D;

        private IntPtr mNextClipBoardViewerHWnd;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool ChangeClipboardChain(IntPtr HWnd, IntPtr HWndNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        #endregion

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            //挂消息钩子
            mNextClipBoardViewerHWnd = SetClipboardViewer(source.Handle);
            source.AddHook(WndProc);

            //注册键盘快捷键
            Hotkey.Regist(this, HotkeyModifiers.MOD_ALT, Key.V, OnHotKeyAltV);
        }

        /// <summary>
        ///  参考：https://blog.csdn.net/xlm289348/article/details/8050957
        ///  MSG=0x308无法收到消息，原因是0x308是在剪贴板发生变化时将消息发送到监听列表中的第一个窗口
        ///  所有这里要收到0x308必须将窗口放到监听列表里
        ///  即在OnSourceInitialized(EventArgs e) 中调用SetClipboardView
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    {
                        SendMessage(mNextClipBoardViewerHWnd, msg, wParam.ToInt32(), lParam.ToInt32());
                        //文本内容检测
                        if (System.Windows.Clipboard.ContainsText())
                        {
                            //System.Windows.Clipboard.GetText()此处有Bug
                            //String ct = System.Windows.Clipboard.GetText();
                            string ct = getClipboardText();
                            if (ct != _lastClipBroadMessage)
                            {
                                _lastClipBroadMessage = ct;
                                ClipBroadChangedEvent?.Invoke(this, ct);
                            }
                        }
                    }
                    break;
                case WM_CHANGECBCHAIN:
                    {
                        if (wParam == (IntPtr)mNextClipBoardViewerHWnd)
                        {
                            mNextClipBoardViewerHWnd = lParam;
                        }
                        else
                        {
                            SendMessage(mNextClipBoardViewerHWnd, msg, wParam.ToInt32(), lParam.ToInt32());
                        }
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
        
        private String getClipboardText()
        {
            for (int i = 0; i < 200; i++)
            {
                try
                {
                    return System.Windows.Clipboard.GetText();
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);//这句加不加都没关系
                }
            }
            return String.Empty;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            //移除消息钩子
            ChangeClipboardChain(source.Handle, mNextClipBoardViewerHWnd);
            source.RemoveHook(WndProc);

            //注销键盘快捷键
            Hotkey.UnRegist(this, OnHotKeyAltV);
            // https://learn.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop#step-4-handling-uninstallation
            ToastNotificationManagerCompat.Uninstall();
        }

        public EventHandler<string> ClipBroadChangedEvent;

        private async void ClipBroadChanged(object? sender, string e)
        {
            await mainViewModel.SendClipboardTextAsync(e);
        }
        
        private void OnHotKeyAltV()
        {
            // convert Popup_ClipBroad.IsOpen
            Popup_ClipBroad.IsOpen = !Popup_ClipBroad.IsOpen;
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Popup_ClipBroad.IsOpen = false;
        }

        private void Popup_ClipBroad_LostFocus(object sender, RoutedEventArgs e)
        {
            //Popup_ClipBroad.IsOpen = false;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Topmost = true;
            Popup_ClipBroad.IsOpen = false;
            this.Topmost = false;
        }
    }
}
