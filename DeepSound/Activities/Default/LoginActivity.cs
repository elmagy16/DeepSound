using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.Credentials;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.SocialLogins;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using Newtonsoft.Json;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnCompleteListener, IOnFailureListener
    {
        #region Variables Basic

        private EditText EmailEditText, PasswordEditText;
        private ProgressBar ProgressBar;
        private FloatingActionButton BtnSignIn;
        private TextView ForgotPassTextView, RegisterTextView;
        private AppCompatButton WoWonderSignInButton;
        private LoginButton FbLoginButton;
        private SignInButton GoogleSignInButton;

        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public static LoginActivity Instance;
        public static GoogleSignInClient MGoogleSignInClient;

        private AppCompatButton ContinueButton;
         
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.LoginLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();
                 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                if (AppSettings.EnableSmartLockForPasswords)
                    BuildClients(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();

                if (!MIsResolving && AppSettings.EnableSmartLockForPasswords)
                {
                    RequestCredentials(false);
                    LoadHintClicked();
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
                ProfileTracker?.StopTracking(); 
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                BtnSignIn = FindViewById<FloatingActionButton>(Resource.Id.fab);
                ForgotPassTextView = FindViewById<TextView>(Resource.Id.txt_forgot_pass);
                RegisterTextView = FindViewById<TextView>(Resource.Id.txt_Regsiter);

                ContinueButton = FindViewById<AppCompatButton>(Resource.Id.btn_continue);
                ContinueButton.Visibility = ViewStates.Gone;
                 
                ProgressBar.Visibility = ViewStates.Gone;
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
                    BtnSignIn.Click += BtnSignInOnClick;
                    ForgotPassTextView.Click += ForgotPassTextViewOnClick;
                    RegisterTextView.Click += LinearRegisterOnClick;
                    ContinueButton.Click += ContinueButtonOnClick;
                }
                else
                {
                    BtnSignIn.Click -= BtnSignInOnClick;
                    ForgotPassTextView.Click -= ForgotPassTextViewOnClick;
                    RegisterTextView.Click -= LinearRegisterOnClick;
                    ContinueButton.Click -= ContinueButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    FbLoginButton.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }

                //#WoWonder 
                if (AppSettings.ShowWoWonderLogin)
                {
                    WoWonderSignInButton = FindViewById<AppCompatButton>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Click += WoWonderSignInButtonOnClick;

                    WoWonderSignInButton.Text = GetString(Resource.String.Lbl_LoginWith) + " " + AppSettings.AppNameWoWonder;
                    WoWonderSignInButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    WoWonderSignInButton = FindViewById<AppCompatButton>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Visibility = ViewStates.Gone;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Login With Facebook
        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Events

        //Continue as
        private void ContinueButtonOnClick(object sender, EventArgs e)
        {
            try
            { 
                RequestCredentials(true); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Login DeepSound
        private async void BtnSignInOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        ProgressBar.Visibility = ViewStates.Visible;
                        BtnSignIn.Visibility = ViewStates.Gone;

                        await AuthApi(EmailEditText.Text.Replace(" ", ""), PasswordEditText.Text); 
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    BtnSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));
            }
        }

        private async Task AuthApi(string email, string password)
        {
            var (apiStatus, respond) = await RequestsAsync.Auth.LoginAsync(email, password, UserDetails.DeviceId);
            if (apiStatus == 200)
            {
                if (respond is LoginObject auth)
                {
                    if (AppSettings.EnableSmartLockForPasswords)
                    {
                        // Save Google Sign In to SmartLock
                        Credential credential = new Credential.Builder(email)
                            .SetName(email)
                            .SetPassword(password)
                            .Build();

                        SaveCredential(credential);
                    }

                    SetDataLogin(auth);

                    StartActivity(new Intent(this, typeof(HomeActivity)));
                    Finish();
                }
                else if (respond is LoginTwoFactorObject factorObject)
                {
                    UserDetails.UserId = factorObject.UserId;
                    var intent = new Intent(this, typeof(VerificationAccountActivity));
                    intent.PutExtra("Type", "TwoFactor");
                    StartActivity(intent);

                    Finish();
                }
            }
            else if (apiStatus == 400)
            {
                if (respond is ErrorObject error)
                {
                    string errorText = error.Error;
                    switch (errorText)
                    {
                        case "Please check your details":
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                            break;
                        case "Incorrect username or password":
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                            break;
                        case "Your account is not activated yet, please check your inbox for the activation link":
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                            break;
                        default:
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            break;
                    }
                }

                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
            }
            else if (apiStatus == 404)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
            }
        }


        //Open Register
        private void LinearRegisterOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Forgot Password
        private void ForgotPassTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ForgotPasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetDataLogin(LoginObject auth)
        {
            try
            {
                UserDetails.Username = EmailEditText.Text;
                UserDetails.FullName = EmailEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.Data.Id;
                UserDetails.Status = "Active";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = EmailEditText.Text;

                Current.AccessToken = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = EmailEditText.Text,
                    Password = PasswordEditText.Text,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList = new ObservableCollection<DataTables.LoginTb> { user };
                 
                UserDetails.IsLogin = true;

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                if (auth.Data != null)
                {
                    ListUtils.MyUserInfoList = new ObservableCollection<UserDataObject> { auth.Data };
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                // Handle e
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                var data = json.ToString();
                var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                //FbEmail = result.Email;

                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            if (AppSettings.EnableSmartLockForPasswords)
                            {
                                // Save Google Sign In to SmartLock
                                Credential credential = new Credential.Builder(result.Email)
                                    .SetAccountType(IdentityProviders.Facebook)
                                    .SetName(result.Name)
                                    //.SetPassword(auth.AccessToken)
                                    .Build();

                                SaveCredential(credential);
                            }
                              
                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            string errorText = error.Error;
                            switch (errorText)
                            {
                                case "Please check your details":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //======================================================

        #region Google
         
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    BtnSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        //Login Api 
                        //string key = Methods.App.GetValueFromManifest(this, "com.google.android.geo.API_KEY");
                         var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                if (AppSettings.EnableSmartLockForPasswords)
                                {
                                    // Save Google Sign In to SmartLock
                                    Credential credential = new Credential.Builder(acct.Email)
                                        .SetAccountType(IdentityProviders.Google)
                                        .SetName(acct.DisplayName)
                                        .SetProfilePictureUri(acct.PhotoUrl)
                                        .Build();

                                    SaveCredential(credential);
                                }
                                    
                                SetDataLogin(auth);

                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                Finish();
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                string errorText = error.Error;
                                switch (errorText)
                                {
                                    case "Please check your details":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Incorrect username or password":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Your account is not activated yet, please check your inbox for the activation link":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        //======================================================

        #region WoWonder

        //Event Click login using WoWonder
        private void WoWonderSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WoWonderLoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void LoginWoWonder(string woWonderAccessToken)
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;
                ForgotPassTextView.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(woWonderAccessToken))
                {
                    //Login Api 
                     var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(woWonderAccessToken, "wowonder", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            FinishAffinity();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            string errorText = error.Error;
                            switch (errorText)
                            {
                                case "Please check your details":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else if (requestCode == RcCredentialsRead)
                {
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else if (requestCode == RcCredentialsSave)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        //Saved
                    }
                    else
                    {
                        //Credential save failed
                    }
                }
                else if (requestCode == RcCredentialsHint)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            OnlyPasswords = true;
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Cross App Authentication

        private static readonly int RcCredentialsSave = 1;
        private static readonly int RcCredentialsRead = 2;
        private static readonly int RcCredentialsHint = 3;

        private bool OnlyPasswords;
        private bool MIsResolving;
        private Credential MCredential;
        private string CredentialType;

        private CredentialsClient MCredentialsClient;
        private GoogleSignInClient MSignInClient;

        private void BuildClients(string accountName)
        {
            try
            {
                var gsoBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken(AppSettings.ClientId)
                    .RequestScopes(new Scope(Scopes.Profile))
                    .RequestScopes(new Scope(Scopes.PlusMe))
                    .RequestScopes(new Scope(Scopes.DriveAppfolder))
                    .RequestServerAuthCode(AppSettings.ClientId)
                    .RequestProfile().RequestEmail();

                if (accountName != null)
                    gsoBuilder.SetAccountName(accountName);
                 
                MCredentialsClient = Credentials.GetClient(this, CredentialsOptions.Default);
                MSignInClient = GoogleSignIn.GetClient(this, gsoBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadHintClicked()
        {
            try
            {
                HintRequest hintRequest = new HintRequest.Builder()
                    .SetHintPickerConfig(new CredentialPickerConfig.Builder()
                        .SetShowCancelButton(true)
                        .Build())
                    .SetIdTokenRequested(false)
                    .SetEmailAddressIdentifierSupported(true)
                    .SetAccountTypes(IdentityProviders.Google)
                    .Build();

                PendingIntent intent = MCredentialsClient.GetHintPickerIntent(hintRequest);
                StartIntentSenderForResult(intent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                MIsResolving = true;
            }
            catch (Exception e)
            {
                //Could not start hint picker Intent
                MIsResolving = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RequestCredentials(bool onlyPasswords)
        {
            try
            {
                OnlyPasswords = onlyPasswords;

                CredentialRequest.Builder crBuilder = new CredentialRequest.Builder()
                    .SetPasswordLoginSupported(true);

                if (!onlyPasswords)
                {
                    crBuilder.SetAccountTypes(IdentityProviders.Google);
                }

                CredentialType = "Request";

                MCredentialsClient.Request(crBuilder.Build()).AddOnCompleteListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void HandleCredential(Credential credential, bool onlyPasswords)
        {
            try
            {
                // See "Handle successful credential requests"  
                MCredential = credential;

                //Log.d(TAG, "handleCredential:" + credential.getAccountType() + ":" + credential.getId());
                if (IdentityProviders.Google.Equals(credential.AccountType))
                {
                    // Google account, rebuild GoogleApiClient to set account name and then try
                    BuildClients(credential.Id);
                    GoogleSilentSignIn();
                }
                else if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                {
                    // Email/password account
                    Console.WriteLine("Signed in as {0}", credential.Id);

                    ContinueButton.Text = GetString(Resource.String.Lbl_ContinueAs) + " " + credential.Id;
                    ContinueButton.Visibility = ViewStates.Visible;

                    if (onlyPasswords)
                    {
                        //send api auth  
                        ProgressBar.Visibility = ViewStates.Visible;
                        BtnSignIn.Visibility = ViewStates.Gone;
                         
                        await AuthApi(credential.Id, credential.Password);
                    }
                }
                else
                {
                    ContinueButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResolveResult(ResolvableApiException rae, int requestCode)
        {
            try
            {
                if (!MIsResolving)
                {
                    try
                    {
                        rae.StartResolutionForResult(this, requestCode);
                        MIsResolving = true;
                    }
                    catch (IntentSender.SendIntentException e)
                    {
                        MIsResolving = false;
                        //Failed to send Credentials intent
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SaveCredential(Credential credential)
        {
            try
            {
                if (credential == null)
                {
                    //Log.w(TAG, "Ignoring null credential.");
                    return;
                }

                CredentialType = "Save";
                MCredentialsClient.Save(credential).AddOnCompleteListener(this, this).AddOnFailureListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnGoogleRevokeClicked()
        {
            if (MCredential != null)
            {
                await MCredentialsClient.DeleteAsync(MCredential);
            }
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            try
            {
                if (CredentialType == "Request")
                {
                    if (task.IsSuccessful && task.Result is CredentialRequestResponse credential)
                    {
                        // Auto sign-in success
                        HandleCredential(credential.Credential, OnlyPasswords);
                        return;
                    }
                }
                else if (CredentialType == "Save")
                {
                    if (task.IsSuccessful)
                    {
                        return;
                    }
                }

                var ee = task.Exception;
                if (ee is ResolvableApiException rae)
                {
                    // Getting credential needs to show some UI, start resolution 
                    if (CredentialType == "Request")
                        ResolveResult(rae, RcCredentialsRead);

                    else if (CredentialType == "Save")
                        ResolveResult(rae, RcCredentialsSave);
                }
                else
                {
                    Console.WriteLine("request: not handling exception {0}", ee);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }

        private async void GoogleSilentSignIn()
        {
            try
            {
                // Try silent sign-in with Google Sign In API
                GoogleSignInAccount silentSignIn = await MSignInClient.SilentSignInAsync();
                if (silentSignIn != null)
                {
                    SetContentGoogle(silentSignIn);
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