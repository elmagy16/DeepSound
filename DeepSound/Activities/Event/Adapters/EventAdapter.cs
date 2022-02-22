using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Event;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Event.Adapters
{
    public class EventAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<EventAdapterClickEventArgs> ItemClick;
        public event EventHandler<EventAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<EventDataObject> EventsList = new ObservableCollection<EventDataObject>();

        public EventAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => EventsList?.Count ?? 0;


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_EventView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EventView, parent, false);
                var vh = new EventAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case EventAdapterViewHolder holder:
                    {
                        var item = EventsList[position];
                        if (item != null) Initialize(holder, item);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(EventAdapterViewHolder holder, EventDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                holder.TxtEventTitle.Text = Methods.FunString.DecodeString(item.Name);
                holder.TxtEventDescription.Text = Methods.FunString.DecodeString(item.Desc);

                if (!string.IsNullOrEmpty(item.OnlineUrl))
                {
                    holder.TxtEventLocation.Text = item.OnlineUrl;
                }
                else if (!string.IsNullOrEmpty(item.RealAddress))
                {
                    holder.TxtEventLocation.Text = item.RealAddress;
                }
                
                holder.TxtEventTime.Text = item.EndDate;

                item.IsOwner = item?.UserId == UserDetails.UserId;
                if (item?.UserId == UserDetails.UserId)
                {
                    holder.TxtEventType.Text = ActivityContext.GetText(Resource.String.Lbl_MyEvent);
                    holder.TxtEventType.Background.SetTint(Color.ParseColor("#E70000"));
                }
                else
                {
                    if (item.IsJoined == 1)
                    {
                        holder.TxtEventType.Text = ActivityContext.GetText(Resource.String.Lbl_Joined);
                        holder.TxtEventType.Background.SetTint(Color.ParseColor("#02BE10"));
                    }
                    else  
                    {
                        holder.TxtEventType.Text = ActivityContext.GetText(Resource.String.Lbl_Event);
                        holder.TxtEventType.Background.SetTint(Color.ParseColor("#F18D05"));
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case EventAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public EventDataObject GetItem(int position)
        {
            return EventsList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                 return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(EventAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(EventAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = EventsList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                switch (string.IsNullOrEmpty(item.Image))
                {
                    case false:
                        d.Add(item.Image);
                        break;
                }
                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }


        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class EventAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }
        public ImageView Image { get; private set; }
        public TextView TxtEventTitle { get; private set; }
        public TextView TxtEventDescription { get; private set; }
        public TextView TxtEventTime { get; private set; }
        public TextView TxtEventLocation { get; private set; }
        public TextView TxtEventType { get; private set; }
        public CardView PostLinkLinearLayout { get; private set; }

        #endregion  

        public EventAdapterViewHolder(View itemView, Action<EventAdapterClickEventArgs> clickListener,Action<EventAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = itemView.FindViewById<ImageView>(Resource.Id.Image);
                TxtEventTitle = itemView.FindViewById<TextView>(Resource.Id.event_titile);
                TxtEventDescription = itemView.FindViewById<TextView>(Resource.Id.event_description);
                TxtEventTime = itemView.FindViewById<TextView>(Resource.Id.event_time);
                TxtEventLocation = itemView.FindViewById<TextView>(Resource.Id.event_location);
                TxtEventType = itemView.FindViewById<TextView>(Resource.Id.event_type);
                PostLinkLinearLayout = itemView.FindViewById<CardView>(Resource.Id.card_view);

                //Event
                itemView.Click += (sender, e) => clickListener(new EventAdapterClickEventArgs{ View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new EventAdapterClickEventArgs{ View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }

    public class EventAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}