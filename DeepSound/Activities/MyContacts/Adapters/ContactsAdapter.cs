using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Java.Util;
using IList = System.Collections.IList;

namespace DeepSound.Activities.MyContacts.Adapters
{
    public class ContactsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    { 
        public event EventHandler<ContactsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<ContactsAdapterClickEventArgs> OnItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UsersList = new ObservableCollection<UserDataObject>();
        private readonly bool ShowButtonFollow;

        public ContactsAdapter(Activity context ,bool showButtonFollow = true)
        {
            try
            {
                ActivityContext = context;
                ShowButtonFollow = showButtonFollow;
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
                //Setup your layout here >> Style_HContactView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);
                var vh = new ContactsAdapterViewHolder(itemView, OnClick, OnLongClick);
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

                if (viewHolder is ContactsAdapterViewHolder holder)
                {
                    var item = UsersList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);

                        if (ShowButtonFollow)
                        {
                            if (!holder.Button.HasOnClickListeners)
                                holder.Button.Click += (sender, e) => OnFollowButtonClick(new FollowFollowingClickEventArgs { View = holder.ItemView, UserClass = item, Position = position, ButtonFollow = holder.Button });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Initialize(ContactsAdapterViewHolder holder, UserDataObject following)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext,following.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                string name = Methods.FunString.DecodeString(DeepSoundTools.GetNameFinal(following));
                holder.Name.Text = Methods.FunString.SubStringCutOf(name, 25);

                if (following.Time != null) 
                    holder.About.Text = Methods.Time.TimeAgo(following.Time.Value, false);
                else
                    holder.About.Text = Methods.Time.TimeAgo(Convert.ToInt32(following.LastActive), false);
                 
                if (ShowButtonFollow)
                {
                    if (following.Id == UserDetails.UserId)
                    {
                        holder.Button.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        if (following.IsFollowing != null && following.IsFollowing.Value) // My Friend
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                            holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);

                            holder.Button.Tag = "true";
                        }
                        else //Not Friend
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                            holder.Button.Tag = "false";
                        }
                    }
                }
                else
                {
                    holder.Button.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        private void OnFollowButtonClick(FollowFollowingClickEventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    if (e.UserClass != null)
                    {
                        if (e.ButtonFollow.Tag?.ToString() == "true")
                        {
                            e.UserClass.IsFollowing = false;
                            e.ButtonFollow.Tag = "false";
                            e.ButtonFollow.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                            e.ButtonFollow.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            e.ButtonFollow.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(e.UserClass.Id.ToString(), false) });
                        }
                        else
                        {
                            e.UserClass.IsFollowing = true;
                            e.ButtonFollow.Tag = "true";
                            e.ButtonFollow.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                            e.ButtonFollow.SetBackgroundResource(Resource.Xml.background_signup2);
                            e.ButtonFollow.SetTextColor(Color.White);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(e.UserClass.Id.ToString(), true) });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }
         
        public override int ItemCount => UsersList?.Count ?? 0;

        public UserDataObject GetItem(int position)
        {
            return UsersList[position];
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

        void OnClick(ContactsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void OnLongClick(ContactsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UsersList[p0];

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

    public class ContactsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton Button { get; private set; }

        #endregion

        public ContactsAdapterViewHolder(View itemView, Action<ContactsAdapterClickEventArgs> clickListener, Action<ContactsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                 
                //Event
                itemView.Click += (sender, e) => clickListener(new ContactsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ContactsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class ContactsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class FollowFollowingClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public UserDataObject UserClass { get; set; }
        public AppCompatButton ButtonFollow { get; set; }  
    } 
}