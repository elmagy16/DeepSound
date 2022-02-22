using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using Com.Braintreepayments.Api.Dropin;
using Com.Razorpay;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Payment;
using DeepSoundClient.Classes.Payment;
using DeepSoundClient.Requests;
using MaterialDialogsCore;
using SecurionPay;
using BaseActivity = DeepSound.Activities.Base.BaseActivity;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class WalletActivity : BaseActivity, MaterialDialog.IListCallback, IPaymentResultListener, ISecurionPayPaymentListener
    {
        #region Variables Basic

        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;

        private TextView TxtMyBalance;
        public EditText TxtAmount;
        private AppCompatButton BtnReplenish;
        private InitPayPalPayment InitPayPalPayment;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitRazorPayPayment InitRazorPay;
        private InitPaySeraPayment PaySeraPayment;
        private InitSecurionPayPayment SecurionPayPayment;
        private string Price;
        private static WalletActivity Instance;

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
                SetContentView(Resource.Layout.WalletLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitBuy();
                InitComponent(); 
                InitToolbar();
                Get_Data_User();

                AdsGoogle.Ad_AdMobNative(this);
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
                if (AppSettings.ShowRazorPay) InitRazorPay?.StopRazorPay();
                if (AppSettings.ShowPayStack) PayStackPayment?.StopPayStack();
                if (AppSettings.ShowCashFree) CashFreePayment?.StopCashFree();
                if (AppSettings.ShowPaySera) PaySeraPayment?.StopPaySera();
                if (AppSettings.ShowSecurionPay) SecurionPayPayment?.StopSecurionPay();
                
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
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitBuy()
        {
            try
            {
                if (AppSettings.ShowPaypal) InitPayPalPayment ??= new InitPayPalPayment(this);
                if (AppSettings.ShowRazorPay) InitRazorPay ??= new InitRazorPayPayment(this);
                if (AppSettings.ShowPayStack) PayStackPayment ??= new InitPayStackPayment(this);
                if (AppSettings.ShowCashFree) CashFreePayment ??= new InitCashFreePayment(this);
                if (AppSettings.ShowPaySera) PaySeraPayment ??= new InitPaySeraPayment(this); 
                if (AppSettings.ShowSecurionPay) SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.SettingsSiteList?.SecurionpayPublicKey);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                Avatar = FindViewById<ImageView>(Resource.Id.avatar);
                TxtProfileName = FindViewById<TextView>(Resource.Id.name);
                TxtUsername = FindViewById<TextView>(Resource.Id.tv_subname);

                TxtMyBalance = FindViewById<TextView>(Resource.Id.myBalance);

                TxtAmount = FindViewById<EditText>(Resource.Id.AmountEditText);
                BtnReplenish = FindViewById<AppCompatButton>(Resource.Id.ReplenishButton);

                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                 
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
                    toolbar.Title = GetText(Resource.String.Lbl_Wallet);
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
                    BtnReplenish.Click += BtnReplenishOnClick;
                }
                else
                {
                    BtnReplenish.Click -= BtnReplenishOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static WalletActivity GetInstance()
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

        #region Events

        private void BtnReplenishOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterAmount), ToastLength.Long)?.Show();
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    return;
                }
                 
                Price = TxtAmount.Text;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (AppSettings.ShowPaypal) arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));
                if (AppSettings.ShowCreditCard) arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));
                if (AppSettings.ShowBankTransfer) arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));
                if (AppSettings.ShowRazorPay) arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));
                if (AppSettings.ShowPayStack) arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));
                if (AppSettings.ShowCashFree) arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));
                if (AppSettings.ShowPaySera) arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));
                if (AppSettings.ShowSecurionPay) arrayAdapter.Add(GetString(Resource.String.Lbl_SecurionPay));
                if (AppSettings.ShowPayUmoney) arrayAdapter.Add(GetString(Resource.String.Lbl_PayUmoney));
                if (AppSettings.ShowAuthorizeNet) arrayAdapter.Add(GetString(Resource.String.Lbl_AuthorizeNet));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region MaterialDialog

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    InitPayPalPayment.BtnPaypalOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
                else if (text == GetString(Resource.String.Lbl_RazorPay))
                {
                    InitRazorPay?.BtnRazorPayOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_PayStack))
                {
                    OpenPayStackDialog();
                }
                else if (text == GetString(Resource.String.Lbl_CashFree))
                {
                    OpenCashFreeDialog();
                }
                else if (text == GetString(Resource.String.Lbl_PaySera))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

                    await PaySera();
                }
                else if (text == GetString(Resource.String.Lbl_SecurionPay))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

                    await SecurionPay();
                }
                else if (text == GetString(Resource.String.Lbl_PayUmoney))
                {
                    OpenIntentPayUmoney();
                }
                else if (text == GetString(Resource.String.Lbl_AuthorizeNet))
                {
                    OpenIntentAuthorizeNet();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "AddFunds");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentPayUmoney()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PayUmoneyPaymentActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentAuthorizeNet()
        {
            try
            {
                Intent intent = new Intent(this, typeof(AuthorizeNetPaymentActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        //Result 
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == InitPayPalPayment.PayPalDataRequestCode)
                    if (resultCode == Result.Ok)
                    {
                        {
                            DropInResult dropInResult =
                                (DropInResult) data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                            if (dropInResult != null)
                            {
                                InitPayPalPayment.DisplayResult(dropInResult.PaymentMethodNonce,
                                    dropInResult.DeviceData);
                                if (Methods.CheckConnectivity())
                                {
                                    var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletPaypalAsync(TxtAmount.Text).ConfigureAwait(false);
                                    if (apiStatus == 200)
                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                TxtAmount.Text = string.Empty;

                                                Toast.MakeText(this,
                                                    GetText(Resource.String.Lbl_PaymentSuccessfully),
                                                    ToastLength.Long)?.Show();
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });
                                    else
                                        Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),
                                        ToastLength.Long)?.Show();
                                }
                            }
                        }
                    }
                    else if (resultCode == Result.Canceled)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                    }
                    else if (resultCode == Result.FirstUser)
                    {
                        var ss = (Exception) data.GetSerializableExtra(DropInActivity.ExtraError);
                        Toast.MakeText(this, ss?.Message, ToastLength.Long)?.Show();
                    }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region RazorPay
         
        public void OnPaymentError(int code, string response)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                Toast.MakeText(this, "Payment failed: " + response, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                if (!string.IsNullOrEmpty(razorpayPaymentId) && Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(Price) * 100;

                    var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletRazorpayAsync(razorpayPaymentId, priceInt.ToString());
                    if (apiStatus == 200)
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                TxtAmount.Text = string.Empty;
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    else
                        Methods.DisplayReportResult(this, respond);
                }
                else if (!string.IsNullOrEmpty(razorpayPaymentId))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region CashFree

        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light)
                    .Title(GetText(Resource.String.Lbl_CashFree)).TitleColorRes(Resource.Color.primary)
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            switch (check)
                            {
                                case false:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                    return;
                            }

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                                return;
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    })
                    .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                    .Build();

                var iconName = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = dialog.CustomView.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = dialog.CustomView.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = dialog.CustomView.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = DeepSoundTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(string name, string email, string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                        {"price", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeCashFreeAsync("initialize", AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CashFreeObject result:
                                        CashFreePayment ??= new InitCashFreePayment(this);
                                        CashFreePayment.DisplayCashFreePayment(result, Price);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
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

        #region PayStack

        private void OpenPayStackDialog()
        {
            try
            {
                var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialogBuilder.Title(Resource.String.Lbl_PayStack).TitleColorRes(Resource.Color.primary);
                dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                {
                    try
                    {
                        switch (s.Length)
                        {
                            case <= 0:
                                return;
                        }

                        var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                        switch (check)
                        {
                            case false:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                            default:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show(); 

                                await PayStack(s.ToString());
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                dialogBuilder.PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive((materialDialog, action) =>
                {

                });
                dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialogBuilder.AlwaysCallSingleChoiceCallback();
                dialogBuilder.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(Price) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"price", priceInt.ToString()},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePayStackAsync("initialize", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case InitializePaymentObject result:
                                    PayStackPayment ??= new InitPayStackPayment(this);
                                    PayStackPayment.DisplayPayStackPayment(result.Url,priceInt.ToString());
                                    break;
                            }

                            break;
                        }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region PaySera

        private async Task PaySera()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"price", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePaySeraAsync("initialize", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            switch (respond)
                            {
                                case InitializePaymentObject result:
                                    PaySeraPayment ??= new InitPaySeraPayment(this);
                                    PaySeraPayment.DisplayPaySeraPayment(result.Url, Price);
                                    break;
                            }

                            break;
                        }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region SecurionPay

        private async Task SecurionPay()
        {
            try
            {
                if (Methods.CheckConnectivity())
                { 
                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeSecurionPayAsync("initialize", Price);
                    switch (apiStatus)
                    {
                        case 200: 
                        {
                            switch (respond)
                            {
                                case InitializeSecurionPayObject result:
                                    SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.SettingsSiteList?.SecurionpayPublicKey);
                                    SecurionPayPayment.DisplaySecurionPayPayment(result.Token, Price , AppSettings.SetTabDarkTheme ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                                    break;
                            }

                            break;
                        }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private async Task SecurionPay(string request , string charge)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.SecurionPayAsync(request, charge);
                    switch (apiStatus)
                    {
                        case 200:
                            Toast.MakeText(this, this.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                            break;
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, this.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
           
        public void OnPaymentError(string error)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPaymentSuccess(SecurionPayResult result)
        {
            try
            {
                if (!string.IsNullOrEmpty(result?.Charge?.Id))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SecurionPay("pay", result.Charge.Id) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                    await ApiRequest.GetInfoData(this, UserDetails.UserId.ToString());

                var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (local != null)
                {
                    GlideImageLoader.LoadImage(this, local.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtProfileName.Text = DeepSoundTools.GetNameFinal(local);
                    TxtUsername.Text = "@" + local.Username;

                    TxtMyBalance.Text = local.WalletFormat;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}