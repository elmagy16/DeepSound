using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Comments;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Comments.Adapters
{
    public class CommentsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CommentAdapterClickEventArgs> OnAvatarClick;
        public event EventHandler<CommentAdapterClickEventArgs> OnLikeClick;
        public event EventHandler<CommentAdapterClickEventArgs> OnItemClick;
        public event EventHandler<CommentAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        private readonly string Type;
        public ObservableCollection<CommentsDataObject> CommentList = new ObservableCollection<CommentsDataObject>();

        public CommentsAdapter(Activity context , string type)
        {
            try
            {
                ActivityContext = context;
                Type = type;
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
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CommentView, parent, false); 
                var vh = new CommentAdapterViewHolder(itemView, Click, AvatarClick, LikeClick, LongClick);
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
                if (viewHolder is CommentAdapterViewHolder holder)
                {
                    var item = CommentList[position];
                    if (item != null)
                    {
                        if (Type == "Blog")
                            GlideImageLoader.LoadImage(ActivityContext, item.UserDataBlog?.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        else
                            GlideImageLoader.LoadImage(ActivityContext, item.UserData?.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        TextSanitizer changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Value));

                        holder.TimeTextView.Text = item.SecondsFormated;

                        holder.LikeNumber.Text = Methods.FunString.FormatPriceValue(item.CountLiked);

                        holder.LikeiconView.Tag = item.IsLikedComment != null && item.IsLikedComment.Value ? "Like" : "Liked";
                        SetLike(holder.LikeiconView);

                        holder.LikeButton.Tag = item.IsLikedComment != null && item.IsLikedComment.Value ? "1" : "0";
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetLike(TextView likeButton)
        {
            try
            {
                if (likeButton?.Tag?.ToString() == "Liked")
                {
                    likeButton.SetTextColor(Color.White);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeartEmpty);
                    likeButton.Tag = "Like";
                }
                else
                {
                    likeButton.SetTextColor(Color.ParseColor("#ed4856"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeart);
                    likeButton.Tag = "Liked";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public override int ItemCount => CommentList?.Count ?? 0;

        public CommentsDataObject GetItem(int position)
        {
            return CommentList[position];
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

        void LikeClick(CommentAdapterClickEventArgs args) => OnLikeClick?.Invoke(this, args);
        void AvatarClick(CommentAdapterClickEventArgs args) => OnAvatarClick?.Invoke(this, args);
        void Click(CommentAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(CommentAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (Type == "Blog" && item.UserDataBlog?.Avatar != "")
                {
                    d.Add(item.UserDataBlog?.Avatar);
                    return d;
                }
                else if (item.UserData?.Avatar != "")
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class CommentAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public SuperTextView CommentText { get; private set; }

        public TextView TimeTextView { get; private set; }

        public TextView LikeiconView { get; private set; }
        public TextView LikeNumber { get; private set; }
        public LinearLayout LikeButton { get; private set; }
     
        #endregion

        public CommentAdapterViewHolder(View itemView, Action<CommentAdapterClickEventArgs> clickListener,Action<CommentAdapterClickEventArgs> avatarClickListener,Action<CommentAdapterClickEventArgs> likeClickListener, Action<CommentAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<SuperTextView>(Resource.Id.active);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                LikeiconView = MainView.FindViewById<TextView>(Resource.Id.Likeicon);
                LikeNumber = MainView.FindViewById<TextView>(Resource.Id.LikeNumber);
                LikeButton = MainView.FindViewById<LinearLayout>(Resource.Id.LikeButton);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LikeiconView, IonIconsFonts.IosHeartEmpty);

                //Event
                LikeButton.Click += (sender, e) => likeClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Holder = this });
                Image.Click += (sender, e) => avatarClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Holder = this });
                itemView.Click += (sender, e) => clickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Holder = this });
                itemView.LongClick += (sender, e) => longClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Holder = this });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class CommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public CommentAdapterViewHolder Holder { get; set; }
    } 
}