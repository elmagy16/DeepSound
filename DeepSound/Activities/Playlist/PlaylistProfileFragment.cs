using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;
using Refractored.Controls;
using Exception = System.Exception;

namespace DeepSound.Activities.Playlist
{
    public class PlaylistProfileFragment : Fragment
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;
        private ImageView CoverImage, IconMore, ImageBack, ShuffleImageView;
        private CircleImageView DiskImage;
        public RowSoundLiteAdapter MAdapter;
        private HomeActivity GlobalContext;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private TextView TxtNamePlaylist, TxtPublisherName, TxtSongCount;
        private PlaylistDataObject PlaylistObject;
        private string PlaylistId = "";
        private FrameLayout BackIcon;
        private RequestOptions GlideRequestOptions;
        private RelativeLayout LayoutImage;
        private FloatingActionButton PlayAllButton;
        private LibrarySynchronizer LibrarySynchronizer;
        private ProgressBar ProgressBar;
        public static PlaylistProfileFragment Instance;

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
                View view = inflater.Inflate(Resource.Layout.PlaylistProfileLayout, container, false);
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

                Instance = this;

                InitComponent(view);
                SetRecyclerViewAdapters();

                SetDataPlaylist(); 
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

                ShuffleImageView = view.FindViewById<ImageView>(Resource.Id.iconTitle);
                ShuffleImageView.Click += EnableShuffleMode;

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recylerSongsPlaylist);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                IconMore = view.FindViewById<ImageView>(Resource.Id.more);
                IconMore.Click += IconMoreOnClick;

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);

                LayoutImage = view.FindViewById<RelativeLayout>(Resource.Id.Layoutimage);
                LayoutImage.SetClipToPadding(true);

                TxtNamePlaylist = view.FindViewById<TextView>(Resource.Id.name);
                TxtSongCount = view.FindViewById<TextView>(Resource.Id.soungCount);
                TxtPublisherName = view.FindViewById<TextView>(Resource.Id.ByUser);

                CoverImage = view.FindViewById<ImageView>(Resource.Id.Coverimage);
                DiskImage = view.FindViewById<CircleImageView>(Resource.Id.Diskimage);
                GlideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High).Apply(RequestOptions.CircleCropTransform().CenterCrop().CircleCrop());

                PlayAllButton = view.FindViewById<FloatingActionButton>(Resource.Id.fab);
                PlayAllButton.Click += PlayAllButtonOnClick;
                PlayAllButton.Tag = "play";
                PlayAllButton.Visibility = ViewStates.Gone;

                ImageBack = view.FindViewById<ImageView>(Resource.Id.ImageBack);
                BackIcon = view.FindViewById<FrameLayout>(Resource.Id.back);
                BackIcon.Click += BackIcon_Click;

                LibrarySynchronizer = new LibrarySynchronizer(Activity);
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
                MAdapter = new RowSoundLiteAdapter(Activity, "PlaylistProfileFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
                MAdapter.OnItemClick += MAdapterOnOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, MAdapter, sizeProvider, 10);
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

        //Back
        private void BackIcon_Click(object sender, EventArgs e)
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

        //Start Play Sound  
        private void MAdapterOnOnItemClick(object sender, RowSoundLiteAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play all Sound 
        private void PlayAllButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.SoundsList.FirstOrDefault();
                if (item != null)
                {
                    GlobalContext?.SoundController?.SetPlayAllButton(PlayAllButton);

                    if (PlayAllButton.Tag?.ToString() == "play")
                    {
                        PlayAllButton.SetImageResource(Resource.Drawable.icon_player_pause);
                        PlayAllButton.Tag = "stop";

                        Constant.PlayPos = 0;
                        GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList);
                    }
                    else
                    {
                        GlobalContext?.SoundController?.StopFragmentSound();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EnableShuffleMode(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.SoundController.ToggleShuffleButton();
                ShuffleImageView.SetColorFilter(Resources.GetColor(Constant.IsSuffle ? Resource.Color.primary : Resource.Color.gnt_gray, Activity.Theme));
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
                LibrarySynchronizer?.PlaylistMoreOnClick(new MorePlaylistClickEventArgs { PlaylistClass = PlaylistObject });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Playlist Songs And Data 

        private void SetDataPlaylist()
        {
            try
            {
                PlaylistId = Arguments.GetString("PlaylistId") ?? "";
                if (!string.IsNullOrEmpty(PlaylistId))
                {
                    PlaylistObject = JsonConvert.DeserializeObject<PlaylistDataObject>(Arguments.GetString("ItemData") ?? "");
                    if (PlaylistObject != null)
                    {
                        Glide.With(this).AsBitmap().Apply(GlideRequestOptions).Load(PlaylistObject.ThumbnailReady).Into(DiskImage);
                        Glide.With(this).AsBitmap().Load(PlaylistObject.ThumbnailReady).Into(CoverImage);

                        var d = PlaylistObject.Name.Replace("<br>", "");
                        TxtNamePlaylist.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 70);

                        TxtPublisherName.Text = PlaylistObject?.Publisher != null ? Context.GetText(Resource.String.Lbl_By) + " " + DeepSoundTools.GetNameFinal(PlaylistObject?.Publisher.Value.PublisherClass) : Context.GetText(Resource.String.Lbl_By);

                        if (!string.IsNullOrEmpty(PlaylistObject.Songs.ToString()))
                            TxtSongCount.Text = PlaylistObject.Songs + " " + GetText(Resource.String.Lbl_Songs);
                        else
                            TxtSongCount.Text = GetText(Resource.String.Lbl_Songs);
                    }

                    StartApiService();
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
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPlaylistSongs });
        }

        private async Task LoadPlaylistSongs()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.SoundsList.Count;
                 var (apiStatus, respond) = await RequestsAsync.Playlist.GetPlaylistSongsAsync(PlaylistId);
                if (apiStatus == 200)
                {
                    if (respond is PlaylistSongsObject result)
                    {
                        var respondList = result.Songs?.Count;
                        if (respondList > 0)
                        {
                            result.Songs = DeepSoundTools.ListFilter(result.Songs);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Songs let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Songs);
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
                ProgressBar.Visibility = ViewStates.Gone;
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;

                if (MAdapter.SoundsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    PlayAllButton.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;
                    PlayAllButton.Visibility = ViewStates.Gone;

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

    }
}