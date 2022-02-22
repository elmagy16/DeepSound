using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using MaterialDialogsCore;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Com.Sothree.Slidinguppanel;
using DeepSound.Activities.Comments;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.FloatingActionButton;
using Refractored.Controls;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SoundController : Object, SeekBar.IOnSeekBarChangeListener, Animator.IAnimatorListener, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback, MaterialDialog.IListCallbackMultiChoice
    {
        #region Variables Basic

        private AppCompatSeekBar SeekSongProgressbar;
        private ProgressBar TopSeekSongProgressbar;
        public FloatingActionButton BtPlay;
        private TextView TvTitleSound, TvDescriptionSound;
        private TextView TvSongCurrentDuration, TvSongTotalDuration, TxtArtistName, TxtArtistAbout, TxtPlaybackSpeed;
        private ImageView BtnIconComments, BtnIconFavorite, BtnIconLike, BtnIconDislike;
        public ImageView BtnIconDownload;
        public ProgressBar ProgressBarDownload;
        private ImageView BtnIconAddTo, BtnIconShare, IconInfo;
        private FrameLayout LinearAddTo, LinearShare, LinearComments, LinearFavorite, LinearLike, LinearDislike;
        private RelativeLayout LinearDownload;
        private ImageView ImageCover;
        public ImageView BackIcon, BtnPlayImage;
        private ImageButton BtnSkipPrev, BtnNext, BtnRepeat, BtnShuffle, BtnBackward, BtnForward;
        private CircleImageView ArtistImageView;
        private Timer Timer;
        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        public readonly SocialIoClickListeners ClickListeners;
        private string TotalIdPlaylistChecked;
        private SoundDownloadAsyncController SoundDownload;
        private RowSoundAdapter Adapter;
        private RequestBuilder FullGlideRequestBuilder;
        private RequestOptions GlideRequestOptions;

        #endregion

        #region General

        public SoundController(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = (HomeActivity)activity ?? HomeActivity.GetInstance();

                ClickListeners = new SocialIoClickListeners(activity);

                PlayerService.ActionSeekTo = ActivityContext.PackageName + ".action.ACTION_SEEK_TO";
                PlayerService.ActionPlay = ActivityContext.PackageName + ".action.ACTION_PLAY";
                PlayerService.ActionPause = ActivityContext.PackageName + ".action.PAUSE";
                PlayerService.ActionStop = ActivityContext.PackageName + ".action.STOP";
                PlayerService.ActionSkip = ActivityContext.PackageName + ".action.SKIP";
                PlayerService.ActionRewind = ActivityContext.PackageName + ".action.REWIND";
                PlayerService.ActionToggle = ActivityContext.PackageName + ".action.ACTION_TOGGLE";
                PlayerService.ActionBackward = ActivityContext.PackageName + ".action.ACTION_BACKWARD";
                PlayerService.ActionForward = ActivityContext.PackageName + ".action.ACTION_FORWARD";
                PlayerService.ActionPlaybackSpeed = ActivityContext.PackageName + ".action.ACTION_PLAYBACK_SPEED";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitializeUi()
        {
            try
            {
                SeekSongProgressbar = ActivityContext.FindViewById<AppCompatSeekBar>(Resource.Id.seek_song_progressbar);
                TopSeekSongProgressbar = ActivityContext.FindViewById<ProgressBar>(Resource.Id.seek_song_progressbar2);
                BtPlay = ActivityContext.FindViewById<FloatingActionButton>(Resource.Id.bt_play);
                BtnSkipPrev = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_skipPrev);
                BtnNext = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_next);
                BtnRepeat = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_repeat);
                BtnShuffle = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_shuffle);
                BtnBackward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Backward);
                BtnForward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Forward);

                TvSongCurrentDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_current_duration);
                TvSongTotalDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_total_duration);
                TxtArtistName = ActivityContext.FindViewById<TextView>(Resource.Id.artist_name);
                TxtArtistAbout = ActivityContext.FindViewById<TextView>(Resource.Id.artist_about);
                ArtistImageView = ActivityContext.FindViewById<CircleImageView>(Resource.Id.image);
                ImageCover = ActivityContext.FindViewById<ImageView>(Resource.Id.image_Cover);
                BtnPlayImage = ActivityContext.FindViewById<ImageView>(Resource.Id.play_button);
                BackIcon = ActivityContext.FindViewById<ImageView>(Resource.Id.BackIcon);

                LinearAddTo = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_playlist);
                BtnIconAddTo = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_addtoplay);
                LinearShare = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_share);
                BtnIconShare = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_shareicon);
                LinearComments = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_comments);
                BtnIconComments = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_comments);
                LinearDownload = ActivityContext.FindViewById<RelativeLayout>(Resource.Id.ll_download);
                BtnIconDownload = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_download);
                ProgressBarDownload = ActivityContext.FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBarDownload.Visibility = ViewStates.Invisible;

                LinearFavorite = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_fav);
                BtnIconFavorite = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_fav);
                LinearLike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_like);
                BtnIconLike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_like);
                LinearDislike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_Dislike);
                BtnIconDislike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_Dislike);
                IconInfo = ActivityContext.FindViewById<ImageView>(Resource.Id.info);
                TvTitleSound = ActivityContext.FindViewById<TextView>(Resource.Id.titleSound);
                TvDescriptionSound = ActivityContext.FindViewById<TextView>(Resource.Id.descriptionSound);
                TxtPlaybackSpeed = ActivityContext.FindViewById<TextView>(Resource.Id.bt_playbackSpeed);



                GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(ActivityContext).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));

                BtnIconDownload.Tag = "Download";
                BtnSkipPrev.Tag = "no";
                BtnNext.Tag = "no";
                BtnRepeat.Tag = "no";
                BtnShuffle.Tag = "no";
                BtnBackward.Tag = "no";
                BtnForward.Tag = "no";

                if (!AppSettings.ShowForwardTrack)
                    BtnForward.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowBackwardTrack)
                    BtnBackward.Visibility = ViewStates.Gone;

                SetProgress();

                // set Event 
                if (!BtnPlayImage.HasOnClickListeners)
                {
                    BtnPlayImage.Click += BtPlayOnClick;
                    IconInfo.Click += IconInfoOnClick;

                    BtPlay.Click += BtPlayOnClick;
                    BackIcon.Click += BackIconOnClick;

                    BtnSkipPrev.Click += BtnSkipPrevOnClick;
                    BtnNext.Click += BtnNextOnClick;
                    BtnRepeat.Click += BtnRepeatOnClick;
                    BtnShuffle.Click += BtnShuffleOnClick;
                    BtnBackward.Click += BtnBackwardOnClick;
                    BtnForward.Click += BtnForwardOnClick;

                    LinearAddTo.Click += ShowMoreOptions;
                    LinearShare.Click += LinearShareOnClick;
                    LinearComments.Click += LinearCommentsOnClick;
                    LinearDownload.Click += LinearDownloadOnClick;
                    LinearFavorite.Click += LinearFavoriteOnClick;
                    LinearLike.Click += LinearLikeOnClick;
                    LinearDislike.Click += LinearDislikeOnClick;
                    TxtPlaybackSpeed.Click += TxtPlaybackSpeedOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Destroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer?.Stop();
                    Timer?.Dispose();
                }

                if (GlobalContext.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Hidden)
                    GlobalContext.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);

                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null) item.IsPlay = false;

                Adapter?.NotifyItemChanged(Constant.PlayPos);

                if (Constant.Player == null)
                    return;

                if (Constant.Player.PlayWhenReady)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    ContextCompat.StartForegroundService(GlobalContext, intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event Click

        //Show info Data song 
        private void IconInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogInfoSong(ActivityContext).Display(item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //click play Sound
        private void BtPlayOnClick(object sender, EventArgs e)
        {
            try
            {
                // check for already playing
                StartOrPausePlayer();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //click Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && (GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    StopFragmentSound();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private FloatingActionButton PlayAllButton;

        public void SetPlayAllButton(FloatingActionButton playAllButton)
        {
            PlayAllButton = playAllButton;
        }

        public void StopFragmentSound()
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    ContextCompat.StartForegroundService(GlobalContext, intent);
                     
                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                }

                if (PlayAllButton != null)
                {
                    PlayAllButton.SetImageResource(Resource.Drawable.icon_play_action_small_vector);
                    PlayAllButton.Tag = "play";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Shuffle Sound
        private void BtnShuffleOnClick(object sender, EventArgs e)
        {
            ToggleShuffleButton();
        }

        public void ToggleShuffleButton()
        {
            try
            {
                Constant.IsSuffle = !Constant.IsSuffle;
                ToggleButtonColor(BtnShuffle);

                if (BtnRepeat?.Tag?.ToString() == "selected")
                {
                    Constant.IsRepeat = false;
                    ToggleButtonColor(BtnRepeat); // clear 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Repeat Sound
        private void BtnRepeatOnClick(object sender, EventArgs e)
        {
            try
            {
                Constant.IsRepeat = !Constant.IsRepeat;
                ToggleButtonColor(BtnRepeat);

                if (BtnShuffle?.Tag?.ToString() == "selected")
                {
                    Constant.IsSuffle = !Constant.IsSuffle;
                    ToggleButtonColor(BtnShuffle);  // clear 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Forward 10 Sec
        private void BtnForwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionForward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Backward 10 Sec
        private void BtnBackwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionBackward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Run Next Sound
        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    {
                        item.IsPlay = false;
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionSkip);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        item.IsPlay = false;
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionSkip);
                        ContextCompat.StartForegroundService(GlobalContext, intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Skip Prev 
        private void BtnSkipPrevOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    {
                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionRewind);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionRewind);
                        ContextCompat.StartForegroundService(GlobalContext, intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Like
        private void LinearLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnLikeSongsClick(new LikeSongsClickEventArgs { LikeButton = BtnIconLike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Dislike
        private void LinearDislikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnDislikeSongsClick(new LikeSongsClickEventArgs { LikeButton = BtnIconDislike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Add Or remove Favorite 
        private void LinearFavoriteOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnFavoriteSongsClick(new FavSongsClickEventArgs { FavButton = BtnIconFavorite, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Comments
        private void LinearCommentsOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogComment(ActivityContext).Display(item, TvSongCurrentDuration.Text);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Download
        private void LinearDownloadOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    //var isPro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro ?? 0;
                    //if (isPro == 0)
                    //{
                    //    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "GoPro");
                    //    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Go_Pro), ActivityContext.GetText(Resource.String.Lbl_Message_UpgradeAccount), ActivityContext.GetText(Resource.String.Lbl_upgrade_now), ActivityContext.GetText(Resource.String.Lbl_Cancel));
                    //    return;
                    //}

                    Methods.Path.Chack_MyFolder();

                    var item = Constant.ArrayListPlay[Constant.PlayPos];

                    if (item != null)
                    {
                        if (!Directory.Exists(Methods.Path.FolderDcimSound))
                            Directory.CreateDirectory(Methods.Path.FolderDcimSound);

                        string filePath;
                        string title;

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                        {
                            /*
                             * this changes is due to scoped storage introduce in Android 10
                             * https://developer.android.com/preview/privacy/storage
                             */

                            var file = Application.Context.GetExternalFilesDir(Environment.DirectoryMusic);
                            title = item.Title;
                            filePath = Methods.MultiMedia.CheckFileIfExits(file + "/" + title);
                        }
                        else
                        {
                            title = item.AudioLocation.Split("/Sound/").Last().Replace("%20", " ");
                            filePath = Methods.MultiMedia.CheckFileIfExits(Methods.Path.FolderDcimSound + title);
                        }


                        if (filePath != "File Dont Exists")
                        {
                            var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                            dialog.Title(Resource.String.Lbl_DeleteSong);
                            dialog.Content(ActivityContext.GetText(Resource.String.Lbl_Do_You_want_to_remove_Song));
                            dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Yes)).OnPositive((o, args) =>
                            {
                                try
                                {
                                    SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, title, ActivityContext);
                                    SoundDownload?.RemoveDiskSoundFile(title, item.Id);

                                    BtnIconDownload.Tag = "Download";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                    BtnIconDownload.SetColorFilter(Color.White);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialog.NegativeText(ActivityContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();
                        }
                        else
                        {
                            SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, ActivityContext);

                            if (!SoundDownload.CheckDownloadLinkIfExits())
                            {
                                SoundDownload.StartDownloadManager(item.Title, item, "Main");

                                ProgressBarDownload.Visibility = ViewStates.Visible;
                                BtnIconDownload.Visibility = ViewStates.Invisible;
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

        //Share
        private void LinearShareOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnShareClick(new ShareSongClickEventArgs { SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShowMoreOptions(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Add_To_Playlist));

                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_RePost));
                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_ReportSong));
                arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_ReportCopyright));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add To Playlist
        private void LinearAddToOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var arrayAdapter = new List<string>();
                var arrayIndexAdapter = new int[] { };
                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ListUtils.PlaylistList?.Count > 0) arrayAdapter.AddRange(ListUtils.PlaylistList.Select(playlistDataObject => Methods.FunString.DecodeString(playlistDataObject.Name)));

                dialogList.Title(ActivityContext.GetText(Resource.String.Lbl_SelectPlaylist))
                    .Items(arrayAdapter)
                    .ItemsCallbackMultiChoice(arrayIndexAdapter, this)
                    .AlwaysCallMultiChoiceCallback()
                    .NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this)
                    .PositiveText(ActivityContext.GetText(Resource.String.Lbl_Done)).OnPositive(this)
                    .NeutralText(ActivityContext.GetText(Resource.String.Lbl_Create)).OnNeutral(this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //speed playback (1x, 1.5x, 2x) 
        private void TxtPlaybackSpeedOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionPlaybackSpeed);

                if (TxtPlaybackSpeed.Text == "1x")
                {
                    intent.PutExtra("PlaybackSpeed", "Medium");
                    TxtPlaybackSpeed.Text = "1.5x";
                }
                else if (TxtPlaybackSpeed.Text == "1.5x")
                {
                    intent.PutExtra("PlaybackSpeed", "High");
                    TxtPlaybackSpeed.Text = "2x";
                }
                else if (TxtPlaybackSpeed.Text == "2x")
                {
                    intent.PutExtra("PlaybackSpeed", "Normal");
                    TxtPlaybackSpeed.Text = "1x";
                }

                TxtPlaybackSpeed.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                ContextCompat.StartForegroundService(GlobalContext, intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Fun >> SeekBar

        // set Progress bar values
        public void SetProgress()
        {
            try
            {
                // Run timer
                Timer = new Timer
                {
                    Interval = 1000
                };
                Timer.Elapsed += TimerOnElapsed;

                SeekSongProgressbar.Max = MusicUtils.MaxProgress;
                SeekSongProgressbar.SetOnSeekBarChangeListener(this);

                TopSeekSongProgressbar.Max = MusicUtils.MaxProgress;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    SeekSongProgressbar.SetProgress(0, true);
                    TopSeekSongProgressbar.SetProgress(0, true);
                }
                else
                {
                    try
                    {
                        // For API < 24 
                        SeekSongProgressbar.Progress = 0;
                        TopSeekSongProgressbar.Progress = 0;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {

        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    // remove message Handler from updating progress bar
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                long progress = seekBar.Progress;

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionSeekTo);
                intent.PutExtra("seekTo", progress);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fun Player

        public void StartPlaySound(SoundDataObject soundObject, ObservableCollection<SoundDataObject> listSound, RowSoundAdapter adapter = null)
        {
            try
            {
                Adapter = adapter;
                GlobalContext?.SetWakeLock();

                if (listSound.Count > 0)
                {
                    Constant.IsPlayed = false;
                    Constant.IsOnline = true;
                    Constant.ArrayListPlay = new ObservableCollection<SoundDataObject>(listSound);
                }

                if (soundObject != null)
                {
                    LoadSoundData(soundObject, false);

                    ReleaseSound();

                    //Play Song  
                    if (GlobalContext?.SlidingUpPanel != null && GlobalContext?.SlidingUpPanel != null)
                    {
                        StartOrPausePlayer();

                        BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        BackIcon.Tag = "Open";

                        if (GlobalContext?.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Collapsed)
                            GlobalContext?.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseSound()
        {
            try
            {
                if (Constant.Player != null)
                {
                    if (Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                        Constant.Player = null!;
                    }
                }

                if (GlobalContext?.Timer != null)
                {
                    GlobalContext.Timer.Enabled = false;
                    GlobalContext.Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartOrPausePlayer()
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                { 
                    if (Constant.ArrayListPlay.Count == 1)
                        Constant.PlayPos = 0;

                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    if (Constant.IsPlayed)
                    {
                        if (Constant.Player.PlayWhenReady)
                        {
                            if (item != null) item.IsPlay = false;
                            intent.SetAction(PlayerService.ActionPause);
                            ContextCompat.StartForegroundService(GlobalContext, intent); 
                        }
                        else
                        {
                            if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                            {
                                if (!Constant.IsOnline || Methods.CheckConnectivity())
                                {
                                    item.IsPlay = true;
                                    intent.SetAction(PlayerService.ActionPlay);
                                    ContextCompat.StartForegroundService(GlobalContext, intent);
                                }
                                else
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }
                            else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                ContextCompat.StartForegroundService(GlobalContext, intent);
                            }
                        }
                    }
                    else
                    {
                        if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                        {
                            if (!Constant.IsOnline || Methods.CheckConnectivity())
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                ContextCompat.StartForegroundService(GlobalContext, intent);
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                        else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                        {
                            item.IsPlay = true;
                            intent.SetAction(PlayerService.ActionPlay);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                    }

                    Adapter?.NotifyItemChanged(Constant.PlayPos);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void LoadSoundData(SoundDataObject soundObject, bool isPlay)
        {
            try
            {
                GlobalContext.RunOnUiThread(() =>
                {
                    try
                    {
                        if (isPlay)
                        {
                            try
                            {
                                var list = Adapter?.SoundsList.Where(sound => sound.IsPlay).ToList();
                                if (list?.Count > 0)
                                    foreach (var all in list)
                                        all.IsPlay = false;

                                var item = Adapter?.GetItem(Constant.PlayPos);
                                if (item != null)
                                    item.IsPlay = true;

                                Adapter?.NotifyDataSetChanged();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }

                        FullGlideRequestBuilder.Load(soundObject.Thumbnail).Into(ImageCover);
                        GlideImageLoader.LoadImage(ActivityContext, soundObject.Thumbnail, ArtistImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        TxtArtistName.Text = Methods.FunString.DecodeString(soundObject.Publisher.Name);

                        var d = soundObject.Title.Replace("<br>", "");
                        TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 30);

                        TvTitleSound.Text = Methods.FunString.DecodeString(d);

                        if (!AppSettings.ShowTitleAlbumOnly)
                        {
                            TvDescriptionSound.Text = string.IsNullOrEmpty(soundObject.AlbumName)
                             ? Methods.FunString.DecodeString(soundObject.CategoryName) + " " + ActivityContext.GetText(Resource.String.Lbl_Music)
                             : Methods.FunString.DecodeString(soundObject.CategoryName) + " " + ActivityContext.GetText(Resource.String.Lbl_Music) + ", " + ActivityContext.GetText(Resource.String.Lbl_InAlbum) + " " + Methods.FunString.DecodeString(soundObject.AlbumName);
                        }
                        else
                        {
                            TvDescriptionSound.Text = ActivityContext.GetText(Resource.String.Lbl_InAlbum) + " " + Methods.FunString.DecodeString(soundObject.AlbumName);
                        }

                        TvSongTotalDuration.Text = soundObject.Duration;
                        TvSongCurrentDuration.Text = "0:00";

                        BtnIconLike.Tag = soundObject.IsLiked != null && soundObject.IsLiked.Value ? "Like" : "Liked" ?? "Liked";
                        ClickListeners.SetLike(BtnIconLike);

                        BtnIconDislike.Tag = soundObject.IsDisLiked != null && soundObject.IsDisLiked.Value ? "Dislike" : "Disliked" ?? "Disliked";
                        ClickListeners.SetDislike(BtnIconDislike);

                        BtnIconFavorite.Tag = soundObject.IsFavoriated != null && soundObject.IsFavoriated.Value ? "Add" : "Added";
                        ClickListeners.SetFav(BtnIconFavorite);

                        //Add CreateCacheMediaSource if have or no 
                        //var fileSplit = soundObject.Id.Split('/').Last();

                        if (soundObject.IsOwner != null && (soundObject.AllowDownloads == 0 && !soundObject.IsOwner.Value))
                        {
                            LinearDownload.Visibility = ViewStates.Gone;
                        }
                        else
                            LinearDownload.Visibility = ViewStates.Visible;

                        //var canDownload = AppSettings.AllowOfflineDownload && UserDetails.IsPro == "1" && !PlayerService.GetPlayerService()!.ShouldShowMusicPurchaseDialog();
                        //LinearDownload.Enabled = canDownload;
                        //BtnIconDownload.Alpha = canDownload ? 1f : 0.3f;

                        TxtPlaybackSpeed.Text = "1x";
                        TxtPlaybackSpeed.SetTextColor(Color.ParseColor("#999999"));

                        var sqlEntity = new SqLiteDatabase();
                        SoundDataObject dataSound = sqlEntity.Get_LatestDownloadsSound(soundObject.Id);
                        if (dataSound != null)
                        {
                            if (!string.IsNullOrEmpty(dataSound.AudioLocation) && (dataSound.AudioLocation.Contains("file://") || dataSound.AudioLocation.Contains("content://") || dataSound.AudioLocation.Contains("storage") || dataSound.AudioLocation.Contains("/data/user/0/")))
                            {
                                if (!Directory.Exists(Methods.Path.FolderDcimSound))
                                    Directory.CreateDirectory(Methods.Path.FolderDcimSound);

                                var filePath = "";

                                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                                {
                                    /*
                                     * this changes is due to scoped storage introduce in Android 10
                                     * https://developer.android.com/preview/privacy/storage
                                     */

                                    var file = Application.Context.GetExternalFilesDir(Environment.DirectoryMusic);
                                    filePath = Methods.MultiMedia.CheckFileIfExits(file + "/" + dataSound.Title);
                                }
                                else
                                {
                                    var title = dataSound.AudioLocation.Split("/Sound/").Last().Replace("%20", " ");
                                    filePath = Methods.MultiMedia.CheckFileIfExits(Methods.Path.FolderDcimSound + title);
                                }

                                if (filePath != "File Dont Exists")
                                {
                                    BtnIconDownload.Tag = "Downloaded";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                    BtnIconDownload.SetColorFilter(Color.Red);
                                }
                                else
                                {
                                    BtnIconDownload.Tag = "Download";
                                    BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                    BtnIconDownload.SetColorFilter(Color.White);
                                }
                            }
                            else
                            {
                                BtnIconDownload.Tag = "Download";
                                BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                BtnIconDownload.SetColorFilter(Color.White);
                            }
                        }
                        else
                        {
                            BtnIconDownload.Tag = "Download";
                            BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                            BtnIconDownload.SetColorFilter(Color.White);
                        }

                        ProgressBarDownload.Visibility = ViewStates.Invisible;
                        BtnIconDownload.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ChangePlayPauseIcon()
        {
            try
            {
                GlobalContext.RunOnUiThread(() =>
                {
                    // check for already playing
                    if (Constant.Player != null && Constant.Player.PlayWhenReady)
                    {
                        // Changing button image to pause button
                        BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                        BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }
                    }
                    else
                    {
                        // Changing button image to play button
                        BtPlay.SetImageResource(Resource.Drawable.icon_player_play);
                        BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_play);

                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SeekUpdate()
        {
            try
            {
                ActivityContext.RunOnUiThread(() =>
                {
                    try
                    {
                        if (Constant.Player == null)
                        {
                            return;
                        }

                        var totalDuration = Constant.Player.Duration;
                        var currentDuration = Constant.Player.CurrentPosition;

                        if (PlayerService.GetPlayerService().ShouldShowMusicPurchaseDialog() && currentDuration >= 2)
                        {
                            PlayerService.GetPlayerService().ShowMusicPurchaseDialog();
                        }

                        // Displaying Total Duration time
                        TvSongTotalDuration.Text = MusicUtils.MilliSecondsToTimer(totalDuration);
                        // Displaying time completed playing
                        TvSongCurrentDuration.Text = MusicUtils.MilliSecondsToTimer(currentDuration);

                        // Updating progress bar
                        int progress = MusicUtils.GetProgressSeekBar(currentDuration, totalDuration);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            SeekSongProgressbar.SetProgress(progress, true);
                            TopSeekSongProgressbar.SetProgress(progress, true);
                        }
                        else
                        {
                            try
                            {
                                // For API < 24 
                                SeekSongProgressbar.Progress = progress;
                                TopSeekSongProgressbar.Progress = progress;
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }

                        if (currentDuration >= totalDuration)
                        {
                            if (Constant.IsRepeat)
                            {
                                Constant.Player.SeekTo(0);
                                Constant.Player.PlayWhenReady = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Runnable

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Running this thread after 10 milliseconds
                SeekUpdate();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Animation Image 

        public void RotateImageAlbum()
        {
            try
            {
                if (Constant.Player != null && !Constant.Player.PlayWhenReady) return;
                ArtistImageView?.Animate()?.SetDuration(100)?.Rotation(ArtistImageView.Rotation + 2f)?.SetListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationCancel(Animator animation)
        {

        }

        public void OnAnimationEnd(Animator animation)
        {
            try
            {
                RotateImageAlbum();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationRepeat(Animator animation)
        {

        }

        public void OnAnimationStart(Animator animation)
        {

        }

        #endregion

        private void ToggleButtonColor(ImageButton bt)
        {
            try
            {
                if (bt != null)
                {
                    string selected = bt.Tag?.ToString();
                    if (selected == "selected")
                    {
                        // selected
                        bt.SetColorFilter(Color.ParseColor("#999999"));
                        bt.Tag = "no";
                    }
                    else
                    {
                        bt.Tag = "selected";
                        bt.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetUiSliding(bool show)
        {
            try
            {
                var soundObject = Constant.ArrayListPlay[Constant.PlayPos];
                if (soundObject != null)
                {
                    if (show)
                    {
                        IconInfo.Visibility = ViewStates.Gone;
                        BtnPlayImage.Visibility = ViewStates.Visible;
                        TopSeekSongProgressbar.Visibility = ViewStates.Visible;

                        TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(soundObject.Title), 30);
                    }
                    else
                    {
                        IconInfo.Visibility = ViewStates.Visible;
                        BtnPlayImage.Visibility = ViewStates.Gone;
                        TopSeekSongProgressbar.Visibility = ViewStates.Invisible;

                        TxtArtistAbout.Text = soundObject.TimeFormatted;
                    } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog >> Add To Playlist Async

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    TotalIdPlaylistChecked = TotalIdPlaylistChecked.Remove(TotalIdPlaylistChecked.Length - 1, 1);
                    if (Methods.CheckConnectivity())
                    {
                        var item = Constant.ArrayListPlay[Constant.PlayPos];
                        if (item != null)
                        {
                            //Sent Api
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.AddToPlaylistAsync(item.Id.ToString(), TotalIdPlaylistChecked) });
                        }
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (p1 == DialogAction.Neutral)
                {
                    ActivityContext.StartActivity(new Intent(ActivityContext, typeof(CreatePlaylistActivity)));
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

        public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
        {
            try
            {
                var list = ListUtils.PlaylistList;
                if (list?.Count > 0)
                {
                    for (int i = 0; i < which.Length; i++)
                    {
                        TotalIdPlaylistChecked += list[i].Id + ",";
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
            return true;
        }
         
        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == ActivityContext.GetText(Resource.String.Lbl_RePost))
                {
                    OnMenuRePostOnClick();
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_ReportSong))
                {
                    OnMenuReportSongOnClick();
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_ReportCopyright))
                {
                    OnMenuReportCopyrightSongOnClick();
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_Add_To_Playlist))
                {
                    LinearAddToOnClick();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        //RePost
        private void OnMenuRePostOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_RePost_Message), ToastLength.Short)?.Show();

                    //Sent Api >>
                    var song = Constant.ArrayListPlay[Constant.PlayPos];
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.RePostTrackAsync(song.Id.ToString()) });
                }
                else
                {
                    Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //ReportSong
        private void OnMenuReportSongOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialogBuilder.Title(Resource.String.Lbl_ReportSong).TitleColorRes(Resource.Color.primary);
                dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;
                        if (Methods.CheckConnectivity())
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_YourReportSong), ToastLength.Short)?.Show();

                            //Sent Api >>
                            var song = Constant.ArrayListPlay[Constant.PlayPos];
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.ReportUnReportTrackAsync(song.Id.ToString(), s.ToString(), true) });
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                dialogBuilder.PositiveText(GlobalContext.GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialogBuilder.NegativeText(GlobalContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialogBuilder.AlwaysCallSingleChoiceCallback();
                dialogBuilder.Build().Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Report Copyright Song
        private void OnMenuReportCopyrightSongOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialogBuilder.Title(Resource.String.Lbl_ReportCopyright).TitleColorRes(Resource.Color.primary);
                dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;

                        if (!materialDialog.PromptCheckBoxChecked)
                        { 
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_copyright_Error), ToastLength.Short)?.Show();
                        }
                        else
                        {
                            if (Methods.CheckConnectivity())
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_YourReportSong), ToastLength.Short)?.Show();
                                //Sent Api >>
                                var song = Constant.ArrayListPlay[Constant.PlayPos];
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Common.CreateCopyrightAsync(song.Id.ToString(), s.ToString()) });
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }
                        } 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                dialogBuilder.CheckBoxPromptRes(Resource.String.Lbl_copyright_Des, false, null);
                dialogBuilder.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialogBuilder.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialogBuilder.AlwaysCallSingleChoiceCallback();
                dialogBuilder.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}