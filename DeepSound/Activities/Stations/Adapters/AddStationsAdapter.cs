using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Stations;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Stations.Adapters
{
    public class AddStationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<AddStationsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<AddStationsAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<SearchStationsObject.Datum> StationsList = new ObservableCollection<SearchStationsObject.Datum>();
        private readonly RequestBuilder FullGlideRequestBuilder;

        public AddStationsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
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
                //Setup your layout here >> Style_StationsView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StationsView, parent, false);
                var vh = new AddStationsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                var users = StationsList[position];
                if (payloads.Count > 0)
                {
                    if (viewHolder is AddStationsAdapterViewHolder holder)
                    {
                        var data = (string)payloads[0];
                        if (data == "true")
                        {
                            holder.BtnAdd.SetBackgroundResource(Resource.Xml.background_signup2);
                            holder.BtnAdd.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.BtnAdd.Text = ActivityContext.GetText(Resource.String.Lbl_Added);
                            holder.BtnAdd.Tag = "true";
                            users.IsAdded = true;
                            holder.BtnAdd.Visibility = ViewStates.Invisible; //wael after update add new api for remove add 
                        }
                        else
                        {
                            holder.BtnAdd.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.BtnAdd.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.BtnAdd.Text = ActivityContext.GetText(Resource.String.Lbl_Add);
                            holder.BtnAdd.Tag = "false";
                            users.IsAdded = false;
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
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is AddStationsAdapterViewHolder holder)
                {
                    var item = StationsList[position];
                    if (item != null)
                    {
                        FullGlideRequestBuilder.Load(item.Image).Into(holder.Image);
                        holder.TxtName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Name), 60);
                        holder.TxtCat.Text = ActivityContext.GetText(Resource.String.Lbl_Genres) + ": " + item.Genre;
                        holder.TxtCountry.Text = ActivityContext.GetText(Resource.String.Lbl_Country) + ": " + item.Country;

                        switch (item.IsAdded)
                        {
                            // My Friend
                            case true:
                            {
                                holder.BtnAdd.SetBackgroundResource(Resource.Xml.background_signup2);
                                holder.BtnAdd.SetTextColor(Color.ParseColor("#ffffff"));
                                holder.BtnAdd.Text = ActivityContext.GetText(Resource.String.Lbl_Added);
                                holder.BtnAdd.Tag = "true";
                                holder.BtnAdd.Visibility = ViewStates.Invisible;//wael after update add new api for remove add 
                                break;
                            }
                            case false:
                            {
                                holder.BtnAdd.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                holder.BtnAdd.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.BtnAdd.Text = ActivityContext.GetText(Resource.String.Lbl_Add);
                                holder.BtnAdd.Tag = "false";
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StationsList?.Count ?? 0;

        public SearchStationsObject.Datum GetItem(int position)
        {
            return StationsList[position];
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

        void Click(AddStationsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(AddStationsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StationsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Image != "")
                {
                    d.Add(item.Image);
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

    public class AddStationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtCat { get; private set; }
        public TextView TxtCountry { get; private set; }
        public AppCompatButton BtnAdd { get; private set; }

        #endregion

        public AddStationsAdapterViewHolder(View itemView, Action<AddStationsAdapterClickEventArgs> clickListener, Action<AddStationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);
                TxtCat = MainView.FindViewById<TextView>(Resource.Id.cat);
                TxtCountry = MainView.FindViewById<TextView>(Resource.Id.country);
                BtnAdd = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                BtnAdd.Visibility = ViewStates.Visible;

                //Event
                BtnAdd.Click += (sender, e) => clickListener(new AddStationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new AddStationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AddStationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}