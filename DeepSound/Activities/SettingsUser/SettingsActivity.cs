using System;
using System.Collections.Generic;
using System.Linq;
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
using DeepSound.Activities.Base;
using DeepSound.Activities.Genres;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.SettingsUser.InviteFriends;
using DeepSound.Activities.SettingsUser.Security;
using DeepSound.Activities.SettingsUser.Support;
using DeepSound.Activities.Tabbes;
using DeepSound.Activities.Tabbes.Fragments;
using DeepSound.Activities.Upgrade;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using MaterialDialogsCore;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/SettingsTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Orientation)]
    public class SettingsActivity : BaseActivity, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private RelativeLayout InfoUserContainer;
        private ImageView ImageUser, IconVerified;
        private TextView Name, Username;

        private FrameLayout EditProfileLayout, MyAccountLayout, MyAddressesLayout, NotificationsLayout, WithdrawalsLayout, WalletLayout, GoProLayout, BlockedUsersLayout;
        private FrameLayout PasswordLayout, TwoFactorLayout, ManageSessionsLayout;
        private FrameLayout ThemeLayout;

        private FrameLayout InterestLayout;
        private FrameLayout RateOurAppLayout, InviteFriendsLayout, AboutAppLayout, TermsOfUseLayout, HelpLayout;
        private FrameLayout DeleteAccountLayout, LogoutLayout;

        private string OpenEvent;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.SettingsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                LoadDataSettings();
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
                base.OnPause();
                AddOrRemoveEvent(false);
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
                if (OpenEvent == "Profile")
                {
                    HomeActivity globalContext = HomeActivity.GetInstance();
                    if (globalContext != null)
                    {
                        globalContext.ProfileFragment = new ProfileFragment();
                        globalContext.FragmentBottomNavigator.DisplayFragment(globalContext.ProfileFragment);
                    }
                }

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                InfoUserContainer = FindViewById<RelativeLayout>(Resource.Id.info_container);
                ImageUser = FindViewById<ImageView>(Resource.Id.image);
                Name = FindViewById<TextView>(Resource.Id.Name);
                IconVerified = FindViewById<ImageView>(Resource.Id.IconVerified);
                Username = FindViewById<TextView>(Resource.Id.username);

                EditProfileLayout = FindViewById<FrameLayout>(Resource.Id.layoutEditProfile);
                MyAccountLayout = FindViewById<FrameLayout>(Resource.Id.layoutMyAccount);
                MyAddressesLayout = FindViewById<FrameLayout>(Resource.Id.layoutMyAddresses);
                NotificationsLayout = FindViewById<FrameLayout>(Resource.Id.layoutNotifications);
                WithdrawalsLayout = FindViewById<FrameLayout>(Resource.Id.layoutWithdrawals);
                WalletLayout = FindViewById<FrameLayout>(Resource.Id.layoutWallet);
                GoProLayout = FindViewById<FrameLayout>(Resource.Id.layoutGoPro);
                BlockedUsersLayout = FindViewById<FrameLayout>(Resource.Id.layoutBlockedUsers);

                PasswordLayout = FindViewById<FrameLayout>(Resource.Id.layoutPassword);
                TwoFactorLayout = FindViewById<FrameLayout>(Resource.Id.layoutTwoFactor);
                ManageSessionsLayout = FindViewById<FrameLayout>(Resource.Id.layoutManageSessions);

                ThemeLayout = FindViewById<FrameLayout>(Resource.Id.layoutTheme);

                InterestLayout = FindViewById<FrameLayout>(Resource.Id.layoutInterest);
               
                  
                RateOurAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutRateOurApp);
                InviteFriendsLayout = FindViewById<FrameLayout>(Resource.Id.layoutInviteFriends);
                AboutAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutAboutApp);
                TermsOfUseLayout = FindViewById<FrameLayout>(Resource.Id.layoutTermsOfUse);
                HelpLayout = FindViewById<FrameLayout>(Resource.Id.layoutHelp);

                DeleteAccountLayout = FindViewById<FrameLayout>(Resource.Id.layoutDeleteAccount);
                LogoutLayout = FindViewById<FrameLayout>(Resource.Id.layoutLogout);
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
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_Settings);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                    InfoUserContainer.Click += InfoUserContainerOnClick;
                    EditProfileLayout.Click += EditProfileLayoutOnClick;
                    MyAccountLayout.Click += MyAccountLayoutOnClick;
                    NotificationsLayout.Click += NotificationsLayoutOnClick;
                    MyAddressesLayout.Click += MyAddressesLayoutOnClick;
                    WithdrawalsLayout.Click += WithdrawalsLayoutOnClick;
                    WalletLayout.Click += WalletLayoutOnClick;
                    GoProLayout.Click += GoProLayoutOnClick;
                    BlockedUsersLayout.Click += BlockedUsersLayoutOnClick;
                    PasswordLayout.Click += PasswordLayoutOnClick;
                    TwoFactorLayout.Click += TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click += ManageSessionsLayoutOnClick;
                    ThemeLayout.Click += ThemeLayoutOnClick;
                    InterestLayout.Click += InterestLayoutOnClick;
                    RateOurAppLayout.Click += RateOurAppLayoutOnClick;
                    InviteFriendsLayout.Click += InviteFriendsLayoutOnClick;
                    AboutAppLayout.Click += AboutAppLayoutOnClick;
                    TermsOfUseLayout.Click += TermsOfUseLayoutOnClick;
                    HelpLayout.Click += HelpLayoutOnClick;
                    DeleteAccountLayout.Click += DeleteAccountLayoutOnClick;
                    LogoutLayout.Click += LogoutLayoutOnClick;
                }
                else
                {
                    InfoUserContainer.Click -= InfoUserContainerOnClick;
                    EditProfileLayout.Click -= EditProfileLayoutOnClick;
                    MyAccountLayout.Click -= MyAccountLayoutOnClick;
                    NotificationsLayout.Click -= NotificationsLayoutOnClick;
                    MyAddressesLayout.Click -= MyAddressesLayoutOnClick;
                    WithdrawalsLayout.Click -= WithdrawalsLayoutOnClick;
                    WalletLayout.Click -= WalletLayoutOnClick;
                    GoProLayout.Click -= GoProLayoutOnClick;
                    BlockedUsersLayout.Click -= BlockedUsersLayoutOnClick;
                    PasswordLayout.Click -= PasswordLayoutOnClick;
                    TwoFactorLayout.Click -= TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click -= ManageSessionsLayoutOnClick;
                    ThemeLayout.Click -= ThemeLayoutOnClick;
                    InterestLayout.Click -= InterestLayoutOnClick;
                    RateOurAppLayout.Click -= RateOurAppLayoutOnClick;
                    InviteFriendsLayout.Click -= InviteFriendsLayoutOnClick;
                    AboutAppLayout.Click -= AboutAppLayoutOnClick;
                    TermsOfUseLayout.Click -= TermsOfUseLayoutOnClick;
                    HelpLayout.Click -= HelpLayoutOnClick;
                    DeleteAccountLayout.Click -= DeleteAccountLayoutOnClick;
                    LogoutLayout.Click -= LogoutLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void InfoUserContainerOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    OpenEvent = "Profile";
                    Finish();
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditProfileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(EditProfileInfoActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void WalletLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WalletActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WithdrawalsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WithdrawalsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GoProLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(GoProActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void BlockedUsersLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(BlockedUsersActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PasswordLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(PasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TwoFactorLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(TwoFactorAuthActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ManageSessionsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ManageSessionsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ThemeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                dialogList.Title(Resource.String.Lbl_Night_Mode);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                if ((int)Build.VERSION.SdkInt >= 29)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private void InterestLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(GenresActivity));
                intent.PutExtra("Event", "Save");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MyAddressesLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", "https://deepsoundscript.com/settings/waelanjo/addresses");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_MyAddresses));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void NotificationsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(NotificationsSettingsActivity))); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MyAccountLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(MyAccountActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void RateOurAppLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StoreReviewApp store = new StoreReviewApp();
                store.OpenStoreReviewPage(PackageName);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void InviteFriendsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    case < 23:
                    {
                        var intent = new Intent(this, typeof(InviteFriendsActivity));
                        StartActivity(intent);
                        break;
                    }
                    default:
                    {
                        //Check to see if any permission in our group is available, if one, then all are
                        if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                        {
                            var intent = new Intent(this, typeof(InviteFriendsActivity));
                            StartActivity(intent);
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(101);
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AboutAppLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializeDeepSound.WebsiteUrl + "/terms/about-us");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_About));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TermsOfUseLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializeDeepSound.WebsiteUrl + "/terms/terms");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_TermsOfUse));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void HelpLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializeDeepSound.WebsiteUrl + "/contact-us");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_Help));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DeleteAccountLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(DeleteAccountActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LogoutLayoutOnClick(object sender, EventArgs e)
        {
            try
            { 
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                dialog.Title(Resource.String.Lbl_Warning).TitleColorRes(Resource.Color.primary);
                dialog.Content(GetText(Resource.String.Lbl_Are_you_logout));
                dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive((materialDialog, action) => ApiRequest.Logout(this));
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    var intent = new Intent(this, typeof(InviteFriendsActivity));
                    StartActivity(intent);
                }
                else 
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;

                string getValue = SharedPref.SharedData?.GetString("Night_Mode_key", string.Empty);

                if (text == GetString(Resource.String.Lbl_Light) && getValue != SharedPref.LightMode)
                {
                    //Set Light Mode   
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_Light);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    SharedPref.ApplyTheme(SharedPref.LightMode);
                    SharedPref.SharedData?.Edit()?.PutString("Night_Mode_key", SharedPref.LightMode)?.Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(HomeActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();
                    OverridePendingTransition(0, 0);
                    StartActivity(intent);
                }
                else if (text == GetString(Resource.String.Lbl_Dark) && getValue != SharedPref.DarkMode)
                {
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_Dark);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    SharedPref.ApplyTheme(SharedPref.DarkMode);
                    SharedPref.SharedData?.Edit()?.PutString("Night_Mode_key", SharedPref.DarkMode)?.Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(HomeActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();
                    OverridePendingTransition(0, 0);
                    StartActivity(intent);
                }
                else if (text == GetString(Resource.String.Lbl_SetByBattery) && getValue != SharedPref.DefaultMode)
                {
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_SetByBattery);
                    SharedPref.SharedData?.Edit()?.PutString("Night_Mode_key", SharedPref.DefaultMode)?.Commit();

                    if ((int)Build.VERSION.SdkInt >= 29)
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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
                    }
                    else
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(this, typeof(HomeActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        intent.AddFlags(ActivityFlags.NoAnimation);
                        FinishAffinity();
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void LoadDataSettings()
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (dataUser != null)
                {
                    GlideImageLoader.LoadImage(this, dataUser.Avatar, ImageUser, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    Name.Text = DeepSoundTools.GetNameFinal(dataUser);
                    //Verified 
                    IconVerified.Visibility = dataUser.Verified == 1 ? ViewStates.Visible : ViewStates.Gone;

                    Username.Text = "@" + dataUser.Username;
                }

                //Delete Preference
                //============== Account_Profile ===================

                if (!AppSettings.ShowWithdrawals)
                    WithdrawalsLayout.Visibility = ViewStates.Gone;

                var isPro = dataUser?.IsPro ?? 0;
                if (!AppSettings.ShowGoPro || isPro != 0)
                    GoProLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowBlockedUsers)
                    BlockedUsersLayout.Visibility = ViewStates.Gone;

                //============== SecurityAccount ===================

                if (!AppSettings.ShowEditPassword)
                    PasswordLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsTwoFactor)
                    TwoFactorLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsManageSessions)
                    ManageSessionsLayout.Visibility = ViewStates.Gone;
                 
                //============== Support ===================

                if (!AppSettings.ShowSettingsRateApp)
                    RateOurAppLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsAbout)
                    AboutAppLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsHelp)
                    HelpLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsTermsOfUse)
                    TermsOfUseLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsDeleteAccount)
                    DeleteAccountLayout.Visibility = ViewStates.Gone;
                 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}