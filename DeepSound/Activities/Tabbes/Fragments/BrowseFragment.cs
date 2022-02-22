using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.Search;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Transitions;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class BrowseFragment : Fragment 
    {
        #region Variables Basic

        public HAlbumsAdapter AlbumsAdapter;
        public HSoundAdapter TopSongsSoundAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, TopSongsViewStub, TopAlbumsViewStub;
        private View Inflated, TopSongsInflated, TopAlbumsInflated;
        private AutoCompleteTextView SearchBox;
        public SearchFragment SearchFragment;
        private AlbumsFragment AlbumsFragment;

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
                View view = inflater.Inflate(Resource.Layout.TBrowseLayout, container, false);
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
                //InitToolbar(view);
                SetRecyclerViewAdapters();

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);
                TopSongsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubTopSongs);
                TopAlbumsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubTopAlbums);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                 
                SearchBox = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchViewBox);
                SearchBox.SetHintTextColor(AppSettings.SetTabDarkTheme ?  Color.White : Color.Gray);
                SearchBox.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                SearchBox.Click += SearchBoxOnClick;
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
                //Top Songs RecyclerView >> LinearLayoutManager.Horizontal 
                TopSongsSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                TopSongsSoundAdapter.OnItemClick += TopSongsSoundAdapterOnOnItemClick;

                // Top Albums RecyclerView >> LinearLayoutManager.Horizontal
                AlbumsAdapter =  new HAlbumsAdapter(Activity);
                AlbumsAdapter.ItemClick += AlbumsAdapterOnItemClick; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                TopSongsSoundAdapter.SoundsList.Clear();
                TopSongsSoundAdapter.NotifyDataSetChanged();

                AlbumsAdapter.AlbumsList.Clear();
                AlbumsAdapter.NotifyDataSetChanged();
                 
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile Albums
        private void AlbumsAdapterOnItemClick(object sender, HAlbumsAdapterClickEventArgs e)
        {
            try
            {
                var item = AlbumsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("AlbumsId", item.Id.ToString());
                    AlbumsFragment = new AlbumsFragment
                    {
                        Arguments = bundle
                    };

                    SharedElementReturnTransition = (TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform));
                    ExitTransition = (TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform));

                    AlbumsFragment.SharedElementEnterTransition = TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform);
                    AlbumsFragment.ExitTransition = TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform);

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(AlbumsFragment, e.Image);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopSongsSoundAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = TopSongsSoundAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, TopSongsSoundAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadBrowse });
        }

        private async Task LoadBrowse()
        {
            if (Methods.CheckConnectivity())
            {
                int countSongsList = TopSongsSoundAdapter.SoundsList.Count;
                int countAlbumsList = AlbumsAdapter.AlbumsList.Count;
                 var (apiStatus, respond) = await RequestsAsync.Common.GetTrendingAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetTrendingObject result)
                    {
                        var respondSongsList = result.TopSongs?.Count;
                        if (respondSongsList > 0)
                        {
                            result.TopSongs = DeepSoundTools.ListFilter(result.TopSongs);

                            if (countSongsList > 0)
                            {
                                foreach (var item in from item in result.TopSongs let check = TopSongsSoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    TopSongsSoundAdapter.SoundsList.Add(item);
                                }
                            }
                            else
                            {
                                TopSongsSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.TopSongs);
                            }
                        }
                        else
                        {
                            if (TopSongsSoundAdapter.SoundsList.Count > 10)
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                        }

                        var respondList = result.TopAlbums?.Count;
                        if (respondList > 0)
                        {
                            if (countAlbumsList > 0)
                            {
                                foreach (var item in from item in result.TopAlbums let check = AlbumsAdapter.AlbumsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    AlbumsAdapter.AlbumsList.Add(item);
                                }
                            }
                            else
                            {
                                AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.TopAlbums);
                            }
                        }
                        else
                        {
                            if (AlbumsAdapter.AlbumsList.Count > 10)
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

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

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (TopSongsSoundAdapter.SoundsList.Count > 0)
                {
                    TopSongsInflated ??= TopSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, TopSongsInflated, TopSongsSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_TopSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += TopSongsMoreOnClick;
                    }
                }

                if (AlbumsAdapter.AlbumsList?.Count > 0)
                {
                    if (TopAlbumsInflated == null)
                        TopAlbumsInflated = TopAlbumsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<DataAlbumsObject>(Activity, TopAlbumsInflated, AlbumsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, Context.GetText(Resource.String.Lbl_TopAlbums_Title),false);
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += TopAlbumsMoreOnClick;
                    }
                }

                if (TopSongsSoundAdapter.SoundsList?.Count == 0 && AlbumsAdapter.AlbumsList?.Count == 0)
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
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TopAlbumsMoreOnClick(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopSongsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "BrowseTopSongs");

                SongsByTypeFragment songsByTypeFragment = new SongsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(songsByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region SearchView

        private void SearchBoxOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Key", "");
                SearchFragment = new SearchFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SearchFragment);
                SearchBox.ClearFocus();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }
         
        
         
        #endregion 
    }
}