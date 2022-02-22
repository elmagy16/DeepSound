using System;
using System.Collections.Generic;
using System.Linq;
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
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Requests;
using Java.IO;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Albums
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditAlbumActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView AlbumImage;
        private CircleButton BtnClose;
        private AppCompatButton BtnSelectImage, BtnSave;
        private TextView TxtSubTitle, IconTitle, IconDescription, IconGenres, IconPrice;
        private EditText TitleEditText, DescriptionEditText, GenresEditText, PriceEditText;
        private string CurrencySymbol = "$", PathImage = "", TypeDialog = "", IdGenres = "", IdPrice = "";
        private DataAlbumsObject AlbumObject;
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
                SetContentView(Resource.Layout.UploadAlbumLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                CurrencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                SetDataAlbum();
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
                TxtSubTitle.Visibility = ViewStates.Gone;

                AlbumImage = FindViewById<ImageView>(Resource.Id.Image);
                BtnClose = FindViewById<CircleButton>(Resource.Id.ImageCircle);
                BtnSelectImage = FindViewById<AppCompatButton>(Resource.Id.btn_AddPhoto);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TitleEditText = FindViewById<EditText>(Resource.Id.TitleEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                DescriptionEditText = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconGenres = FindViewById<TextView>(Resource.Id.IconGenres);
                GenresEditText = FindViewById<EditText>(Resource.Id.GenresEditText);

                IconPrice = FindViewById<TextView>(Resource.Id.IconPrice);
                PriceEditText = FindViewById<EditText>(Resource.Id.PriceEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);
                BtnSave.Text = GetText(Resource.String.Lbl_Submit);

                Methods.SetColorEditText(TitleEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(DescriptionEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(GenresEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(PriceEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.TextWidth);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.AudioDescription);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPrice, FontAwesomeIcon.User);

                Methods.SetFocusable(GenresEditText);
                Methods.SetFocusable(PriceEditText);

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
                    toolbar.Title = GetString(Resource.String.Lbl_EditAlbum);
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
                    BtnSave.Click += BtnSaveOnClick;
                    GenresEditText.Touch += GenresEditTextOnClick;
                    PriceEditText.Touch += PriceEditTextOnClick;
                }
                else
                {
                    BtnClose.Click -= BtnCloseOnClick;
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
                    BtnSave.Click -= BtnSaveOnClick;
                    GenresEditText.Touch -= GenresEditTextOnClick;
                    PriceEditText.Touch -= PriceEditTextOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Click Save data Album
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

                    var dictionary = new Dictionary<string, string>
                    {
                        {"title", TitleEditText.Text},
                        {"description", DescriptionEditText.Text},
                        {"album-id", AlbumObject.AlbumId},
                        {"category_id", IdGenres},
                        {"album-price", IdPrice},
                    };

                    if (!string.IsNullOrEmpty(PathImage))
                    {
                        dictionary.Add("album-thumbnail", PathImage);
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                     var (apiStatus, respond) = await RequestsAsync.Albums.UpdateAlbumSongsAsync(dictionary); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is UpdateAlbumObject result)
                        {
                            Console.WriteLine(result.Url);

                            Toast.MakeText(this, GetText(Resource.String.Lbl_UpdatedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

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
                GlideImageLoader.LoadImage(this, "Grey_Offline", AlbumImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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

        //Price
        private void PriceEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Price";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = ListUtils.PriceList.Select(item => CurrencySymbol + item.Price).ToList();

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
                                GlideImageLoader.LoadImage(this, file.Path, AlbumImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                UploadImage(file.Path);
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
                    IdPrice = ListUtils.PriceList[position]?.Id.ToString();
                    PriceEditText.Text = text;
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

        private async void UploadImage(string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                     var (apiStatus, respond) = await RequestsAsync.Tracks.UploadThumbnailAsync(path).ConfigureAwait(false);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetDataAlbum()
        {
            try
            {
                AlbumObject = JsonConvert.DeserializeObject<DataAlbumsObject>(Intent?.GetStringExtra("ItemData") ?? "");
                if (AlbumObject != null)
                {
                    PathImage = "";

                    GlideImageLoader.LoadImage(this, AlbumObject.Thumbnail, AlbumImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TitleEditText.Text = Methods.FunString.DecodeString(AlbumObject.Title);
                    DescriptionEditText.Text = Methods.FunString.DecodeString(AlbumObject.Description);

                    GenresEditText.Text = Methods.FunString.DecodeString(AlbumObject.CategoryName);

                    if (AlbumObject.Price.ToString() == "0.00" || AlbumObject.Price.ToString() == "0")
                        PriceEditText.Text = GetText(Resource.String.Lbl_Free);
                    else
                        PriceEditText.Text = CurrencySymbol + AlbumObject.Price;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}