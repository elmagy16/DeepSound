using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.PaymentGoogle;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using InAppBilling.Lib;
using MaterialDialogsCore;
using BaseActivity = DeepSound.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Upgrade
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : BaseActivity, IBillingPaymentListener, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private LinearLayout OptionLinerLayout, MainLayout;
        private AppCompatButton UpgradeButton;
        private TextView HeadText, PriceText;

        private TextDecorator TextDecorator;
        private RelativeLayout RelativeLayout;
        private BillingSupport BillingSupport;

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
                SetContentView(Resource.Layout.Go_Pro_Layout);
                
                TextDecorator = new TextDecorator();

                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    BillingSupport = new BillingSupport(this, InAppBillingGoogle.ProductId, AppSettings.Cert, InAppBillingGoogle.ListProductSku, this);  

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    BillingSupport?.Destroy();

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
                OptionLinerLayout = FindViewById<LinearLayout>(Resource.Id.OptionLinerLayout);
                UpgradeButton = FindViewById<AppCompatButton>(Resource.Id.UpgradeButton);
                HeadText = FindViewById<TextView>(Resource.Id.headText);
                PriceText = FindViewById<TextView>(Resource.Id.priceTextView);
                MainLayout = FindViewById<LinearLayout>(Resource.Id.mainLayout);
                RelativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);

                HeadText.Text = GetText(Resource.String.Lbl_Title_Pro1) + " " + AppSettings.ApplicationName + " " + GetText(Resource.String.Lbl_Title_Pro2);

                if (AppSettings.SetTabDarkTheme)
                {
                    MainLayout.SetBackgroundResource(Resource.Drawable.ShadowLinerLayoutDark);
                    RelativeLayout.SetBackgroundResource(Resource.Drawable.price_gopro_item_style_dark);
                }

                var list = ListUtils.SettingsSiteList;
                if (list != null)
                {
                    PriceText.Text = list.ProPrice + list.CurrencySymbol;
                }

                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                string name = "go_pro_array";
                int resourceId = Resources.GetIdentifier(name, "array", ApplicationInfo.PackageName);
                if (resourceId == 0)
                {
                    return;
                }

                string[] planArray = Resources.GetStringArray(resourceId);
                if (planArray != null)
                {
                    foreach (string options in planArray)
                    {
                        if (!string.IsNullOrEmpty(options))
                        {
                            AppCompatTextView text = new AppCompatTextView(this)
                            {
                                Text = options,
                                TextSize = 13
                            };
                            text.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));
                            text.Gravity = GravityFlags.CenterHorizontal;
                            text.SetTypeface(font, TypefaceStyle.Normal);
                            TextDecorator.Content = options;
                            TextDecorator.DecoratedContent = new Android.Text.SpannableString(options);
                            //TextDecorator.SetTextColor(IonIconsFonts.Checkmark, "#43a735");
                            //TextDecorator.SetTextColor(IonIconsFonts.Close, "#e13c4c");

                            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);//height and width are inpixel
                            layoutParams.SetMargins(0, 30, 0, 0);

                            text.LayoutParameters = layoutParams;
                            OptionLinerLayout.AddView(text);
                            TextDecorator.Build(text, TextDecorator.DecoratedContent);
                        }
                    }
                } 
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

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
                    UpgradeButton.Click += UpgradeButtonOnClick;
                }
                else
                {
                    UpgradeButton.Click -= UpgradeButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void UpgradeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Wallet));
                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));
                  
                dialogList.Title(GetText(Resource.String.Lbl_Go_Pro));
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

        #endregion
          
        #region MaterialDialog
         
        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string text)
        {
            try
            {
                if (text == GetString(Resource.String.Lbl_Wallet))
                {
                    if (DeepSoundTools.CheckWallet())
                    {
                        if (Methods.CheckConnectivity())
                        {
                            var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("go_pro", "");
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);

                                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.IsPro = 1;

                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);

                                        if (AppSettings.ShowGoPro && dataUser.IsPro != 1) return;
                                        var mainFragmentProIcon = HomeActivity.GetInstance()?.MainFragment?.ProIcon;
                                        if (mainFragmentProIcon != null)
                                            mainFragmentProIcon.Visibility = ViewStates.Gone;
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long)?.Show();
                                    Finish();
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
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                { 
                    BillingSupport?.PurchaseNow("membership");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Billing
         
        public void OnPaymentError(string error)
        {
            Console.WriteLine(error);
        }

        public async void OnPaymentSuccess(IList<Purchase> result)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.UpgradeMembershipAsync(UserDetails.UserId.ToString());
                    if (apiStatus == 200)
                    {
                        var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.IsPro = 1;

                            var sqlEntity = new SqLiteDatabase();
                            sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);

                            var mainFragmentProIcon = HomeActivity.GetInstance()?.MainFragment?.ProIcon;
                            if (mainFragmentProIcon != null)
                                mainFragmentProIcon.Visibility = ViewStates.Gone;
                        }

                        Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                        Finish();
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}