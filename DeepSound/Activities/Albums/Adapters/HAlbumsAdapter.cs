using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Albums.Adapters
{
    public class HAlbumsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public event EventHandler<HAlbumsAdapterClickEventArgs> ItemClick;
        public event EventHandler<HAlbumsAdapterClickEventArgs> ItemLongClick;

        public ObservableCollection<DataAlbumsObject> AlbumsList = new ObservableCollection<DataAlbumsObject>();

        public HAlbumsAdapter(Activity context)
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

        public override int ItemCount => AlbumsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HorizontalSoundView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HAlbumsView, parent, false);
                var vh = new HAlbumsAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is not HAlbumsAdapterViewHolder holder) return;

                var item = AlbumsList[position];
               
                if (item == null)
                    return;

                Glide.With(ActivityContext).Load(item.Thumbnail).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).Error(Resource.Drawable.ImagePlacholder)).Into(holder.Image);

                holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);

                if (Math.Abs(item.Price) > 0)
                {
                    holder.Badge3.Visibility = ViewStates.Visible;
                    var currencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                    var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == item.Price)?.Price ?? item.Price.ToString();

                    holder.Badge3.Text =  currencySymbol + price;
                }

                var count = !string.IsNullOrEmpty(item.CountSongs) ? item.CountSongs : item.SongsCount ?? "0";

                holder.Badge2.Text = count + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);
                 
                holder.TxtSecondaryText.Text = DeepSoundTools.GetNameFinal(item.Publisher ?? item.UserData);
                holder.TxtCountSound.Text = item.CategoryName;

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public DataAlbumsObject GetItem(int position)
        {
            return AlbumsList[position];
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

        private void OnClick(HAlbumsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(HAlbumsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = AlbumsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);
                 
                if (!string.IsNullOrEmpty(item.Thumbnail))
                {
                    d.Add(item.Thumbnail);
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

    public class HAlbumsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSecondaryText { get; private set; }
        public TextView TxtCountSound { get; private set; }
        public TextView Badge2 { get; private set; }

        public TextView Badge3 { get; private set; }

        #endregion

        public HAlbumsAdapterViewHolder(View itemView, Action<HAlbumsAdapterClickEventArgs> clickListener, Action<HAlbumsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.imageSound);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.titleTextView);
                TxtSecondaryText = MainView.FindViewById<TextView>(Resource.Id.seconderyText);
                TxtCountSound = MainView.FindViewById<TextView>(Resource.Id.image_countSound);
                Badge2 = MainView.FindViewById<TextView>(Resource.Id.badge2);
                Badge3 = MainView.FindViewById<TextView>(Resource.Id.badge3);

                //Event
                itemView.Click += (sender, e) => clickListener(new HAlbumsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition,Image = Image });
                itemView.LongClick += (sender, e) => longClickListener(new HAlbumsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = Image });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }

    public class HAlbumsAdapterClickEventArgs : EventArgs
    {
        public ImageView Image { get; set; }
        public View View { get; set; }
        public int Position { get; set; }
    }
}