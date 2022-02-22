using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Base;
using DeepSound.Activities.Chat.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LastChatActivity : BaseActivity 
    {
        #region Variables Basic

        public static LastChatAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private static Toolbar ToolBar;
        private AdView MAdView;
        private static LastChatActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                 
                GetLastChatLocal();

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                MAdView?.Resume();
                base.OnResume();
                AddOrRemoveEvent(true);
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
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        protected override void OnDestroy()
        {
            try
            {
                ListUtils.ChatList = MAdapter.UserList;

                MAdapter?.UserList.Clear();
                MAdapter?.NotifyDataSetChanged();

                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                var mainLayout = FindViewById<CoordinatorLayout>(Resource.Id.mainLayout);
                mainLayout.SetPadding(0, 0, 0, 0);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetPadding(5, 0, 0, 0);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                MAdapter = new LastChatAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<DataConversation>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                  
                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Chats);
                    ToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    ToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
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
                    MAdapter.OnItemClick += MAdapterOnOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.OnItemClick -= MAdapterOnOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static LastChatActivity GetInstance()
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

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Event Scroll #LastChat
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && MAdapter.UserList.Count > 10)
                {
                    StartApiService(item.ChatTime.ToString());
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
           
        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                ListUtils.ChatList.Clear();

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                SqLiteDatabase database = new SqLiteDatabase();
                database.ClearLastChat();
                database.ClearAll_Messages();
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnOnItemClick(object sender, LastChatAdapterClickEventArgs e)
        {
            try
            {
                HomeActivity.GetInstance()?.SetService();

                if (ToolBar.Visibility != ViewStates.Visible)
                    ToolBar.Visibility = ViewStates.Visible;

                // read the item which removes bold from the row >> event click open ChatBox by user id
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                { 
                    item.GetCountSeen = 0;
                    if (item.GetLastMessage != null) item.GetLastMessage.Value.GetLastMessageClass.Seen = 1;
                    Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                    intent.PutExtra("UserId", item.User.Id.ToString());
                    intent.PutExtra("TypeChat", "LastChat");
                    intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.User));
                     
                    StartActivity(intent);
                    MAdapter.NotifyItemChanged(e.Position);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void GetLastChatLocal()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                ListUtils.ChatList = new ObservableCollection<DataConversation>();
                var list = dbDatabase.GetAllLastChat();
                if (list.Count > 0)
                {
                    ListUtils.ChatList = new ObservableCollection<DataConversation>(list);
                    MAdapter.UserList = ListUtils.ChatList;
                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    SwipeRefreshLayout.Refreshing = true;

                    StartApiService();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.UserList.Count;

                var (apiStatus, respond) = await RequestsAsync.Chat.GetConversationListAsync("15", offset);
                if (apiStatus != 200 || respond is not GetConversationListObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data.RemoveAll(a => a.GetLastMessage?.GetLastMessageClass == null);

                        if (countList > 0)
                        {
                            LoadDataJsonLastChat(result);
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<DataConversation>(result.Data);
                            MAdapter.UserList = new ObservableCollection<DataConversation>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                MainScrollEvent.IsLoading = false;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        public void LoadDataJsonLastChat(GetConversationListObject result)
        {
            try
            {
                if (MAdapter != null)
                {
                    if (result.Data.Count > 0)
                        result.Data.RemoveAll(a => a.GetLastMessage?.GetLastMessageClass == null);

                    if (MAdapter.UserList?.Count > 0)
                    {
                        foreach (var user in result.Data)
                        {
                            var checkUser = MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
                            if (checkUser?.GetLastMessage?.GetLastMessageClass != null)
                            {
                                int index = MAdapter.UserList.IndexOf(checkUser);

                                //checkUser.Id = user.Id;
                                //if (checkUser.Owner != user.Owner) checkUser.Owner = user.Owner;
                               
                                if (checkUser.GetLastMessage?.GetLastMessageClass.Time != user.GetLastMessage?.GetLastMessageClass.Time)
                                    if (checkUser.GetLastMessage != null) if (user.GetLastMessage != null) checkUser.GetLastMessage.Value.GetLastMessageClass.Time = user.GetLastMessage.Value.GetLastMessageClass.Time;
                                if (checkUser.GetLastMessage?.GetLastMessageClass.Seen != user.GetLastMessage?.GetLastMessageClass.Seen)
                                    if (checkUser.GetLastMessage != null) if (user.GetLastMessage != null) checkUser.GetLastMessage.Value.GetLastMessageClass.Seen = user.GetLastMessage.Value.GetLastMessageClass.Seen;
                              
                                if (checkUser.GetCountSeen != user.GetCountSeen) checkUser.GetCountSeen = user.GetCountSeen;
                                if (checkUser.User != user.User) checkUser.User = user.User;

                                if (checkUser.GetLastMessage?.GetLastMessageClass.ApiType != user.GetLastMessage?.GetLastMessageClass.ApiType) continue;
                                if (checkUser.GetLastMessage != null)
                                {
                                    if (user.GetLastMessage != null)
                                    {
                                        checkUser.GetLastMessage.Value.GetLastMessageClass.ApiType = user.GetLastMessage.Value.GetLastMessageClass.ApiType;

                                        if (checkUser.GetLastMessage?.GetLastMessageClass.Text !=
                                            user.GetLastMessage?.GetLastMessageClass.Text)
                                        {
                                            checkUser.GetLastMessage.Value.GetLastMessageClass.Text =
                                                user.GetLastMessage?.GetLastMessageClass.Text;

                                            if (index > -1)
                                            {
                                                RunOnUiThread(() =>
                                                {
                                                    MAdapter.UserList.Move(index, 0);
                                                    MAdapter.NotifyItemMoved(index, 0);
                                                });
                                            }
                                        }

                                        if (checkUser.GetLastMessage?.GetLastMessageClass.Image != user.GetLastMessage?.GetLastMessageClass.Image)
                                        {
                                            checkUser.GetLastMessage.Value.GetLastMessageClass.Image = user.GetLastMessage?.GetLastMessageClass.Image;

                                            if (index > -1)
                                            {
                                                RunOnUiThread(() =>
                                                {
                                                    MAdapter.UserList.Move(index, 0);
                                                    MAdapter.NotifyItemMoved(index, 0);
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var dataUser = MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
                                        if (dataUser == null)
                                        {
                                            MAdapter.UserList.Insert(0, user);
                                            MAdapter.NotifyItemInserted(0);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    } 
                                });
                            }
                        }
                    }
                    else
                    {
                        MAdapter.UserList = new ObservableCollection<DataConversation>(result.Data);
                        MAdapter.NotifyDataSetChanged();
                    }

                    ListUtils.ChatList = MAdapter.UserList;
                }

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMessage);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
} 