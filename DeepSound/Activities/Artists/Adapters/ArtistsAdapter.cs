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
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Artists.Adapters
{
    public class ArtistsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<ArtistsAdapterClickEventArgs> OnItemClick;
        //public event EventHandler<ArtistsAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> ArtistsList = new ObservableCollection<UserDataObject>();

        public ArtistsAdapter(Activity context)
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
                //Setup your layout here >> Style_ArtistsView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ArtistsView, parent, false);
                var vh = new ArtistsAdapterViewHolder(itemView, Click);
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
                if (viewHolder is ArtistsAdapterViewHolder holder)
                {
                    var item = ArtistsList[position];
                    if (item != null)
                    {
                        holder.Name.Text = Methods.FunString.SubStringCutOf(DeepSoundTools.GetNameFinal(item), 20);

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        holder.Verified.Visibility = item.Verified == 1 ? ViewStates.Visible : ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => ArtistsList?.Count ?? 0;

        public UserDataObject GetItem(int position)
        {
            return ArtistsList[position];
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

        void Click(ArtistsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
       // void LongClick(ArtistsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ArtistsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
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

    public class ArtistsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        private View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Verified { get; private set; }
        public LinearLayout MainLayout { get; private set; }

        #endregion

        public ArtistsAdapterViewHolder(View itemView, Action<ArtistsAdapterClickEventArgs> clickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLayout = MainView.FindViewById<LinearLayout>(Resource.Id.mainlayout);
                Image = MainView.FindViewById<ImageView>(Resource.Id.people_profile_sos);
                Name = MainView.FindViewById<TextView>(Resource.Id.people_profile_name);
                Verified = MainView.FindViewById<TextView>(Resource.Id.verified);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Verified, IonIconsFonts.CheckmarkCircle);

                //Event
                itemView.Click += (sender, e) => clickListener(new ArtistsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = Image });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ArtistsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public ImageView Image { get; set; }

    }
}