//###############################################################
// Author >> Elin Doughouz
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.SlidingPaneLayout.Widget;
using Com.Google.Android.Play.Core.Install.Model;
using Com.Sothree.Slidinguppanel;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Chat;
using DeepSound.Activities.Chat.Service;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.Tabbes.Adapters;
using DeepSound.Activities.Tabbes.Fragments;
using DeepSound.Activities.Upload;
using DeepSound.Activities.UserProfile;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Q.Rorbin.Badgeview;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Exception = System.Exception;
using Math = System.Math;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class HomeActivity : AppCompatActivity, SlidingPaneLayout.IPanelSlideListener, SlidingUpPanelLayout.IPanelSlideListener, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private static HomeActivity Instance;
        public SlidingUpPanelLayout SlidingUpPanel;
        public MainFeedFragment MainFragment;
        public TrendingFragment PlaylistFragment;
        public BrowseFragment BrowseFragment;
        public LibraryFragment LibraryFragment;
        public ProfileFragment ProfileFragment;
        private LinearLayout NavigationTabBar;
        public CustomNavigationController FragmentBottomNavigator;
        private PowerManager.WakeLock Wl;
        public SoundController SoundController;
        public Timer Timer;
        private string RunTimer = "Run", TypeImage = "";
        public LibrarySynchronizer LibrarySynchronizer;
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        private bool RecentlyBackPressed;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (ShouldReturn())
                {
                    return;
                }

                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                AddFlagsWakeLock();

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;
                Constant.Context = this;

                LibrarySynchronizer = new LibrarySynchronizer(this);

                //Get Value 
                InitComponent(); 
                SetupBottomNavigationView();

                SoundController = new SoundController(this);
                SoundController.InitializeUi();

                new Handler(Looper.MainLooper).Post(new Runnable(GetGeneralAppData));
                if (UserDetails.IsLogin)
                {
                    // Run timer
                    Timer = new Timer
                    {
                        Interval = AppSettings.RefreshGetNotification
                    };
                    Timer.Elapsed += TimerOnElapsed;
                    Timer.Enabled = true;
                    Timer.Start();
                }

                GetOneSignalNotification();
                SetService();
                CheckOptimization(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void CheckOptimization()
        {
            try
            { 
                if (!AppSettings.EnableOptimizationApp || UserDetails.IsOptimizationApp) return;
                if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;

                var pm = (PowerManager)ApplicationContext?.GetSystemService(Context.PowerService);
                if (pm == null) return;

                bool result = pm.IsIgnoringBatteryOptimizations(PackageName);
                if (result)
                    return;

                UserDetails.IsOptimizationApp = true;
                SharedPref.SharedData?.Edit()?.PutBoolean(SharedPref.PrefKeyOptimizationApp, true)?.Commit();

                var intent = new Intent(); 
                if (pm.IsIgnoringBatteryOptimizations(PackageName))
                {
                    intent.SetAction(Settings.ActionIgnoreBatteryOptimizationSettings);
                }
                else
                {
                    intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(Uri.Parse("package:" + PackageName));
                    StartActivity(intent);
                }
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
                base.OnResume();

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

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                } 

                OffWakeLock();
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {  
                Intent intent = new Intent(this, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionStop);

                if (!Constant.IsLoggingOut && !Constant.IsChangingTheme)
                {
                    ContextCompat.StartForegroundService(this, intent);
                }

                StopService(intent);

                Constant.IsLoggingOut = false;
                Constant.IsChangingTheme = false;

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

                FragmentBottomNavigator?.DisableAllNavigationButton();
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
                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                SlidingUpPanel.AddPanelSlideListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static HomeActivity GetInstance()
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

        public void SetToolBar(Toolbar toolbar, string title, bool showIconBack = true, bool setBackground = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (setBackground)
                        toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions ¯\_(ツ)_/¯

        private async void GetOneSignalNotification()
        {
            try
            {
                string type = Intent?.GetStringExtra("TypeNotification") ?? "Don't have type";
                if (!string.IsNullOrEmpty(type) && type != "Don't have type")
                {
                    if (type == "User")
                    {
                        OpenProfile(OneSignalNotification.UserData.Id, OneSignalNotification.UserData);
                    }
                    else if (type == "Track")
                    {
                         var (apiStatus, respond) = await RequestsAsync.Tracks.GetTrackInfoAsync(OneSignalNotification.TrackId);
                        if (apiStatus.Equals(200))
                        {
                            if (respond is GetTrackInfoObject result)
                            {
                                Constant.PlayPos = 0;
                                SoundController?.StartPlaySound(result.Data, new ObservableCollection<SoundDataObject> { result.Data });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunApiTimer();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void RunApiTimer()
        {
            try
            {
                if (RunTimer == "Run")
                {
                    RunTimer = "Off";
                     
                    var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications();
                    if (MainFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                    {
                        UserDetails.CountNotificationsStatic = Convert.ToInt32(countNotifications);
                        ShowOrHideBadgeViewIcon(UserDetails.CountNotificationsStatic, true);
                    }
                    else
                    {
                        ShowOrHideBadgeViewIcon();
                    }

                    Console.WriteLine(countMessages);
                    //if (tabMessages != null && countMessages != 0 && countMessages != CountMessagesStatic)
                    //{
                    //    RunOnUiThread(() =>
                    //    {
                    //        try
                    //        {
                    //            CountMessagesStatic = countMessages;
                    //            tabMessages.BadgeTitle = countMessages.ToString();
                    //            tabMessages.UpdateBadgeTitle(countMessages.ToString());
                    //            tabMessages.ShowBadge();
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            Methods.DisplayReportResultTrack(e);
                    //        }
                    //    });
                    //}  
                }
                RunTimer = "Run";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                RunTimer = "Run";
            }
        }

        public void ShowOrHideBadgeViewIcon(int countNotifications = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (MainFragment?.NotificationIcon != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                QBadgeView badge = new QBadgeView(this);
                                badge.BindTarget(MainFragment?.NotificationIcon);
                                badge.SetBadgeNumber(countNotifications);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                //badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            if (MainFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                            {
                                new QBadgeView(this).BindTarget(MainFragment?.NotificationIcon).Hide(true);
                                UserDetails.CountNotificationsStatic = 0;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetupBottomNavigationView()
        {
            try
            {
                NavigationTabBar = FindViewById<LinearLayout>(Resource.Id.buttomnavigationBar);
                FragmentBottomNavigator = new CustomNavigationController(this);
                 
                MainFragment = new MainFeedFragment();
                PlaylistFragment = new TrendingFragment();
                BrowseFragment = new BrowseFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(MainFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(PlaylistFragment);
                FragmentBottomNavigator.FragmentListTab2.Add(BrowseFragment);

                if (UserDetails.IsLogin)
                {
                    LibraryFragment = new LibraryFragment();
                    ProfileFragment = new ProfileFragment();
                    FragmentBottomNavigator.FragmentListTab3.Add(LibraryFragment);
                    FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);
                }

                FragmentBottomNavigator.ShowFragment0();

                if (UserDetails.IsLogin && LibraryFragment != null && LibraryFragment?.MAdapter == null)
                    LibraryFragment.MAdapter = new LibraryAdapter(this);

                //if (UserDetails.IsLogin && PlaylistFragment?.PlaylistAdapter == null)
                //    PlaylistFragment.PlaylistAdapter = new HPlaylistAdapter(this, true) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event Back

        public override void OnBackPressed()
        {
            try
            {
                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (FragmentBottomNavigator.GetCountFragment() > 0)
                {
                    FragmentNavigatorBack();
                }
                else
                {
                    if (RecentlyBackPressed)
                    {
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                    }
                    else
                    {
                        RecentlyBackPressed = true;
                        Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                FragmentBottomNavigator.OnBackStackClickFragment();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Upload An Album
        public void BtnUploadAnAlbumOnClick()
        {
            try
            {
                StartActivity(new Intent(this, typeof(UploadAlbumActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Upload Single Song
        public void BtnUploadSingleSongOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                    new IntentController(this).OpenIntentAudio(); //505
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                        new PermissionsController(this).RequestPermission(100);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BtnImportSongOnClick()
        {
            try
            {
                StartActivity(new Intent(this, typeof(ImportSongActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode) //==> change Avatar Or Cover
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);

                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;

                            if (ProfileFragment?.ImageAvatar == null || ProfileFragment?.ImageCover == null)
                                return;

                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                if (TypeImage == "Avatar")
                                {
                                    GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment?.ImageAvatar, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                    await Task.Run(async () =>
                                    {
                                        try
                                        {
                                             var (apiStatus, respond) = await RequestsAsync.User.UpdateAvatarAsync(resultPathImage);
                                            if (apiStatus.Equals(200))
                                            {
                                                if (respond is UpdateImageUserObject image)
                                                {
                                                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                                    if (dataUser != null)
                                                    {
                                                        dataUser.Avatar = image.Img;

                                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                        dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                    }
                                                }
                                            }
                                            else Methods.DisplayReportResult(this, respond);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                else if (TypeImage == "Cover")
                                {
                                    GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment?.ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                    await Task.Run(async () =>
                                    {
                                        try
                                        {
                                             var (apiStatus, respond) = await RequestsAsync.User.UpdateCoverAsync(resultPathImage);
                                            if (apiStatus.Equals(200))
                                            {
                                                if (respond is UpdateImageUserObject image)
                                                {
                                                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                                    if (dataUser != null)
                                                    {
                                                        dataUser.Cover = image.Img;

                                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                        dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                    }
                                                }
                                            }
                                            else Methods.DisplayReportResult(this, respond);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }
                        }
                    }
                }
                else if (requestCode == 505 && resultCode == Result.Ok) //==> Audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Audio")
                        {
                            Intent intent = new Intent(this, typeof(UploadSongActivity));
                            intent.PutExtra("SongLocation", filepath);
                            StartActivity(intent);
                        }
                    }
                } 
                else if (requestCode == 4711)
                {
                    switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                    {
                        case Result.Ok:
                            // In app update success
                            if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                Toast.MakeText(this, "App updated", ToastLength.Short)?.Show();
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, "In app update cancelled", ToastLength.Short)?.Show();
                            break;
                        case (Result)ActivityResult.ResultInAppUpdateFailed:
                            Toast.MakeText(this, "In app update failed", ToastLength.Short)?.Show();
                            break;
                    }
                }
                else if (requestCode == 200 && resultCode == Result.Ok)
                {
                    var name = data.GetStringExtra("name") ?? "";
                    if (!string.IsNullOrEmpty(name) && ProfileFragment != null && !name.Equals(ProfileFragment.TxtFullName.Text)) 
                        ProfileFragment.TxtFullName.Text = data.GetStringExtra("name");
                }
                else if (requestCode == 3500 && resultCode == Result.Ok) //Add Product
                {
                    if (string.IsNullOrEmpty(data?.GetStringExtra("itemData"))) return;

                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        var productsList = MainFragment?.ProductFragment?.MAdapter?.ProductsList;
                        if (productsList != null)
                        {
                            productsList.Add(item);

                            MainFragment.ProductFragment?.MAdapter.NotifyDataSetChanged(); 
                        }
                    } 
                }
                else if (requestCode == 4500 && resultCode == Result.Ok) //Add Event
                {
                    if (string.IsNullOrEmpty(data?.GetStringExtra("itemData"))) return;

                    var item = JsonConvert.DeserializeObject<EventDataObject>(data.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        var eventList = PlaylistFragment?.EventFragment?.MAdapter?.EventsList;
                        if (eventList != null)
                        {
                            eventList.Add(item);

                            PlaylistFragment?.EventFragment?.MAdapter.NotifyDataSetChanged();
                        }
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
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery(TypeImage);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        new IntentController(this).OpenIntentAudio(); //505
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

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm?.NewWakeLock(WakeLockFlags.Partial, "My Tag");
                    Wl?.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Listener Panel Layout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            var percentage = (float)Math.Abs(verticalOffset) / appBarLayout.TotalScrollRange;
            Console.WriteLine(percentage);
        }

        public void OnPanelClosed(View panel)
        {

        }

        public void OnPanelOpened(View panel)
        {

        }

        public void OnPanelSlide(View panel, float slideOffset)
        {
            try
            {
                NavigationTabBar.Alpha = 1 - slideOffset;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon?.Tag?.ToString() == "Close")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                        //NavigationTabBar.Hide();
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Hidden && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Anchored)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Close")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                    // Toast.MakeText(this, "p1 Anchored + Anchored ", ToastLength.Short)?.Show();
                }
                if (p1 == SlidingUpPanelLayout.PanelState.Collapsed && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
                if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);
                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        SoundController?.BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                        SoundController?.BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Purchase the song Or album

        private static SoundDataObject PaymentSoundObject;
        private static DataAlbumsObject PaymentAlbumsObject;
    
        public void OpenDialogPurchaseSound(SoundDataObject soundObject)
        {
            try
            {
                PaymentSoundObject = soundObject;

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(Resource.String.Lbl_PurchaseRequired);
                dialog.Content(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.PositiveText(GetText(Resource.String.Lbl_Purchase)).OnPositive(async (materialDialog, action) =>
                {
                    try
                    { 
                        if (DeepSoundTools.CheckWallet())
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("buy_song", PaymentSoundObject.AudioId);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);
                                        PaymentSoundObject.IsPurchased = true;
                                        Constant.PlayPos = Constant.ArrayListPlay.IndexOf(PaymentSoundObject);
                                        SoundController?.StartPlaySound(PaymentSoundObject, Constant.ArrayListPlay);

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                            dialogBuilder.Title(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.Content(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.PositiveText(GetText(Resource.String.Lbl_AddWallet)).OnPositive((materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        } 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenDialogPurchaseAlbum(DataAlbumsObject albumsObject)
        {
            try
            {
                PaymentAlbumsObject = albumsObject;

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(Resource.String.Lbl_PurchaseRequired);
                dialog.Content(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.PositiveText(GetText(Resource.String.Lbl_Purchase)).OnPositive(async (materialDialog, action) =>
                {
                    try
                    { 
                        if (DeepSoundTools.CheckWallet())
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("buy_album", PaymentAlbumsObject.AlbumId);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);
                                        PaymentAlbumsObject.IsPurchased = 1;

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                            dialogBuilder.Title(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.Content(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.PositiveText(GetText(Resource.String.Lbl_AddWallet)).OnPositive((materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        } 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            { 
                if (!UserDetails.IsLogin)
                    return;
 
                if (run)
                {
                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this);
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
 
        public void OnReceiveResult(string resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetConversationListObject>(resultData);
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OpenDialogGallery(string typeImage)
        {
            try
            {
                TypeImage = typeImage;
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
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenProfile(long userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("UserId", userId.ToString());
                    if (item.Artist == 0)
                    {
                        UserProfileFragment userProfileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(userProfileFragment);
                    }
                    else
                    {
                        //open profile Artist
                        ArtistsProfileFragment artistsProfileFragment = new ArtistsProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(artistsProfileFragment);
                    }
                }
                else
                {
                    if (UserDetails.IsLogin)
                        FragmentBottomNavigator.ShowFragment4();
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }

                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active" && UserDetails.IsLogin)
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }

                sqlEntity.GetSettings();
                if (UserDetails.IsLogin)
                {
                    sqlEntity.GetDataMyInfo();

                    var libraryItems = sqlEntity.Get_LibraryItem();

                    var favItem = libraryItems?.FirstOrDefault(a => a.SectionId == "3")?.SongsCount.ToString() ?? "0";
                    var likedItem = libraryItems?.FirstOrDefault(a => a.SectionId == "1")?.SongsCount.ToString() ?? "0"; 

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this), ApiRequest.GetMyPlaylist_Api, () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()), () => LoadFavorites(favItem), () => LoadLiked(likedItem) });
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
                }

                ListUtils.GenresList = sqlEntity.Get_GenresList();
                ListUtils.PriceList = sqlEntity.Get_PriceList();

                if (ListUtils.GenresList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGenres_Api });

                if (ListUtils.PriceList?.Count == 0 && AppSettings.ShowPrice)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPrices_Api });

                RunOnUiThread(() =>
                {
                    try
                    {
                        if (AppSettings.ShowGoPro && UserDetails.IsLogin)
                        {
                            var pro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro;
                            MainFragment.ProIcon.Visibility = pro == 1 ? ViewStates.Gone : ViewStates.Visible;
                        }
                        else
                        {
                            MainFragment.ProIcon.Visibility = ViewStates.Gone;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadFavorites(string limit = "0")
        {
            if (Methods.CheckConnectivity())
            {
                 var (apiStatus, respond) = await RequestsAsync.User.GetFavoriteAsync(UserDetails.UserId.ToString(), limit);
                if (apiStatus == 200)
                {

                    if (respond is GetSoundObject result)
                    {
                        result.Data.SoundList = DeepSoundTools.ListFilter(result.Data?.SoundList);

                        var respondList = result.Data?.SoundList?.Count;
                        if (respondList > 0)
                        {
                            ListUtils.FavoritesList = result.Data?.SoundList;
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                } 
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private async Task LoadLiked(string limit = "0")
        {
            if (Methods.CheckConnectivity())
            {
                 var (apiStatus, respond) = await RequestsAsync.User.GetLikedAsync(UserDetails.UserId.ToString(), limit);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundObject result)
                    {
                        var respondList = result.Data?.SoundList?.Count;
                        if (respondList > 0)
                        {
                            ListUtils.LikedSongs = result.Data?.SoundList;
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                }
            }
            else
            { 
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private void InAppUpdate()
        {
            try
            {
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = SharedPref.InAppReview.GetBoolean(SharedPref.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();

                        SharedPref.InAppReview?.Edit()?.PutBoolean(SharedPref.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool ShouldReturn()
        {
            try
            {
                if (!SplashScreenActivity.SplashWasShown)
                {
                    NavigateToSplashAndFinish();
                }

                return !SplashScreenActivity.SplashWasShown;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return !SplashScreenActivity.SplashWasShown;
            }
        }

        private void NavigateToSplashAndFinish()
        {
            try
            {
                base.OnCreate(null);

                var intents = new Intent(this, typeof(SplashScreenActivity));
                intents.AddFlags(ActivityFlags.NewTask);
                intents.AddFlags(ActivityFlags.ClearTop);

                StartActivity(intents);

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}