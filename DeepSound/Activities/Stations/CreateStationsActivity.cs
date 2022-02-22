using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Base;
using DeepSound.Activities.Stations.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller; 
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Stations;
using DeepSoundClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace DeepSound.Activities.Stations
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateStationsActivity : BaseActivity 
    {
        #region Variables Basic

        private AddStationsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private Toolbar ToolBar;
        private SearchView SearchView; 
        private ProgressBar ProgressBarLoader;
        private string SearchText = "a";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window?.SetSoftInputMode(SoftInput.AdjustNothing);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateStationsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                 
                AdsGoogle.Ad_Interstitial(this);
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
                SearchView?.ClearFocus();

                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ProgressBarLoader = (ProgressBar)FindViewById(Resource.Id.sectionProgress);
                ProgressBarLoader.Visibility = ViewStates.Gone;

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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = "";
                    ToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    ToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }

                SearchView = FindViewById<SearchView>(Resource.Id.searchViewBox);
                SearchView.SetQuery("", false);
                SearchView.SetIconifiedByDefault(false);
                SearchView.OnActionViewExpanded();
                SearchView.Iconified = false;
                SearchView.QueryTextChange += SearchViewOnQueryTextChange;
                SearchView.QueryTextSubmit += SearchViewOnQueryTextSubmit;
                //SearchView.ClearFocus();

                //Change text colors
                var editText = (EditText)SearchView?.FindViewById(Resource.Id.search_src_text);
                editText?.SetHintTextColor(Color.Gray);
                editText?.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchView?.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon?.Parent;
                linearLayoutSearchView?.RemoveView(searchViewIcon);

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
                MAdapter = new AddStationsAdapter(this) { StationsList = new ObservableCollection<SearchStationsObject.Datum>() };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                
                Inflated ??= EmptyStateLayout.Inflate();

                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);

                x.EmptyStateButton.Visibility = ViewStates.Invisible;
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    //x.EmptyStateButton.Click += TryAgainButton_Click;
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
                    MAdapter.OnItemClick += MAdapterOnOnItemClick;
                }
                else
                {
                    MAdapter.OnItemClick -= MAdapterOnOnItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void MAdapterOnOnItemClick(object sender, AddStationsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                { 
                    MAdapter.NotifyItemChanged(e.Position, item.IsAdded == false ? "true" : "false");

                    Toast.MakeText(this, GetString(Resource.String.Lbl_AddedSuccessfully), ToastLength.Short)?.Show();

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.AddStationsAsync(item.RadioId, item.Name , item.Url , item.Logo , item.Genre , item.Country) });
                    else
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void SearchViewOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        { 
            SearchText = e.NewText;
        }

        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                SearchText = e.NewText;

                SearchView.ClearFocus(); 

                MAdapter.StationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (Methods.CheckConnectivity())
                {
                    if (MAdapter.StationsList.Count > 0)
                    {
                        MAdapter.StationsList.Clear();
                        MAdapter.NotifyDataSetChanged();
                    }

                    ProgressBarLoader.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Close keyboard
                    InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                    if (inputManager != null && inputManager.IsActive)
                    {
                        if (ToolBar != null)
                        {
                            inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                            inputManager?.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
                        }
                    }

                    StartApiService();
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                    }

                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                    x.EmptyStateButton.Visibility = ViewStates.Visible;

                    ProgressBarLoader.Visibility = ViewStates.Gone;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
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
                        MAdapter?.StationsList?.Clear();
                        MAdapter?.NotifyDataSetChanged();

                        if (ProgressBarLoader != null)
                            ProgressBarLoader.Visibility = ViewStates.Visible;

                        if (EmptyStateLayout != null)
                            EmptyStateLayout.Visibility = ViewStates.Gone;
                         
                        StartApiService();
                    }
                }
                else
                {
                    Inflated ??= EmptyStateLayout?.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick; 
                    }

                    x.EmptyStateButton.Visibility = ViewStates.Invisible;
                    if (EmptyStateLayout != null)
                    {
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }

                    ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
        }

        private async Task StartSearchRequest()
        { 
            int countStationsList = MAdapter.StationsList.Count;
             
            var (apiStatus, respond) = await RequestsAsync.User.SearchStationsAsync(SearchText);
            if (apiStatus == 200)
            {
                if (respond is SearchStationsObject result)
                {
                    var respondStationsList = result.Data?.Count;
                    if (respondStationsList > 0)
                    {
                        foreach (var item in from item in result.Data let check = MAdapter.StationsList.FirstOrDefault(a => a.RadioId == item.RadioId) where check == null select item)
                        {
                            var isAdded = ListUtils.MyUserInfoList.FirstOrDefault()?.Stations?.FirstOrDefault(a => a.Lyrics == item.RadioId);
                            item.IsAdded = isAdded != null;
                             
                            MAdapter.StationsList.Add(item);
                        }

                        if (countStationsList > 0)
                        { 
                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countStationsList, MAdapter.StationsList.Count - countStationsList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    } 
                }
            }
            else
            {
                Methods.DisplayReportResult(this, respond); 
            }

            RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            { 
                ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.StationsList.Count > 0)
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick; 
                    }

                    x.EmptyStateButton.Visibility = ViewStates.Invisible;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            { 
                ProgressBarLoader.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();
                MAdapter.StationsList.Clear();
                MAdapter.NotifyDataSetChanged();
                 
                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                   return;
                }

                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                    Search();
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick; 
                    }

                    x.EmptyStateButton.Visibility = ViewStates.Invisible;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
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