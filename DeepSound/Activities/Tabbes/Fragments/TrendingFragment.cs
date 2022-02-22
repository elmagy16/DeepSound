using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.SwipeRefreshLayout.Widget;
using DeepSound.Activities.Blog;
using DeepSound.Activities.Blog.Adapters;
using DeepSound.Activities.Event;
using DeepSound.Activities.Event.Adapters;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Blog;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class TrendingFragment : Fragment 
    {
        #region Variables Basic

        private HPlaylistAdapter PublicPlaylistAdapter;
        private EventAdapter EventAdapter;
        private BlogAdapter BlogAdapter;
        //private StoryAdapter StoryAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, EventViewStub, BlogViewStub, PublicPlaylistViewStub;
        private View Inflated, EventInflated, BlogInflated, PublicPlaylistInflated;
        private RecyclerViewOnScrollListener EventScrollEvent, PublicPlaylistScrollEvent;
        public PlaylistProfileFragment PlaylistProfileFragment;
        private ProgressBar ProgressBar;
        private FloatingActionButton BtnAdd;
        private TemplateRecyclerInflater RecyclerInflaterEvent, RecyclerInflaterBlog, RecyclerInflaterPublicPlaylist;
        private EventProfileFragment EventProfileFragment;
        public EventFragment EventFragment;

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
                View view = inflater.Inflate(Resource.Layout.TTrendingLayout, container, false);
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
                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress);
                ProgressBar.Visibility = ViewStates.Visible;

                EventViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubEvent);
                BlogViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubBlog);
                PublicPlaylistViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPublicePlaylist);
                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                BtnAdd = (FloatingActionButton)view.FindViewById(Resource.Id.floatingAdd);
                BtnAdd.Visibility = UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                BtnAdd.Click += BtnAddOnClick;

                var publisherAdView = view.FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(publisherAdView);
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
                //Public Playlist RecyclerView
                PublicPlaylistAdapter = new HPlaylistAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                PublicPlaylistAdapter.OnItemClick += PublicPlaylistAdapterOnOnItemClick;
                 
                //Event RecyclerView
                EventAdapter = new EventAdapter(Activity) { EventsList = new ObservableCollection<EventDataObject>() };
                EventAdapter.ItemClick += EventAdapterOnItemClick;

                //Blog RecyclerView
                BlogAdapter = new BlogAdapter(Activity) { BlogList = new ObservableCollection<ArticleDataObject>()};
                BlogAdapter.ItemClick += BlogAdapterOnOnItemClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Refresh

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                EventAdapter.EventsList.Clear();
                EventAdapter.NotifyDataSetChanged();

                BlogAdapter.BlogList.Clear();
                BlogAdapter.NotifyDataSetChanged();

                PublicPlaylistAdapter.PlaylistList.Clear();
                PublicPlaylistAdapter.NotifyDataSetChanged();

                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
                 
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Get Public Playlist , Event =>> Api 
         
        private void StartApiService()
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPublicPlaylist() , () => GetEvent(), GetBlog });
            }
            else
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
        }

        private async Task GetPublicPlaylist(string offset  = "0")
        {
            if (PublicPlaylistScrollEvent != null && PublicPlaylistScrollEvent.IsLoading)
            {
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                return;
            }

            if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = true;

            int countList = PublicPlaylistAdapter.PlaylistList.Count;
             var (apiStatus, respond) = await RequestsAsync.Playlist.GetPublicPlaylistAsync("15", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is PlaylistObject result)
                {
                    var respondList = result.Playlist.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Playlist let check = PublicPlaylistAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            PublicPlaylistAdapter.PlaylistList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity.RunOnUiThread(() => { PublicPlaylistAdapter.NotifyItemRangeInserted(countList - 1, PublicPlaylistAdapter.PlaylistList.Count - countList); });
                        }
                        else
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                PublicPlaylistInflated ??= PublicPlaylistViewStub.Inflate();

                                RecyclerInflaterPublicPlaylist = new TemplateRecyclerInflater();
                                RecyclerInflaterPublicPlaylist.InflateLayout<PlaylistDataObject>(Activity, PublicPlaylistInflated, PublicPlaylistAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Hot_Playlist) , false);

                                if (PublicPlaylistScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterPublicPlaylist.LayoutManager);
                                    PublicPlaylistScrollEvent = playlistRecyclerViewOnScrollListener;
                                    PublicPlaylistScrollEvent.LoadMoreEvent += PublicPlaylistScrollEventOnLoadMoreEvent;
                                    RecyclerInflaterPublicPlaylist.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                    PublicPlaylistScrollEvent.IsLoading = false;
                                }
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            if (PublicPlaylistAdapter.PlaylistList.Count > 10 && !RecyclerInflaterPublicPlaylist.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("PublicPlaylist"); });
        }
        
        private async Task GetEvent(string offset = "0")
        {
            if (!AppSettings.EnableEvent)
                return;

            if (EventScrollEvent != null && EventScrollEvent.IsLoading)
            {
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                return;
            }

            if (EventScrollEvent != null) EventScrollEvent.IsLoading = true;

            int countList = EventAdapter.EventsList.Count;
             var (apiStatus, respond) = await RequestsAsync.Event.GetEventsAsync("15", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is GetEventObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = EventAdapter.EventsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            EventAdapter.EventsList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity.RunOnUiThread(() => { EventAdapter.NotifyItemRangeInserted(countList - 1, EventAdapter.EventsList.Count - countList); });
                        }
                        else
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                EventInflated ??= EventViewStub.Inflate();

                                RecyclerInflaterEvent = new TemplateRecyclerInflater();
                                RecyclerInflaterEvent.InflateLayout<EventDataObject>(Activity, EventInflated, EventAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Event));
                                if (!RecyclerInflaterEvent.MainLinear.HasOnClickListeners)
                                {
                                    RecyclerInflaterEvent.MainLinear.Click += null!;
                                    RecyclerInflaterEvent.MainLinear.Click += EventMoreOnClick;
                                }

                                if (EventScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener eventRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterEvent.LayoutManager);
                                    EventScrollEvent = eventRecyclerViewOnScrollListener;
                                    EventScrollEvent.LoadMoreEvent += EventScrollEventOnLoadMoreEvent;
                                    RecyclerInflaterEvent.Recyler.AddOnScrollListener(eventRecyclerViewOnScrollListener);
                                    EventScrollEvent.IsLoading = false;
                                }
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterEvent.Recyler != null)
                            if (EventAdapter.EventsList.Count > 10 && !RecyclerInflaterEvent.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreEvents), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("Event"); });
        }
         
        private async Task GetBlog()
        {
            if (!AppSettings.ShowBlog)
                return;
              
            int countList = BlogAdapter.BlogList.Count;
             var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesAsync("15", "0");
            if (apiStatus.Equals(200))
            {
                if (respond is GetArticlesObject result)
                {
                    var respondList = result.ArticleList.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.ArticleList let check = BlogAdapter.BlogList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            BlogAdapter.BlogList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity.RunOnUiThread(() => { BlogAdapter.NotifyItemRangeInserted(countList - 1, BlogAdapter.BlogList.Count - countList); });
                        }
                        else
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                BlogInflated ??= BlogViewStub.Inflate();

                                RecyclerInflaterBlog = new TemplateRecyclerInflater();
                                RecyclerInflaterBlog.InflateLayout<EventDataObject>(Activity, BlogInflated, BlogAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Article));
                                if (!RecyclerInflaterBlog.MainLinear.HasOnClickListeners)
                                {
                                    RecyclerInflaterBlog.MainLinear.Click += null!;
                                    RecyclerInflaterBlog.MainLinear.Click += BlogMoreOnClick;
                                } 
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterBlog.Recyler != null)
                            if (BlogAdapter.BlogList.Count > 10 && !RecyclerInflaterBlog.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("Blog"); });
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;

                SwipeRefreshLayout.Refreshing = false;

                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                 
                if (type == "PublicPlaylist")
                {
                    if (PublicPlaylistAdapter?.PlaylistList?.Count > 0)
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            RecyclerInflaterPublicPlaylist.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    } 
                } 
                
                if (type == "Event")
                {
                    if (EventAdapter?.EventsList?.Count > 0)
                    {
                        if (RecyclerInflaterEvent.Recyler != null)
                            RecyclerInflaterEvent.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    } 
                }

                if (PublicPlaylistAdapter?.PlaylistList?.Count == 0 && EventAdapter?.EventsList?.Count == 0)
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
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;

                SwipeRefreshLayout.Refreshing = false;
                if (ProgressBar.Visibility == ViewStates.Visible)
                    ProgressBar.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
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

        #region Scroll

        private void EventScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = EventAdapter.EventsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !EventScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetEvent(item.Id.ToString()) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PublicPlaylistScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = PublicPlaylistAdapter.PlaylistList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !PublicPlaylistScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPublicPlaylist(item.Id.ToString()) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Event 
         
        private void BlogMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(BlogActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void EventMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                EventFragment = new EventFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(EventFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }
         
        private void BlogAdapterOnOnItemClick(object sender, BlogAdapterClickEventArgs e)
        {
            try
            {
                var item = BlogAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Intent intent = new Intent(Activity, typeof(ShowArticleActivity));
                    intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                    Activity.StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }


        private void EventAdapterOnItemClick(object sender, EventAdapterClickEventArgs e)
        {
            try
            {
                var item = EventAdapter.EventsList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("EventId", item.Id.ToString());

                    EventProfileFragment = new EventProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(EventProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PublicPlaylistAdapterOnOnItemClick(object sender, PlaylistAdapterClickEventArgs e)
        {
            try
            {
                var item = PublicPlaylistAdapter.PlaylistList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("PlaylistId", item.Id.ToString());

                    PlaylistProfileFragment = new PlaylistProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(PlaylistProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        // Create 
        private void BtnAddOnClick(object sender, EventArgs e)
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

        #endregion
    }
}