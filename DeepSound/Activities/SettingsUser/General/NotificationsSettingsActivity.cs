using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class NotificationsSettingsActivity : BaseActivity
    {
        #region Variables Basic

        private LinearLayout FollowUserLayout, LikedTrackLayout, LikedCommentLayout, ArtistStatusChangedLayout, ReceiptStatusChangedLayout, NewTrackLayout, CommentMentionLayout, CommentReplayMentionLayout;
        private Switch SwitchFollowUser, SwitchLikedTrack, SwitchLikedComment, SwitchArtistStatusChanged, SwitchReceiptStatusChanged, SwitchNewTrack, SwitchCommentMention, SwitchCommentReplayMention;
    
        private Toolbar Toolbar; 
        private PublisherAdView PublisherAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                // Create your application here
                SetContentView(Resource.Layout.SettingsNotificationsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                GetMyInfoData();
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
                PublisherAdView?.Resume();
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
                base.OnPause();
                AddOrRemoveEvent(false);
                PublisherAdView?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();
                base.OnDestroy();
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
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                FollowUserLayout = FindViewById<LinearLayout>(Resource.Id.follow_userLayout);
                SwitchFollowUser = FindViewById<Switch>(Resource.Id.Switch_follow_user);

                LikedTrackLayout = FindViewById<LinearLayout>(Resource.Id.liked_trackLayout);
                SwitchLikedTrack = FindViewById<Switch>(Resource.Id.Switch_liked_track);

                LikedCommentLayout = FindViewById<LinearLayout>(Resource.Id.liked_commentLayout);
                SwitchLikedComment = FindViewById<Switch>(Resource.Id.Switch_liked_comment);

                ArtistStatusChangedLayout = FindViewById<LinearLayout>(Resource.Id.artist_status_changedLayout);
                SwitchArtistStatusChanged = FindViewById<Switch>(Resource.Id.Switch_artist_status_changed);

                ReceiptStatusChangedLayout = FindViewById<LinearLayout>(Resource.Id.artist_status_changedLayout);
                SwitchReceiptStatusChanged = FindViewById<Switch>(Resource.Id.Switch_artist_status_changed);

                NewTrackLayout = FindViewById<LinearLayout>(Resource.Id.new_trackLayout);
                SwitchNewTrack = FindViewById<Switch>(Resource.Id.Switch_new_track);

                CommentMentionLayout = FindViewById<LinearLayout>(Resource.Id.comment_mentionLayout);
                SwitchCommentMention = FindViewById<Switch>(Resource.Id.Switch_comment_mention);

                CommentReplayMentionLayout = FindViewById<LinearLayout>(Resource.Id.comment_replay_mentionLayout);
                SwitchCommentReplayMention = FindViewById<Switch>(Resource.Id.Switch_comment_replay_mention);
                  
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_Notifications);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    SwitchFollowUser.CheckedChange += SwitchFollowUserOnCheckedChange;
                    SwitchLikedTrack.CheckedChange += SwitchLikedTrackOnCheckedChange;
                    SwitchLikedComment.CheckedChange += SwitchLikedCommentOnCheckedChange;
                    SwitchArtistStatusChanged.CheckedChange += SwitchArtistStatusChangedOnCheckedChange;
                    SwitchReceiptStatusChanged.CheckedChange += SwitchReceiptStatusChangedOnCheckedChange;
                    SwitchNewTrack.CheckedChange += SwitchNewTrackOnCheckedChange;
                    SwitchCommentMention.CheckedChange += SwitchCommentMentionOnCheckedChange;
                    SwitchCommentReplayMention.CheckedChange += SwitchCommentReplayMentionOnCheckedChange;
                }
                else
                {
                    SwitchFollowUser.CheckedChange -= SwitchFollowUserOnCheckedChange;
                    SwitchLikedTrack.CheckedChange -= SwitchLikedTrackOnCheckedChange;
                    SwitchLikedComment.CheckedChange -= SwitchLikedCommentOnCheckedChange;
                    SwitchArtistStatusChanged.CheckedChange -= SwitchArtistStatusChangedOnCheckedChange;
                    SwitchReceiptStatusChanged.CheckedChange -= SwitchReceiptStatusChangedOnCheckedChange;
                    SwitchNewTrack.CheckedChange -= SwitchNewTrackOnCheckedChange;
                    SwitchCommentMention.CheckedChange -= SwitchCommentMentionOnCheckedChange;
                    SwitchCommentReplayMention.CheckedChange -= SwitchCommentReplayMentionOnCheckedChange;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private string EmailOnFollowUserPref, EmailOnLikedTrackPref, EmailOnLikedCommentPref, EmailOnArtistStatusChangedPref, EmailOnReceiptStatusChangedPref, EmailOnNewTrackPref, EmailOnCommentMentionPref, EmailOnCommentReplayMentionPref;
        private void SwitchFollowUserOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault(); 
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnFollowUser = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnFollowUserPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnFollowUser = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser); 
                            }

                            EmailOnFollowUserPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_follow_user", EmailOnFollowUserPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString() , dataNotification) });
                }
                else
                {
                    Toast.MakeText(this,GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchLikedTrackOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                    {
                        if (dataUser != null)
                        {
                            dataUser.EmailOnLikedTrack = 1;
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                        }

                        EmailOnLikedTrackPref = "1";
                        break;
                    }
                    //No >> value = 0
                    default:
                    {
                        if (dataUser != null)
                        {
                            dataUser.EmailOnLikedTrack = 0;
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                        }

                        EmailOnLikedTrackPref = "0";
                        break;
                    }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void SwitchLikedCommentOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedComment = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnLikedCommentPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedComment = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnLikedCommentPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchArtistStatusChangedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnArtistStatusChanged = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnArtistStatusChangedPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnArtistStatusChanged = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnArtistStatusChangedPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchReceiptStatusChangedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReceiptStatusChanged = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnReceiptStatusChangedPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReceiptStatusChanged = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnReceiptStatusChangedPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_receipt_status_changed", EmailOnReceiptStatusChangedPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchNewTrackOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnNewTrack = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnNewTrackPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnNewTrack = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnNewTrackPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_new_track", EmailOnNewTrackPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCommentMentionOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentMention = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnCommentMentionPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentMention = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnCommentMentionPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_comment_mention", EmailOnCommentMentionPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCommentReplayMentionOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentReplayMention = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnCommentReplayMentionPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentReplayMention = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnCommentReplayMentionPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_comment_replay_mention", EmailOnCommentReplayMentionPref},
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (dataUser != null)
                {  
                    SwitchFollowUser.Checked = dataUser.EmailOnFollowUser == 1;
                    SwitchLikedTrack.Checked = dataUser.EmailOnLikedTrack == 1;
                    SwitchLikedComment.Checked = dataUser.EmailOnLikedComment == 1;
                    SwitchArtistStatusChanged.Checked = dataUser.EmailOnArtistStatusChanged == 1;
                    SwitchReceiptStatusChanged.Checked = dataUser.EmailOnReceiptStatusChanged == 1;
                    SwitchNewTrack.Checked = dataUser.EmailOnNewTrack == 1;
                    SwitchCommentMention.Checked = dataUser.EmailOnCommentMention == 1;
                    SwitchCommentReplayMention.Checked = dataUser.EmailOnCommentReplayMention == 1; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}