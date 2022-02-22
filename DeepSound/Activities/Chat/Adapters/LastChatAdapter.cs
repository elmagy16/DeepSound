using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AmulyaKhare.TextDrawableLib;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Chat;
using Java.Util;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Chat.Adapters
{
    public class LastChatAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;

        public ObservableCollection<DataConversation> UserList = new ObservableCollection<DataConversation>();
        public event EventHandler<LastChatAdapterClickEventArgs> OnItemClick;
        public event EventHandler<LastChatAdapterClickEventArgs> OnItemLongClick;

        public LastChatAdapter(Activity context)
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is LastChatAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    { 
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(LastChatAdapterViewHolder holder, DataConversation item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.User.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                string name = DeepSoundTools.GetNameFinal(item.User);
                if (holder.TxtUsername.Text != name)
                {
                    holder.TxtUsername.Text = name;
                }

                //If message contains Media files 
                if (item.GetLastMessage?.GetLastMessageClass != null)
                {
                    switch (item.GetLastMessage?.GetLastMessageClass.ApiType)
                    {
                        case ApiType.Text:
                        {
                            holder.LastMessagesIcon.Visibility = ViewStates.Gone;
                            holder.TxtLastMessages.Text = item.GetLastMessage.Value.GetLastMessageClass != null && item.GetLastMessage.Value.GetLastMessageClass.Text.Contains("http")
                                ? Methods.FunString.SubStringCutOf(item.GetLastMessage?.GetLastMessageClass.Text, 30) : Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.GetLastMessage?.GetLastMessageClass.Text, 30))
                                  ?? ActivityContext.GetText(Resource.String.Lbl_SendMessage);
                            break;
                        }
                        case ApiType.Image:
                        {
                            holder.LastMessagesIcon.Visibility = ViewStates.Visible;
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.LastMessagesIcon, IonIconsFonts.Images);
                            holder.TxtLastMessages.Text = Application.Context.GetText(Resource.String.Lbl_SendImageFile);
                            break;
                        }
                    }

                    //last seen time  
                    holder.TxtTimestamp.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.ChatTime), true);

                    //Check read message
                    if (item.GetLastMessage?.GetLastMessageClass.ToId != UserDetails.UserId && item.GetLastMessage?.GetLastMessageClass.FromId == UserDetails.UserId)
                    {
                        if (item.GetLastMessage?.GetLastMessageClass.Seen == 0)
                        {
                            holder.ImageColor.Visibility = ViewStates.Invisible;
                            holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                        }
                        else
                        {
                            holder.ImageColor.Visibility = ViewStates.Invisible;
                            holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                        }
                    }
                    else if (item.GetLastMessage?.GetLastMessageClass.ToId == UserDetails.UserId && item.GetLastMessage?.GetLastMessageClass.FromId != UserDetails.UserId)
                    {
                        if (item.GetLastMessage?.GetLastMessageClass.Seen == 0)
                        {
                            holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Bold);

                            if (item.GetCountSeen != 0)
                            {
                                var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(25).EndConfig().BuildRound(item.GetCountSeen.ToString(), Color.ParseColor(AppSettings.MainColor));
                                holder.ImageColor.SetImageDrawable(drawable);
                                holder.ImageColor.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                holder.ImageColor.Visibility = ViewStates.Invisible;
                            }
                        }
                        else
                        {
                            holder.ImageColor.Visibility = ViewStates.Invisible;
                            holder.TxtUsername.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                            holder.TxtLastMessages.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastChatView, parent, false);
                var vh = new LastChatAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;
          
        public DataConversation GetItem(int position)
        {
            return UserList[position];
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

        private void Click(LastChatAdapterClickEventArgs args)
        {
            OnItemClick?.Invoke(this, args);
        }

        private void LongClick(LastChatAdapterClickEventArgs args)
        {
            OnItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.User.Avatar != "")
                {
                    d.Add(item.User.Avatar);
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
                .Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class LastChatAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public RelativeLayout LytParent { get; private set; }
        public TextView TxtUsername { get; private set; }
        public TextView LastMessagesIcon { get; private set; }
        public TextView TxtLastMessages { get; private set; }
        public TextView TxtTimestamp { get; private set; }
        public ImageView ImageAvatar { get; private set; }
        public ImageView ImageColor { get; private set; }

        //public RelativeLayout LytChecked { get; private set; }
        //public RelativeLayout LytImage { get; private set; }

        #endregion

        public LastChatAdapterViewHolder(View itemView, Action<LastChatAdapterClickEventArgs> clickListener, Action<LastChatAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                LytParent = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
                TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
                LastMessagesIcon = (AppCompatTextView)MainView.FindViewById(Resource.Id.LastMessages_icon);
                TxtLastMessages = (TextView)MainView.FindViewById(Resource.Id.Txt_LastMessages);
                TxtTimestamp = (TextView)MainView.FindViewById(Resource.Id.Txt_timestamp);
                ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.ImageAvatar);

                ImageColor = (ImageView)MainView.FindViewById(Resource.Id.image_view);

                //LytChecked = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_checked);
                //LytImage = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_image);


                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LastChatAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}