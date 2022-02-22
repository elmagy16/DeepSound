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
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Blog;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Blog.Adapters
{
    public class BlogAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public Dictionary<string, string> CategoryColor = new Dictionary<string, string>();
        public ObservableCollection<ArticleDataObject> BlogList = new ObservableCollection<ArticleDataObject>();

        public BlogAdapter(Activity context)
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

        public override int ItemCount => BlogList?.Count ?? 0;

        public event EventHandler<BlogAdapterClickEventArgs> ItemClick;
        public event EventHandler<BlogAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Article_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ArticleView, parent, false);
                var vh = new BlogAdapterViewHolder(itemView, OnClick, OnLongClick);
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

                if (viewHolder is BlogAdapterViewHolder holder)
                {
                    var item = BlogList[position];
                    if (item != null) Initialize(holder, item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(BlogAdapterViewHolder holder, ArticleDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, !string.IsNullOrEmpty(item.Thumbnail) ? item.Thumbnail : "blackdefault", holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                holder.Title.Text = Methods.FunString.DecodeString(item.Title);
                holder.Time.Text = item.CreatedAt;

                holder.Category.Text = CategoriesController.Get_Translate_Categories_Communities(item.Category?.ToString(), "", "Blog");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public ArticleDataObject GetItem(int position)
        {
            return BlogList[position];
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

        private void OnClick(BlogAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(BlogAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = BlogList[p0];

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
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
        }
    }

    public class BlogAdapterViewHolder : RecyclerView.ViewHolder 
    {
        public BlogAdapterViewHolder(View itemView, Action<BlogAdapterClickEventArgs> clickListener, Action<BlogAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                Category = MainView.FindViewById<TextView>(Resource.Id.subtitle);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Time = MainView.FindViewById<TextView>(Resource.Id.date);
 
                //Event
                itemView.Click += (sender, e) => clickListener(new BlogAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new BlogAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Category { get; private set; }
        public TextView Time { get; private set; }

        #endregion
    }

    public class BlogAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}