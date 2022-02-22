using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo;
using DeepSoundClient.Classes.Global;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Songs.Adapters
{
    public class RowSoundAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public event EventHandler<RowSoundAdapterClickEventArgs> OnItemClick;
        public event EventHandler<RowSoundAdapterClickEventArgs> OnItemLongClick;

        public ObservableCollection<SoundDataObject> SoundsList = new ObservableCollection<SoundDataObject>();
        private readonly SocialIoClickListeners ClickListeners;
        private readonly string NamePage;

        public RowSoundAdapter(Activity context, string namePage)
        {
            try
            {
                ActivityContext = context;
                NamePage = namePage;
                HasStableIds = true;
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                ClickListeners = new SocialIoClickListeners(context);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SoundsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SongView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SongView, parent, false);
                var vh = new RowSoundAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is not RowSoundAdapterViewHolder holder) return;

                var item = SoundsList[position];
                if (item == null)
                    return;

                holder.CountItemTextView.Text = position.ToString("D2");

                Glide.With(ActivityContext).Load(item.Thumbnail).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).Error(Resource.Drawable.ImagePlacholder)).Into(holder.Image);

                holder.TxtSongName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Title), 60);
               
                if (item.Publisher != null)
                    holder.TxtGenresName.Text = item.CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music) + " - " + DeepSoundTools.GetNameFinal(item.Publisher);
                else
                    holder.TxtGenresName.Text = item.CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music);
                 
                holder.LikeButton.Tag = item.IsLiked != null && item.IsLiked.Value ? "Like" : "Liked";
                ClickListeners.SetLike(holder.LikeButton);

                //holder.CountLike.Text = item.CountLikes.ToString();
                //holder.CountStars.Text = item.CountFavorite.ToString(); 
                //holder.CountViews.Text = item.CountViews;
                //holder.CountShare.Text = item.CountShares.ToString();
                //holder.CountComment.Text = item.CountComment.ToString();

                holder.TxtSongDuration.Text = item.Duration;

                if (item.IsPlay)
                {
                    holder.Image.Visibility = ViewStates.Gone;
                    holder.CardViewImage.Visibility = ViewStates.Gone;
                    holder.Equalizer.Visibility = ViewStates.Visible;
                    holder.Equalizer.AnimateBars();
                }
                else
                {
                    holder.Image.Visibility = ViewStates.Visible;
                    holder.CardViewImage.Visibility = ViewStates.Visible;
                    holder.Equalizer.Visibility = ViewStates.Gone;
                    holder.Equalizer.StopBars();
                }

                if (!holder.MoreButton.HasOnClickListeners)
                    holder.MoreButton.Click += (sender, e) => ClickListeners.OnMoreClick(new MoreSongClickEventArgs { View = holder.MainView, SongsClass = item }, NamePage);

                holder.LikeButton.Click += (s, e)
                    => ((HomeActivity)ActivityContext).SoundController.ClickListeners.OnLikeSongsClick(new LikeSongsClickEventArgs { LikeButton = holder.LikeButton, SongsClass = item }, NamePage);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

        private void OnClick(RowSoundAdapterClickEventArgs args)
        {
            OnItemClick?.Invoke(this, args);
        }

        private void OnLongClick(RowSoundAdapterClickEventArgs args)
        {
            OnItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SoundsList[p0];

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

    public class RowSoundAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; private set; }
        public CardView CardViewImage { get; private set; }
        public ImageView Image { get; private set; }
        public EqualizerView Equalizer { get; private set; }
        public TextView TxtSongName { get; private set; }
        public TextView TxtGenresName { get; private set; }

        public TextView TxtSongDuration { get; private set; }
        public ImageButton MoreButton { get; private set; }

        public ImageView LikeButton { get; private set; }

        public TextView CountItemTextView { get; private set; }

        public RowSoundAdapterViewHolder(View itemView, Action<RowSoundAdapterClickEventArgs> clickListener, Action<RowSoundAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                CardViewImage = MainView.FindViewById<CardView>(Resource.Id.cardview2);
                Image = MainView.FindViewById<ImageView>(Resource.Id.imageView_songlist);
                Equalizer = MainView.FindViewById<EqualizerView>(Resource.Id.equalizer_view);
                TxtSongName = MainView.FindViewById<TextView>(Resource.Id.textView_songname);
                TxtGenresName = MainView.FindViewById<TextView>(Resource.Id.textView_catname);
                LikeButton = MainView.FindViewById<ImageView>(Resource.Id.likeImageview);


                TxtSongDuration = MainView.FindViewById<TextView>(Resource.Id.textView_songduration);

                MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);
                CountItemTextView = MainView.FindViewById<TextView>(Resource.Id.textView_count);




                //Event
                itemView.Click += (sender, e) => clickListener(new RowSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new RowSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class RowSoundAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}