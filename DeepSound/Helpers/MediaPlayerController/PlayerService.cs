using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Media.Session;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using DeepSound.Activities;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Requests;
using Uri = Android.Net.Uri;
using AudioAttributes = Com.Google.Android.Exoplayer2.Audio.AudioAttributes;

namespace DeepSound.Helpers.MediaPlayerController
{
    [Service(Exported = false)]
    public class PlayerService : Service
    {
        #region Variables Basic

        public static string ActionBackward;
        public static string ActionForward;
        public static string ActionPlaybackSpeed;
        public static string ActionSeekTo;
        public static string ActionPlay;
        public static string ActionPause;
        public static string ActionStop;
        public static string ActionSkip;
        public static string ActionRewind;
        public static string ActionToggle;
        private static NotificationCompat.Builder Notification;
        private RemoteViews BigViews, SmallViews;
        private readonly string NotificationChannelId = "deepsound_ch_1";
        private NotificationManager MNotificationManager;
        private CallBroadcastReceiver OnCallIncome;
        private static PlayerService Service;
        private HomeActivity GlobalContext;
        private SoundDataObject Item;

        private HeadPhoneBroadcastReceiver OnHeadPhoneDetect;
        private readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter.Builder(Application.Context).Build();
        private IMediaSource PlayerSource;
        private PlayerEvents PlayerListener;
        private AudioManager AudioManager;
        private ComponentName ComponentName;
        private MyAudioFocusChangeListener OnAudioFocusChangeListener;
        private AudioFocusRequestClass FocusRequest;
        private readonly int ResumeWindow = 0;
        private readonly long ResumePosition = 0;

        #endregion

        #region General

        public static PlayerService GetPlayerService()
        {
            return Service;
        }

        public PlayerService()
        {

        }

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;

                GlobalContext = HomeActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                OnCallIncome = new CallBroadcastReceiver();
                OnHeadPhoneDetect = new HeadPhoneBroadcastReceiver();

                RegisterReceiver(OnCallIncome, new IntentFilter("android.intent.action.PHONE_STATE"));
                RegisterReceiver(OnHeadPhoneDetect, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));

                SetAudioFocus();
                InitializePlayer();

                IsFirstTime = true;
                CreateNoti();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetAudioFocus()
        {
            try
            {
                if (OnAudioFocusChangeListener == null)
                {
                    OnAudioFocusChangeListener = new MyAudioFocusChangeListener();
                    AudioManager = (AudioManager)Constant.Context.GetSystemService(Context.AudioService);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        var playbackAttributes = new Android.Media.AudioAttributes.Builder()
                            .SetUsage(AudioUsageKind.Media)
                            .SetContentType(AudioContentType.Music)
                            .Build();

                        FocusRequest = new AudioFocusRequestClass.Builder(AudioFocus.Gain)
                            .SetAudioAttributes(playbackAttributes)
                            .SetAcceptsDelayedFocusGain(true)
                            .SetOnAudioFocusChangeListener(OnAudioFocusChangeListener)
                            .Build();

                        AudioManager.RequestAudioFocus(FocusRequest);
                    }
                    else
                    {
#pragma warning disable 618
                        AudioManager.RequestAudioFocus(OnAudioFocusChangeListener, Stream.Music, AudioFocus.Gain);
#pragma warning restore 618
                    }

                    ComponentName = new ComponentName(Constant.Context.PackageName, new MediaButtonIntentReceiver().Class.Name);
#pragma warning disable 618
                    AudioManager.RegisterMediaButtonEventReceiver(ComponentName);
#pragma warning restore 618 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializePlayer()
        {
            try
            {
                if (Constant.Player == null)
                {
                    AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                    var trackSelector = new DefaultTrackSelector(Application.Context, trackSelectionFactory);
                    Constant.Player = new SimpleExoPlayer.Builder(Constant.Context).Build();
                    Constant.Player.ShuffleModeEnabled = Constant.IsSuffle;
                    PlayerListener = new PlayerEvents(this);
                    Constant.Player.AddListener(PlayerListener);
                    Constant.Player.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(C.UsageMedia).SetContentType(C.ContentTypeMusic).Build(), true);
                    Constant.Player.SeekParameters = SeekParameters.Default;
                    Constant.Player.AudioAttributes = AudioAttributes.Default;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);
                string action = intent.Action;
                if (action == ActionSeekTo)
                {
                    SeekTo(intent.Extras?.GetLong("seekTo") ?? 0);
                }
                else if (action == ActionPlay)
                {
                    Play();
                }
                else if (action == ActionPause)
                {
                    Pause();
                }
                else if (action == ActionStop)
                {
                    Stop(intent);
                }
                else if (action == ActionRewind)
                {
                    Previous();
                }
                else if (action == ActionSkip)
                {
                    Next();
                }
                else if (action == ActionBackward)
                {
                    Backward();
                }
                else if (action == ActionForward)
                {
                    Forward();
                }
                else if (action == ActionToggle)
                {
                    TogglePlay();
                }
                else if (action == ActionPlaybackSpeed)
                {
                    PlaybackSpeed(intent.Extras?.GetString("PlaybackSpeed") ?? "Normal");
                }

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }

        private void HandleFirstPlay()
        {
            try
            {
                Constant.IsPlayed = false;

                SetPlayAudio();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SeekTo(long seek)
        {
            try
            {
                var totalDuration = Constant.Player.Duration;
                var currentPosition = MusicUtils.ProgressToTimer(seek, totalDuration);

                // forward or backward to certain seconds
                Constant.Player.SeekTo(currentPosition);

                GlobalContext?.SoundController?.SeekUpdate();

                if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                    Constant.Player.PlayWhenReady = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Play()
        {
            try
            {
                if (Constant.IsPlayed)
                {
                    Constant.Player.PlayWhenReady = true;
                    // GlobalContext?.SoundController?.SetProgress();
                }
                else
                {
                    HandleFirstPlay();
                }
                GlobalContext?.SoundController?.RotateImageAlbum();
                ChangePlayPauseIcon();
                UpdateNotiPlay(Constant.Player.PlayWhenReady);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Pause()
        {
            try
            {
                Constant.Player.PlayWhenReady = false;

                GlobalContext?.SoundController?.RotateImageAlbum();
                ChangePlayPauseIcon();
                UpdateNotiPlay(Constant.Player.PlayWhenReady);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Stop(Intent intent)
        {
            try
            {
                GlobalContext?.SoundController?.RotateImageAlbum();
                ChangePlayPauseIcon();
                Constant.IsPlayed = false;
                GlobalContext?.SoundController?.ReleaseSound();
                GlobalContext?.SoundController?.Destroy();
                RemoveNoti();

                if (AudioManager != null)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        if (FocusRequest != null) AudioManager.AbandonAudioFocusRequest(FocusRequest);
                    }
                    else
#pragma warning disable 618
                        AudioManager.AbandonAudioFocus(OnAudioFocusChangeListener);
#pragma warning restore 618

#pragma warning disable 618
                    AudioManager.UnregisterMediaButtonEventReceiver(ComponentName);
#pragma warning restore 618
                }

                try { UnregisterReceiver(OnCallIncome); }catch { }
                try { UnregisterReceiver(OnHeadPhoneDetect); }catch { }
                 
                if (intent != null) StopService(intent);
                StopForeground(true);

                GlobalContext?.OffWakeLock(); 
                StopSelf();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Previous()
        {
            try
            {
                SetBuffer(true);
                if (Constant.IsSuffle)
                {
                    Random rand = new Random();
                    if (Constant.ArrayListPlay.Count > 0)
                        Constant.PlayPos = rand.Next(Constant.ArrayListPlay.Count);
                }
                else
                {
                    if (Constant.PlayPos > 0)
                    {
                        Constant.PlayPos -= 1;
                    }
                    else
                    {
                        Constant.PlayPos = Constant.ArrayListPlay.Count - 1;
                    }
                }
                GlobalContext?.SoundController?.RotateImageAlbum();
                HandleFirstPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Next()
        {
            try
            {
                SetBuffer(true);
                if (Constant.IsSuffle)
                {
                    Random rand = new Random();
                    Constant.PlayPos = rand.Next(Constant.ArrayListPlay.Count - 1 + 1);
                }
                else
                {
                    if (Constant.PlayPos < Constant.ArrayListPlay.Count - 1)
                    {
                        Constant.PlayPos += 1;
                    }
                    else
                    {
                        Constant.PlayPos = 0;
                    }
                }

                GlobalContext?.SoundController?.RotateImageAlbum();
                HandleFirstPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Backward()
        {
            try
            {
                var bTime = 10000; // 10 Sec
                if (Constant.Player != null)
                {
                    var sTime = Constant.Player.CurrentPosition;

                    if ((sTime - bTime) > 0)
                    {
                        sTime -= bTime;
                        Constant.Player.SeekTo(sTime);

                        GlobalContext?.SoundController?.SeekUpdate();

                        if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                            Constant.Player.PlayWhenReady = true;
                    }
                    else
                    {
                        Toast.MakeText(Constant.Context, "Cannot jump backward 10 seconds", ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Forward()
        {
            try
            {
                var fTime = 10000; // 10 Sec
                if (Constant.Player != null)
                {
                    var eTime = Constant.Player.Duration;
                    var sTime = Constant.Player.CurrentPosition;
                    if ((sTime + fTime) <= eTime)
                    {
                        sTime += fTime;
                        Constant.Player.SeekTo(sTime);

                        GlobalContext?.SoundController?.SeekUpdate();

                        if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                            Constant.Player.PlayWhenReady = true;
                    }
                    else
                    {
                        Toast.MakeText(Constant.Context, "Cannot jump forward 10 seconds", ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TogglePlay()
        {
            try
            {
                if (Constant.Player != null && Constant.Player.PlayWhenReady)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PlaybackSpeed(string speed)
        {
            try
            {
                const float speedNormal = 1f;
                const float speedMedium = 1.5f;
                const float speedHigh = 2f;

                if (Constant.Player != null)
                {
                    PlaybackParameters param = new PlaybackParameters(speedNormal);
                    switch (speed)
                    {
                        case "Medium":
                            param = new PlaybackParameters(speedMedium);
                            break;
                        case "High":
                            param = new PlaybackParameters(speedHigh);
                            break;
                        case "Normal":
                            param = new PlaybackParameters(speedNormal);
                            break;
                    }

                    Constant.Player.PlaybackParameters = param;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region data Noti

        private void ShowNotification()
        {
            try
            {
                StartForeground(101, Notification.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveNoti()
        {
            try
            {
                MNotificationManager.CancelAll();
                Notification = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CreateNoti()
        {
            try
            {
                BigViews = new RemoteViews(PackageName, Resource.Layout.CustomNotificationLayout);
                SmallViews = new RemoteViews(PackageName, Resource.Layout.CustomNotificationSmallLayout);

                Intent notificationIntent = new Intent(this, typeof(SplashScreenActivity));
                notificationIntent.SetAction(Intent.ActionMain);
                notificationIntent.AddCategory(Intent.CategoryLauncher);
                notificationIntent.PutExtra("isnoti", true);



                PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);

                Intent previousIntent = new Intent(this, typeof(PlayerService));
                previousIntent.SetAction(ActionRewind);
                PendingIntent ppreviousIntent = PendingIntent.GetService(this, 0, previousIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Intent playIntent = new Intent(this, typeof(PlayerService));
                playIntent.SetAction(ActionToggle);
                PendingIntent pplayIntent = PendingIntent.GetService(this, 0, playIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Intent nextIntent = new Intent(this, typeof(PlayerService));
                nextIntent.SetAction(ActionSkip);
                PendingIntent pnextIntent = PendingIntent.GetService(this, 0, nextIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Intent closeIntent = new Intent(this, typeof(PlayerService));
                closeIntent.SetAction(ActionStop);
                PendingIntent pcloseIntent = PendingIntent.GetService(this, 0, closeIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Notification = new NotificationCompat.Builder(this, NotificationChannelId)
                    .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Mipmap.icon))
                    .SetContentTitle(AppSettings.ApplicationName)
                    .SetPriority((int)NotificationPriority.Max)
                    .SetContentIntent(pendingIntent)
                    .SetSmallIcon(Resource.Drawable.icon_notification)
                    .SetTicker(Constant.ArrayListPlay[Constant.PlayPos]?.Title)
                    .SetChannelId(NotificationChannelId)
                    .SetOngoing(true)
                    .SetAutoCancel(true)
                    .SetOnlyAlertOnce(true);

                NotificationChannel mChannel;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationImportance importance = NotificationImportance.Low;
                    mChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                    MNotificationManager.CreateNotificationChannel(mChannel);

                  
                    var mMediaSession = new MediaSession(Application.Context, AppSettings.ApplicationName);
                    mMediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);  

                    Notification.SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()
                             //.SetMediaSession(mMediaSession.SessionToken)  
                            .SetShowCancelButton(true)
                            .SetShowActionsInCompactView(0, 1, 2)
                           .SetCancelButtonIntent(MediaButtonReceiver.BuildMediaButtonPendingIntent(Application.Context, PlaybackState.ActionStop)))
                        .AddAction(new NotificationCompat.Action(Resource.Xml.ic_skip_previous, "Previous", ppreviousIntent))
                        .AddAction(new NotificationCompat.Action(Resource.Xml.ic_pause, "Pause", pplayIntent))
                        .AddAction(new NotificationCompat.Action(Resource.Xml.ic_skip_next, "Next", pnextIntent))
                        .AddAction(new NotificationCompat.Action(Resource.Drawable.icon_close_vector, "Close", pcloseIntent));
                }
                else
                {
                    string songName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.Title);
                    string genresName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.CategoryName) + " " + Application.Context.Resources?.GetString(Resource.String.Lbl_Music);

                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_play, pplayIntent);
                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_next, pnextIntent);
                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_prev, ppreviousIntent);
                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_close, pcloseIntent);
                    SmallViews.SetOnClickPendingIntent(Resource.Id.status_bar_collapse, pcloseIntent);

                    BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                    BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                    SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                    BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                    SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                    BigViews.SetImageViewResource(Resource.Id.imageView_noti, Resource.Mipmap.icon);
                    SmallViews.SetImageViewResource(Resource.Id.status_bar_album_art, Resource.Mipmap.icon);
                    Notification.SetCustomContentView(SmallViews).SetCustomBigContentView(BigViews);
                }

                ShowNotification();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNoti()
        {
            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        string songName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.Title);
                        string genresName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.CategoryName) + " " + GlobalContext?.GetText(Resource.String.Lbl_Music);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            Notification.SetContentTitle(songName);
                            Notification.SetContentText(genresName);
                        }
                        else
                        {
                            BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                            BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                            SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                            SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                        }

                        UpdateNotiPlay(Constant.Player.PlayWhenReady);
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

        private void UpdateNotiPlay(bool isPlay)
        {
            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        string songName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.Title);
                        string genresName = Methods.FunString.DecodeString(Constant.ArrayListPlay[Constant.PlayPos]?.CategoryName) + " " + GlobalContext?.GetText(Resource.String.Lbl_Music);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            Intent playIntent = new Intent(this, typeof(PlayerService));
                            playIntent.SetAction(ActionToggle);
                            PendingIntent pPreviousIntent = PendingIntent.GetService(this, 0, playIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                            if (isPlay)
                            {
                                if (Notification?.MActions.Count > 0)
                                    Notification.MActions[1] = new NotificationCompat.Action(Resource.Xml.ic_pause, "Pause", pPreviousIntent);
                            }
                            else
                            {
                                if (Notification?.MActions.Count > 0)
                                    Notification.MActions[1] = new NotificationCompat.Action(Resource.Xml.ic_play_arrow, "Play", pPreviousIntent);
                            }

                            if (!string.IsNullOrEmpty(songName))
                                Notification?.SetContentTitle(songName);
                            if (!string.IsNullOrEmpty(genresName))
                                Notification?.SetContentText(genresName);
                        }
                        else
                        {
                            if (isPlay)
                            {
                                BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                            }
                            else
                            {
                                BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                            }

                            if (!string.IsNullOrEmpty(songName))
                                BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                            if (!string.IsNullOrEmpty(genresName))
                                BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                            if (!string.IsNullOrEmpty(genresName))
                                SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                            if (!string.IsNullOrEmpty(songName))
                                SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                        }

                        var url = Constant.ArrayListPlay[Constant.PlayPos]?.Thumbnail?.Replace(" ", "%20");
                        if (!string.IsNullOrEmpty(url))
                        {
                            var bit = BitmapUtil.GetImageBitmapFromUrl(url);
                            if (bit != null)
                                Notification?.SetLargeIcon(bit);
                        }

                        MNotificationManager.Notify(101, Notification?.Build());
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

        #region Player && Listener

        public bool ShouldShowMusicPurchaseDialog()
        { 
            try
            {
                Item = Constant.ArrayListPlay[Constant.PlayPos];
                if (Item == null)
                    return false;

                if (Item.IsOwner != null && Item.IsOwner.Value || Item.Price == 0)
                    return false;

                if (Item.IsPurchased != null && Item.IsPurchased.Value)
                    return false;

                if (Item.Price != 0 && !string.IsNullOrEmpty(Item.DemoTrack))
                    return true;

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            } 
        }

        public void ShowMusicPurchaseDialog()
        {
            PauseAndReset();
            GlobalContext?.OpenDialogPurchaseSound(Item);
        }

        private void PauseAndReset()
        {
            try
            {
                Pause();
                if (Constant.Player != null)
                {
                    Constant.Player.SeekTo(0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnCompletion()
        {
            try
            {
                if (IsReset)
                    return;

                if (ShouldShowMusicPurchaseDialog())
                {
                    //This song is paid, You must pay
                    ShowMusicPurchaseDialog();
                }
                else
                {
                    if (Constant.IsRepeat)
                    {
                        Constant.Player.SeekTo(ResumeWindow, 0);
                        Constant.Player.PlayWhenReady = true;
                    }
                    else
                    {
                        if (Constant.IsSuffle)
                        {
                            Random rand = new Random();
                            Constant.PlayPos = rand.Next((Constant.ArrayListPlay.Count - 1) + 1);
                            SetPlayAudio();
                        }
                        else
                        {
                            SetNext();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private bool IsFirstTime;
        private async void OnPrepared()
        {
            try
            {
                Constant.IsPlayed = true;
                Constant.Player.PlayWhenReady = true;

                bool haveResumePosition = ResumeWindow != C.IndexUnset;
                if (haveResumePosition)
                    Constant.Player.SeekTo(ResumeWindow, ResumePosition);

                IsReset = false;

                SetBuffer(false);

                if (!IsFirstTime)
                {
                    CreateNoti();
                }

                IsFirstTime = false;
                UpdateNoti();

                if (Item != null)
                {
                    //add to Recent Played
                    if (UserDetails.IsLogin)
                        GlobalContext?.LibrarySynchronizer.AddToRecentlyPlayed(Item);

                    var (apiStatus, respond) = await RequestsAsync.Tracks.GetTrackInfoAsync(Item.Id.ToString()).ConfigureAwait(false);
                    if (apiStatus.Equals(200))
                    {
                        if (respond is GetTrackInfoObject result)
                        {
                            var data = Constant.ArrayListPlay.FirstOrDefault(a => a.Id == Item.Id);
                            if (data != null)
                            {
                                data = result.Data;
                                Item = result.Data;
                            }

                            Console.WriteLine(data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void ChangeText()
        {
            try
            {
                if (Item != null)
                    GlobalContext?.SoundController?.LoadSoundData(Item, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool IsReset;
        private void SetPlayAudio()
        {
            try
            { 
                try
                {
                    if (Constant.Player != null && Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                    }

                    Constant.Player = null!;
                    InitializePlayer();

                    //GC Collector
                    GC.Collect();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                IsReset = true;

                Item = Constant.ArrayListPlay[Constant.PlayPos];
                if (Item == null) return;

                ChangeText();
                Uri mediaUri;

                if (Item.AudioLocation.Contains("stream") && Item.AudioLocationOriginal.Contains("stream"))
                {
                    mediaUri = Uri.Parse(Item.AudioLocationOriginal);
                }
                else if (!string.IsNullOrEmpty(Item.AudioLocation) && Item.AudioLocation.Contains("http"))
                { 
                    if (Item.IsOwner != null && Item.IsOwner.Value || Item.Price == 0)
                    {
                        mediaUri = Uri.Parse(Item.AudioLocation);
                    }
                    else if (Item.IsPurchased != null && Item.IsPurchased.Value)
                    {
                        mediaUri = Uri.Parse(Item.AudioLocation);
                    } 
                    else if (Item.Price != 0 && !string.IsNullOrEmpty(Item.DemoTrack))
                    {
                        if (!Item.DemoTrack.Contains(InitializeDeepSound.WebsiteUrl))
                            Item.DemoTrack = InitializeDeepSound.WebsiteUrl + "/" + Item.DemoTrack;

                        mediaUri = Uri.Parse(Item.DemoTrack);
                    }
                    else
                        mediaUri = Uri.Parse(Item.AudioLocation);
                }
                else if (!string.IsNullOrEmpty(Item.AudioLocation) && (Item.AudioLocation.Contains("file://") || Item.AudioLocation.Contains("content://") || Item.AudioLocation.Contains("storage") || Item.AudioLocation.Contains("/data/user/0/")))
                {
                    mediaUri = Uri.Parse(Item.AudioLocation);
                }
                else
                    mediaUri = Uri.Parse(Item.AudioLocation);

                PlayerSource = null!;
                PlayerSource = GetMediaSourceFromUrl(mediaUri, "normal");
                Constant.Player?.Prepare(PlayerSource);
                Constant.Player?.AddListener(PlayerListener);

                OnPrepared();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var mBandwidthMeter = new DefaultBandwidthMeter.Builder(GlobalContext).Build();
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(Constant.Context, Util.GetUserAgent(Constant.Context, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(Constant.Context, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(Constant.Context, AppSettings.ApplicationName), new DefaultBandwidthMeter.Builder(GlobalContext).Build()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(Constant.Context, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(Constant.Context, AppSettings.ApplicationName), null));
                int type = Util.InferContentType(uri, null);
                var src = type switch
                {
                    C.TypeSs => new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeDash => new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeHls => new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    C.TypeOther => new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    _ => new ProgressiveMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri)
                };
                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void SetBuffer(bool isBuffer)
        {
            try
            {
                if (isBuffer) return;
                GlobalContext?.RunOnUiThread(() =>
                {
                    GlobalContext?.SoundController?.SetProgress();
                    ChangePlayPauseIcon();
                    GlobalContext?.SoundController?.SeekUpdate();
                    GlobalContext?.SoundController?.RotateImageAlbum();
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetNext()
        {
            try
            {
                if (Constant.PlayPos < Constant.ArrayListPlay.Count - 1)
                    Constant.PlayPos += 1;
                else
                    Constant.PlayPos = 0;

                GlobalContext?.SoundController?.RotateImageAlbum();

                SetPlayAudio();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ChangePlayPauseIcon()
        {
            try
            {
                GlobalContext?.SoundController?.ChangePlayPauseIcon();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        [BroadcastReceiver]
        private class CallBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    string a = intent.GetStringExtra(TelephonyManager.ExtraState);
                    if (Constant.Player.PlayWhenReady)
                    {
                        if (a.Equals(TelephonyManager.ExtraStateOffhook) || a.Equals(TelephonyManager.ExtraStateRinging))
                        {
                            Constant.Player.PlayWhenReady = false;
                        }
                        else if (a.Equals(TelephonyManager.ExtraStateIdle))
                        {
                            Constant.Player.PlayWhenReady = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        [BroadcastReceiver]
        private class HeadPhoneBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    //if (Constant.Player.PlayWhenReady)
                    //{
                    //    GetPlayerService()?.TogglePlay();
                    //}
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        [BroadcastReceiver]
        private class MediaButtonIntentReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    string intentAction = intent.Action;
                    if (!Intent.ActionMediaButton.Equals(intentAction))
                    {
                        return;
                    }
                    var key = intent.GetParcelableExtra(Intent.ExtraKeyEvent);
                    if (key == null)
                    {
                        return;
                    }

                    if (key is KeyEvent keyEvent)
                    {
                        var action = keyEvent.Action;
                        if (action == KeyEventActions.Down)
                        {
                            // do something
                            if (GetPlayerService() != null)
                            {
                                Intent intentPause = new Intent(context, typeof(PlayerService));
                                intentPause.SetAction(ActionToggle);
                                ContextCompat.StartForegroundService(context, intent);
                            }
                        }
                        InvokeAbortBroadcast();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);

                }
            }
        }

        private class MyAudioFocusChangeListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
        {
            public void OnAudioFocusChange(AudioFocus focusChange)
            {
                try
                {
                    switch (focusChange)
                    {
                        case AudioFocus.Gain:
                        case AudioFocus.LossTransientCanDuck:
                            // Resume your media player here
                            break;
                        case AudioFocus.Loss:
                        case AudioFocus.LossTransient:
                            try
                            {
                                //if (Constant.Player.PlayWhenReady)
                                //{
                                //    GetPlayerService()?.TogglePlay();
                                //}
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class PlayerEvents : Java.Lang.Object, IPlayerEventListener
        {
            private static PlayerService PlayerController;

            public PlayerEvents(PlayerService controller)
            {
                PlayerController = controller;
            }

            public void OnLoadingChanged(bool isLoading)
            {

            }

            public void OnPlaybackParametersChanged(PlaybackParameters playbackParameters)
            {

            }

            public void OnPlayerError(ExoPlaybackException error)
            {
                try
                {
                    Constant.Player.PlayWhenReady = false;
                    PlayerController.SetBuffer(false);
                    PlayerController.ChangePlayPauseIcon();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                try
                {
                    if (playbackState == IPlayer.StateEnded)
                    {
                        PlayerController.OnCompletion();
                    }
                    else if (playbackState == IPlayer.StateReady && playWhenReady)
                    {

                    }

                    if (playWhenReady)
                    {
                        PlayerController.GlobalContext?.SetWakeLock();
                    }
                    else
                    {
                        PlayerController.GlobalContext?.OffWakeLock();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPositionDiscontinuity(int reason)
            {

            }

            public void OnRepeatModeChanged(int repeatMode)
            {

            }

            public void OnSeekProcessed()
            {

            }

            public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
            {

            }

            public void OnTimelineChanged(Timeline timeline, int reason)
            {

            }

            public void OnIsPlayingChanged(bool isPlaying)
            {

            }

            public void OnTracksChanged(TrackGroupArray trackGroups, TrackSelectionArray trackSelections)
            {

            }
        }

    }
}