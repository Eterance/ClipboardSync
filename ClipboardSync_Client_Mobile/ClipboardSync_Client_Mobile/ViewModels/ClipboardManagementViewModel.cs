using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using ClipboardSync.Commom.ExtensionMethods;
using ClipboardSync.Commom.Services;

namespace ClipboardSync_Client_Mobile.ViewModels
{
    public class ClipboardManagementViewModel : ViewModelBase
    {
        public bool IsConnected 
        {
            get
            {
                return _isConnected;
            }
            private set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
        public EventHandler<string> NeedClipboardSetText { get; set; }
        public EventHandler<Exception> UnexpectedError { get; set; }
        public EventHandler<string> ToastMessage { get; set; }

        public ObservableCollection<string> HistoryList { get; set; }
        public ObservableCollection<string> PinnedList { get; set; }
        public string IPEndPointsString
        {
            get 
            { 
                return _ipEndPointsString; 
            }
            set 
            {
                _ipEndPointsString = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }
        
        public int HistoryListCapacity 
        {
            get
            {
                return _historyListCapacity;
                //return Preferences.Get(nameof(HistoryListCapacity), 30);
            }
            set
            {
                _historyListCapacity = value;
                OnPropertyChanged();
            }
        }
        
        public int ServerCacheCapacity 
        {
            get { return _serverCacheCapacity; }
            set 
            { 
                _serverCacheCapacity = value; 
                OnPropertyChanged();
            }
        }
        
        
        public string ConnectionStatusInstruction
        {
            get { return _connectionStatusInstruction; }
            set
            {
                _connectionStatusInstruction = value;
                OnPropertyChanged();
            }
        }
            
        public ICommand SaveAndConnectCommand { get; set; }
        public ICommand ApplyServerCacheCapacityCommand { get; set; }
        public ICommand ApplyHistoryListCapacityCommand { get; set; }
        /// <summary>
        /// 负责暂时缓存需要发送的信息。
        /// </summary>
        private Queue<string> tempList { get; set; }

        private bool _isConnected = false;
        private string _ipEndPointsString;
        private int _serverCacheCapacity;
        private int _historyListCapacity = Preferences.Get(_historyListCapacityKey, 30);
        private string _connectionStatusInstruction;
        private static readonly string _ipEndPointsKey = "IPEndPoints";
        private static readonly string _historyListCapacityKey = "HistoryListCapacity";        
        private SignalRCoreService _signalRCoreService;
        
        private CancellationTokenSource conncetingTokenSource;

        public ClipboardManagementViewModel(SignalRCoreService service = null)
        {
            //Singleton = this;
            _signalRCoreService = service ?? new SignalRCoreService();
            //_signalRCoreService.ConnectSuccessed += (sender, e) => ConnectionStatusInstruction = $"已连接到 {e}";
            //_signalRCoreService.ConnectFailed += (sender, e) => ConnectionStatusInstruction = $"error: {e}";
            _signalRCoreService.ConnectStatusUpdate += (sender, e) => ConnectionStatusInstruction = e;
            _signalRCoreService.MessagesSync += SyncMessagesAsync;
            _signalRCoreService.MessageReceived += ReceiveMessage;
            _signalRCoreService.ServerCacheCapacityUpdated += (sender, e) =>
            {
                ServerCacheCapacity = e;
                if (e <= 0)
                {
                    ToastMessage?.Invoke(
                        this,
                        $"{Localization.Resources.ServerCacheCapacityChanged2}{Localization.Resources.Unlimited}{Localization.Resources.Period}"
                        );
                }
                else 
                {
                    ToastMessage?.Invoke(
                        this, 
                        $"{Localization.Resources.ServerCacheCapacityChanged2}{_serverCacheCapacity}{Localization.Resources.Period}"
                        );
                }
                
            };
            _signalRCoreService.UnexpectedError += (sender, e) => UnexpectedError?.Invoke(sender, e);
            _signalRCoreService.Connected += (sender, e) => IsConnected = true;
            _signalRCoreService.ConnectFailed += (sender, e) => IsConnected = false;
            _signalRCoreService.LostConnection += async (sender, e) =>
            {
                await ConnectAsync();
            };


            HistoryList = new();
            // System.InvalidOperationException: 'Cannot change ObservableCollection during a CollectionChanged event.'
            //HistoryList.CollectionChanged += (sender, e) => CheckHistoryListCapacity();
            PinnedList = new(PinnedListFileService.Load<string>());
            PinnedList.CollectionChanged += (sender, e) => SavePinnedList();

            
            SaveAndConnectCommand = new Command(SetIPEndPointsAsync);
            ApplyServerCacheCapacityCommand = new Command(ApplyServerCacheCapacity);
            ApplyHistoryListCapacityCommand = new Command(ApplyHistoryListCapacity);
        }

        private void ApplyServerCacheCapacity()
        {
            _ = _signalRCoreService.SetServerCacheCapacity(ServerCacheCapacity);
        }

        private void ApplyHistoryListCapacity()
        {
            Preferences.Set(_historyListCapacityKey, HistoryListCapacity);
            HistoryList.ApplyCapacityLimit(HistoryListCapacity);
            if (HistoryListCapacity <= 0)
            {
                ToastMessage?.Invoke(this, $"{Localization.Resources.ClipboardHistoryCapacityChanged2}{Localization.Resources.Unlimited}{Localization.Resources.Period}");
            }
            else
            {
                ToastMessage?.Invoke(this, $"{Localization.Resources.ClipboardHistoryCapacityChanged2}{HistoryListCapacity}{Localization.Resources.Period}");
            }
        }

        private void SavePinnedList()
        {
            PinnedListFileService.Save(new List<string>(PinnedList));
        }

        public void Initialize()
        {
            ConnectionStatusInstruction = Localization.Resources.NotConnected;
            
            HistoryListCapacity = Preferences.Get(_historyListCapacityKey, 30);

            bool hasIpKey = Preferences.ContainsKey(_ipEndPointsKey);
            if (hasIpKey == true)
            {
                IPEndPointsString = Preferences.Get(_ipEndPointsKey, "");
                _ = ConnectAsync();
            }
            else 
            {
                IPEndPointsString = "";
                ConnectionStatusInstruction = Localization.Resources.NoServerAddr;
            }
        }

        private async void SetIPEndPointsAsync()
        {
            IPEndPointsString = IPEndPointsString.Trim();
            Preferences.Set(_ipEndPointsKey, IPEndPointsString);
            await ConnectAsync();
        }

        public async Task ConnectAsync()
        {
            // 
            if (conncetingTokenSource != null)
            {
                conncetingTokenSource.Cancel();
            }
            IsConnected = false;
            string ipEndPointsString = Preferences.Get(_ipEndPointsKey, "");
            conncetingTokenSource = new();
            await _signalRCoreService.ConnectAsync(SeperateIPEndPoints(ipEndPointsString), conncetingTokenSource.Token);
        }
        

        private List<string> SeperateIPEndPoints(string ipEndPointsString)
        {
            // ipEndPoints: ip1:port1; ip2:port2; etc...
            // seprate by ';' and trim
            List<string> ipEndPointList = new();
            string[] ipEndPointArray = ipEndPointsString.Split(';');
            foreach (string ipEndPoint in ipEndPointArray)
            {
                ipEndPointList.Add(ipEndPoint.Trim());
            }
            return ipEndPointList;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns>是否成功添加新消息。如果消息已经存在，返回 false。</returns>
        private bool AddNewHistory(string message)
        {
            // 两张表里都没有，加进剪贴板
            if (HistoryList.Contains(message) != true && PinnedList.Contains(message) != true)
            {
                HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
                NeedClipboardSetText?.Invoke(this, message);
                return true;
            }
            // 剪贴板有，并且不在第一位，移到前面
            else if (HistoryList.Contains(message) == true && PinnedList.Contains(message) != true && HistoryList[0] != message)
            {
                HistoryList.Remove(message);
                HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
                NeedClipboardSetText?.Invoke(this, message);
                return false;
            }
            return false;
        }

        private void SyncMessagesAsync(object sender, List<string> messages)
        {
            int addNew = 0;
            foreach (var message in messages)
            {
                bool added = AddNewHistory(message);
                if (added == true)
                {
                    addNew++;
                }
            }
        }

        private void ReceiveMessage(object sender, string message)
        {
            AddNewHistory(message);
        }

        public void SendText(string text)
        {
            if (HistoryList.Contains(text) != true && PinnedList.Contains(text) != true)
            {
                _ = _signalRCoreService.SendMessage(text);
                ToastMessage?.Invoke(this, Localization.Resources.Sent);
                AddNewHistory(text);
            }
        }

        /// <summary>
        /// Move the message from HistoryList to the top of PinnedList.
        /// </summary>
        /// <param name="message"></param>
        public void PinFromHistory(string message)
        {
            if (HistoryList.Contains(message))
            {
                HistoryList.Remove(message);
                PinnedList.Insert(0, message);
            }
        }

        /// <summary>
        /// Move the message from PinnedList to the top of HistoryList.
        /// </summary>
        /// <param name="message"></param>
        public void Unpin(string message)
        {
            if (PinnedList.Contains(message))
            {
                PinnedList.Remove(message);
                HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
            }
        }
    }
}
