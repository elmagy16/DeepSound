using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Search
{
    public class SearchFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        public SearchSongsFragment SongsTab;
        private SearchAlbumsFragment AlbumsTab;
        private SearchPlaylistFragment PlaylistTab;
        private SearchArtistsFragment ArtistsTab;
        private TabLayout TabLayout;
        private ViewPager2 ViewPager;
        private HomeActivity GlobalContext;
        private SearchView SearchBox;
        private TextView FilterButton;
        private string SearchText = "";
        public string OffsetSongs = "0", OffsetAlbums = "0", OffsetPlaylist = "0", OffsetArtists = "0";
        private MainTabAdapter Adapter;

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
                View view = inflater.Inflate(Resource.Layout.SearchLayout, container, false);
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

                SearchText = Arguments?.GetString("Key") ?? "";

                InitComponent(view);
                InitToolbar(view); 
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
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.Searchtabs);
                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.Searchviewpager);

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                //Set Tab
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                FilterButton = (TextView)view.FindViewById(Resource.Id.filter_icon);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
                FilterButton.Click += FilterButtonOnClick;

                GetAppData();

                AdsGoogle.Ad_Interstitial(Activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                GlobalContext.SetToolBar(toolbar, "");

                SearchBox = view.FindViewById<SearchView>(Resource.Id.searchBox);
                SearchBox.SetQuery("", false);
                SearchBox.SetIconifiedByDefault(false);
                SearchBox.OnActionViewExpanded();
                SearchBox.Iconified = false;
                SearchBox.QueryTextChange += SearchBoxOnQueryTextChange;
                SearchBox.QueryTextSubmit += SearchBoxOnQueryTextSubmit;
                SearchBox.ClearFocus();

                //Change text colors
                var editText = (EditText)SearchBox?.FindViewById(Resource.Id.search_src_text);
                editText?.SetHintTextColor(Color.Gray);
                editText?.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchBox?.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon?.Parent;
                linearLayoutSearchView?.RemoveView(searchViewIcon);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                SongsTab = new SearchSongsFragment();
                AlbumsTab = new SearchAlbumsFragment();
                PlaylistTab = new SearchPlaylistFragment();
                ArtistsTab = new SearchArtistsFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(SongsTab, GetText(Resource.String.Lbl_Songs));
                Adapter.AddFragment(AlbumsTab, GetText(Resource.String.Lbl_Albums));
                Adapter.AddFragment(PlaylistTab, GetText(Resource.String.Lbl_Playlist));
                Adapter.AddFragment(ArtistsTab, GetText(Resource.String.Lbl_Artists));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal; 
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();

                TabLayout.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
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

        //show Filter
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var searchFilter = new SearchFilterBottomDialogFragment();
                searchFilter.Show(ChildFragmentManager, "searchFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchBoxOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                SearchText = e.NewText;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchBoxOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                SearchBox.ClearFocus();
                SongsTab.MAdapter.SoundsList.Clear();
                SongsTab.MAdapter.NotifyDataSetChanged();

                AlbumsTab.MAdapter.AlbumsList.Clear();
                AlbumsTab.MAdapter.NotifyDataSetChanged();

                PlaylistTab.MAdapter.PlaylistList.Clear();
                PlaylistTab.MAdapter.NotifyDataSetChanged();

                ArtistsTab.MAdapter.UsersList.Clear();
                ArtistsTab.MAdapter.NotifyDataSetChanged();

                OffsetSongs = "0";
                OffsetAlbums = "0";
                OffsetPlaylist = "0";
                OffsetArtists = "0";

                if (Methods.CheckConnectivity())
                {
                    if (SongsTab.MAdapter.SoundsList.Count > 0)
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (AlbumsTab.MAdapter.AlbumsList.Count > 0)
                    {
                        AlbumsTab.MAdapter.AlbumsList.Clear();
                        AlbumsTab.MAdapter.NotifyDataSetChanged();
                    }
                    ToggleProgressLoaderVisibility(true);
                    ToggleEmptyStateVisibility(false);

                    StartApiService();
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoConnection);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ToggleProgressLoaderVisibility(false);
                    ToggleEmptyStateVisibility(true);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ToggleProgressLoaderVisibility(bool isVisible)
        {
            SongsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            ArtistsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            PlaylistTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            AlbumsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
        }

        private void ToggleEmptyStateVisibility(bool isVisible)
        {
            SongsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            ArtistsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            PlaylistTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            AlbumsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchBox.ClearFocus();
                SongsTab.MAdapter.SoundsList.Clear();
                SongsTab.MAdapter.NotifyDataSetChanged();

                AlbumsTab.MAdapter.AlbumsList.Clear();
                AlbumsTab.MAdapter.NotifyDataSetChanged();

                PlaylistTab.MAdapter.PlaylistList.Clear();
                PlaylistTab.MAdapter.NotifyDataSetChanged();

                ArtistsTab.MAdapter.UsersList.Clear();
                ArtistsTab.MAdapter.NotifyDataSetChanged();


                OffsetSongs = "0";
                OffsetAlbums = "0";
                OffsetPlaylist = "0";
                OffsetArtists = "0";

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                //ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    if (SongsTab.MAdapter.SoundsList.Count > 0)
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();
                    }

                    ToggleProgressLoaderVisibility(true);
                    ToggleEmptyStateVisibility(false);
                    StartApiService();
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ToggleProgressLoaderVisibility(false);
                    ToggleEmptyStateVisibility(true);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        private void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();

                        AlbumsTab.MAdapter.AlbumsList.Clear();
                        AlbumsTab.MAdapter.NotifyDataSetChanged();

                        PlaylistTab.MAdapter.PlaylistList.Clear();
                        PlaylistTab.MAdapter.NotifyDataSetChanged();

                        ArtistsTab.MAdapter.UsersList.Clear();
                        ArtistsTab.MAdapter.NotifyDataSetChanged();

                        ToggleProgressLoaderVisibility(true);
                        ToggleEmptyStateVisibility(false);

                        StartApiService();
                    }
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ToggleProgressLoaderVisibility(false);
                    ToggleEmptyStateVisibility(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
        }

        private async Task StartSearchRequest()
        {
            if (SongsTab.MainScrollEvent.IsLoading)
                return;

            SongsTab.MainScrollEvent.IsLoading = true;
            AlbumsTab.MainScrollEvent.IsLoading = true;
            PlaylistTab.MainScrollEvent.IsLoading = true;
            ArtistsTab.MainScrollEvent.IsLoading = true;

            int countSongsList = SongsTab.MAdapter.SoundsList.Count;
            int countAlbumsList = AlbumsTab.MAdapter.AlbumsList.Count;
            int countPlaylistList = PlaylistTab.MAdapter.PlaylistList.Count;
            int countBlogList = ArtistsTab.MAdapter.UsersList.Count;

             var (apiStatus, respond) = await RequestsAsync.Common.SearchAsync(SearchText, UserDetails.FilterGenres, UserDetails.FilterPrice, "10", OffsetSongs, "10", OffsetAlbums, "10", OffsetArtists, "10", OffsetPlaylist);
            if (apiStatus == 200)
            {
                if (respond is SearchObject result)
                {
                    var respondSongsList = result.Data?.Songs?.Count;
                    if (respondSongsList > 0)
                    {
                        result.Data.Songs = DeepSoundTools.ListFilter(result.Data?.Songs);

                        if (countSongsList > 0)
                        {
                            foreach (var item in from item in result.Data?.Songs let check = SongsTab.MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                SongsTab.MAdapter.SoundsList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { SongsTab.MAdapter.NotifyItemRangeInserted(countSongsList - 1, SongsTab.MAdapter.SoundsList.Count - countSongsList); });
                        }
                        else
                        {
                            SongsTab.MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.Songs);
                            Activity.RunOnUiThread(() => { SongsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (SongsTab.MAdapter.SoundsList.Count > 10 && !SongsTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                    }

                    var respondAlbumsList = result.Data?.Albums?.Count;
                    if (respondAlbumsList > 0)
                    {
                        if (countAlbumsList > 0)
                        {
                            foreach (var item in from item in result.Data?.Albums let check = AlbumsTab.MAdapter.AlbumsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                AlbumsTab.MAdapter.AlbumsList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { AlbumsTab.MAdapter.NotifyItemRangeInserted(countAlbumsList - 1, AlbumsTab.MAdapter.AlbumsList.Count - countAlbumsList); });
                        }
                        else
                        {
                            AlbumsTab.MAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.Data?.Albums);
                            Activity.RunOnUiThread(() => { AlbumsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (AlbumsTab.MAdapter.AlbumsList.Count > 10 && !AlbumsTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short)?.Show();
                    }

                    var respondPlaylistList = result.Data?.Playlist?.Count;
                    if (respondPlaylistList > 0)
                    {
                        if (countPlaylistList > 0)
                        {
                            foreach (var item in from item in result.Data?.Playlist let check = PlaylistTab.MAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                PlaylistTab.MAdapter.PlaylistList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { PlaylistTab.MAdapter.NotifyItemRangeInserted(countPlaylistList - 1, PlaylistTab.MAdapter.PlaylistList.Count - countPlaylistList); });
                        }
                        else
                        {
                            PlaylistTab.MAdapter.PlaylistList = new ObservableCollection<PlaylistDataObject>(result.Data?.Playlist);
                            Activity.RunOnUiThread(() => { PlaylistTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (PlaylistTab.MAdapter.PlaylistList.Count > 10 && !PlaylistTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
                    }

                    var respondBlogList = result.Data?.Artist?.Count;
                    if (respondBlogList > 0)
                    {
                        //result.Data?.Artist.RemoveAll(a => a.Artist == 0);

                        if (countBlogList > 0)
                        {
                            foreach (var item in from item in result.Data?.Artist let check = ArtistsTab.MAdapter.UsersList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ArtistsTab.MAdapter.UsersList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { ArtistsTab.MAdapter.NotifyItemRangeInserted(countBlogList - 1, ArtistsTab.MAdapter.UsersList.Count - countBlogList); });
                        }
                        else
                        {
                            ArtistsTab.MAdapter.UsersList = new ObservableCollection<UserDataObject>(result.Data?.Artist);
                            Activity.RunOnUiThread(() => { ArtistsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (ArtistsTab.MAdapter.UsersList.Count > 10 && !ArtistsTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            SongsTab.MainScrollEvent.IsLoading = false;
            AlbumsTab.MainScrollEvent.IsLoading = false;
            PlaylistTab.MainScrollEvent.IsLoading = false;
            ArtistsTab.MainScrollEvent.IsLoading = false;

            Activity.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                ToggleProgressLoaderVisibility(false);

                if (SongsTab.MAdapter.SoundsList.Count > 0)
                {
                    SongsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    SongsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }


                if (AlbumsTab.MAdapter.AlbumsList.Count > 0)
                {
                    AlbumsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (AlbumsTab.Inflated == null)
                        AlbumsTab.Inflated = AlbumsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(AlbumsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    AlbumsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (PlaylistTab.MAdapter.PlaylistList.Count > 0)
                {
                    PlaylistTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (PlaylistTab.Inflated == null)
                        PlaylistTab.Inflated = PlaylistTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(PlaylistTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    PlaylistTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (ArtistsTab.MAdapter.UsersList.Count > 0)
                {
                    ArtistsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (ArtistsTab.Inflated == null)
                        ArtistsTab.Inflated = ArtistsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(ArtistsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ArtistsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

            }
            catch (Exception e)
            {
                //SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        public void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";

                //ViewPager.SetCurrentItem(0, true);

                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                ListUtils.GenresList = sqlEntity.Get_GenresList();
                if (ListUtils.GenresList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {ApiRequest.GetGenres_Api});

                if (AppSettings.ShowPrice)
                {
                    ListUtils.PriceList = sqlEntity.Get_PriceList();
                    if (ListUtils.PriceList?.Count == 0)
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {ApiRequest.GetPrices_Api});
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}