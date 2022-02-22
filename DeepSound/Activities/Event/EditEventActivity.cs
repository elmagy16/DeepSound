using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using AndroidHUD;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Requests;
using Java.IO;
using MaterialDialogsCore;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Event
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditEventActivity : BaseActivity, View.IOnClickListener, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private TextView TxtSave;
        private RelativeLayout LayoutEventPhoto, LayoutEventVideoTrailer;
        private ImageView ImageCover, ImageVideoTrailer;
        private EditText TxtName, TxtDescription, TxtLocation, TxtLocationData, TxtStartDate, TxtStartTime, TxtEndDate, TxtEndTime, TxtTimezone, TxtSellTickets, TxtTicketsAvailable, TxtTicketPrice;
        private LinearLayout LayoutTicketsData;
        private string Timezone, Location, SellTicket = "no", EventPathImage, EventPathVideo, ImageType, TypeDialog = "";
        private PublisherAdView PublisherAdView;
        private Dictionary<string, string> TimezonesList;

        private EventDataObject EventData;
        private string EventId;

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
                SetContentView(Resource.Layout.CreateEventLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GetDataEvent();
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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                DestroyBasic();
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

        private void InitComponent()
        {
            try
            {
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtSave.Text = GetText(Resource.String.Lbl_Save);

                LayoutEventPhoto = FindViewById<RelativeLayout>(Resource.Id.LayoutEventPhoto);
                ImageCover = FindViewById<ImageView>(Resource.Id.imageCover);

                LayoutEventVideoTrailer = FindViewById<RelativeLayout>(Resource.Id.LayoutEventVideoTrailer);
                ImageVideoTrailer = FindViewById<ImageView>(Resource.Id.imageVideoTrailer);

                TxtName = FindViewById<EditText>(Resource.Id.NameEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                TxtLocation = FindViewById<EditText>(Resource.Id.LocationText);
                TxtLocationData = FindViewById<EditText>(Resource.Id.LocationDataText);

                TxtStartDate = FindViewById<EditText>(Resource.Id.StartDateEditText);
                TxtStartTime = FindViewById<EditText>(Resource.Id.StartTimeEditText);

                TxtEndDate = FindViewById<EditText>(Resource.Id.EndDateEditText);
                TxtEndTime = FindViewById<EditText>(Resource.Id.EndTimeEditText);

                TxtTimezone = FindViewById<EditText>(Resource.Id.TimezoneText);
                TxtSellTickets = FindViewById<EditText>(Resource.Id.SellTicketsText);

                LayoutTicketsData = FindViewById<LinearLayout>(Resource.Id.LayoutTicketsData);
                TxtTicketsAvailable = FindViewById<EditText>(Resource.Id.TicketsAvailableEditText);
                TxtTicketPrice = FindViewById<EditText>(Resource.Id.TicketPriceEditText);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocationData, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartDate, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartTime, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndDate, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndTime, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTimezone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSellTickets, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTicketsAvailable, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTicketPrice, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtStartTime);
                Methods.SetFocusable(TxtEndTime);
                Methods.SetFocusable(TxtStartDate);
                Methods.SetFocusable(TxtEndDate);

                Methods.SetFocusable(TxtLocation);
                Methods.SetFocusable(TxtTimezone);
                Methods.SetFocusable(TxtSellTickets);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                LayoutTicketsData.Visibility = ViewStates.Gone;
                TxtSellTickets.Hint = GetText(Resource.String.Lbl_No);
                SellTicket = "no";

                TxtStartTime.SetOnClickListener(this);
                TxtEndTime.SetOnClickListener(this);
                TxtStartDate.SetOnClickListener(this);
                TxtEndDate.SetOnClickListener(this);

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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_EditEvent);
                    toolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                if (addEvent)
                {
                    LayoutEventPhoto.Click += LayoutEventPhotoOnClick;
                    LayoutEventVideoTrailer.Click += LayoutEventVideoTrailerOnClick;
                    TxtLocation.Touch += TxtLocationOnTouch;
                    TxtTimezone.Touch += TxtTimezoneOnTouch;
                    TxtSellTickets.Touch += TxtSellTicketsOnTouch;
                    TxtSave.Click += TxtSaveOnClick;
                }
                else
                {
                    LayoutEventPhoto.Click -= LayoutEventPhotoOnClick;
                    LayoutEventVideoTrailer.Click -= LayoutEventVideoTrailerOnClick;
                    TxtLocation.Touch -= TxtLocationOnTouch;
                    TxtTimezone.Touch -= TxtTimezoneOnTouch;
                    TxtSellTickets.Touch -= TxtSellTicketsOnTouch;
                    TxtSave.Click -= TxtSaveOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                PublisherAdView?.Destroy();

                TxtSave = null!;
                PublisherAdView = null!;
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void LayoutEventVideoTrailerOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogVideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LayoutEventPhotoOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery("Image");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtTimezoneOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Timezone";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = TimezonesList.Select(item => item.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Timezone)).TitleColorRes(Resource.Color.primary);
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

        private void TxtLocationOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Location";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Online));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RealLocation));

                dialogList.Title(GetText(Resource.String.Lbl_Location)).TitleColorRes(Resource.Color.primary);
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

        private void TxtSellTicketsOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "SellTicket";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Yes));
                arrayAdapter.Add(GetText(Resource.String.Lbl_No));

                dialogList.Title(GetText(Resource.String.Lbl_SellTickets)).TitleColorRes(Resource.Color.primary);
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

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtName.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescription), ToastLength.Short)?.Show();
                    return;
                }

                if (TxtDescription.Text.Length < 10)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_DescriptionIsShort), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescription), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtStartDate.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectStartDate), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtEndDate.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectEndDate), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtLocation.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectLocation), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtStartTime.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectStartTime), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtEndTime.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectEndTime), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(EventPathImage))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                    return;
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", TxtName.Text},
                        {"desc", TxtDescription.Text},
                        {"location", Location},
                        {"start_date", TxtStartDate.Text},
                        {"start_time", TxtStartTime.Text},
                        {"end_date", TxtEndDate.Text},
                        {"end_time", TxtEndTime.Text},
                        {"timezone", Timezone},
                        {"sell_tickets", SellTicket},
                    };

                    switch (Location)
                    {
                        case "online":
                            keyValues.Add("online_url", TxtLocationData.Text);
                            break;
                        case "real":
                            keyValues.Add("real_address", TxtLocationData.Text);
                            break;
                    }

                    if (SellTicket == "yes")
                    {
                        keyValues.Add("available_tickets", TxtTicketsAvailable.Text);
                        keyValues.Add("ticket_price", TxtTicketPrice.Text);
                    }

                    var (apiStatus, respond) = await RequestsAsync.Event.CreateEventAsync(keyValues, EventPathImage, EventPathVideo);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateEventObject result)
                        {
                            AndHUD.Shared.Dismiss(this);
                            Console.WriteLine(result.Message);

                            Intent intent = new Intent();
                            intent.PutExtra("itemData", JsonConvert.SerializeObject(result.Data));
                            SetResult(Result.Ok, intent);

                            Toast.MakeText(this, GetString(Resource.String.Lbl_EventSuccessfullyEdited), ToastLength.Short)?.Show();

                            Finish();
                        }
                    }
                    else
                        Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
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

                if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    var result = CropImage.GetActivityResult(data);
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        {
                            EventPathImage = resultUri.Path;

                            File file2 = new File(resultUri.Path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageCover);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //"Uri" >> filepath
                            //"Thumbnail" >> fullPathFile.Path

                            EventPathVideo = filepath;

                            File file2 = new File(fullPathFile.Path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageVideoTrailer);

                        }
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //"Uri" >> filepath
                            //"Thumbnail" >> fullPathFile.Path

                            EventPathVideo = filepath;

                            File file2 = new File(fullPathFile.Path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageVideoTrailer);

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
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (ImageType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image":
                                OpenDialogGallery("Image");
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "VideoCamera":
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                        }
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

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (TypeDialog == "Timezone")
                {
                    Timezone = TimezonesList.FirstOrDefault(item => item.Value == itemString).Key;
                    TxtTimezone.Text = itemString;
                }
                else if (TypeDialog == "Location")
                {
                    TxtLocation.Text = itemString;
                    if (itemString == GetText(Resource.String.Lbl_Online))
                    {
                        Location = "online";
                        TxtLocationData.Hint = GetText(Resource.String.Lbl_Url);
                    }
                    else if (itemString == GetText(Resource.String.Lbl_RealLocation))
                    {
                        Location = "real";
                        TxtLocationData.Hint = GetText(Resource.String.Lbl_Address);
                    }
                }
                else if (TypeDialog == "SellTicket")
                {
                    TxtSellTickets.Text = itemString;
                    if (itemString == GetText(Resource.String.Lbl_Yes))
                    {
                        SellTicket = "yes";
                        LayoutTicketsData.Visibility = ViewStates.Visible;
                    }
                    else if (itemString == GetText(Resource.String.Lbl_No))
                    {
                        SellTicket = "no";
                        LayoutTicketsData.Visibility = ViewStates.Gone;
                    }
                }
                else if (TypeDialog == "DialogVideo")
                {
                    if (itemString == GetText(Resource.String.Lbl_VideoGallery))
                    {
                        ImageType = "Video";

                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher
                            case < 23:
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            default:
                                {
                                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                                    {
                                        //requestCode >> 501 => video Gallery
                                        new IntentController(this).OpenIntentVideoGallery();
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
                                }
                        }
                    }
                    else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera))
                    {
                        ImageType = "VideoCamera";

                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher
                            case < 23:
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            default:
                                {
                                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                                    {
                                        //requestCode >> 513 => video Camera
                                        new IntentController(this).OpenIntentVideoCamera();
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
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

        private void OpenDialogVideo()
        {
            try
            {
                TypeDialog = "DialogVideo";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.Title(GetText(Resource.String.Lbl_SelectVideoFrom)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void OpenDialogGallery(string imageType)
        {
            try
            {
                ImageType = imageType;
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
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
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
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

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == TxtStartTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtEndTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtStartDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartDate.Text = time.Date.ToString("yyyy-MM-dd");
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
                else if (v.Id == TxtEndDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndDate.Text = time.Date.ToString("yyyy-MM-dd");
                    });
                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void GetDataEvent()
        {
            try
            {              
                TimezonesList = DeepSoundTools.GetTimezonesList();

                EventData = JsonConvert.DeserializeObject<EventDataObject>(Intent?.GetStringExtra("EventView") ?? "");
                if (EventData != null)
                {
                    EventId = EventData.Id.ToString();

                    TxtName.Text = Methods.FunString.DecodeString(EventData.Name);
                    TxtDescription.Text = Methods.FunString.DecodeString(EventData.Desc);

                    if (!string.IsNullOrEmpty(EventData.OnlineUrl))
                    {
                        TxtLocation.Hint = GetText(Resource.String.Lbl_Url);
                        Location = "online";
                        TxtLocationData.Text = EventData.OnlineUrl;
                    }
                    else if (!string.IsNullOrEmpty(EventData.RealAddress))
                    {
                        TxtLocation.Hint = GetText(Resource.String.Lbl_Address);
                        Location = "real";
                        TxtLocationData.Text = EventData.RealAddress;
                    }
                      
                    TxtStartDate.Text = EventData.StartDate;
                    TxtStartTime.Text = EventData.StartTime;
                    TxtEndDate.Text = EventData.EndDate;
                    TxtEndTime.Text = EventData.EndTime;

                    Timezone = EventData.Timezone;
                    TxtTimezone.Text  = TimezonesList.FirstOrDefault(item => item.Key == EventData.Timezone).Value;

                    if (EventData.AvailableTickets > 0 || EventData.TicketPrice > 0)
                    {
                        SellTicket = "yes";
                        LayoutTicketsData.Visibility = ViewStates.Visible;
                        TxtSellTickets.Text = GetText(Resource.String.Lbl_Yes);

                        TxtTicketsAvailable.Text = EventData.AvailableTickets?.ToString(); 
                        TxtTicketPrice.Text = EventData.TicketPrice?.ToString();
                    }
                    else  
                    {
                        SellTicket = "no";
                        LayoutTicketsData.Visibility = ViewStates.Gone; 
                        TxtSellTickets.Text = GetText(Resource.String.Lbl_No);

                        TxtTicketsAvailable.Text = "";
                        TxtTicketPrice.Text = "";
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