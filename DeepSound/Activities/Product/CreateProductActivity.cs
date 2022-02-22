using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using DeepSound.Activities.Base;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Java.IO;
using MaterialDialogsCore;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;
using Console = System.Console;

namespace DeepSound.Activities.Product
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateProductActivity : BaseActivity, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private TextView TxtSave;
        private EditText TxtTitle, TxtDescription, TxtTags, TxtPrice, TxtTotalItem, TxtRelatedToSong, TxtCategory;
        private string CategoryId = "", SongId = "", TypeDialog = "";
        private PublisherAdView PublisherAdView;
        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private List<SoundDataObject> LatestSongsList;

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
                SetContentView(Resource.Layout.CreateProductLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GetMyInfoData();
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

                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);
                TxtTags = FindViewById<EditText>(Resource.Id.TagsText);
                TxtPrice = FindViewById<EditText>(Resource.Id.PriceText);
                TxtTotalItem = FindViewById<EditText>(Resource.Id.TotalItemText);
                TxtRelatedToSong = FindViewById<EditText>(Resource.Id.RelatedToSongText);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryText);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);
                
                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTags, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTotalItem, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrice, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRelatedToSong, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 

                Methods.SetFocusable(TxtRelatedToSong);
                Methods.SetFocusable(TxtCategory);

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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_CreateProduct);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<AttachmentsObject>() };
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                var attach = new AttachmentsObject
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();
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
                    MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    TxtSave.Click += TxtSaveOnClick;
                    TxtCategory.Touch += TxtCategoryOnClick;
                    TxtRelatedToSong.Touch += TxtRelatedToSongOnTouch;
                }
                else
                {
                    MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    TxtSave.Click -= TxtSaveOnClick;
                    TxtCategory.Touch -= TxtCategoryOnClick;
                    TxtRelatedToSong.Touch -= TxtRelatedToSongOnTouch;
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
                TxtTitle = null!;
                TxtPrice = null!;
                TxtCategory = null!;
                
                MAdapter = null!;
                MRecycler = null!;
                LayoutManager = null!;
                PublisherAdView = null!;
                CategoryId = "";
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        MAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;

                    if (item.TypeAttachment != "Default") return;
                    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtRelatedToSongOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                 
                if (LatestSongsList?.Count > 0)
                {
                    TypeDialog = "RelatedToSong";

                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    var arrayAdapter = LatestSongsList.Select(item => Methods.FunString.DecodeString(item.Title)).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_RelatedToSong)).TitleColorRes(Resource.Color.primary);
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, GetText(Resource.String.Lbl_Error_NotHaveSong));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                if (CategoriesController.ListCategoriesProducts.Count > 0)
                {
                    TypeDialog = "Categories";

                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_SelectCategories)).TitleColorRes(Resource.Color.primary);
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Products");
                }
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

                if (string.IsNullOrEmpty(TxtTitle.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTitle), ToastLength.Short)?.Show();
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
                   
                if (string.IsNullOrEmpty(TxtTags.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTags), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtPrice.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChoosePrice), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTotalItem.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTotalItem), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtRelatedToSong.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterRelatedToSong), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtCategory.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterCategory), ToastLength.Short)?.Show();
                    return;
                }
                 
                var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                if (list.Count == 0)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var (apiStatus, respond) = await RequestsAsync.Product.CreateProductAsync(TxtTitle.Text, TxtDescription.Text, TxtTags.Text, TxtPrice.Text, TxtTotalItem.Text, SongId, CategoryId, list);
                    if (apiStatus == 200)
                    {
                        if (respond is GetProductDataObject result)
                        {
                            AndHUD.Shared.Dismiss(this);
                            Console.WriteLine(result.Message);
                               
                            Intent intent = new Intent();
                            intent.PutExtra("itemData", JsonConvert.SerializeObject(result.Data));
                            SetResult(Result.Ok, intent);

                            Toast.MakeText(this, GetString(Resource.String.Lbl_ProductSuccessfullyEdited), ToastLength.Short)?.Show();

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
                            var productPathImage = resultUri.Path;
                            var attach = new AttachmentsObject
                            {
                                Id = MAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "image[]",
                                FileSimple = productPathImage,
                                FileUrl = productPathImage
                            };

                            MAdapter.Add(attach);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
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

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (TypeDialog == "Categories")
                {
                    CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;
                    TxtCategory.Text = itemString;
                }
                else if (TypeDialog == "RelatedToSong")
                {
                    SongId = LatestSongsList?.FirstOrDefault(item => Methods.FunString.DecodeString(item.Title) == itemString)?.Id.ToString() ?? "";
                    TxtRelatedToSong.Text = itemString;
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
                if ((int) Build.VERSION.SdkInt < 23)
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
         
        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                LatestSongsList = ListUtils.MyUserInfoList?.FirstOrDefault()?.Latestsongs;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}