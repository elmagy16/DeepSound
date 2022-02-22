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
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Songs.Adapters
{
    public class HSoundAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<HSoundAdapterClickEventArgs> OnItemClick;
        public event EventHandler<HSoundAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<SoundDataObject> SoundsList = new ObservableCollection<SoundDataObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly RequestOptions GlideRequestOptions;
        public HSoundAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                //Setup your layout here >> Style_HorizontalSound
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HorizontalSoundView, parent, false);
                var vh = new HSoundAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is HSoundAdapterViewHolder holder)
                {
                    var item = SoundsList[position];
                    if (item != null)
                    {
                        FullGlideRequestBuilder.Load(item.Thumbnail).Into(holder.Image);

                        holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);
                        holder.TxtSeconderyText.Text = item.CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music) + " - " + DeepSoundTools.GetNameFinal(item.Publisher);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SoundsList?.Count ?? 0;

        public SoundDataObject GetItem(int position)
        {
            return SoundsList[position];
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

        void Click(HSoundAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(HSoundAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SoundsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);
                 
                d.Add(item.Thumbnail);
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
            return FullGlideRequestBuilder.Load(p0.ToString());
        }

    }

    public class HSoundAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSeconderyText { get; private set; }

        #endregion

        public HSoundAdapterViewHolder(View itemView, Action<HSoundAdapterClickEventArgs> clickListener, Action<HSoundAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.imageSound);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.titleTextView);
                TxtSeconderyText = MainView.FindViewById<TextView>(Resource.Id.seconderyText);

                //Event
                itemView.Click += (sender, e) => clickListener(new HSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new HSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class HSoundAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; } 
    }
}