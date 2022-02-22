using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.MyContacts;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Activities.Tabbes.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Exception = System.Exception;
using AndroidX.Fragment.App;
using AndroidX.SwipeRefreshLayout.Widget;
using DeepSound.Activities.Chat;
using Google.Android.Material.AppBar;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Artists
{
    public class ArtistsProfileFragment : Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private ImageView ImageAvatar, ImageCover;
        private TextView TxtUserName, IconVerified, IconPro, TxtCountFollowers, /*TxtFollowers,*/ TxtCountFollowing, /*TxtFollowing,*/ TxtCountTracks/*, TxtTracks*/;
        private SuperTextView TxtAbout;
        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;
        private CircleButton BtnFollow, BtnMessage, BtnMore;
        private LinearLayout LayoutFollowers, LayoutFollowing/*, LayoutTracks*/, LoadingLayout;
        public HSoundAdapter LatestSongsAdapter, TopSongsAdapter, StoreAdapter;
        private ActivitiesAdapter ActivitiesAdapter;
        public HAlbumsAdapter AlbumsAdapter;
        private ViewStub EmptyStateLayout, LatestSongsViewStub, TopSongsViewStub, AlbumsViewStub, StoreViewStub, ActivitiesViewStub;
        private View Inflated, LatestSongsInflated, TopSongsInflated, AlbumsInflated, StoreInflated, ActivitiesInflated;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private AlbumsFragment AlbumsFragment;
        private Details DetailsCounter;
        private UserDataObject DataUser;
        private string UserId = "";
        private static ArtistsProfileFragment Instance;
         
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
                View view = inflater.Inflate(Resource.Layout.ArtistsProfileLayout, container, false);
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

                UserId = Arguments.GetString("UserId") ?? "";
                DataUser = JsonConvert.DeserializeObject<UserDataObject>(Arguments.GetString("ItemData") ?? "");

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetMyInfoData();

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
         
        public override void OnDestroyView()
        {
            try
            {
                Instance = null!;
                base.OnDestroyView();
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
                CollapsingToolbar = (CollapsingToolbarLayout)view.FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);

                ImageAvatar = (ImageView)view.FindViewById(Resource.Id.imageAvatar);
                ImageCover = (ImageView)view.FindViewById(Resource.Id.cover_image);


                BtnFollow = (CircleButton)view.FindViewById(Resource.Id.AddUserbutton);
                BtnMessage = (CircleButton)view.FindViewById(Resource.Id.message_button);
                BtnMore = (CircleButton)view.FindViewById(Resource.Id.morebutton);

                BtnFollow.Click += BtnAddUserOnClick;
                BtnMessage.Click += BtnMessageOnClick;
                BtnMore.Click += BtnMoreOnClick;

                TxtUserName = (TextView)view.FindViewById(Resource.Id.username_profile);
                IconVerified = (TextView)view.FindViewById(Resource.Id.verified);
                IconPro = (TextView)view.FindViewById(Resource.Id.pro);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconVerified, IonIconsFonts.CheckmarkCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPro, FontAwesomeIcon.Rocket);

                IconVerified.Visibility = ViewStates.Gone;
                IconPro.Visibility = ViewStates.Gone;

                LayoutFollowers = (LinearLayout)view.FindViewById(Resource.Id.followersLayout);
                //TxtFollowers = (TextView)view.FindViewById(Resource.Id.txtFollowers);
                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.CountFollowers);
                LayoutFollowers.Click += LayoutFollowersOnClick;

                LayoutFollowing = (LinearLayout)view.FindViewById(Resource.Id.followingLayout);
                //TxtFollowing = (TextView)view.FindViewById(Resource.Id.txtFollowing);
                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.CountFollowing);
                LayoutFollowing.Click += LayoutFollowingOnClick;

                //LayoutTracks = (LinearLayout)view.FindViewById(Resource.Id.tracksLayout);
                //TxtTracks = (TextView)view.FindViewById(Resource.Id.txtTracks);
                TxtCountTracks = (TextView)view.FindViewById(Resource.Id.CountTracks);

                LoadingLayout = (LinearLayout)view.FindViewById(Resource.Id.Loading_LinearLayout);
                LoadingLayout.Visibility = ViewStates.Visible;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                 
                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);
                LatestSongsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubLatestSongs);
                TopSongsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubTopSongs);
                AlbumsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubAlbums);
                StoreViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubStore);
                ActivitiesViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubActivities);

                TxtAbout = (SuperTextView)view.FindViewById(Resource.Id.AboutTextview);
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
                GlobalContext.SetToolBar(toolbar, " ", true, false);
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
                //Latest Songs RecyclerView >> LinearLayoutManager.Horizontal 
                LatestSongsAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                LatestSongsAdapter.OnItemClick += LatestSongsAdapterOnOnItemClick;

                //Top Songs RecyclerView >> LinearLayoutManager.Horizontal 
                TopSongsAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                TopSongsAdapter.OnItemClick += TopSongsAdapterOnOnItemClick;

                //Albums RecyclerView >> LinearLayoutManager.Horizontal 
                AlbumsAdapter = new HAlbumsAdapter(Activity);
                AlbumsAdapter.ItemClick += AlbumsAdapterOnItemClick;

                //Store RecyclerView >> LinearLayoutManager.Horizontal 
                StoreAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                StoreAdapter.OnItemClick += TopSongsAdapterOnOnItemClick;

                //Activities RecyclerView >> LinearLayoutManager.Vertical
                ActivitiesAdapter = new ActivitiesAdapter(Activity) { ActivityList = new ObservableCollection<ActivityDataObject>() };
                ActivitiesAdapter.OnItemClick += ActivitiesAdapterOnOnItemClick;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static ArtistsProfileFragment GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        #endregion

        #region Event

        //Start Play Sound 
        private void TopSongsAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = TopSongsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, TopSongsAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void LatestSongsAdapterOnOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = LatestSongsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, LatestSongsAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void ActivitiesAdapterOnOnItemClick(object sender, ActivitiesAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = ActivitiesAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = 0;
                        GlobalContext?.SoundController?.StartPlaySound(item.TrackData?.TrackDataClass, new ObservableCollection<SoundDataObject> { item.TrackData?.TrackDataClass });
                    }
                }
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
                if (e.Position > -1)
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
                        GlobalContext.FragmentBottomNavigator.DisplayFragment(AlbumsFragment);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show Info User
        private void BtnMessageOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Context, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("UserItem", JsonConvert.SerializeObject(DataUser));
                Context.StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Follow  
        private void BtnAddUserOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                switch (BtnFollow?.Tag?.ToString())
                {
                    case "Add": //Sent follow 
                        BtnFollow.SetColor(Color.ParseColor(AppSettings.MainColor));
                        BtnFollow.SetImageResource(Resource.Drawable.ic_tick);
                        BtnFollow.Tag = "friends";
                        DataUser.IsFollowing = true;

                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_followed),
                            ToastLength.Short)?.Show();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, true) });
                        break;
                    case "friends": //Sent un follow 
                        BtnFollow.SetColor(Color.ParseColor("#444444"));
                        BtnFollow.SetImageResource(Resource.Drawable.ic_add);
                        BtnFollow.Tag = "Add";
                        DataUser.IsFollowing = false;

                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_Unfollowed),
                            ToastLength.Short)?.Show();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, false) });
                        break;
                }

                var dataUser = GlobalContext?.MainFragment?.ArtistsAdapter?.ArtistsList?.FirstOrDefault(a => a.Id == DataUser.Id);
                if (dataUser != null)
                {
                    dataUser.IsFollowing = DataUser.IsFollowing;
                    GlobalContext.MainFragment.ArtistsAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //More 
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
               
                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_ProfileInfo));
               
                if (UserDetails.IsLogin)
                    arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Block));

                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_CopyLinkToProfile));
                  
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(Context.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //wael
        private void ActivitiesMoreOnClick(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoreMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "UserProfileStore");
                bundle.PutString("UserId", UserId);

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

       
        private void AlbumsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("AlbumsType", "UserProfileAlbums");
                bundle.PutString("UserId", UserId);

                AlbumsByTypeFragment albumsByTypeFragment = new AlbumsByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(albumsByTypeFragment);
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
                bundle.PutString("SongsType", "UserProfileTopSongs");
                bundle.PutString("UserId", UserId);

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

        private void LatestSongsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("SongsType", "UserProfileLatestSongs");
                bundle.PutString("UserId", UserId);

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

        //Open User Contact >> Following
        private void LayoutFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Following");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(contactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open User Contact >> Followers
        private void LayoutFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Followers");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(contactsFragment);
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
                        Instance = null;
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

        #region Load Data User

        private void GetMyInfoData()
        {
            try
            {
                LoadDataUser();

                StartApiService();
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProfile });
        }

        private async Task LoadProfile()
        {
            if (Methods.CheckConnectivity())
            {
                string fetch = "albums,latest_songs,top_songs";

                if (AppSettings.ShowStore)
                    fetch += ",store";
                
                if (UserDetails.IsLogin)
                    fetch += ",activities";

                var (apiStatus, respond) = await RequestsAsync.User.ProfileAsync(UserId, fetch);
                if (apiStatus.Equals(200))
                {
                    if (respond is ProfileObject result)
                    {
                        if (result.Data != null)
                        {
                            DataUser = result.Data;

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    LoadDataUser();

                                    if (result.Details != null)
                                    {
                                        DetailsCounter = result.Details;

                                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.Details.Followers);
                                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.Details.Following);

                                        var trackCount = CalculateTotalTracks(result);

                                        TxtCountTracks.Text = trackCount;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            //Add Latest Songs
                            if (result.Data?.Latestsongs?.Count > 0)
                            {
                                LatestSongsAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.Latestsongs);
                            }

                            //Add Latest Songs
                            if (result.Data?.TopSongs?.Count > 0)
                            {
                                TopSongsAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.TopSongs);
                            }

                            //Add Albums
                            if (result.Data?.Albums?.Count > 0)
                            {
                                AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.Data?.Albums);
                            }

                            //Add Store
                            if (result.Data?.Store?.Count > 0)
                            {
                                StoreAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.Store);
                            }

                            //Add Activities
                            if (result.Data?.Activities?.Count > 0  && UserDetails.IsLogin)
                            {
                                ActivitiesAdapter.ActivityList = new ObservableCollection<ActivityDataObject>(result.Data.Activities);
                            }

                            //SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            //dbDatabase.InsertOrUpdate_DataMyInfo(result.Data);
                            //dbDatabase.Dispose();
                        }
                    }
                }
                else
                {
                    if (Activity != null)
                    {
                        Methods.DisplayReportResult(Activity, respond);
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Activity?.RunOnUiThread(() =>
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
                });
            }
        }

        private string CalculateTotalTracks(ProfileObject result)
        {
            var totalTracks = new List<SoundDataObject>();

            if (result.Data?.Latestsongs != null)
                totalTracks.AddRange(result.Data?.Latestsongs);

            if (result.Data?.TopSongs != null)
                totalTracks.AddRange(result.Data?.TopSongs);

            if (result.Data?.Store != null)
                totalTracks.AddRange(result.Data?.Store);

            if (result.Data?.Albums != null)
                foreach (var album in (result.Data?.Albums).Where(album => album.Songs != null))
                {
                    totalTracks.AddRange(album.Songs);
                }

            var trackCount = totalTracks.GroupBy(x => x.Id).Select(x => x);
            return trackCount.Count().ToString();
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

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;
                LoadingLayout.Visibility = ViewStates.Gone;

                if (LatestSongsAdapter.SoundsList?.Count > 0)
                {
                    LatestSongsInflated ??= LatestSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, LatestSongsInflated, LatestSongsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_LatestSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += LatestSongsMoreOnClick;
                    }
                }

                if (TopSongsAdapter.SoundsList?.Count > 0)
                {
                    TopSongsInflated ??= TopSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, TopSongsInflated, TopSongsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_TopSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += TopSongsMoreOnClick;
                    }
                }

                if (AlbumsAdapter.AlbumsList?.Count > 0)
                {
                    AlbumsInflated ??= AlbumsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<DataAlbumsObject>(Activity, AlbumsInflated, AlbumsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Albums));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += AlbumsMoreOnClick;
                    }
                }

                if (StoreAdapter.SoundsList?.Count > 0)
                {
                    StoreInflated ??= StoreViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, StoreInflated, StoreAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Store_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += StoreMoreOnClick;
                    }
                }

                if (ActivitiesAdapter.ActivityList?.Count > 0)
                {
                    ActivitiesInflated ??= ActivitiesViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<ActivityDataObject>(Activity, ActivitiesInflated, ActivitiesAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, Context.GetText(Resource.String.Lbl_Activities_Title) , false);
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null!;
                        recyclerInflater.MainLinear.Click += ActivitiesMoreOnClick;
                    }
                }

                if (LatestSongsAdapter.SoundsList?.Count == 0 && TopSongsAdapter.SoundsList?.Count == 0 && AlbumsAdapter.AlbumsList?.Count == 0 && StoreAdapter.SoundsList?.Count == 0 && ActivitiesAdapter.ActivityList?.Count == 0)
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
                LoadingLayout.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadDataUser()
        {
            try
            {
                if (Activity == null)
                {
                    return;
                }

                CollapsingToolbar.Title = DeepSoundTools.GetNameFinal(DataUser);

                GlideImageLoader.LoadImage(Activity, DataUser.Avatar, ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                GlideImageLoader.LoadImage(Activity, DataUser.Cover, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                TxtUserName.Text = DeepSoundTools.GetNameFinal(DataUser);

                TextSanitizer aboutSanitizer = new TextSanitizer(TxtAbout, Activity);
                aboutSanitizer.Load(DeepSoundTools.GetAboutFinal(DataUser));

                IconPro.Visibility = DataUser.IsPro == 1 ? ViewStates.Visible : ViewStates.Gone;
                IconVerified.Visibility = DataUser.Verified == 1 ? ViewStates.Visible : ViewStates.Gone;

                if (DataUser.IsFollowing != null && DataUser.IsFollowing.Value) // My Friend
                {
                    BtnFollow.SetColor(Color.ParseColor(AppSettings.MainColor));
                    BtnFollow.SetImageResource(Resource.Drawable.ic_tick);
                    BtnFollow.Tag = "friends";
                }
                else  //Not Friend
                {
                    BtnFollow.SetColor(Color.ParseColor("#444444"));
                    BtnFollow.SetImageResource(Resource.Drawable.ic_add);
                    BtnFollow.Tag = "Add";
                }
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
                LatestSongsAdapter.SoundsList.Clear();
                LatestSongsAdapter.NotifyDataSetChanged();

                TopSongsAdapter.SoundsList.Clear();
                TopSongsAdapter.NotifyDataSetChanged();

                AlbumsAdapter.AlbumsList.Clear();
                AlbumsAdapter.NotifyDataSetChanged();

                StoreAdapter.SoundsList.Clear();
                StoreAdapter.NotifyDataSetChanged();

                ActivitiesAdapter.ActivityList.Clear();
                ActivitiesAdapter.NotifyDataSetChanged();

                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == Context.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Long)?.Show();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnBlockUserAsync(DataUser.Id.ToString(), true) });

                        GlobalContext.FragmentNavigatorBack();
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == Context.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    string url = DataUser?.Url;
                    GlobalContext.SoundController.ClickListeners.OnMenuCopyOnClick(url);
                }
                else if (text == Context.GetText(Resource.String.Lbl_ProfileInfo))
                {
                    Intent intent = new Intent(Activity, typeof(DialogInfoUserActivity));
                    intent.PutExtra("ItemDataUser", JsonConvert.SerializeObject(DataUser));
                    intent.PutExtra("ItemDataDetails", JsonConvert.SerializeObject(DetailsCounter));
                    Activity.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}