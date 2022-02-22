using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.Security
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class TwoFactorAuthActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView IconTwoFactor;
        private EditText TxtTwoFactor, TxtTwoFactorCode;
        private AppCompatButton SaveButton;
        private string TypeTwoFactor, TypeDialog = "Confirmation";

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
                SetContentView(Resource.Layout.TwoFactorAuthLayout);

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
                IconTwoFactor = FindViewById<TextView>(Resource.Id.IconTwoFactor);
                TxtTwoFactor = FindViewById<EditText>(Resource.Id.TwoFactorEditText);

                //IconTwoFactorCode = FindViewById<TextView>(Resource.Id.IconTwoCodeFactor);

                TxtTwoFactorCode = FindViewById<EditText>(Resource.Id.TwoFactorCodeEditText);
                TxtTwoFactorCode.Visibility = ViewStates.Invisible;

                SaveButton = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTwoFactor, FontAwesomeIcon.ShieldAlt);

                Methods.SetColorEditText(TxtTwoFactor, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTwoFactorCode, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtTwoFactor);

                //wael
                var twoFactorUSer = ListUtils.MyUserInfoList?.FirstOrDefault()?.TwoFactor;
                if (twoFactorUSer == 0)
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Disable);
                    TypeTwoFactor = "off";
                }
                else
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Enable);
                    TypeTwoFactor = "on";
                }

                AdsGoogle.Ad_AdMobNative(this);
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
                    toolbar.Title = GetString(Resource.String.Lbl_TwoFactor);
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
                    TxtTwoFactor.Touch += TxtTwoFactorOnTouch;
                    SaveButton.Click += SaveButtonOnClick;
                }
                else
                {
                    TxtTwoFactor.Touch -= TxtTwoFactorOnTouch;
                    SaveButton.Click -= SaveButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        // check code email if good or no than update data user 
        private async void SendButtonOnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTwoFactorCode.Text) || string.IsNullOrWhiteSpace(TxtTwoFactorCode.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.User.VerifyTwoFactorAsync(UserDetails.UserId.ToString(), TxtTwoFactorCode.Text);
                if (apiStatus == 200)
                {
                    if (respond is MessageDataObject result)
                    {
                        Console.WriteLine(result.Data);

                        var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                        if (local != null)
                        {
                            local.TwoFactor = 1;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.InsertOrUpdate_DataMyInfo(local);
                        }

                        Toast.MakeText(this, GetText(Resource.String.Lbl_TwoFactorOn), ToastLength.Short)?.Show();
                        AndHUD.Shared.Dismiss(this);

                        Finish();
                    }
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtTwoFactorOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Confirmation";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_Enable), GetString(Resource.String.Lbl_Disable)
                };

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AutoDismiss(true).AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // send data and send api and show liner add code email 
        private async void SaveButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (TypeDialog == "Confirmation")
                {
                    switch (TypeTwoFactor)
                    {
                        case "on":
                            {
                                //Show a progress
                                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                                var (apiStatus, respond) = await RequestsAsync.User.UpdateTwoFactorAsync(UserDetails.UserId.ToString(), "enable");
                                if (apiStatus == 200)
                                {
                                    if (respond is not MessageDataObject result) return;
                                    if (result.Data.Contains("We have sent you an email with the confirmation code"))
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_ConfirmationCodeSent), ToastLength.Short)?.Show();

                                        AndHUD.Shared.Dismiss(this);

                                        TypeDialog = "ConfirmationCode";

                                        TxtTwoFactorCode.Visibility = ViewStates.Visible;
                                        SaveButton.Text = GetText(Resource.String.Btn_Send);
                                    }
                                    else
                                    {
                                        //Show a Error image with a message
                                        Methods.DisplayAndHudErrorResult(this, respond);
                                    }
                                }
                                else
                                {
                                    Methods.DisplayAndHudErrorResult(this, respond);
                                }

                                break;
                            }
                        case "off":
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.User.UpdateTwoFactorAsync(UserDetails.UserId.ToString(), "disable") });
                            var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                            if (local != null)
                            {
                                local.TwoFactor = 0;

                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(local);
                            }

                            Finish();
                            break;
                    }
                }
                else if (TypeDialog == "ConfirmationCode")
                {
                    SendButtonOnClick();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_Enable))
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Enable);
                    TypeTwoFactor = "on";
                    TxtTwoFactorCode.Visibility = ViewStates.Invisible;
                }
                else if (itemString == GetText(Resource.String.Lbl_Disable))
                {
                    TxtTwoFactor.Text = GetText(Resource.String.Lbl_Disable);
                    TypeTwoFactor = "off";
                    TxtTwoFactorCode.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

    }
}