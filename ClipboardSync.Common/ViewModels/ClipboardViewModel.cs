using ClipboardSync.Common.ExtensionMethods;
using ClipboardSync.Common.Localization;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardSync.Common.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using ClipboardSync.Common.Services;
using ClipboardSync.Common.Exceptions;

namespace ClipboardSync.Common.ViewModels
{
    public class ClipboardViewModel : ViewModelBase
    {
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                SetValue(ref _isConnected, value);
            }
        }
        public bool IsInitialized

        {
            get => _isInitialized;
            private set
            {
                SetValue(ref _isInitialized, value);
            }
        }
        /// <summary>
        /// On WPF, ObservableCollection cannot modify out of the dispatch thread
        /// If UIDispatcherInvoker assigned, meaning all ObservableCollection modification will using this DispatcherInvoke
        /// </summary>
        public Action<Action>? UIDispatcherInvoker { get; set; }
        public EventHandler<string>? NeedClipboardSetText { get; set; }
        public EventHandler<Exception>? UnexpectedError { get; set; }
        public Action<string>? Toast { get; set; }
        public bool SuppressSendTextToastMessage { get; set; } = false;

        public ObservableCollection<string>? HistoryList
        {
            get => _historyList;
            private set
            {
                SetValue(ref _historyList, value);
            }
        }
        public ObservableCollection<string>? PinnedList
        {
            get => _pinnedList;
            set => SetValue(ref _pinnedList, value);
        }
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
            get => _historyListCapacity;
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
            get => _settingsService;
        }

        // Because Xamarin.Forms.Command can't use at WPF
        // Use a cross-platform command
        // https://prismlibrary.com/docs/commands/commanding.html
        public DelegateCommand SaveAndConnectCommand { get; private set; }
        public DelegateCommand ApplyServerCacheCapacityCommand { get; private set; }
        public DelegateCommand ApplyHistoryListCapacityCommand { get; private set; }
        public DelegateCommand ClearHistoryListCommand { get; private set; }
        public Action<string, TaskCompletionSource<bool>>? LoginMethod { get; set; }

        private bool _isConnected = false;
        private bool _isInitialized = false;
        private string _ipEndPointsString = "";
        private int _serverCacheCapacity;
        //private int _historyListCapacity = Preferences.Get(_historyListCapacityKey, 30);
        private int _historyListCapacity = 30;
        private string _connectionStatusInstruction = "";
        private static readonly string _ipEndPointsKey = "IPEndPoints";
        private static readonly string _historyListCapacityKey = "HistoryListCapacity";
        private ClipboardSignalRService _signalRCoreService;

        private CancellationTokenSource? conncetingTokenSource;
        private ISettingsService _settingsService;
        private AuthenticationService _authService;
        private ObservableCollection<string>? _historyList;
        private ObservableCollection<string>? _pinnedList;

        public ClipboardViewModel(
            ISettingsService settingsService,
            ClipboardSignalRService signalrService,
            AuthenticationService authService,
            Action<Action>? uiDispatcherInvoker = null,
            Action<string>? toast = null
            )
        {
            _settingsService = settingsService;
            _authService = authService;
            _signalRCoreService = signalrService;
            UIDispatcherInvoker = uiDispatcherInvoker ?? UIDispatcherInvoker;
            Toast = toast ?? Toast;
            _signalRCoreService.ConnectStatusUpdate += (sender, e) => ConnectionStatusInstruction = e;
            _signalRCoreService.MessagesSync += SyncMessagesAsync;
            _signalRCoreService.MessageReceived += ReceiveMessage;
            _signalRCoreService.ServerCacheCapacityUpdated += (sender, e) =>
            {
                ServerCacheCapacity = e;
                if (e <= 0)
                {
                    Toast?.Invoke($"{Resources.ServerCacheCapacityChanged2}{Resources.Unlimited}{Resources.Period}");
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
            _signalRCoreService.LostConnection += async (sender, e) =>
            {
                await TryConnectAllUrlAsync();
            };

            HistoryList = new();
            HistoryListCapacity = _settingsService.Get(_historyListCapacityKey, 30);

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
            _settingsService.Set(_historyListCapacityKey, HistoryListCapacity);
            UseUIDispatcherInvoke(delegate // <--- HERE
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
            UseUIDispatcherInvoke(delegate // <--- HERE
            {
                HistoryList.Clear();
            });
        }

        private void SavePinnedList()
        {
            _settingsService.PinnedListFileHelper.Save(new List<string>(PinnedList));
        }

        public async void Initialize()
        {
            if (_isInitialized) return;
            PinnedList = new(await _settingsService.PinnedListFileHelper.Load());
            PinnedList.CollectionChanged += (sender, e) => SavePinnedList();
            HistoryListCapacity = _settingsService.Get(_historyListCapacityKey, 30);
            ConnectionStatusInstruction = Resources.NotConnected;
            IPEndPointsString = _settingsService.Get(_ipEndPointsKey, "");

            IsConnected = false;
            _isInitialized = true;
        }

        public async void Initialize(string serverUrl)
        {
            if (_isInitialized) return;
            PinnedList = new(await _settingsService.PinnedListFileHelper.Load());
            PinnedList.CollectionChanged += (sender, e) => SavePinnedList();
            ConnectionStatusInstruction = Resources.NotConnected;
            var connectTask = ConnectAsync(serverUrl);
            // System.InvalidOperationException: 'Cannot change ObservableCollection during a CollectionChanged event.'
            //HistoryList.CollectionChanged += (sender, e) => CheckHistoryListCapacity();
            //connectTask.Wait();
            _isInitialized = true;
        }

        private async void SetIPEndPointsAsync()
        {
            IPEndPointsString = IPEndPointsString.Trim();
            _settingsService.Set(_ipEndPointsKey, IPEndPointsString);
            await TryConnectAllUrlAsync();
        }

        public async Task TryConnectAllUrlAsync()
        {
            bool hasIpKey = _settingsService.IsContainsKey(_ipEndPointsKey);
            IsConnected = false;
            if (hasIpKey == true)
            {
                string ipEndPointsString = _settingsService.Get(_ipEndPointsKey, "");
                conncetingTokenSource = new();
                foreach (string ipEndpoint in SeperateIPEndPoints(ipEndPointsString))
                {
                    bool result = await ConnectAsync($"http://{ipEndpoint}/");
                    if (result == true)
                    {
                        IsConnected = true;
                        return;
                    }
                }
            }
            else
            {
                IPEndPointsString = "";
                ConnectionStatusInstruction = Resources.NoServerAddr;
            }
        }

        public async Task<bool> ConnectAsync(string url)
        {
            if (conncetingTokenSource != null)
            {
                conncetingTokenSource.Cancel();
            }
            IsConnected = false;
            _authService.ServerUrl = url;
            var connectivity = await _authService.Ping();
            if (connectivity == false)
            {
                return false;
            }
            while (true)
            {
                try
                {
                    string accessToken = await _authService.GetAccessTokenAsync();
                    // Got Access Token
                    conncetingTokenSource = new();
                    try
                    {
                        return await _signalRCoreService.ConnectAsync($"{url}ServerHub", accessToken, conncetingTokenSource.Token);
                    }
                    catch (Exception ex) // 401 unauthorize, need re-login
                    {
                        // delete stored jwt tokens to go to login
                        await _authService.DeleteTokensPairAsync();
                    }
                }
                catch (HttpRequestException hre)
                {
                    //TODO 
                    throw new NotImplementedException();
                }
                catch (NeedLoginException nle)
                {
                    if (LoginMethod == null)
                    {
                        throw new NeedLoginException(url);
                    }
                    // https://blog.51cto.com/u_11283245/5236975
                    TaskCompletionSource<bool> completionToken = new();
                    LoginMethod(url, completionToken);
                    var loginResult = await completionToken.Task;
                    if (loginResult == false) // User cancal login this server
                    {
                        return false;
                    }
                    else
                    {
                        // Re-login completed, and got Access Token next turn
                        continue;
                    }
                }
            }
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
            try
            {
                // 两张表里都没有，加进剪贴板
                if (HistoryList.Contains(message) != true && PinnedList.Contains(message) != true)
                {
                    UseUIDispatcherInvoke(delegate // <--- HERE
                    {
                        HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                    });
                    OnPropertyChanged();
                    //HistoryList.Insert(0, message);
                    //CheckHistoryListCapacity();
                    NeedClipboardSetText?.Invoke(this, message);
                    return true;
                }
                // 剪贴板有，并且不在第一位，移到前面
                else if (HistoryList.Contains(message) == true && PinnedList.Contains(message) != true && HistoryList[0] != message)
                {
                    UseUIDispatcherInvoke(delegate // <--- HERE
                    {
                        HistoryList.Remove(message);
                        HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                    });
                    OnPropertyChanged();
                    //HistoryList.Insert(0, message);
                    //CheckHistoryListCapacity();
                    NeedClipboardSetText?.Invoke(this, message);
                    return false;
                }
            }
            catch (Exception ex)
            {
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
            if (text == null || text.Trim() == "") return;
            _ = _signalRCoreService.SendMessage(text);
            if (HistoryList.Contains(text) != true && PinnedList.Contains(text) != true)
            {
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
                UseUIDispatcherInvoke(delegate // <--- HERE
                {
                    HistoryList.Remove(message);
                    PinnedList.Insert(0, message);
                    OnPropertyChanged();
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
                UseUIDispatcherInvoke(delegate // <--- HERE
                {
                    PinnedList.Remove(message);
                    HistoryList.InsertWithCapacityLimit(0, message, HistoryListCapacity);
                    OnPropertyChanged();
                });
                //HistoryList.Insert(0, message);
                //CheckHistoryListCapacity();
            }
        }
    }

}
