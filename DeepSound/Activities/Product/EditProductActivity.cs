using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using MaterialDialogsCore;
using Newtonsoft.Json;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Console = System.Console;

namespace DeepSound.Activities.Product
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditProductActivity : BaseActivity, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private TextView TxtSave, TxtAddImages;
        private EditText TxtTitle, TxtDescription, TxtTags, TxtPrice, TxtTotalItem, TxtRelatedToSong, TxtCategory;
        private string CategoryId = "", SongId = "", TypeDialog = "";
        private PublisherAdView PublisherAdView;
        private RecyclerView MRecycler;
        private List<SoundDataObject> LatestSongsList;
        private string ProductId;
        private ProductDataObject ProductData;

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
                GetDataProduct();
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

                TxtAddImages = (TextView)FindViewById(Resource.Id.AddImagesText);
                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);
                TxtAddImages.Visibility = ViewStates.Gone;
                MRecycler.Visibility = ViewStates.Gone;

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
                    toolBar.Title = GetText(Resource.String.Lbl_EditProduct);
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
                    TxtSave.Click += TxtSaveOnClick;
                    TxtCategory.Touch += TxtCategoryOnClick;
                    TxtRelatedToSong.Touch += TxtRelatedToSongOnTouch;
                }
                else
                {
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

                MRecycler = null!;
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

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Product.EditProductAsync(ProductId,TxtTitle.Text, TxtDescription.Text, TxtTags.Text, TxtPrice.Text, TxtTotalItem.Text, SongId, CategoryId);
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
          
        private void GetDataProduct()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                LatestSongsList = ListUtils.MyUserInfoList?.FirstOrDefault()?.Latestsongs;
                 
                ProductData = JsonConvert.DeserializeObject<ProductDataObject>(Intent?.GetStringExtra("ProductView") ?? "");
                if (ProductData != null)
                {
                    ProductId = ProductData.Id.ToString();
                      
                    TxtTitle.Text = Methods.FunString.DecodeString(ProductData.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(ProductData.Desc);
                    TxtTags.Text = ProductData.Tags;
                    TxtPrice.Text = ProductData.Price.ToString();
                    TxtTotalItem.Text = ProductData.Units.ToString();
                     
                    SongId = ProductData.RelatedSong?.RelatedSongClass?.Id.ToString();
                    TxtRelatedToSong.Text = Methods.FunString.DecodeString(ProductData.RelatedSong?.RelatedSongClass?.Title);

                    var category = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesId == ProductData.CatId.ToString());
                    if (category != null)
                    {
                        TxtCategory.Text = category.CategoriesName;
                        CategoryId = category.CategoriesId;
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