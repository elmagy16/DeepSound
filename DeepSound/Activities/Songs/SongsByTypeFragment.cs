using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Songs
{
    public class SongsByTypeFragment : Fragment
    {
        #region Variables Basic

        public RowSoundAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string SongsType, UserId;
        private AdView MAdView;
        private RecyclerViewOnScrollListener MainScrollEvent;

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
                View view = inflater.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false); 
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

                SongsType = Arguments.GetString("SongsType") ?? "";
                UserId = Arguments.GetString("UserId") ?? "";

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetSongsByType();

                AdsGoogle.Ad_Interstitial(Activity); 
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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
                MAdView?.Destroy();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
              
                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                 
                switch (SongsType)
                {
                    case "Popular":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Popular_Title));
                        break;
                    }
                    case "RecentlyPlayed":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_RecentlyPlayed));
                        break;
                    }
                    case "NewReleases":
                    case "UserProfileLatestSongs":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_LatestSongs_Title));
                        break;
                    }
                    case "UserProfileTopSongs":
                    case "BrowseTopSongs":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_TopSongs_Title));
                        break;
                    }
                    case "UserProfileStore":
                    {
                        GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Store_Title));
                        break;
                    }
                    default:
                        GlobalContext.SetToolBar(toolbar, SongsType);
                        break;
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
                MAdapter = new RowSoundAdapter(Activity, "SongsByTypeFragment") {SoundsList = new ObservableCollection<SoundDataObject>()};
                MAdapter.OnItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false; 
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
         
        //Start Play Sound 
        private void MAdapterOnItemClick(object sender, RowSoundAdapterClickEventArgs e)
        {
            try
            {
                var list = MAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                if (list.Count > 0)
                {
                    foreach (var all in list)
                    {
                        all.IsPlay = false;

                        var index = MAdapter.SoundsList.IndexOf(all);
                        MAdapter.NotifyItemChanged(index);
                    }
                }

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    item.IsPlay = true;
                    MAdapter.NotifyItemChanged(e.Position);

                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList, MAdapter);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            { 
                //Code get last id where LoadMore >>
                var item = MAdapter.SoundsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    else
                    {
                        switch (SongsType)
                        {
                            case "Popular":
                                {
                                    //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPopular(item.Id.ToString()) });
                                    break;
                                }
                            case "RecentlyPlayed":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadRecentlyPlayed(item.Id.ToString()) });
                                    break;
                                }
                            case "NewReleases":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadNewReleases(item.Id.ToString()) });
                                    break;
                                }
                            case "UserProfileLatestSongs":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadLatestSongs(item.Id.ToString()) });
                                    break;
                                }
                            case "BrowseTopSongs":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadTopSongs(item.Id.ToString()) });
                                    break;
                                }
                            case "UserProfileTopSongs":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadTopSongsUser(item.Id.ToString()) });
                                    break;
                                }
                            case "UserProfileStore":
                                {
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadStoreUser(item.Id.ToString()) });
                                    break;
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

        #region Load Data Soungs

        private void GetSongsByType()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    switch (SongsType)
                    {
                        case "Popular":
                        {
                            if (GlobalContext?.MainFragment?.PopularSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.PopularSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        }
                        case "RecentlyPlayed":
                        {
                            if (GlobalContext?.MainFragment?.RecentlyPlayedSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.RecentlyPlayedSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        }
                        case "NewReleases":
                        {
                            if (GlobalContext?.MainFragment?.NewReleasesSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.MainFragment?.NewReleasesSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        }
                        case "UserProfileLatestSongs":
                        {
                            if (ArtistsProfileFragment.GetInstance()?.LatestSongsAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = ArtistsProfileFragment.GetInstance()?.LatestSongsAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            } 
                            break;
                        }
                        case "BrowseTopSongs":
                        {
                            if (GlobalContext?.BrowseFragment?.TopSongsSoundAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = GlobalContext?.BrowseFragment?.TopSongsSoundAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        }
                        case "UserProfileTopSongs": 
                        {
                            if (ArtistsProfileFragment.GetInstance()?.TopSongsAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = ArtistsProfileFragment.GetInstance()?.TopSongsAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        }
                        case "UserProfileStore":
                        {
                            if (ArtistsProfileFragment.GetInstance()?.StoreAdapter?.SoundsList?.Count > 0)
                            {
                                MAdapter.SoundsList = ArtistsProfileFragment.GetInstance()?.StoreAdapter?.SoundsList;
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        } 
                    }

                    Activity.RunOnUiThread(ShowEmptyPage);
                }
                else
                {
                    Inflated = EmptyStateLayout.Inflate();
                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                    }

                    Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private async Task LoadStoreUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserStoreAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
         
        private async Task LoadTopSongsUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserTopSongAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundObject result)
                    {
                        var respondList = result.Data?.SoundList?.Count;
                        if (respondList > 0)
                        {
                            result.Data.SoundList = DeepSoundTools.ListFilter(result.Data.SoundList);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data.SoundList let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data.SoundList);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
        
        private async Task LoadTopSongs(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Common.TopSongsAsync("15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
        
        private async Task LoadLatestSongs(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserLatestSongAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
         
        private async Task LoadNewReleases(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Common.GetNewReleasesAsync("15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
         
        private async Task LoadRecentlyPlayed(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetRecentlyPlayedAsync(UserDetails.UserId.ToString(), "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundObject result)
                    {
                        var respondList = result.Data?.SoundList.Count;
                        if (respondList > 0)
                        {
                            result.Data.SoundList = DeepSoundTools.ListFilter(result.Data?.SoundList);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data?.SoundList let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.SoundList);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
         
        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.SoundsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone; 
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

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
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                GetSongsByType();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
}