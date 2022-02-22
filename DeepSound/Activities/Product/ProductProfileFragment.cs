using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using MaterialDialogsCore;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace DeepSound.Activities.Product
{
    public class ProductProfileFragment : Fragment, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private HomeActivity GlobalContext;

        private CoordinatorLayout ParentView;
        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private TextView IconBack, IconMore;
        private ViewPager ImagePager;
        private TextView TxtTitle, TxtCategory, TxtCountReviews, TxtPrice;
        private RatingBar RatingBar;
         
        private LinearLayout ButtonLayout;
        private EditText TxtCount;
        private AppCompatButton BtnAddToCart;
         
        private LinearLayout RelatedToSongLayout;
        private TextView TxtNameSong;
         
        private LinearLayout TopLayoutDescription;
        private TextView IconDescription, BtToggleDescription;
        private LinearLayout ExpandDescriptionLayout;
        private SuperTextView TxtDescription;

        private LinearLayout TopLayoutReviews;
        private TextView IconReviews, BtToggleReviews, TxtEmptyReviews;
        private LinearLayout ExpandReviewsLayout;
        private RecyclerView MRecycler;
        private ProductReviewsAdapter MAdapter;
        private LinearLayoutManager LayoutManager;

        private LinearLayout TopLayoutTags;
        private TextView IconTags, BtToggleTags;
        private LinearLayout ExpandTagsLayout;
        private SuperTextView TxtTags;

        private LinearLayout TopLayoutProfile;
        private RelativeLayout ProfileLayout;
        private TextView IconProfile, BtToggleProfile;
        private LinearLayout ExpandProfileLayout;
        private ImageView ImageProfile;
        private TextView NameProfile, AboutProfile;
        private AppCompatButton ButtonFollow;

        private string ProductId;
        private ProductDataObject ProductObject;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.ProductProfileLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                ProductId = Arguments.GetString("ProductId") ?? "";
                ProductObject = JsonConvert.DeserializeObject<ProductDataObject>(Arguments.GetString("ItemData") ?? "");

                InitComponent(view); 
                SetRecyclerViewAdapters();
                 
                AdsGoogle.Ad_Interstitial(Activity);
                StartApiService(); 
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

        public override void OnResume()
        {
            try
            {
                base.OnResume(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            { 
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ParentView = view.FindViewById<CoordinatorLayout>(Resource.Id.parent_view);
                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.app_bar_layout);
                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);

                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);
                ImagePager = view.FindViewById<ViewPager>(Resource.Id.viewPager);
                IconMore = view.FindViewById<TextView>(Resource.Id.IconMore);

                ButtonLayout = view.FindViewById<LinearLayout>(Resource.Id.ButtonLayout);
                TxtCount = view.FindViewById<EditText>(Resource.Id.countEditText);
                BtnAddToCart = view.FindViewById<AppCompatButton>(Resource.Id.ButtonAddToCart);

                TxtTitle = view.FindViewById<TextView>(Resource.Id.title);
                TxtCategory = view.FindViewById<TextView>(Resource.Id.category);
                RatingBar = view.FindViewById<RatingBar>(Resource.Id.ratingBar);
                TxtCountReviews = view.FindViewById<TextView>(Resource.Id.count_reviews);
                TxtPrice = view.FindViewById<TextView>(Resource.Id.price);

                RelatedToSongLayout = view.FindViewById<LinearLayout>(Resource.Id.RelatedToSongLayout);
                TxtNameSong = view.FindViewById<TextView>(Resource.Id.nameSong);

                TopLayoutDescription = view.FindViewById<LinearLayout>(Resource.Id.top_layout_description);
                IconDescription = view.FindViewById<TextView>(Resource.Id.icon_description);
                BtToggleDescription = view.FindViewById<TextView>(Resource.Id.bt_toggle_description);
                ExpandDescriptionLayout = view.FindViewById<LinearLayout>(Resource.Id.lyt_expand_description);
                TxtDescription = view.FindViewById<SuperTextView>(Resource.Id.descriptionText);

                TopLayoutReviews = view.FindViewById<LinearLayout>(Resource.Id.top_layout_reviews);
                IconReviews = view.FindViewById<TextView>(Resource.Id.icon_reviews);
                BtToggleReviews = view.FindViewById<TextView>(Resource.Id.bt_toggle_reviews);
                ExpandReviewsLayout = view.FindViewById<LinearLayout>(Resource.Id.lyt_expand_reviews);
                TxtEmptyReviews = view.FindViewById<TextView>(Resource.Id.emptyReviews_view);
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.reviewsRecycle);

                TopLayoutTags = view.FindViewById<LinearLayout>(Resource.Id.top_layout_tags);
                IconTags = view.FindViewById<TextView>(Resource.Id.icon_tags);
                BtToggleTags = view.FindViewById<TextView>(Resource.Id.bt_toggle_tags);
                ExpandTagsLayout = view.FindViewById<LinearLayout>(Resource.Id.lyt_expand_tags);
                TxtTags = view.FindViewById<SuperTextView>(Resource.Id.tagsText);


                TopLayoutProfile = view.FindViewById<LinearLayout>(Resource.Id.top_layout_profile);
                ProfileLayout = view.FindViewById<RelativeLayout>(Resource.Id.card_pro_layout);
                IconProfile = view.FindViewById<TextView>(Resource.Id.icon_profile);
                BtToggleProfile = view.FindViewById<TextView>(Resource.Id.bt_toggle_profile);
                ExpandProfileLayout = view.FindViewById<LinearLayout>(Resource.Id.lyt_expand_profile);
                ImageProfile = view.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                NameProfile = view.FindViewById<TextView>(Resource.Id.card_name);
                AboutProfile = view.FindViewById<TextView>(Resource.Id.card_dist);
                ButtonFollow = view.FindViewById<AppCompatButton>(Resource.Id.cont);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ArrowForward : IonIconsFonts.ArrowBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMore, IonIconsFonts.More);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , BtToggleDescription , IonIconsFonts.ArrowDropdown);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , BtToggleReviews, IonIconsFonts.ArrowDropdown);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , BtToggleTags, IonIconsFonts.ArrowDropdown);  
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , BtToggleProfile, IonIconsFonts.ArrowDropdown); //IonIconsFonts.ArrowDropup

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , IconDescription, IonIconsFonts.InformationCircleOutline); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons , IconReviews, IonIconsFonts.Quote);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTags, FontAwesomeIcon.Tags);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconProfile, IonIconsFonts.Person);
                
                ExpandDescriptionLayout.Visibility = ViewStates.Gone;
                ExpandReviewsLayout.Visibility = ViewStates.Gone;
                ExpandProfileLayout.Visibility = ViewStates.Gone;

                IconBack.Click += BackIconOnClick;
                IconMore.Click += IconMoreOnClick;

                TxtCount.TextChanged += TxtCountOnTextChanged;

                RelatedToSongLayout.Click += RelatedToSongLayoutOnClick;
                BtnAddToCart.Click += BtnAddToCartOnClick;

                TopLayoutDescription.Click += BtToggleDescriptionOnClick;
                TopLayoutReviews.Click += BtToggleReviewsOnClick;
                TopLayoutTags.Click += BtToggleTagsOnClick;
                TopLayoutProfile.Click += BtToggleProfileOnClick;
                 
                ProfileLayout.Click += ProfileLayoutOnClick;
                ButtonFollow.Click += ButtonFollowOnClick; 
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
                MAdapter = new ProductReviewsAdapter(Activity) { ReviewsList = new ObservableCollection<ProductReviewsDataObject>()};
                MAdapter.OnItemClick += MAdapterOnOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ProductReviewsDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
            try
            {
                switch (item.ItemId)
                {
                    case Android.Resource.Id.Home:
                        GlobalContext.FragmentNavigatorBack();
                        return true;
                }
                return base.OnOptionsItemSelected(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        #endregion

        #region Event
         
        private void TxtCountOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TxtCount.Text))
                {
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.ChangeQtyProductAsync(ProductObject.Id?.ToString(), TxtCount.Text) });
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnAddToCartOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
               
                switch (BtnAddToCart?.Tag?.ToString())
                {
                    case "false":
                        BtnAddToCart.Text = GetText(Resource.String.Lbl_RemoveFromCart);
                        BtnAddToCart.Tag = "true";
                        ProductObject.AddedToCart = 1;
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(ProductObject.Id?.ToString() , "Add") });
                        break;
                    default:
                        BtnAddToCart.Text = GetText(Resource.String.Lbl_AddToCart);
                        BtnAddToCart.Tag = "false";
                        ProductObject.AddedToCart = 0;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(ProductObject.Id?.ToString() , "Remove") });
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ProductObject.UserId == UserDetails.UserId && UserDetails.IsLogin)
                {
                    //arrayAdapter.Add(GetText(Resource.String.Lbl_DeleteProduct)); wael Next Update
                    arrayAdapter.Add(GetText(Resource.String.Lbl_EditProduct));
                }

                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                dialogList.Title(GetText(Resource.String.Lbl_Products));
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

        //Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RelatedToSongLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = ProductObject.RelatedSong;
                if (item?.RelatedSongClass == null)
                    return;

                Constant.PlayPos = 0;
                GlobalContext?.SoundController?.StartPlaySound(item?.RelatedSongClass, new ObservableCollection<SoundDataObject>(){ item?.RelatedSongClass });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile 
        private void MAdapterOnOnItemClick(object sender, ProductReviewsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item?.UserData?.Id != null) 
                        GlobalContext.OpenProfile(item.UserData.Id, item.UserData);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile 
        private void ProfileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ProductObject?.UserData != null)
                    GlobalContext.OpenProfile(ProductObject.UserData.Id, ProductObject.UserData);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Follow User
        private void ButtonFollowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    if (ProductObject.UserData != null)
                    {
                        if (ButtonFollow.Tag?.ToString() == "true")
                        {
                            ProductObject.UserData.IsFollowing = false;
                            ButtonFollow.Tag = "false";
                            ButtonFollow.Text = GetText(Resource.String.Lbl_Follow);
                            ButtonFollow.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            ButtonFollow.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(ProductObject.UserData.Id.ToString(), false) });
                        }
                        else
                        {
                            ProductObject.UserData.IsFollowing = true;
                            ButtonFollow.Tag = "true";
                            ButtonFollow.Text = GetText(Resource.String.Lbl_Following);
                            ButtonFollow.SetBackgroundResource(Resource.Xml.background_signup2);
                            ButtonFollow.SetTextColor(Color.White);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(ProductObject.UserData.Id.ToString(), true) });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtToggleProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                ToggleSection(BtToggleProfile, ExpandProfileLayout);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtToggleTagsOnClick(object sender, EventArgs e)
        {
            try
            {
                ToggleSection(BtToggleTags, ExpandTagsLayout);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtToggleReviewsOnClick(object sender, EventArgs e)
        {
            try
            {
                ToggleSection(BtToggleReviews, ExpandReviewsLayout);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtToggleDescriptionOnClick(object sender, EventArgs e)
        {
            try
            {
                ToggleSection(BtToggleDescription, ExpandDescriptionLayout);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Animate

        private void ToggleSection(View bt, View lyt)
        {
            try
            {
                bool show = ToggleArrow(bt);
                if (show)
                {
                    lyt.Measure(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    int targtetHeight = lyt.MeasuredHeight;

                    lyt.LayoutParameters.Height = 0;
                    lyt.Visibility = ViewStates.Visible;

                    ActionAnimation animation = new ActionAnimation(lyt, targtetHeight , "Expand");
                    lyt.StartAnimation(animation);
                }
                else
                {
                    int targtetHeight = lyt.MeasuredHeight;
                    ActionAnimation animation = new ActionAnimation(lyt, targtetHeight, "Collapse");
                    animation.Duration = ((int)(targtetHeight / lyt.Context.Resources?.DisplayMetrics.Density));
                    lyt.StartAnimation(animation);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool ToggleArrow(View view)
        {
            try
            {
                if (view.Rotation == 0)
                {
                    view?.Animate()?.SetDuration(200)?.Rotation(180);
                    return true;
                }
                else
                {
                    view?.Animate()?.SetDuration(200)?.Rotation(0);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
         
        private class ActionAnimation : Animation
        {
            private readonly View View;
            private readonly int TargtetHeight;
            private readonly string Type;
            public ActionAnimation(View v, int targtetHeight , string type)
            {
                View = v;
                TargtetHeight = targtetHeight;
                Type = type;
            }

            protected override void ApplyTransformation(float interpolatedTime, Transformation t)
            {
                try
                {
                    if (Type == "Expand")
                    {
                        View.LayoutParameters.Height = interpolatedTime == 1 ? ViewGroup.LayoutParams.WrapContent : (int)(TargtetHeight * interpolatedTime);
                        View.RequestLayout();
                    }
                    else
                    {
                        if (interpolatedTime == 1)
                        {
                            View.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            View.LayoutParameters.Height = TargtetHeight - (int)(TargtetHeight * interpolatedTime);
                            View.RequestLayout();
                        }
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    base.ApplyTransformation(interpolatedTime, t); 
                }
            }

            public override bool WillChangeBounds()
            {
                return true;
            }
        }
         
        #endregion

        #region Load Data Product 

        private void StartApiService()
        {
            SetDataProduct();

            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProductById , ProductReviewsById });
        }

        private async Task LoadProductById()
        {
            if (Methods.CheckConnectivity())
            { 
                var (apiStatus, respond) = await RequestsAsync.Product.GetProductByIdAsync(ProductId);
                if (apiStatus == 200)
                {
                    if (respond is GetProductDataObject result)
                    {
                        ProductObject = result.Data;
                        Activity.RunOnUiThread(SetDataProduct);  
                    }
                }
                else Methods.DisplayReportResult(Activity, respond); 
            }
            else
            {
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
          
        private async Task ProductReviewsById()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.ReviewsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Product.GetProductReviewsAsync(ProductId);
                if (apiStatus == 200)
                {
                    if (respond is GetProductReviewsObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.ReviewsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.ReviewsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ReviewsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.ReviewsList = new ObservableCollection<ProductReviewsDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.ReviewsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreNotificationsFound), ToastLength.Short)?.Show(); 
                        }
                    }

                    Activity.RunOnUiThread(() =>
                    {
                        try
                        {
                            if (MAdapter.ReviewsList.Count > 0)
                            {
                                MRecycler.Visibility = ViewStates.Visible;
                                TxtEmptyReviews.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                MRecycler.Visibility = ViewStates.Gone;
                                TxtEmptyReviews.Visibility = ViewStates.Visible;
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }); 
                }
                else Methods.DisplayReportResult(Activity, respond); 
            }
            else
            {
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
          
        private void SetDataProduct()
        {
            try
            {
                if (ProductObject != null)
                {
                    List<string> listImageUser = new List<string>();
                    switch (ProductObject.Images?.Count)
                    {
                        case > 0:
                            listImageUser.AddRange(ProductObject.Images.Select(t => t.Image));
                            break;
                        default:
                            listImageUser.Add(ProductObject.Images?[0]?.Image);
                            break;
                    }

                    switch (ImagePager.Adapter)
                    {
                        case null:
                            ImagePager.Adapter = new MultiImagePagerAdapter(this, listImageUser);
                            ImagePager.CurrentItem = 0;

                            break;
                    }
                    ImagePager.Adapter.NotifyDataSetChanged();

                    TxtTitle.Text = Methods.FunString.DecodeString(ProductObject.Title);

                    TxtCategory.Text = CategoriesController.Get_Translate_Categories_Communities(ProductObject.CatId.ToString(), "", "Products");

                    var currencyIcon = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";
                    TxtPrice.Text = currencyIcon + " " + ProductObject.Price;

                    RatingBar.Rating = float.Parse(ProductObject.Rating);

                    TxtCountReviews.Text = Methods.FunString.FormatPriceValue(ProductObject.ReviewsCount.Value);

                    if (ProductObject.UserId == UserDetails.UserId)
                    {
                        ButtonLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        if (ProductObject.AddedToCart == 1)
                        {
                            BtnAddToCart.Text = Activity.GetText(Resource.String.Lbl_RemoveFromCart);
                            BtnAddToCart.Tag = "true";
                        }
                        else
                        {
                            BtnAddToCart.Text = Activity.GetText(Resource.String.Lbl_AddToCart);
                            BtnAddToCart.Tag = "false";
                        }

                        TxtCount.Text = "1";
                    }
                    var descriptionAutoLink = new TextSanitizer(TxtDescription, Activity);
                    descriptionAutoLink.Load(Methods.FunString.DecodeString(ProductObject.Desc));

                    var tagsAutoLink = new TextSanitizer(TxtTags, Activity);
                    tagsAutoLink.Load(Methods.FunString.DecodeString(ProductObject.Tags).Replace(",", " #"));
                     
                    if (ProductObject.RelatedSong?.RelatedSongClass != null)
                    {
                        TxtNameSong.Text = Methods.FunString.DecodeString(ProductObject.RelatedSong?.RelatedSongClass.Title);
                    }

                    if (ProductObject.UserData != null)
                    {
                        GlideImageLoader.LoadImage(Activity, ProductObject.UserData.Avatar, ImageProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        string name = Methods.FunString.DecodeString(DeepSoundTools.GetNameFinal(ProductObject.UserData));
                        NameProfile.Text = Methods.FunString.SubStringCutOf(name, 25);

                        if (ProductObject.UserData.Time != null)
                            AboutProfile.Text = Methods.Time.TimeAgo(ProductObject.UserData.Time.Value, false);
                        else
                            AboutProfile.Text = Methods.Time.TimeAgo(Convert.ToInt32(ProductObject.UserData.LastActive), false);

                        if (ProductObject.UserData.Id == UserDetails.UserId)
                        {
                            ButtonFollow.Visibility = ViewStates.Invisible;
                        }
                        else
                        {
                            if (ProductObject.UserData.IsFollowing != null && ProductObject.UserData.IsFollowing.Value) // My Friend
                            {
                                ButtonFollow.SetBackgroundResource(Resource.Xml.background_signup2);
                                ButtonFollow.SetTextColor(Color.ParseColor("#ffffff"));
                                ButtonFollow.Text = GetText(Resource.String.Lbl_Following);
                                ButtonFollow.Tag = "true";
                            }
                            else //Not Friend
                            {
                                ButtonFollow.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                ButtonFollow.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                ButtonFollow.Text = GetText(Resource.String.Lbl_Follow);
                                ButtonFollow.Tag = "false";
                            }
                        }
                    }
                }
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
                if (text == GetText(Resource.String.Lbl_DeleteProduct))
                {
                    if (!UserDetails.IsLogin)
                    {
                        PopupDialogController dialogController = new PopupDialogController(Activity, null, "Login");
                        dialogController.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                        return;
                    }

                    if (Methods.CheckConnectivity())
                    { 
                        var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialogBuilder.Title(GetText(Resource.String.Lbl_DeleteProduct));
                        dialogBuilder.Content(GetText(Resource.String.Lbl_AreYouSureDeleteProduct));
                        dialogBuilder.PositiveText(GetText(Resource.String.Lbl_YesButKeepSongs)).OnPositive((materialDialog, action) =>
                        { 
                            try
                            {
                                var dataProductFragment = GlobalContext?.MainFragment.ProductFragment;
                                var list2 = dataProductFragment?.MAdapter?.ProductsList;
                                var dataMyProduct = list2?.FirstOrDefault(a => a.Id == ProductObject?.Id);
                                if (dataMyProduct != null)
                                {
                                    int index = list2.IndexOf(dataMyProduct);
                                    if (index >= 0)
                                    {
                                        list2?.Remove(dataMyProduct);
                                        dataProductFragment?.MAdapter?.NotifyItemRemoved(index);
                                    }
                                }

                                Toast.MakeText(Activity, GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Product.DeleteProductAsync(ProductObject?.Id.ToString()) });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialogBuilder.NegativeText(GetText(Resource.String.Lbl_YesDeleteEverything)).OnNegative(new MyMaterialDialog());
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                        dialogBuilder.Build().Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == GetText(Resource.String.Lbl_EditProduct))
                {
                    if (!UserDetails.IsLogin)
                    {
                        PopupDialogController dialogController = new PopupDialogController(Activity, null, "Login");
                        dialogController.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                        return;
                    }

                    var intent = new Intent(Activity, typeof(EditProductActivity));
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(ProductObject));
                    Activity.StartActivity(intent);
                } 
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as Song
                    if (!CrossShare.IsSupported)
                    {
                        return;
                    }

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = ProductObject?.Title,
                        Text = "",
                        Url = ProductObject?.Url
                    });
                }  
                else if (text == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(Activity, ProductObject?.Url);
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