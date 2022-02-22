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
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Tabbes.Adapters
{
    public class LibraryAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<LibraryAdapterClickEventArgs> ItemClick;
        public event EventHandler<LibraryAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public readonly ObservableCollection<Classes.LibraryItem> LibraryList = new ObservableCollection<Classes.LibraryItem>();

        public LibraryAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                AddLibrarySectionViews(); 
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
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LibraryView, parent, false);
                var vh = new LibraryAdapterViewHolder(itemView, OnClick, OnLongClick); 
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    //var s = payloads[0].ToString();
                    if (viewHolder is LibraryAdapterViewHolder holder)
                    {
                        var item = LibraryList[position];
                        if (item != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.BackgroundImage, holder.BacgroundImageview, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                             
                            if (item.SongsCount == 0)
                            {
                                holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                holder.SectionVideosCounTextView.Text = item.SongsCount + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);
                            }
                        }
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (position >= 0)
                {
                    if (viewHolder is LibraryAdapterViewHolder holder)
                    {

                        var item = LibraryList[position];
                        if (item != null)
                        {
                            if (item.SectionId == "1") // Liked
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.IosHeartEmpty);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Liked);
                            }
                            else if (item.SectionId == "2") // RecentlyPlayed
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.Play);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_RecentlyPlayed);
                            }
                            else if (item.SectionId == "3") // Favorites
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.StarOutline);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Favorites);
                            }
                            else if (item.SectionId == "4") // LatestDownloads
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.Stats);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_LatestDownloads);
                            }
                            else if (item.SectionId == "5") // Share
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.ShareAlt);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Shared);
                            }
                            else if (item.SectionId == "6") // Purchases
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.LogoUsd);
                                holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Purchases);
                            }

                            if (item.SongsCount == 0)
                            {
                                holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                holder.SectionVideosCounTextView.Text = item.SongsCount + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);
                            }

                            if (item.BackgroundImage == null)
                                return;

                            var background = item.SongsCount > 0 ? item.BackgroundImage : "blackdefault";
                            GlideImageLoader.LoadImage(ActivityContext, background, holder.BacgroundImageview, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Add(Classes.LibraryItem item)
        {
            try
            {
                var check = LibraryList.FirstOrDefault(a => a.SectionId == item.SectionId);
                if (check == null)
                {
                    LibraryList.Add(item);
                    NotifyItemInserted(LibraryList.IndexOf(LibraryList.Last()));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public Classes.LibraryItem GetItem(int position)
        {
            return LibraryList[position];
        }

        public override int ItemCount => LibraryList?.Count ?? 0;

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(LibraryList[position].SectionId);
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

        void OnClick(LibraryAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(LibraryAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = LibraryList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.BackgroundImage != "")
                {
                    d.Add(item.BackgroundImage);
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


        private void AddLibrarySectionViews()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                var check = sqlEntity.Get_LibraryItem();

                if (check != null && check.Count > 0)
                {
                    foreach (var all in check)
                    {
                        Classes.LibraryItem item = new Classes.LibraryItem
                        {
                            SectionId = all.SectionId,
                            SectionText = all.SectionText,
                            SongsCount = all.SongsCount,
                            BackgroundImage = all.BackgroundImage
                        };

                        Add(item); 
                    }
                    NotifyDataSetChanged();
                }
                else
                {
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "1",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Liked),
                        SongsCount = 0,
                        BackgroundImage = "blackdefault"
                    });

                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "2",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_RecentlyPlayed),
                        SongsCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "3",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Favorites),
                        SongsCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    if (AppSettings.AllowOfflineDownload)
                    {
                        LibraryList.Add(new Classes.LibraryItem
                        {
                            SectionId = "4",
                            SectionText = ActivityContext.GetText(Resource.String.Lbl_LatestDownloads),
                            SongsCount = 0,
                            BackgroundImage = "blackdefault"
                        });
                    }
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "5",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Shared),
                        SongsCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "6",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Purchases),
                        SongsCount = 0,
                        BackgroundImage = "blackdefault"
                    });

                    NotifyDataSetChanged();
                    sqlEntity.InsertLibraryItem(LibraryList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LibraryAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView BacgroundImageview { get; private set; }
        public TextView SectionTextView { get; private set; }
        public TextView SectionVideosCounTextView { get; private set; }
        public TextView SectionIconView { get; private set; }

        #endregion

        public LibraryAdapterViewHolder(View itemView, Action<LibraryAdapterClickEventArgs> clickListener, Action<LibraryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                BacgroundImageview = (ImageView)MainView.FindViewById(Resource.Id.Imagelibraryvideo);
                SectionTextView = MainView.FindViewById<TextView>(Resource.Id.libraryText);
                SectionIconView = MainView.FindViewById<TextView>(Resource.Id.libraryicon);
                SectionVideosCounTextView = MainView.FindViewById<TextView>(Resource.Id.LibraryVideosCount);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LibraryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LibraryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LibraryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}