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
using DeepSound.Activities.Library.Listeners;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Playlist.Adapters
{
    public class HPlaylistAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<PlaylistAdapterClickEventArgs> OnItemClick;
        public event EventHandler<PlaylistAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PlaylistDataObject> PlaylistList = new ObservableCollection<PlaylistDataObject>();
        private readonly bool ShowMore;
        private readonly LibrarySynchronizer LibrarySynchronizer;
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly bool IsShowingForSearch;

        public HPlaylistAdapter(Activity context, bool showMore = false, bool isShowingForSearch = false)
        {
            try
            {
                IsShowingForSearch = isShowingForSearch;
                ActivityContext = context;
                HasStableIds = true;
                ShowMore = showMore;
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
                LibrarySynchronizer = new LibrarySynchronizer(context);
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
                //Setup your layout here >> Style_Playlist
                var layoutId = IsShowingForSearch ? Resource.Layout.search_playlist_item : Resource.Layout.Style_PlaylistView;
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(layoutId, parent, false);
                var vh = new PlaylistAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is PlaylistAdapterViewHolder holder)
                {
                    var item = PlaylistList[position];
                    if (item != null)
                    {
                        FullGlideRequestBuilder.Load(item.ThumbnailReady).Into(holder.Image);
                        holder.TxtName.Text = Methods.FunString.DecodeString(item.Name);
                        holder.TxtCountSongs.Text = item.Songs + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);

                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PlaylistList?.Count ?? 0;

        public PlaylistDataObject GetItem(int position)
        {
            return PlaylistList[position];
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

        void Click(PlaylistAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(PlaylistAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PlaylistList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.ThumbnailReady != "")
                {
                    d.Add(item.ThumbnailReady);
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
            return FullGlideRequestBuilder.Load(p0.ToString());
        }

    }

    public class PlaylistAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtCountSongs { get; private set; }


        #endregion

        public PlaylistAdapterViewHolder(View itemView, Action<PlaylistAdapterClickEventArgs> clickListener, Action<PlaylistAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);
                //  TxtUserName = MainView.FindViewById<TextView>(Resource.Id.UserName);
                TxtCountSongs = MainView.FindViewById<TextView>(Resource.Id.count);
                // TxtIconCount = MainView.FindViewById<TextView>(Resource.Id.IconCount);
                // MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);

                // FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtIconCount, IonIconsFonts.IosMusicalNotes);

                //Event
                itemView.Click += (sender, e) => clickListener(new PlaylistAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PlaylistAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class PlaylistAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}