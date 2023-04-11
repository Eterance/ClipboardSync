using ClipboardSync.Common.ExtensionMethods;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.Localization;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardSync.Common.ViewModels
{
    public class ClipboardManagementViewModel : ViewModelBase
    {
        public bool IsConnected 
        {
            get => _isConnected;
            private set
            {
                SetValue(ref _isConnected, value);
            }
        }
        /// <summary>
        /// On WPF, ObservableCollection cannot modify out of the dispatch thread
        /// If UIDispatcherInvoker assigned, meaning all ObservableCollection modification will using this DispatcherInvoke
        /// </summary>
        public Action<Action> UIDispatcherInvoker { get; set; }
        public EventHandler<string> NeedClipboardSetText { get; set; }
        public EventHandler<Exception> UnexpectedError { get; set; }
        public Action<string> Toast { get; set; }
        public bool SuppressSendTextToastMessage { get; set; } = false;

        public ObservableCollection<string> HistoryList { get; set; }
        public ObservableCollection<string> PinnedList { get; set; }
        public string IPEndPointsString
        {
            get => _ipEndPointsString;
            set 
            {
                SetValue(ref _ipEndPointsString, value);
            }
        }
        
        public int HistoryListCapacity 
        {
            get =>  _historyListCapacity;
            set
            {
                SetValue(ref _historyListCapacity, value);
            }
        }
        
        public int ServerCacheCapacity 
        {
            get => _serverCacheCapacity;
            set 
            {
                SetValue(ref _serverCacheCapacity, value);
            }
        }
        
        
        public string ConnectionStatusInstruction
        {
            get => _connectionStatusInstruction;
            set
            {
                SetValue(ref _connectionStatusInstruction, value);
            }
        }

        public ISettingsService SettingsService
        {
            get => settingsService;
        }

        // Because Xamarin.Forms.Command can't use at WPF
        // Use a cross-platform command
        // https://prismlibrary.com/docs/commands/commanding.html
        public DelegateCommand SaveAndConnectCommand { get; private set; }
        public DelegateCommand ApplyServerCacheCapacityCommand { get; private set; }
        public DelegateCommand ApplyHistoryListCapacityCommand { get; private set; }
        public DelegateCommand ClearHistoryListCommand { get; private set; }
        /// <summary>
        /// 负责暂时缓存需要发送的信息。
        /// </summary>
        private Queue<string> tempList { get; set; }

        private bool _isConnected = false;
        private string _ipEndPointsString;
        private int _serverCacheCapacity;
        //private int _historyListCapacity = Preferences.Get(_historyListCapacityKey, 30);
        private int _historyListCapacity = 30;
        private string _connectionStatusInstruction;
        private static readonly string _ipEndPointsKey = "IPEndPoints";
        private static readonly string _historyListCapacityKey = "HistoryListCapacity";        
        private SignalRCoreService _signalRCoreService;
        
        private CancellationTokenSource conncetingTokenSource;
        private ISettingsService settingsService;

        public ClipboardManagementViewModel(
            ISettingsService settingsService = null,
            SignalRCoreService service = null,
            Action<Action> uiDispatcherInvoker = null,
            Action<string> toast = null
            )
        {
            this.settingsService = settingsService ?? this.settingsService;
            _signalRCoreService = service ?? new SignalRCoreService();
            UIDispatcherInvoker = uiDispatcherInvoker ?? UIDispatcherInvoker;
            Toast = toast ?? Toast;
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
                    Toast($"{Resources.ServerCacheCapacityChanged2}{Resources.Unlimited}{Resources.Period}");
                    Toast?.Invoke(
                        $"{Resources.ServerCacheCapacityChanged2}{Resources.Unlimited}{Resources.Period}"
                        );
                }
                else 
                {
                    Toast?.Invoke(
                        $"{Resources.ServerCacheCapacityChanged2}{_serverCacheCapacity}{Resources.Period}"
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
            HistoryListCapacity = this.settingsService.Get(_historyListCapacityKey, 30);
            // System.InvalidOperationException: 'Cannot change ObservableCollection during a CollectionChanged event.'
            //HistoryList.CollectionChanged += (sender, e) => CheckHistoryListCapacity();
            PinnedList = new(this.settingsService.PinnedListFile.Load<string>());
            PinnedList.CollectionChanged += (sender, e) => SavePinnedList();

            
            SaveAndConnectCommand = new DelegateCommand(SetIPEndPointsAsync);
            ApplyServerCacheCapacityCommand = new DelegateCommand(ApplyServerCacheCapacity);
            ApplyHistoryListCapacityCommand = new DelegateCommand(ApplyHistoryListCapacity);
            ClearHistoryListCommand = new DelegateCommand(ClearHistoryList);
        }

        private void ApplyServerCacheCapacity()
        {
            _ = _signalRCoreService.SetServerCacheCapacity(ServerCacheCapacity);
        }

        private void ApplyHistoryListCapacity()
        {
            settingsService.Set(_historyListCapacityKey, HistoryListCapacity);
            UseUIDispatcherInvoke((Action)delegate // <--- HERE
            {
                HistoryList.ApplyCapacityLimit(HistoryListCapacity);
            });
            if (HistoryListCapacity <= 0)
            {
                Toast?.Invoke($"{Resources.ClipboardHistoryCapacityChanged2}{Resources.Unlimited}{Resources.Period}");
            }
            else
            {
                Toast?.Invoke($"{Resources.ClipboardHistoryCapacityChanged2}{HistoryListCapacity}{Resources.Period}");
            }
        }

        private void ClearHistoryList()
        {
            UseUIDispatcherInvoke((Action)delegate // <--- HERE
            {
                HistoryList.Clear();
            });
        }

        private void SavePinnedList()
        {
            settingsService.PinnedListFile.Save(new List<string>(PinnedList));
        }

        public void Initialize()
        {
            ConnectionStatusInstruction = Resources.NotConnected;
            
            HistoryListCapacity = settingsService.Get(_historyListCapacityKey, 30);

            bool hasIpKey = settingsService.IsContainsKey(_ipEndPointsKey);
            if (hasIpKey == true)
            {
                IPEndPointsString = settingsService.Get(_ipEndPointsKey, "");
                _ = ConnectAsync();
            }
            else 
            {
                IPEndPointsString = "";
                ConnectionStatusInstruction = Resources.NoServerAddr;
            }
        }

        private async void SetIPEndPointsAsync()
        {
            IPEndPointsString = IPEndPointsString.Trim();
            settingsService.Set(_ipEndPointsKey, IPEndPointsString);
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
            string ipEndPointsString = settingsService.Get(_ipEndPointsKey, "");
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
        /// In WPF platform, change binding list outside Ui thread is illegal.
        /// </summary>
        /// <param name="act"></param>
        private void UseUIDispatcherInvoke(Action act)
        {
            // https://stackoverflow.com/questions/18331723/this-type-of-collectionview-does-not-support-changes-to-its-sourcecollection-fro
            if (UIDispatcherInvoker != null)
            {
                UIDispatcherInvoker(act);
            }
            else
            {
                act();
            }
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
                UseUIDispatcherInvoke((Action)delegate // <--- HERE
                {
                    HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                });                
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
                NeedClipboardSetText?.Invoke(this, message);
                return true;
            }
            // 剪贴板有，并且不在第一位，移到前面
            else if (HistoryList.Contains(message) == true && PinnedList.Contains(message) != true && HistoryList[0] != message)
            {
                UseUIDispatcherInvoke((Action)delegate // <--- HERE
                {
                    HistoryList.Remove(message);
                    HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                });
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
                if (!SuppressSendTextToastMessage)
                { 
                    Toast?.Invoke(Resources.Sent);
                }
                AddNewHistory(text);
            }
        }

        /// <summary>
        /// Move the message from HistoryList to the top of PinnedList.
        /// </summary>
        /// <param name="message"></param>
        public void Pin(string message)
        {
            if (HistoryList.Contains(message))
            {
                UseUIDispatcherInvoke((Action)delegate // <--- HERE
                {
                    HistoryList.Remove(message);
                    PinnedList.Insert(0, message);
                });
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
                UseUIDispatcherInvoke((Action)delegate // <--- HERE
                {
                    PinnedList.Remove(message);
                    HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                });
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
            }
        }
    }
}
