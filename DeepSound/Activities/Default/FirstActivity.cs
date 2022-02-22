using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.SQLite;
using DeepSoundClient;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FirstActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView ImageBackgroundGradation;
        private AppCompatButton BtnLogin, BtnRegister, BtnSkip;
        private TextView TxtFirstTitle;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            { 
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this , true);

                // Create your application here
                SetContentView(Resource.Layout.FirstLayout);

                InitializeDeepSound.Initialize(AppSettings.Cert, PackageName, AppSettings.TurnTrustFailureOnWebException);
                 
                //Get Value And Set Toolbar
                InitComponent();
                 
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.GetSettings_Api(this)}); 
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
                ImageBackgroundGradation = FindViewById<ImageView>(Resource.Id.backgroundGradation);
                BtnLogin = FindViewById<AppCompatButton>(Resource.Id.LoginButton);
                BtnRegister = FindViewById<AppCompatButton>(Resource.Id.RegisterButton);
                TxtFirstTitle = FindViewById<TextView>(Resource.Id.firstTitle);
                BtnSkip = FindViewById<AppCompatButton>(Resource.Id.SkipButton);
                 
                var metrics = Resources.System.DisplayMetrics;
                int height = metrics.HeightPixels;
                int width = metrics.WidthPixels;

                int[] color = { Color.ParseColor(AppSettings.BackgroundGradationColor1), Color.ParseColor(AppSettings.BackgroundGradationColor2) };
                var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, width, height);
                if (bitmap != null)
                {
                    ImageBackgroundGradation.SetImageBitmap(bitmap);
                }

                Console.WriteLine(gradient);
                TxtFirstTitle.Text = GetText(Resource.String.Lbl_FirstSubTitle);
                 
                if (!AppSettings.ShowSkipButton)
                    BtnSkip.Visibility = ViewStates.Gone;

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);
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
                    BtnLogin.Click += BtnLoginOnClick;
                    BtnRegister.Click += BtnRegisterOnClick;
                    BtnSkip.Click += SkipButtonOnClick;
                }
                else
                {
                    BtnLogin.Click -= BtnLoginOnClick;
                    BtnRegister.Click -= BtnRegisterOnClick;
                    BtnSkip.Click -= SkipButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void SkipButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.Username = "";
                UserDetails.FullName = "";
                UserDetails.Password = "";
                UserDetails.AccessToken = "";
                UserDetails.UserId = 0;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = "";
                UserDetails.Email = "";
                  
                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = "",
                    Password = "",
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                UserDetails.IsLogin = false;

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(HomeActivity)) : new Intent(this, typeof(HomeActivity)));
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnRegisterOnClick(object sender, EventArgs e)
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

        private void BtnLoginOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion
          
    }
}