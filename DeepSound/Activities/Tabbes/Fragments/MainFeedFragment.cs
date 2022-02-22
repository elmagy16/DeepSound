using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.ViewPager.Widget;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Artists.Adapters;
using DeepSound.Activities.Default;
using DeepSound.Activities.Genres.Adapters;
using DeepSound.Activities.Notification;
using DeepSound.Activities.Product;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.Search;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Upgrade;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class MainFeedFragment : Fragment 
    {
        #region Variables Basic

        public ArtistsAdapter ArtistsAdapter;
        private GenresAdapter GenresAdapter;
        public HSoundAdapter NewReleasesSoundAdapter, RecentlyPlayedSoundAdapter, PopularSoundAdapter;
        public ProductAdapter ProductAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, BrowseViewStub, NewReleasesViewStub, RecentlyPlayedViewStub, PopularViewStub, ProductViewStub,  ArtistsViewStub;
        private View Inflated, BrowseInflated, NewReleasesInflated, RecentlyPlayedInflated, PopularInflated, ProductInflated, ArtistsInflated;       
      
        public TextView ProIcon, SearchIcon ,  NotificationIcon, AddIcon, CartIcon;
        private ViewPager ViewPagerView;
        private CircleIndicator ViewPagerCircleIndicator;
        private ObservableCollection<SoundDataObject> RecommendedList;
        private ProgressBar ProgressBar; 
        private RecyclerViewOnScrollListener ArtistsScrollEvent;
        public SongsByGenresFragment SongsByGenresFragment;
        public SongsByTypeFragment SongsByTypeFragment;
        private RelativeLayout MainAlert;
        public SearchFragment SearchFragment;
        public ProductFragment ProductFragment;
        private ProductProfileFragment ProductProfileFragment;

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
                View view = inflater.Inflate(Resource.Layout.TMainFeedLayout, container, false);

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

                InitComponent(view);
               // InitToolbar(view);
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(StartApiService);
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
         
        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress);
                ProgressBar.Visibility = ViewStates.Visible;
                 
                BrowseViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubBrowse);
                NewReleasesViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubNewReleases);
                RecentlyPlayedViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubRecentlyPlayed);
                PopularViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPopular);
                ProductViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubProduct);
                ArtistsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubArtists);

                ViewPagerView = view.FindViewById<ViewPager>(Resource.Id.viewpager2);
                ViewPagerCircleIndicator = (CircleIndicator)view.FindViewById(Resource.Id.indicator);
                ViewPagerView.PageMargin = 6;
                ViewPagerView.SetClipChildren(false);
                ViewPagerView.SetPageTransformer(true, new CarouselEffectTransformer2(Activity));

                NotificationIcon = (TextView)view.FindViewById(Resource.Id.notificationIcon);
                SearchIcon = (TextView)view.FindViewById(Resource.Id.searchIcon);
                CartIcon = (TextView)view.FindViewById(Resource.Id.cartIcon);
                AddIcon = (TextView)view.FindViewById(Resource.Id.addIcon);
                ProIcon = (TextView)view.FindViewById(Resource.Id.proIcon);
                ProIcon.Visibility = ViewStates.Gone;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, NotificationIcon, IonIconsFonts.Notifications);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, SearchIcon, IonIconsFonts.Search);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AddIcon, IonIconsFonts.AddCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, CartIcon, IonIconsFonts.Cart);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, ProIcon, FontAwesomeIcon.Rocket);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                NotificationIcon.Click += NotificationIconOnClick;
                SearchIcon.Click += SearchIconOnClick;
                AddIcon.Click += AddIconOnClick;
                ProIcon.Click += ProIconOnClick;
                CartIcon.Click += CartIconOnClick;

                if (!AppSettings.ShowGoPro || !UserDetails.IsLogin) 
                    ProIcon.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.EnableProduct || !UserDetails.IsLogin)
                    CartIcon.Visibility = ViewStates.Gone;
                else
                    CartIcon.Visibility = ViewStates.Visible;

                NotificationIcon.Visibility = UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                AddIcon.Visibility = UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                 
                MainAlert = (RelativeLayout)view.FindViewById(Resource.Id.mainAlert);
                MainAlert.Visibility = !UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                MainAlert.Click += MainAlertOnClick;
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
                RecommendedList = new ObservableCollection<SoundDataObject>(); 
                
                //Browse RecyclerView >> LinearLayoutManager.Horizontal
                GenresAdapter = new GenresAdapter(Activity) { GenresList = new ObservableCollection<GenresObject.DataGenres>() };
                GenresAdapter.GenresList = ListUtils.GenresList;
                GenresAdapter.OnItemClick += GenresAdapterOnOnItemClick;

                //New Releases RecyclerView >> LinearLayoutManager.Horizontal 
                NewReleasesSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() }; 
                NewReleasesSoundAdapter.OnItemClick += NewReleasesSoundAdapterOnOnItemClick;

                // Recently Played RecyclerView >> LinearLayoutManager.Horizontal
                RecentlyPlayedSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                RecentlyPlayedSoundAdapter.OnItemClick += RecentlyPlayedSoundAdapterOnOnItemClick;

                // Popular RecyclerView >> LinearLayoutManager.Horizontal
                PopularSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                PopularSoundAdapter.OnItemClick += PopularSoundAdapterOnOnItemClick;

                // Product RecyclerView >> LinearLayoutManager.Horizontal
                ProductAdapter = new ProductAdapter(Activity) { ProductsList = new ObservableCollection<ProductDataObject>() };
                ProductAdapter.OnItemClick += ProductAdapterOnOnItemClick;

                ArtistsAdapter = new ArtistsAdapter(Activity);
                ArtistsAdapter.OnItemClick += ArtistsAdapterOnOnItemClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void AddIconOnClick(object sender, EventArgs e)
        {
            try
            {
                AddBottomSheetFragment addBottomSheetFragment = new AddBottomSheetFragment();
                addBottomSheetFragment.Show(ChildFragmentManager, addBottomSheetFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MainAlertOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProductAdapterOnOnItemClick(object sender, ProductAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = ProductAdapter.GetItem(e.Position);
                    if (item == null)
                        return;

                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("ProductId", item.Id.ToString());

                    ProductProfileFragment = new ProductProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(ProductProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from NewRelease
        private void NewReleasesSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = NewReleasesSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, NewReleasesSoundAdapter.SoundsList);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from Popular
        private void PopularSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = PopularSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position; 
                    GlobalContext?.SoundController?.StartPlaySound(item, PopularSoundAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from RecentlyPlayed
        private void RecentlyPlayedSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = RecentlyPlayedSoundAdapter.GetItem(e.Position);
                    if (item != null)
                    { 
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, RecentlyPlayedSoundAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open Songs By Genres
        private void GenresAdapterOnOnItemClick(object sender, GenresAdapterClickEventArgs e)
        {
            try
            {
                var item = GenresAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("GenresId", item.Id.ToString());
                    bundle.PutString("GenresText", item.CateogryName);

                    SongsByGenresFragment = new SongsByGenresFragment
                    {
                        Arguments = bundle
                    };
                    GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByGenresFragment);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile  
        private void ArtistsAdapterOnOnItemClick(object sender, ArtistsAdapterClickEventArgs e)
        {
            try
            { 
                var item = ArtistsAdapter.GetItem(e.Position);
                if (item?.Id != null) GlobalContext.OpenProfile(item.Id, item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open go to pro Page  
        private void ProIconOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(GoProActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Cart List
        private void CartIconOnClick(object sender, EventArgs e)
        {
            try
            {
                CartFragment cartFragment = new CartFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(cartFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Open Notification Page
        private void NotificationIconOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.ShowOrHideBadgeViewIcon();
                 
                NotificationFragment notificationFragment = new NotificationFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(notificationFragment); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Search Page
        private void SearchIconOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchFragment = new SearchFragment();
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(SearchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BrowseMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show all Genres
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopularMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "Popular");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecentlyPlayedMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "RecentlyPlayed");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void NewReleasesMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "NewReleases");

                SongsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ArtistsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                ArtistsFragment fragment = new ArtistsFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(fragment); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Artists Scroll
        private void ArtistsScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                if (ArtistsScrollEvent.IsLoading == false)
                {
                    ArtistsScrollEvent.IsLoading = true;
                    var item = ArtistsAdapter.ArtistsList.LastOrDefault();
                    if (item != null)
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadArtists(item.Id.ToString()) });
                        ArtistsScrollEvent.IsLoading = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void ProductMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                ProductFragment = new ProductFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(ProductFragment); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Refresh

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                NewReleasesSoundAdapter.SoundsList.Clear();
                NewReleasesSoundAdapter.NotifyDataSetChanged();

                RecentlyPlayedSoundAdapter.SoundsList.Clear();
                RecentlyPlayedSoundAdapter.NotifyDataSetChanged();

                PopularSoundAdapter.SoundsList.Clear();
                PopularSoundAdapter.NotifyDataSetChanged();

                GenresAdapter.GenresList.Clear();
                GenresAdapter.NotifyDataSetChanged();

                ProductAdapter.ProductsList.Clear();
                ProductAdapter.NotifyDataSetChanged();

                ArtistsAdapter.ArtistsList.Clear();
                ArtistsAdapter.NotifyDataSetChanged();

                RecommendedList.Clear();
                ViewPagerView.Adapter = null!;

                if (ArtistsScrollEvent != null) ArtistsScrollEvent.IsLoading = false;

                EmptyStateLayout.Visibility = ViewStates.Gone;
                 
                StartApiService();                
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Load Discover Api

        private void StartApiService()
        { 
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {LoadDiscover, ApiRequest.GetGenres_Api, () => LoadArtists()});
            }
            else
            {
                Activity.RunOnUiThread(() =>
                {
                    try
                    {
                        SwipeRefreshLayout.Refreshing = false;

                        Inflated = EmptyStateLayout.Inflate();
                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                            x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        }

                        Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
        }

        private async Task LoadDiscover()
        {
            try
            {
                 var (apiStatus, respond) = await RequestsAsync.Common.GetDiscoverAsync();
                if (apiStatus == 200)
                {
                    if (respond is DiscoverObject result)
                    {
                        if (result.Randoms?.Recommended?.Count > 0)
                        {
                            result.Randoms.Recommended = DeepSoundTools.ListFilter(result.Randoms?.Recommended);
                            RecommendedList = new ObservableCollection<SoundDataObject>();
                            foreach (var item in result.Randoms?.Recommended)
                            { 
                                RecommendedList.Add(item);
                            }
                        }

                        if (result.NewReleases?.NewReleasesClass?.Data?.AnythingArray?.Count > 0)
                        {
                            var soundsList = new ObservableCollection<SoundDataObject>(result.NewReleases?.NewReleasesClass?.Data?.AnythingArray);
                            NewReleasesSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(soundsList.ToList()));
                        }
                        else if (result.NewReleases?.NewReleasesClass?.Data?.NewReleasesDataList?.Count > 0)
                        {
                            var soundsList = new ObservableCollection<SoundDataObject>(result.NewReleases?.NewReleasesClass?.Data?.NewReleasesDataList?.Values);
                            NewReleasesSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(soundsList.ToList()));
                        }

                        if (result.RecentlyPlayed?.RecentlyPlayedClass?.Data?.Count > 0)
                        { 
                            if (RecentlyPlayedSoundAdapter.SoundsList.Count > 0)
                            {
                                var newItemList = result.RecentlyPlayed?.RecentlyPlayedClass?.Data.Where(c => !RecentlyPlayedSoundAdapter.SoundsList.Select(fc => fc.Id).Contains(c.Id)).ToList();
                                if (newItemList.Count > 0)
                                {
                                    ListUtils.AddRange(RecentlyPlayedSoundAdapter.SoundsList, newItemList);
                                } 
                            }
                            else
                            {                            
                                RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.RecentlyPlayed?.RecentlyPlayedClass?.Data); 
                            }

                            var soundDataObjects = RecentlyPlayedSoundAdapter.SoundsList?.Reverse();
                            Console.WriteLine(soundDataObjects);

                            var list = RecentlyPlayedSoundAdapter.SoundsList.OrderBy(o => o.Views);
                            RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(list.ToList())); 
                        }

                        if (result.MostPopularWeek != null && result.MostPopularWeek?.MostPopularWeekClass?.Data?.Count > 0)
                        {
                            PopularSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(result.MostPopularWeek?.MostPopularWeekClass?.Data));
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity.RunOnUiThread(ShowEmptyPage);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {   () => LoadProduct() });  
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadArtists(string offsetArtists = "0")
        {
            if (ArtistsScrollEvent != null && ArtistsScrollEvent.IsLoading)
                return;

            if (ArtistsScrollEvent != null) ArtistsScrollEvent.IsLoading = true;
             
            int countList = ArtistsAdapter.ArtistsList.Count;
             var (apiStatus, respond) = await RequestsAsync.User.GetArtistsAsync("20", offsetArtists).ConfigureAwait(false);
            if (apiStatus == 200)
            {
                if (respond is GetUserObject result)
                {
                    var respondList = result.Data?.UserList.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data?.UserList let check = ArtistsAdapter.ArtistsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ArtistsAdapter.ArtistsList.Add(item);
                            }

                            Activity.RunOnUiThread(() =>
                            {
                                ArtistsAdapter.NotifyItemRangeInserted(countList, ArtistsAdapter.ArtistsList.Count - countList);
                            });
                        }
                        else
                        {
                            ArtistsAdapter.ArtistsList = new ObservableCollection<UserDataObject>(result.Data?.UserList);

                            Activity.RunOnUiThread(() =>
                            {
                                ArtistsInflated ??= ArtistsViewStub.Inflate();

                                TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                recyclerInflater.InflateLayout<UserDataObject>(Activity, ArtistsInflated, ArtistsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Artists));
                                if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                {
                                    recyclerInflater.MainLinear.Click += null!;
                                    recyclerInflater.MainLinear.Click += ArtistsMoreOnClick;
                                }

                                if (ArtistsScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(recyclerInflater.LayoutManager);
                                    ArtistsScrollEvent = playlistRecyclerViewOnScrollListener;
                                    ArtistsScrollEvent.LoadMoreEvent += ArtistsScrollEventOnLoadMoreEvent;
                                    recyclerInflater.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                    ArtistsScrollEvent.IsLoading = false;
                                }
                            }); 
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            if (ArtistsScrollEvent != null) ArtistsScrollEvent.IsLoading = false; 
            //Activity.RunOnUiThread(ShowEmptyPage);
        }

        private async Task LoadProduct(string offsetProduct = "0")
        {
            if (!AppSettings.EnableProduct)
                return;
             
            int countList = ProductAdapter.ProductsList.Count;
            var (apiStatus, respond) = await RequestsAsync.Product.GetProductsAsync("20", offsetProduct).ConfigureAwait(false);
            if (apiStatus == 200)
            {
                if (respond is GetProductObject result)
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = ProductAdapter.ProductsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ProductAdapter.ProductsList.Add(item);
                            }

                            Activity.RunOnUiThread(() =>
                            {
                                ProductAdapter.NotifyItemRangeInserted(countList, ProductAdapter.ProductsList.Count - countList);
                            });
                        }
                        else
                        {
                            ProductAdapter.ProductsList = new ObservableCollection<ProductDataObject>(result.Data);

                            Activity.RunOnUiThread(() =>
                            {
                                ProductInflated ??= ProductViewStub.Inflate();

                                TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                recyclerInflater.InflateLayout<ProductDataObject>(Activity, ProductInflated, ProductAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Products));
                                if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                {
                                    recyclerInflater.MainLinear.Click += null!;
                                    recyclerInflater.MainLinear.Click += ProductMoreOnClick;
                                } 
                            }); 
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            //Activity.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;

                if (RecommendedList?.Count > 0)
                {
                    if (ViewPagerView.Adapter == null)
                    {
                        ViewPagerView.Adapter = new ImageCoursalViewPager(Activity, RecommendedList);
                        ViewPagerView.CurrentItem = 1;
                        ViewPagerCircleIndicator.SetViewPager(ViewPagerView);
                    }
                    ViewPagerView.Adapter.NotifyDataSetChanged();
                }
                else
                {
                    ViewPagerView.Visibility = ViewStates.Gone;
                }

                if (NewReleasesSoundAdapter.SoundsList?.Count > 0)
                {
                    NewReleasesInflated ??= NewReleasesViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, NewReleasesInflated, NewReleasesSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_LatestSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += NewReleasesMoreOnClick;
                    }
                }

                if (RecentlyPlayedSoundAdapter.SoundsList?.Count > 0)
                {
                    RecentlyPlayedInflated ??= RecentlyPlayedViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, RecentlyPlayedInflated, RecentlyPlayedSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_RecentlyPlayed));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += RecentlyPlayedMoreOnClick;
                    }
                }

                if (PopularSoundAdapter.SoundsList?.Count > 0)
                {
                    PopularInflated ??= PopularViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, PopularInflated, PopularSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Popular_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += PopularMoreOnClick;
                    }
                }

                if (GenresAdapter.GenresList.Count == 0)
                    GenresAdapter.GenresList = new ObservableCollection<GenresObject.DataGenres>(ListUtils.GenresList);

                if (GenresAdapter.GenresList.Count > 0)
                {
                    BrowseInflated ??= BrowseViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<GenresObject.DataGenres>(Activity, BrowseInflated, GenresAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Genres), false);
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += BrowseMoreOnClick;
                    }
                }

                if (RecommendedList?.Count == 0 && NewReleasesSoundAdapter?.SoundsList?.Count == 0 && RecentlyPlayedSoundAdapter?.SoundsList?.Count == 0 &&
                    PopularSoundAdapter?.SoundsList?.Count == 0 && GenresAdapter?.GenresList?.Count == 0 && ArtistsAdapter.ArtistsList?.Count == 0)
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSound);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    StartApiService();
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