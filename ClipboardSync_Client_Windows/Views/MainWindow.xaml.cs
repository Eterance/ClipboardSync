using ClipboardSync.Commom.ViewModels;
using ClipboardSync_Client_Windows.Services;
using ClipboardSync_Client_Windows.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

namespace ClipboardSync_Client_Windows.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SignalRCoreClientLib _signalRCoreClientLib;
        public static MainWindow? mainWindow;
        BindingList<string> _historyList = new();
        BindingList<string> _PinnedList = new();
        Grid _InOperatingGrid_History;
        Grid _InOperatingGrid_Pinned;
        string _lastClipBroadMessage = "";

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            ClipBroadChangedEvent += ClipBroadChanged;

            ClipboardManagementViewModel viewModel = new ClipboardManagementViewModel(new WindowsSettingsService());
            // No need to Toast in desktop platform
            // viewModel.ToastMessage += (sender, e) => {};
            viewModel.Initialize();
            ListBox_HistoryList.ItemsSource = _historyList;
            ListBox_PinnedList.ItemsSource = _PinnedList;
            DataContext = new MainWindowViewModel(viewModel);
        }

        public async void connectServer()
        {
            await _signalRCoreClientLib.Connect(tb_ip.Text, int.Parse(tb_port.Text));
        }

        private void SyncMessages(object? sender, List<string> messages)
        {
            foreach (var message in messages)
            {
                mainWindow.Dispatcher.Invoke((() =>
                {
                    AddNewHistory(message);
                }));
            }
        }
        
        private void ReceiveMessage(object? sender, string message)
        {
            mainWindow.Dispatcher.Invoke((() =>
            {
                AddNewHistory(message);
            }));
            
        }

        private void ErrorOcurred(object? sender, string message)
        {
            MessageBox.Show(message);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //_signalRCoreClientLib.SendMessage(tb1.Text);
            //_historyList.Insert(0, tb1.Text);
            _signalRCoreClientLib = new SignalRCoreClientLib();
            _signalRCoreClientLib.MessagesSync += SyncMessages;
            _signalRCoreClientLib.MessageReceived += ReceiveMessage;
            _signalRCoreClientLib.ErrorOcurr += ErrorOcurred;
            connectServer();
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
        

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Grid parent = (Grid)((Button)sender).Parent;
            parent.Visibility = Visibility.Hidden;
        }

        private void Delete_History_Button_Click(object sender, RoutedEventArgs e)
        {
            //string history = ListBox_HistoryList.SelectedItem as string;
            
            _historyList.Remove(GetChoosenItemText(sender));
        }

        private string GetChoosenItemText(object sender)
        {
            Grid garndpa = (Grid)((Grid)((Button)sender).Parent).Parent;
            List<TextBlock> result = FindChildControl.GetChildObjects<TextBlock>(garndpa, typeof(TextBlock));
            return result[0].Text;
        }
        
        private void Delete_Pinned_Button_Click(object sender, RoutedEventArgs e)
        {
            //string history = ListBox_PinnedList.SelectedItem as string;
            _PinnedList.Remove(GetChoosenItemText(sender));
        }

        private void Pin_Button_Click(object sender, RoutedEventArgs e)
        {
            string history = GetChoosenItemText(sender);
            _historyList.Remove(history);
            _PinnedList.Insert(0, history);
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
        }

        public EventHandler<string> ClipBroadChangedEvent;

        private void ClipBroadChanged(object? sender, string e)
        {
            _ = _signalRCoreClientLib.SendMessage(e);
            AddNewHistory(e);
        }

        
        private void AddNewHistory(string message)
        {
            // 两张表里都没有，加进剪贴板
            if (_historyList.Contains(message) != true && _PinnedList.Contains(message) != true)
            {
                _historyList.Insert(0, message);
            }
            // 剪贴板有，移到前面
            else if (_historyList.Contains(message) == true && _PinnedList.Contains(message) != true)
            {
                _historyList.Remove(message);
                _historyList.Insert(0, message);
            }
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

        private void Delete_All_Button_Click(object sender, RoutedEventArgs e)
        {
            _historyList.Clear();
        }
        
    }
}
