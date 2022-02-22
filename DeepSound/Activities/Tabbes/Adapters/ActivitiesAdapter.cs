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
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.User;
using Java.Util;
using Refractored.Controls;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Tabbes.Adapters
{
    public class ActivitiesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<ActivitiesAdapterClickEventArgs> OnItemClick;
        public event EventHandler<ActivitiesAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        public ObservableCollection<ActivityDataObject> ActivityList = new ObservableCollection<ActivityDataObject>();
        private readonly SocialIoClickListeners ClickListeners;

        public ActivitiesAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                ClickListeners = new SocialIoClickListeners(context); 
                GlobalContext = HomeActivity.GetInstance(); 
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
                //Setup your layout here >> Style_RowSongsPlaylistView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ActivityView, parent, false);
                var vh = new ActivitiesAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is ActivitiesAdapterViewHolder holder)
                {
                    var item = ActivityList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        GlideImageLoader.LoadImage(ActivityContext, item.SThumbnail, holder.ImageSong, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                        holder.TxtName.Text = DeepSoundTools.GetNameFinal(item.UserData);
                        holder.TxtTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.ActivityText) + " " + item.ActivityTimeFormatted, 35);

                        if (item.TrackData?.TrackDataClass != null)
                        {
                            holder.SongLayout.Visibility = ViewStates.Visible;
                            
                            holder.TxtTitleSong.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.TrackData?.TrackDataClass.Title), 80);
                            holder.TxtTime.Text = item.TrackData?.TrackDataClass.Duration;
                              
                            if (!holder.MoreButton.HasOnClickListeners)
                                holder.MoreButton.Click += (sender, e) => ClickListeners.OnMoreClick(new MoreSongClickEventArgs {View = holder.MainView, SongsClass = item.TrackData?.TrackDataClass });
                        }
                        else
                        {
                            holder.SongLayout.Visibility = ViewStates.Gone;
                            holder.MoreButton.Visibility = ViewStates.Gone;
                        }

                        if (!holder.TxtTitle.HasOnClickListeners)
                            holder.TxtTitle.Click += (sender, e) => { GlobalContext?.OpenProfile(item.UserData.Id, item.UserData); }; 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => ActivityList?.Count ?? 0;

        public ActivityDataObject GetItem(int position)
        {
            return ActivityList[position];
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

        void Click(ActivitiesAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(ActivitiesAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ActivityList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.SThumbnail != "")
                {
                    d.Add(item.SThumbnail);
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

    public class ActivitiesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public CircleImageView ImageUser { get; private set; }
        public ImageView ImageSong { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtTitleSong { get; private set; }
        public TextView TxtTime { get; private set; }
        public ImageButton MoreButton { get; private set; }
        public RelativeLayout SongLayout { get; private set; }
         
        #endregion

        public ActivitiesAdapterViewHolder(View itemView, Action<ActivitiesAdapterClickEventArgs> clickListener, Action<ActivitiesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ImageUser = MainView.FindViewById<CircleImageView>(Resource.Id.imageUser);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.title);  
                MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);

                SongLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.songLayout);
                ImageSong = MainView.FindViewById<ImageView>(Resource.Id.imageSong);
                TxtTitleSong = MainView.FindViewById<TextView>(Resource.Id.titleSong); 
                TxtTime = MainView.FindViewById<TextView>(Resource.Id.time);

                MoreButton.Visibility = ViewStates.Visible;

                //Event
                itemView.Click += (sender, e) => clickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ActivitiesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}