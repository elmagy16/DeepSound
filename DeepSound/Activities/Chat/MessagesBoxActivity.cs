using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using DeepSound.Activities.Chat.Adapters;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.IO;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MessagesBoxActivity : AppCompatActivity
    {
        #region Variables Basic

        private AppCompatImageView ChatEmojisImage;
        private RelativeLayout RootView;
        private EmojiconEditText EmojisIconEditTextView;
        private CircleButton ChatSendButton, ImageButton;
        private static Toolbar TopChatToolBar;
        public RecyclerView ChatBoxRecyclerView;
        private LinearLayoutManager MLayoutManager;
        public static UserMessagesAdapter MAdapter;
        private string LastSeenUser = "", TaskWork = "";
        private long BeforeMessageId, FirstMessageId;
        private static long Userid;// to_id
        private static Timer Timer;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private UserDataObject UserInfoData;
        private HomeActivity GlobalContext; 
        private static MessagesBoxActivity Instance;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                base.OnCreate(savedInstanceState);

                Window?.SetBackgroundDrawableResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Set our view from the "MessagesBox_Layout" layout resource
                SetContentView(Resource.Layout.MessagesBoxLayout);

                Instance = this;

                var data = Intent?.GetStringExtra("UserId") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data)) Userid = int.Parse(data); // to_id

                string json = Intent?.GetStringExtra("UserItem");
                var item = JsonConvert.DeserializeObject<UserDataObject>(json);
                if (item != null) UserInfoData = item;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                //Set Title ToolBar and data chat user After that get messages 
                LoadData_ItemUser();

                AdsGoogle.Ad_RewardedVideo(this);

                GlobalContext = HomeActivity.GetInstance();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static MessagesBoxActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }

                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                    Timer = null!;
                }

                MAdapter?.MessageList.Clear();
                MAdapter?.NotifyDataSetChanged();

                

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        SharedPref.ApplyTheme(SharedPref.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        SharedPref.ApplyTheme(SharedPref.DarkMode);
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Window?.SetBackgroundDrawableResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = FindViewById<RelativeLayout>(Resource.Id.rootChatWindowView);

                ChatEmojisImage = FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojisIconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                ChatSendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                ChatBoxRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyler);
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                ImageButton = FindViewById<CircleButton>(Resource.Id.imageButton);

                if (AppSettings.SetTabDarkTheme)
                {
                    ImageButton.SetColor(Color.Rgb(40, 40, 40));
                }

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                var emojisIcon = new EmojIconActions(this, RootView, EmojisIconEditTextView, ChatEmojisImage);
                emojisIcon.ShowEmojIcon();

                Methods.SetColorEditText(EmojisIconEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                TopChatToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopChatToolBar != null)
                {
                    TopChatToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    TopChatToolBar.SetSubtitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(TopChatToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    TopChatToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {

                MAdapter = new UserMessagesAdapter(this);

                ChatBoxRecyclerView.SetItemAnimator(null);

                MLayoutManager = new LinearLayoutManager(this);
                ChatBoxRecyclerView.SetLayoutManager(MLayoutManager);
                ChatBoxRecyclerView.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ChatSendButton.Touch += Chat_sendButton_Touch;
                    ImageButton.Click += ImageButtonOnClick;
                }
                else
                {
                    ChatSendButton.Touch -= Chat_sendButton_Touch;
                    ImageButton.Click -= ImageButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set ToolBar and data chat user

        //Set ToolBar and data chat user
        private void LoadData_ItemUser()
        {
            try
            {
                if (UserInfoData != null)
                {
                    SupportActionBar.Title = DeepSoundTools.GetNameFinal(UserInfoData);
                    
                    string timeAgo;
                    if (UserInfoData.Time != null)
                        timeAgo = Methods.Time.TimeAgo(UserInfoData.Time.Value, false);
                    else
                        timeAgo = Methods.Time.TimeAgo(Convert.ToInt32(UserInfoData.LastActive), false);

                    SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + timeAgo;
                    LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + timeAgo;
                }

                Get_Messages();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Messages

        //Get Messages Local Or Api
        private void Get_Messages()
        {
            try
            {
                BeforeMessageId = 0;
                MAdapter.MessageList.Clear();
                MAdapter.NotifyDataSetChanged();

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessagesList(UserDetails.UserId, Userid, BeforeMessageId);
                if (localList == "1") //Database.. Get Messages Local
                {
                    MAdapter.NotifyDataSetChanged();

                    //Scroll Down >> 
                    ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
                else //Or server.. Get Messages Api
                {
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                    GetMessages_API();
                }

                //Set Event Scroll
                XamarinRecyclerViewOnScrollListener onScrollListener = new XamarinRecyclerViewOnScrollListener(MLayoutManager, SwipeRefreshLayout);
                onScrollListener.LoadMoreEvent += Messages_OnScroll_OnLoadMoreEvent;
                ChatBoxRecyclerView.AddOnScrollListener(onScrollListener);
                TaskWork = "Working";

                //Run timer
                Timer = new Timer { Interval = AppSettings.MessageRequestSpeed, Enabled = true };
                Timer.Elapsed += TimerOnElapsed_MessageUpdater;
                Timer.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Messages From API 
        private async void GetMessages_API()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;

                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    BeforeMessageId = 0;

                    var (apiStatus, respond) = await RequestsAsync.Chat.GetChatConversationsAsync(Userid.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GetChatConversationsObject result)
                        {
                            if (result.Data.Count > 0)
                            {
                                MAdapter.MessageList = new ObservableCollection<ChatMessagesDataObject>(result.Data);
                                MAdapter.NotifyDataSetChanged();

                                //Insert to DataBase
                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

                                //Scroll Down >> 
                                ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                SwipeRefreshLayout.Refreshing = false;
                                SwipeRefreshLayout.Enabled = false;
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
        }

        #endregion

        //Timer Message Updater >> Get New Message
        private void TimerOnElapsed_MessageUpdater(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Code get last Message id where Updater >>
                MessageUpdater();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Updater

        private async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (!Methods.CheckConnectivity())
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                    else
                    {
                        int countList = MAdapter.MessageList.Count;
                        string afterId = MAdapter.MessageList.LastOrDefault()?.Id.ToString() ?? "";
                        var (apiStatus, respond) = await RequestsAsync.Chat.GetChatConversationsAsync(Userid.ToString(), "30", afterId);
                        if (apiStatus == 200)
                        {
                            if (respond is GetChatConversationsObject result)
                            {
                                int responseList = result.Data.Count;
                                if (responseList > 0)
                                {
                                    if (countList > 0)
                                    {
                                        foreach (var item in from item in result.Data let check = MAdapter.MessageList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                        {
                                            MAdapter.MessageList.Add(item);

                                            int index = MAdapter.MessageList.IndexOf(item);
                                            if (index > -1)
                                            {
                                                RunOnUiThread(() =>
                                                {
                                                    try
                                                    {
                                                        MAdapter.NotifyItemInserted(index);

                                                        //Scroll Down >> 
                                                        ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);
                                                    }
                                                    catch (Exception ee)
                                                    {
                                                        Console.WriteLine(ee);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MAdapter.MessageList = new ObservableCollection<ChatMessagesDataObject>(result.Data);
                                    }

                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var lastCountItem = MAdapter.ItemCount;
                                            if (countList > 0)
                                                MAdapter.NotifyItemRangeInserted(lastCountItem, MAdapter.MessageList.Count - 1);
                                            else
                                                MAdapter.NotifyDataSetChanged();

                                            //Insert to DataBase
                                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

                                            //Scroll Down >> 
                                            ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                            var lastMessage = MAdapter.MessageList.LastOrDefault();
                                            if (lastMessage != null)
                                            {
                                                var dataUser = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.User.Id == Userid);
                                                if (dataUser?.GetLastMessage != null)
                                                {
                                                    dataUser.GetLastMessage.Value.GetLastMessageClass.Text = lastMessage.Text;
                                                    int index = LastChatActivity.MAdapter.UserList.IndexOf(dataUser);

                                                    LastChatActivity.MAdapter.UserList.Move(index, 0);
                                                    LastChatActivity.MAdapter.NotifyItemMoved(index, 0);

                                                    var data = LastChatActivity.MAdapter.UserList.FirstOrDefault(a => a.User.Id == dataUser.User.Id);
                                                    if (data != null)
                                                    {
                                                        data.User = dataUser.User;
                                                        data.GetCountSeen = dataUser.GetCountSeen;
                                                        data.GetLastMessage = dataUser.GetLastMessage;
                                                        data.ChatTime = dataUser.ChatTime;

                                                        LastChatActivity.MAdapter.NotifyItemChanged(LastChatActivity.MAdapter.UserList.IndexOf(data));
                                                    }
                                                }
                                            }

                                            if (AppSettings.RunSoundControl)
                                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");

                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                TaskWork = "Working";
            }
        }

        public static void UpdateOneMessage(ChatMessagesDataObject messages)
        {
            try
            {
                var checker = MAdapter.MessageList.FirstOrDefault(a => a.Id == messages.Id);
                if (checker != null)
                {
                    checker.Id = messages.Id;
                    checker.FromId = messages.FromId;
                    checker.ToId = messages.ToId;
                    checker.Text = messages.Text;
                    checker.Seen = messages.Seen;
                    checker.Time = messages.Time;
                    checker.FromDeleted = messages.FromDeleted;
                    checker.ToDeleted = messages.ToDeleted;
                    checker.SentPush = messages.SentPush;
                    checker.NotificationId = messages.NotificationId;
                    checker.TypeTwo = messages.TypeTwo;
                    checker.Image = messages.Image;
                    checker.FullImage = messages.FullImage;
                    checker.ApiPosition = messages.ApiPosition;
                    checker.ApiType = messages.ApiType;

                    MAdapter.NotifyItemChanged(MAdapter.MessageList.IndexOf(checker));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load More

        private async void LoadMoreMessages()
        {
            try
            {
                //Run Load More Api 
                var local = LoadMoreMessagesDatabase();
                if (local == "1")
                {

                }
                else
                {
                    var api = await LoadMoreMessagesApi();
                    if (api == "1")
                    {

                    }
                    else
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        SwipeRefreshLayout.Enabled = false;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private string LoadMoreMessagesDatabase()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessageList(Convert.ToInt32(UserDetails.UserId), Userid, FirstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    localList = new List<DataTables.MessageTb>(localList.OrderByDescending(a => a.Id));

                    foreach (var message in localList.Select(messages => new ChatMessagesDataObject
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        FromDeleted = messages.FromDeleted,
                        ToDeleted = messages.ToDeleted,
                        SentPush = messages.SentPush,
                        NotificationId = messages.NotificationId,
                        TypeTwo = messages.TypeTwo,
                        Image = messages.Image,
                        FullImage = messages.FullImage,
                        ApiPosition = JsonConvert.DeserializeObject<ApiPosition>(messages.ApiPosition),
                        ApiType = JsonConvert.DeserializeObject<ApiType>(messages.ApiType),
                    }))
                    {
                        MAdapter.MessageList.Insert(0, message);
                        MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(MAdapter.MessageList.FirstOrDefault()));

                        var index = MAdapter.MessageList.FirstOrDefault(a => a.Id == FirstMessageId);
                        if (index == null) continue;
                        MAdapter.NotifyItemChanged(MAdapter.MessageList.IndexOf(index));
                        //Scroll Down >> 
                        ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.IndexOf(index));
                    }
                    return "1";
                }
                return "0";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        private async Task<string> LoadMoreMessagesApi()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Chat.GetChatConversationsAsync(Userid.ToString(), "30", "0", FirstMessageId.ToString()).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is GetChatConversationsObject result)
                        {
                            if (result.Data.Count > 0)
                            {
                                var listApi = new ObservableCollection<ChatMessagesDataObject>();

                                foreach (var messages in result.Data.OrderBy(a => a.Time))
                                {
                                    MAdapter.MessageList.Insert(0, messages);
                                    MAdapter.NotifyItemInserted(MAdapter.MessageList.IndexOf(MAdapter.MessageList.FirstOrDefault()));

                                    var index = MAdapter.MessageList.FirstOrDefault(a => a.Id == FirstMessageId);
                                    if (index != null)
                                    {
                                        MAdapter.NotifyItemChanged(MAdapter.MessageList.IndexOf(index));
                                        //Scroll Down >> 
                                        ChatBoxRecyclerView.ScrollToPosition(MAdapter.MessageList.IndexOf(index));
                                    }

                                    listApi.Insert(0, messages);

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.InsertOrReplaceMessages(listApi);
                                }
                                return "1";
                            }
                            return "0";
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                return "0";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "0";
            }
        }

        #endregion

        #region Events

        //Send Message type => "right_text"
        private void OnClick_OfSendButton()
        {
            try
            {
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                if (string.IsNullOrEmpty(EmojisIconEditTextView.Text) || string.IsNullOrWhiteSpace(EmojisIconEditTextView.Text))
                {

                }
                else
                {
                    //Here on This function will send Text Messages to the user 

                    //remove \n in a string
                    string replacement = Regex.Replace(EmojisIconEditTextView.Text, @"\t|\n|\r", "");

                    if (Methods.CheckConnectivity())
                    {   
                        ChatMessagesDataObject message = new ChatMessagesDataObject
                        {
                            Id = Convert.ToInt32(unixTimestamp),
                            FromId = UserDetails.UserId,
                            ToId = Userid,
                            Text = replacement,
                            Seen = 0,
                            Time = unixTimestamp,
                            FromDeleted = 0,
                            ToDeleted = 0,
                            SentPush = 0,
                            NotificationId = "",
                            TypeTwo = "",
                            Image = "",
                            FullImage = "",
                            ApiPosition = ApiPosition.Right,
                            ApiType = ApiType.Text,
                            Hash = time2
                        };

                        MAdapter.MessageList.Add(message);

                        int index = MAdapter.MessageList.IndexOf(MAdapter.MessageList.Last());
                        if (index > -1)
                        {
                            MAdapter.NotifyItemInserted(index);
                            //Scroll Down >> 
                            ChatBoxRecyclerView.ScrollToPosition(index);
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MessageController.SendMessageTask(Userid, EmojisIconEditTextView.Text, "", time2, UserInfoData) }); 
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }

                    EmojisIconEditTextView.Text = "";
                }

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Send messages >>  type text
        private void Chat_sendButton_Touch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action == MotionEventActions.Down)
                {
                    OnClick_OfSendButton();
                }
                e.Handled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Sent image >> type media (OnActivityResult)
        private void ImageButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
          
        #endregion

        #region Scroll

        //Event Scroll #Messages
        private void Messages_OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Start Loader Get from Database or API Request >>
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                FirstMessageId = 0;

                //Code get first Message id where LoadMore >>
                var mes = MAdapter.MessageList.FirstOrDefault();
                if (mes != null)
                {
                    FirstMessageId = mes.Id;
                }

                if (FirstMessageId > 0)
                {
                    LoadMoreMessages();
                }

                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            private readonly LinearLayoutManager LayoutManager;
            private readonly SwipeRefreshLayout SwipeRefreshLayout;

            public XamarinRecyclerViewOnScrollListener(LinearLayoutManager layoutManager, SwipeRefreshLayout swipeRefreshLayout)
            {
                LayoutManager = layoutManager;
                SwipeRefreshLayout = swipeRefreshLayout;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
                    if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
                    {
                        //Load More  from API Request
                        LoadMoreEvent?.Invoke(this, null);
                        //Start Load More messages From Database
                    }
                    else
                    {
                        if (SwipeRefreshLayout.Refreshing)
                        {
                            SwipeRefreshLayout.Refreshing = false;
                            SwipeRefreshLayout.Enabled = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Menu

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MessagesBox_Menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
         
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true; 
                case Resource.Id.menu_block:
                    MenuBlockClick();
                    break;
                case Resource.Id.menu_clear_chat:
                    MenuClearChatClick();
                    break; 
            }

            return base.OnOptionsItemSelected(item);
        }
          
        //Block User
        private void MenuBlockClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var list = GlobalContext?.ProfileFragment?.ContactsFragment?.MAdapter?.UsersList;
                    if (list?.Count > 0)
                    {
                        var dataFav = list.FirstOrDefault(a => a.Id == Userid);
                        if (dataFav != null)
                        {
                            list.Remove(dataFav);
                            GlobalContext?.ProfileFragment?.ContactsFragment?.MAdapter.NotifyDataSetChanged();
                        }
                    }
                     
                    MenuClearChatClick();

                    Toast.MakeText(this, GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short)?.Show();

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnBlockUserAsync(Userid.ToString(),true) });
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MenuClearChatClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    MAdapter.MessageList.Clear();
                    MAdapter.NotifyDataSetChanged();

                    var userDelete = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.User.Id == Userid);
                    if (userDelete != null)
                    {
                        LastChatActivity.MAdapter.UserList.Remove(userDelete);

                        var index = LastChatActivity.MAdapter.UserList.IndexOf(userDelete);
                        if (index > -1)
                            LastChatActivity.MAdapter.NotifyDataSetChanged();
                    }

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.DeleteUserLastChat(Userid.ToString());
                    dbDatabase.DeleteAllMessagesUser(UserDetails.UserId.ToString(), Userid.ToString());

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Chat.DeleteChatAsync(Userid.ToString()) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }  

        #endregion
           
        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);
                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;
                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                                //Sent image 
                                ChatMessagesDataObject message = new ChatMessagesDataObject
                                {
                                    Id = Convert.ToInt32(unixTimestamp),
                                    FromId = UserDetails.UserId,
                                    ToId = Userid,
                                    Text = "",
                                    Seen = 0,
                                    Time = unixTimestamp,
                                    FromDeleted = 0,
                                    ToDeleted = 0,
                                    SentPush = 0,
                                    NotificationId = "",
                                    TypeTwo = "",
                                    Image = resultPathImage,
                                    FullImage = "",
                                    ApiPosition = ApiPosition.Right,
                                    ApiType = ApiType.Image,
                                    Hash = time2
                                };

                                MAdapter.MessageList.Add(message);

                                int index = MAdapter.MessageList.IndexOf(MAdapter.MessageList.Last());
                                if (index > -1)
                                {
                                    MAdapter.NotifyItemInserted(index);

                                    //Scroll Down >> 
                                    ChatBoxRecyclerView.ScrollToPosition(index);
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MessageController.SendMessageTask(Userid, "", resultPathImage, time2, UserInfoData) });

                                EmojisIconEditTextView.Text = "";

                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        //request Code 108
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}