using System;
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
using DeepSoundClient.Classes.Global;
using Java.IO;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Playlist.Adapters
{ 
    public class AttachmentsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AttachmentsAdapterClickEventArgs> DeleteItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<AttachmentsObject> AttachmentList = new ObservableCollection<AttachmentsObject>();
        public AttachmentsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => AttachmentList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Attachment_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AttachmentView, parent, false);
                var vh = new AttachmentsAdapterViewHolder(itemView, DeleteClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                switch (viewHolder)
                {
                    case AttachmentsAdapterViewHolder holder:
                    {
                        var item = AttachmentList[position];
                        if (item != null)
                        {
                            switch (item.TypeAttachment)
                            {
                                case "Default":
                                    Glide.With(ActivityContext).Load(Resource.Drawable.addImage).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                                    break;
                                default:
                                {
                                    if (item.FileSimple.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(ActivityContext, item.FileSimple, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    } 
                                    else
                                    {
                                        Glide.With(ActivityContext).Load(new File(item.FileUrl)).Apply(new RequestOptions()).Into(holder.Image);
                                    }

                                    break;
                                }
                            }

                            holder.ImageDelete.Visibility = item.TypeAttachment == "Default" ? ViewStates.Invisible : ViewStates.Visible;
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case AttachmentsAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Function 
        public void Add(AttachmentsObject item)
        {
            try
            {
                AttachmentList.Add(item);
                NotifyItemInserted(AttachmentList.IndexOf(AttachmentList.Last()));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Remove(AttachmentsObject item)
        {
            try
            {
                var index = AttachmentList.IndexOf(AttachmentList.FirstOrDefault(a => a.Id == item.Id));
                if (index != -1)
                {
                    AttachmentList.Remove(item);
                    NotifyItemRemoved(index);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void RemoveAll()
        {
            try
            {
                AttachmentList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public AttachmentsObject GetItem(int position)
        {
            return AttachmentList[position];
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
        
        private void DeleteClick(AttachmentsAdapterClickEventArgs args)
        {
            DeleteItemClick?.Invoke(this, args);
        }

        private void Click(AttachmentsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(AttachmentsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class AttachmentsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
          
        public ImageView Image { get; private set; }
        public CircleButton ImageDelete { get; private set; }

        #endregion

        public AttachmentsAdapterViewHolder(View itemView, Action<AttachmentsAdapterClickEventArgs> clickDeleteListener,Action<AttachmentsAdapterClickEventArgs> clickListener,Action<AttachmentsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values          
                Image = (ImageView) MainView.FindViewById(Resource.Id.Image);

                ImageDelete = MainView.FindViewById<CircleButton>(Resource.Id.ImageCircle);

                //Create an Event
                ImageDelete.Click += (sender, e) => clickDeleteListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.Click += (sender, e) => clickListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = BindingAdapterPosition});
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }

    public class AttachmentsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}