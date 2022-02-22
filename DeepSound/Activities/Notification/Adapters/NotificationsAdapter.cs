using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Notification.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<NotificationsObject.Notifiation> NotificationsList = new ObservableCollection<NotificationsObject.Notifiation>();

        public NotificationsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Notifications_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_NotificationView, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is NotificationsAdapterViewHolder holder)
                {
                    var item = NotificationsList[position];
                    if (item != null)
                    {
                        if (item.UserData?.UserDataClass != null)
                        {
                            holder.UserNameNoitfy.Text = DeepSoundTools.GetNameFinal(item.UserData?.UserDataClass);

                            GlideImageLoader.LoadImage(ActivityContext, item.UserData?.UserDataClass.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        }

                        if (item.NType == "your_song_is_ready")
                        {
                            holder.UserNameNoitfy.Text = AppSettings.ApplicationName;
                            Glide.With(ActivityContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                        }
                         
                        switch (item.NType)
                        {
                            case "follow_user":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.PersonAdd)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.PersonAdd);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_FollowUser);
                                break;
                            }
                            case "liked_track":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.ThumbsUp)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.ThumbsUp);
                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_LikedTrack);
                                break;
                            }
                            case "liked_comment":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Pricetag)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Pricetag);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_LikedComment);
                                break;
                            }
                            case "purchased":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Cash)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Cash);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_PurchasedYourSong);
                                break;
                            }
                            case "approved_artist":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.CheckmarkCircle)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.CheckmarkCircle);

                                holder.Description.Text = ActivityContext.GetText(Resource.String.Lbl_ApprovedArtist);
                                break;
                            }
                            case "decline_artist":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Sad)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Sad);

                                holder.Description.Text =ActivityContext.GetText(Resource.String.Lbl_DeclineArtist);
                                break;
                            }
                            case "new_track":
                            {
                                if (holder.IconNotify.Text != IonIconsFonts.Add)
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify,IonIconsFonts.Add);

                                holder.Description.Text =ActivityContext.GetText(Resource.String.Lbl_UploadNewTrack);
                                break;
                            }
                            default:
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotify, IonIconsFonts.Notifications);
                                holder.Description.Text = item.NText;
                                break;
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => NotificationsList?.Count ?? 0;

        public NotificationsObject.Notifiation GetItem(int position)
        {
            return NotificationsList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        void Click(NotificationsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(NotificationsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.UserData?.UserDataClass?.Avatar))
                {
                    d.Add(item.UserData?.UserDataClass.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        } 
    }

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView ImageUser { get; private set; }
        public View CircleIcon { get; set; }
        public TextView IconNotify { get; private set; }
        public TextView UserNameNoitfy { get; private set; }
        public TextView Description { get; private set; }

        #endregion

        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener, Action<NotificationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
                CircleIcon = MainView.FindViewById<View>(Resource.Id.CircleIcon);
                IconNotify = (TextView)MainView.FindViewById(Resource.Id.IconNotifications);
                UserNameNoitfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
                Description = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);
                 
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = ImageUser });
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = ImageUser });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public ImageView Image { get; set; }
    }
}