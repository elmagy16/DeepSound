using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Stations.Adapters
{
    public class StationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<StationsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<StationsAdapterClickEventArgs> OnItemLongClick;
        
        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        public ObservableCollection<SoundDataObject> StationsList = new ObservableCollection<SoundDataObject>();
        private readonly SocialIoClickListeners ClickListeners;
        private readonly RequestBuilder FullGlideRequestBuilder;

        public StationsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                ClickListeners = new SocialIoClickListeners(context);
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StationsView, parent, false);
                var vh = new StationsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is StationsAdapterViewHolder holder)
                {
                    var item = StationsList[position];
                    if (item != null)
                    {                         
                        FullGlideRequestBuilder.Load(item.Thumbnail).Into(holder.Image);
                        holder.TxtName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Title), 60);
                        holder.TxtCat.Text = ActivityContext.GetText(Resource.String.Lbl_Genres) + ": " + item.CategoryName;
                        holder.TxtCountry.Text = ActivityContext.GetText(Resource.String.Lbl_Country) + ": " + item.Description;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StationsList?.Count ?? 0;

        public SoundDataObject GetItem(int position)
        {
            return StationsList[position];
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

        void Click(StationsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(StationsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StationsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
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

    public class StationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image  { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtCat { get; private set; } 
        public TextView TxtCountry { get; private set; } 
         
        #endregion

        public StationsAdapterViewHolder(View itemView, Action<StationsAdapterClickEventArgs> clickListener, Action<StationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);
                TxtCat = MainView.FindViewById<TextView>(Resource.Id.cat);
                TxtCountry = MainView.FindViewById<TextView>(Resource.Id.country); 

                //Event
                itemView.Click += (sender, e) => clickListener(new StationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class StationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}