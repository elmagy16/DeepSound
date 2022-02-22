using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AT.Markushi.UI;
using DeepSound.Activities.Base;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Requests;
using Java.IO;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Upload
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class UploadSongActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView PlaylistImage;
        private CircleButton BtnClose;
        private AppCompatButton BtnSelectImage, BtnSave;
        private TextView TxtSubTitle, IconTitle, IconDescription, IconTags, IconGenres, IconPrice, IconAvailability, IconAgeRestriction, IconLyrics, IconAllowDownloads;
        private EditText TitleEditText, DescriptionEditText, TagsEditText, GenresEditText, PriceEditText, AgeRestrictionEditText, LyricsEditText, AllowDownloadsEditText;
        private RadioButton RbPublic, RbPrivate;
        private string CurrencySymbol = "$", Status = "0", PathImage = "", SongLocation = "", TypeDialog = "", IdGenres = "", IdPrice = "0", IdAgeRestriction = "", IdAllowDownloads = "";
        private PublisherAdView PublisherAdView;

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
                SetContentView(Resource.Layout.UploadSongLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                CurrencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                var data = Intent?.GetStringExtra("SongLocation") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => UploadSong(data) });
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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                PublisherAdView?.Destroy(); 
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
                TxtSubTitle = FindViewById<TextView>(Resource.Id.subTitle);

                PlaylistImage = FindViewById<ImageView>(Resource.Id.Image);
                BtnClose = FindViewById<CircleButton>(Resource.Id.ImageCircle);
                BtnSelectImage = FindViewById<AppCompatButton>(Resource.Id.btn_AddPhoto);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TitleEditText = FindViewById<EditText>(Resource.Id.TitleEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                DescriptionEditText = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconLyrics = FindViewById<TextView>(Resource.Id.IconLyrics);
                LyricsEditText = FindViewById<EditText>(Resource.Id.LyricsEditText);

                IconTags = FindViewById<TextView>(Resource.Id.IconTags);
                TagsEditText = FindViewById<EditText>(Resource.Id.TagsEditText);

                IconPrice = FindViewById<TextView>(Resource.Id.IconPrice);
                PriceEditText = FindViewById<EditText>(Resource.Id.PriceEditText);

                IconGenres = FindViewById<TextView>(Resource.Id.IconGenres);
                GenresEditText = FindViewById<EditText>(Resource.Id.GenresEditText);

                IconAvailability = FindViewById<TextView>(Resource.Id.IconAvailability);
                RbPublic = FindViewById<RadioButton>(Resource.Id.radioPublic);
                RbPrivate = FindViewById<RadioButton>(Resource.Id.radioPrivate);

                IconAgeRestriction = FindViewById<TextView>(Resource.Id.IconAgeRestriction);
                AgeRestrictionEditText = FindViewById<EditText>(Resource.Id.AgeRestrictionEditText);

                IconAllowDownloads = FindViewById<TextView>(Resource.Id.IconAllowDownloads);
                AllowDownloadsEditText = FindViewById<EditText>(Resource.Id.AllowDownloadsEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                RbPublic.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                RbPrivate.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetColorEditText(TitleEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(DescriptionEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(LyricsEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TagsEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(GenresEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(PriceEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(AgeRestrictionEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(AllowDownloadsEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.TextWidth);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTags, FontAwesomeIcon.Tags);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.AudioDescription);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPrice, IonIconsFonts.Cash);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAvailability, FontAwesomeIcon.ShieldAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAgeRestriction, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLyrics, FontAwesomeIcon.FileAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAllowDownloads, FontAwesomeIcon.Download);

                Methods.SetFocusable(GenresEditText);
                Methods.SetFocusable(PriceEditText);
                Methods.SetFocusable(AgeRestrictionEditText);
                Methods.SetFocusable(AllowDownloadsEditText);

                RbPublic.Checked = true;
                Status = "0";

                if (!AppSettings.ShowPrice)
                {
                    PriceEditText.Visibility = ViewStates.Gone;
                    IconPrice.Visibility = ViewStates.Gone;
                }

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolbar.Title = GetString(Resource.String.Lbl_UploadSong);
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
                    BtnClose.Click += BtnCloseOnClick;
                    BtnSelectImage.Click += BtnSelectImageOnClick;
                    RbPublic.CheckedChange += RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange += RadioPrivateOnCheckedChange;
                    BtnSave.Click += BtnSaveOnClick;
                    GenresEditText.Touch += GenresEditTextOnClick;
                    PriceEditText.Touch += PriceEditTextOnClick;
                    AgeRestrictionEditText.Touch += AgeRestrictionEditTextOnClick;
                    AllowDownloadsEditText.Touch += AllowDownloadsEditTextOnTouch;
                }
                else
                {
                    BtnClose.Click -= BtnCloseOnClick;
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
                    RbPublic.CheckedChange -= RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange -= RadioPrivateOnCheckedChange;
                    BtnSave.Click -= BtnSaveOnClick;
                    GenresEditText.Touch -= GenresEditTextOnClick;
                    PriceEditText.Touch -= PriceEditTextOnClick;
                    AgeRestrictionEditText.Touch -= AgeRestrictionEditTextOnClick;
                    AllowDownloadsEditText.Touch -= AllowDownloadsEditTextOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Click Save data Playlist
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TitleEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTitleSong), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(DescriptionEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescriptionSong), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TagsEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTags), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(GenresEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseGenres), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PriceEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChoosePrice), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(AgeRestrictionEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseAgeRestriction), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(AllowDownloadsEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseAllowDownloads), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImage))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                     var (apiStatus, respond) = await RequestsAsync.Tracks.SubmitTrackAsync(TitleEditText.Text, DescriptionEditText.Text, LyricsEditText.Text, TagsEditText.Text, PathImage, SongLocation, IdGenres, Status, IdAgeRestriction, IdAllowDownloads, IdPrice); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is SubmitTrackObject result)
                        {
                            var ffmpegSystem = ListUtils.SettingsSiteList?.FfmpegSystem ?? "on";
                            if (ffmpegSystem == "on")
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.ConvertTrackAsync(result.AudioId, result.SongLocation) });

                            Toast.MakeText(this, GetText(Resource.String.Lbl_AddedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            result.TrackData.AudioLocation = !result.TrackData.AudioLocation.Contains(InitializeDeepSound.WebsiteUrl)
                                    ? InitializeDeepSound.WebsiteUrl + "/" + result.SongLocation
                                    : result.SongLocation;

                            var dataProfileFragment = HomeActivity.GetInstance()?.ProfileFragment?.SongsFragment?.MAdapter;
                            dataProfileFragment?.SoundsList.Insert(0, result.TrackData);
                            dataProfileFragment?.NotifyItemInserted(dataProfileFragment.SoundsList.IndexOf(dataProfileFragment.SoundsList.FirstOrDefault()));

                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Private
        private void RadioPrivateOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPrivate.Checked;
                if (isChecked)
                {
                    RbPublic.Checked = false;
                    Status = "1";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Public
        private void RadioPublicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPublic.Checked;
                if (isChecked)
                {
                    RbPrivate.Checked = false;
                    Status = "0";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Gallery 
        private void BtnSelectImageOnClick(object sender, EventArgs e)
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

        //Remove image 
        private void BtnCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                PathImage = "";
                GlideImageLoader.LoadImage(this, "Grey_Offline", PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Price
        private void PriceEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Price";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                foreach (var item in ListUtils.PriceList)
                    if (item.Price == "0.00" || item.Price == "0.0" || item.Price == "0")
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Free));
                    else
                        arrayAdapter.Add(CurrencySymbol + item.Price);

                dialogList.Title(GetText(Resource.String.Lbl_Price));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Genres
        private void GenresEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Genres";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = ListUtils.GenresList.Select(item => item.CateogryName).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Genres));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AgeRestriction
        private void AgeRestrictionEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "AgeRestriction";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_AgeRestrictionText0),
                    GetString(Resource.String.Lbl_AgeRestrictionText1)
                };

                dialogList.Title(GetText(Resource.String.Lbl_AgeRestriction));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AllowDownloads
        private void AllowDownloadsEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "AllowDownloads";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_Yes),
                    GetString(Resource.String.Lbl_No),
                };

                dialogList.Title(GetText(Resource.String.Lbl_AllowDownloads));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
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

                var result = CropImage.GetActivityResult(data);

                //If its from Camera or Gallery
                if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                var file = Uri.FromFile(new File(resultUri.Path));
                                GlideImageLoader.LoadImage(this, file.Path, PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => UploadImage(file.Path) });
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }
                        }
                    }
                }
                else if (requestCode == CropImage.CropImageActivityResultErrorCode)
                {
                    Exception error = result.Error;
                    Console.WriteLine(error);
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

        #region MaterialDialog

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

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;

                if (TypeDialog == "Genres")
                {
                    IdGenres = ListUtils.GenresList[position]?.Id.ToString();
                    GenresEditText.Text = text;
                }
                else if (TypeDialog == "Price")
                {
                    IdPrice = ListUtils.PriceList[position]?.Price;
                    PriceEditText.Text = text;
                }
                else if (TypeDialog == "AgeRestriction")
                {
                    IdAgeRestriction = position.ToString();
                    AgeRestrictionEditText.Text = text;
                }
                else if (TypeDialog == "AllowDownloads")
                {
                    if (text == GetString(Resource.String.Lbl_Yes))
                    {
                        IdAllowDownloads = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No))
                    {
                        IdAllowDownloads = "0";
                    }

                    AllowDownloadsEditText.Text = text;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void OpenDialogGallery()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
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
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task UploadImage(string path)
        {
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
            {
                 var (apiStatus, respond) = await RequestsAsync.Tracks.UploadThumbnailAsync(path);
                if (apiStatus.Equals(200))
                {
                    if (respond is UploadThumbnailObject resultUpload)
                        PathImage = resultUpload.Thumbnail;
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
        }

        private async Task UploadSong(string path)
        {
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
            {
                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Processing));

                 var (apiStatus, respond) = await RequestsAsync.Tracks.UploadTrackAsync(path);
                if (apiStatus.Equals(200))
                {
                    if (respond is UploadTrackObject resultUpload)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_AddedSuccessfully), ToastLength.Short)?.Show();

                        SongLocation = resultUpload.FilePath;

                        TxtSubTitle.Text = GetText(Resource.String.Lbl_subTitleUploadSong) + " " + resultUpload.FileName;
                     
                        AndHUD.Shared.Dismiss(this);
                    }
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                    Finish();
                }
            }
        }
    }
}