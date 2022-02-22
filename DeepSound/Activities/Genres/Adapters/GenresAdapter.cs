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
using DeepSoundClient.Classes.User;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Genres.Adapters
{
    public class GenresAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GenresAdapterClickEventArgs> OnItemClick;
        public event EventHandler<GenresAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GenresObject.DataGenres> GenresList = new ObservableCollection<GenresObject.DataGenres>();
        private readonly RequestBuilder FullGlideRequestBuilder;

        public GenresAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;

                var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High).Apply(RequestOptions.CircleCropTransform().CenterCrop().CircleCrop());
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                //Setup your layout here >> Style_GenresSoundView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GenresSoundView, parent, false);
                var vh = new GenresAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is GenresAdapterViewHolder holder)
                {
                    var item = GenresList[position];
                    if (item != null)
                    {
                        FullGlideRequestBuilder.Load(item.BackgroundThumb).Into(holder.GenresImage);
                        //GlideImageLoader.LoadImage(ActivityContext, item.BackgroundThumb, holder.GenresImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        //holder.GenresImage.ClearColorFilter();
                        //holder.GenresImage.SetColorFilter(Color.ParseColor(item.Color), PorterDuff.Mode.Lighten);

                        holder.TxtName.Text = item.CateogryName;

                        //if (item.Checked)
                        //{ 
                        //    holder.TxtCheck.Visibility = ViewStates.Visible;
                        //    holder.TxtName.Visibility = ViewStates.Gone; 
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => GenresList?.Count ?? 0;

        public GenresObject.DataGenres GetItem(int position)
        {
            return GenresList[position];
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

        void Click(GenresAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(GenresAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GenresList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.BackgroundThumb != "")
                {
                    d.Add(item.BackgroundThumb);
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class GenresAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView GenresImage { get; private set; }
        public TextView TxtName { get; private set; }
      

        #endregion

        public GenresAdapterViewHolder(View itemView, Action<GenresAdapterClickEventArgs> clickListener, Action<GenresAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                GenresImage = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.titleText);

                //Event
                itemView.Click += (sender, e) => clickListener(new GenresAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GenresAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class GenresAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; } 
    }
}