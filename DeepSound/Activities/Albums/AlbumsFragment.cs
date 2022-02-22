using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;
using Refractored.Controls;

namespace DeepSound.Activities.Albums
{
    public class AlbumsFragment : Fragment
    {
        #region Variables Basic

        private RecyclerView MRecycler;
        public static RowSoundLiteAdapter MAdapter;
        private LinearLayoutManager MLayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private HomeActivity GlobalContext;
        private TextView AlbumName, CountSoungAlbumText, NameUserText;
        private ImageView ImageCover, ImageAlbum, IconMore;
        private CircleImageView ImageAvatar;
        private CollapsingToolbarLayout CollapsingToolbar;
        private AppBarLayout AppBarLayout;
        private DataAlbumsObject AlbumsObject;
        private string AlbumsId = "";
        private FrameLayout BackIcon;
        private RequestBuilder FullGlideRequestBuilder;
        private RequestOptions GlideRequestOptions;
        private LibrarySynchronizer LibrarySynchronizer;
        private AppCompatButton BuyButton;
        public FloatingActionButton PlayAllButton;
        private ProgressBar ProgressBar;

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
                View view = inflater.Inflate(Resource.Layout.AlbumsLayout, container, false); 
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

                AlbumsId = Arguments.GetString("AlbumsId") ?? "";

                InitComponent(view);
                GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(this).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
                //InitToolbar(view);
                SetRecyclerViewAdapters();

                SetDataAlbums();

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progress);
                ProgressBar.Visibility = ViewStates.Visible;

                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.albums_recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                IconMore = view.FindViewById<ImageView>(Resource.Id.more);
                IconMore.Click += IconMoreOnClick;

                PlayAllButton = view.FindViewById<FloatingActionButton>(Resource.Id.fab);
                PlayAllButton.Click += PlayAllButtonOnClick;
                PlayAllButton.Tag = "play";
                PlayAllButton.Visibility = ViewStates.Gone;

                BuyButton = view.FindViewById<AppCompatButton>(Resource.Id.BuyButton);
                BuyButton.Click += BuyButtonOnClick;
                BuyButton.Visibility = ViewStates.Gone;

                ImageCover = view.FindViewById<ImageView>(Resource.Id.imageCover);
                ImageAvatar = view.FindViewById<CircleImageView>(Resource.Id.imageAvatar);
                ImageAlbum = view.FindViewById<ImageView>(Resource.Id.imageSound);
                AlbumName = view.FindViewById<TextView>(Resource.Id.albumName);
                CountSoungAlbumText = view.FindViewById<TextView>(Resource.Id.CountSoungAlbumText);
                NameUserText = view.FindViewById<TextView>(Resource.Id.nameUserText);
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
                MAdapter = new RowSoundLiteAdapter(Activity, "AlbumsFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
                MAdapter.OnItemClick += MAdapterOnItemClick;
                MLayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(MLayoutManager);
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

        //Buy Album
        private void BuyButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.OpenDialogPurchaseAlbum(AlbumsObject);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Icon More
        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                LibrarySynchronizer?.AlbumsOnMoreClick(new MoreAlbumsClickEventArgs { AlbumsClass = AlbumsObject });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Back
        private void BackIcon_Click(object sender, EventArgs e)
        {
            GlobalContext.FragmentNavigatorBack();
        }


        //Start Play Sound 
        private void MAdapterOnItemClick(object sender, RowSoundLiteAdapterClickEventArgs e)
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

        #endregion

        #region Load Albums Songs And Data 

        private void SetDataAlbums()
        {
            try
            {
                AlbumsId = Arguments.GetString("AlbumsId") ?? "";
                if (!string.IsNullOrEmpty(AlbumsId))
                {
                    AlbumsObject = JsonConvert.DeserializeObject<DataAlbumsObject>(Arguments.GetString("ItemData") ?? "");
                    if (AlbumsObject != null)
                    {

                        var d = AlbumsObject.Title.Replace("<br>", "");
                        AlbumName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 80);

                        var count = !string.IsNullOrEmpty(AlbumsObject.CountSongs) ? AlbumsObject.CountSongs : AlbumsObject.SongsCount ?? "0";

                        var text = count + " " + Context.GetText(Resource.String.Lbl_Songs);
                        if (AppSettings.ShowCountPurchases)
                            text = text + " - " + AlbumsObject.Purchases + " " + Context.GetText(Resource.String.Lbl_Purchases);

                        CountSoungAlbumText.Text = text;
                        NameUserText.Text = DeepSoundTools.GetNameFinal(AlbumsObject.Publisher ?? AlbumsObject.UserData);

                        var imageUrl = AlbumsObject.Thumbnail;

                        FullGlideRequestBuilder.Load(imageUrl).Into(ImageCover);
                        FullGlideRequestBuilder.Load(imageUrl).Into(ImageAlbum);

                        if (AlbumsObject.Publisher != null)
                        {
                            Glide.With(this).AsBitmap().Apply(GlideRequestOptions).Load(AlbumsObject.Publisher.Avatar).Into(ImageAvatar);
                        }
                        else
                        {
                            Glide.With(this).AsBitmap().Apply(GlideRequestOptions).Load(AlbumsObject.UserData.Avatar).Into(ImageAvatar);
                        }

                        if (AlbumsObject.IsOwner != null && Math.Abs(AlbumsObject.Price) > 0 && !AlbumsObject.IsOwner.Value && AlbumsObject.IsPurchased == 0)
                        {
                            BuyButton.Visibility = ViewStates.Visible;
                            ProgressBar.Visibility = ViewStates.Gone;
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSoundWithPaid);

                            EmptyStateLayout.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            BuyButton.Visibility = ViewStates.Gone;
                            StartApiService();
                        }
                    }
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadAlbumsSongs });
        }

        private async Task LoadAlbumsSongs()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.SoundsList.Count;
                 var (apiStatus, respond) = await RequestsAsync.Albums.GetAlbumSongsAsync(AlbumsId);
                if (apiStatus == 200)
                {
                    if (respond is GetAlbumSongsObject result)
                    {
                        var respondList = result.Songs?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                result.Songs = DeepSoundTools.ListFilter(result.Songs);

                                foreach (var item in from item in result.Songs let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() =>
                                {
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList);
                                });
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