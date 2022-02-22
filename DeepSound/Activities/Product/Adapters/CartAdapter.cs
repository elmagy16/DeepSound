using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Product;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Product.Adapters
{
    public class CartAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CartAdapterClickEventArgs> OnRemoveButtonItemClick;
        public event EventHandler<CartAdapterClickEventArgs> OnItemClick;
        public event EventHandler<CartAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<CartDataObject> CartsList = new ObservableCollection<CartDataObject>();

        public CartAdapter(Activity context)
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
                //Setup your layout here >> Style_CartView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CartView, parent, false);
                var vh = new CartAdapterViewHolder(itemView, RemoveButtonClick, Click, LongClick);
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
                if (viewHolder is CartAdapterViewHolder holder)
                {
                    var item = CartsList[position];
                    if (item?.Product != null)
                    {
                        var image = item.Product.Images.FirstOrDefault()?.Image;
                        GlideImageLoader.LoadImage(ActivityContext, image, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                      
                        holder.Name.Text = Methods.FunString.DecodeString(item.Product.Title);
                         
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CartsList?.Count ?? 0;

        public CartDataObject GetItem(int position)
        {
            return CartsList[position];
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

        void RemoveButtonClick(CartAdapterClickEventArgs args) => OnRemoveButtonItemClick?.Invoke(this, args);
        void Click(CartAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(CartAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CartsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                var image = item.Product?.Images.FirstOrDefault()?.Image;
                if (!string.IsNullOrEmpty(image))
                {
                    d.Add(image);
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

    public class CartAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public CircleButton RemoveButton { get; private set; }

        #endregion

        public CartAdapterViewHolder(View itemView, Action<CartAdapterClickEventArgs> removeButtonClickListener,  Action<CartAdapterClickEventArgs> clickListener, Action<CartAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                RemoveButton = MainView.FindViewById<CircleButton>(Resource.Id.remove_cartButton);

                //Create an Event
                RemoveButton.Click += (sender, e) => removeButtonClickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

                itemView.Click += (sender, e) => clickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class CartAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}