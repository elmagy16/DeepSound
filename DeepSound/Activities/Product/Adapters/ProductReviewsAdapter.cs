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
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Product;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Product.Adapters
{
    internal class ProductReviewsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<ProductReviewsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<ProductReviewsAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<ProductReviewsDataObject> ReviewsList = new ObservableCollection<ProductReviewsDataObject>();

        public ProductReviewsAdapter(Activity context)
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
                //Setup your layout here >> Style_ReviewsView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ReviewsView, parent, false);
                var vh = new ProductReviewsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is ProductReviewsAdapterViewHolder holder)
                {
                    var item = ReviewsList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        holder.Name.Text = DeepSoundTools.GetNameFinal(item.UserData);

                        switch (string.IsNullOrEmpty(item.Review))
                        {
                            case false:
                                holder.Review.Text = Methods.FunString.DecodeString(item.Review);
                                holder.Review.Visibility = ViewStates.Visible;
                                break;
                            default:
                                holder.Review.Visibility = ViewStates.Gone;
                                break;
                        }

                        holder.Count.Text = item.Star?.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => ReviewsList?.Count ?? 0;

        public ProductReviewsDataObject GetItem(int position)
        {
            return ReviewsList[position];
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

        void Click(ProductReviewsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(ProductReviewsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ReviewsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.UserData?.Avatar))
                {
                    d.Add(item.UserData?.Avatar);
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

    public class ProductReviewsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Count { get; private set; }
        public TextView Review { get; private set; }

        #endregion

        public ProductReviewsAdapterViewHolder(View itemView, Action<ProductReviewsAdapterClickEventArgs> clickListener, Action<ProductReviewsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Count = MainView.FindViewById<TextView>(Resource.Id.count);
                Review = MainView.FindViewById<TextView>(Resource.Id.review);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new ProductReviewsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ProductReviewsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ProductReviewsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}