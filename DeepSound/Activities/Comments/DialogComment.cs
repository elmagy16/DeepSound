using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Bumptech.Glide.Util;
using DeepSound.Activities.Comments.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Comments;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Exception = System.Exception;

namespace DeepSound.Activities.Comments
{
    public class DialogComment: Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private readonly Activity ActivityContext;
        private Dialog CommentWindow;
        private TextView IconClose;
        private CommentsAdapter MAdapter;
        private readonly HomeActivity GlobalContext;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private RelativeLayout RootView;
        private EmojiconEditText EmojisIconEditTextView;
        private ImageView EmojisIcon;
        private CircleButton SendButton;
        private ProgressBar ProgressBarLoader;
        private string TimeComment;
        private int UnixTimestamp;
        private SoundDataObject DataObject;
        private CommentsDataObject ItemComments;

        #endregion

        public DialogComment(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = HomeActivity.GetInstance();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
       
        public void Display(SoundDataObject dataObject , string time)
        {
            try
            {
                DataObject = dataObject;
                TimeComment = time;
                 
                CommentWindow = new Dialog(ActivityContext, Resource.Style.D1NoTitleDim);

                if (AppSettings.EnableBlurBackgroundComment)
                {
                    Resources res = Application.Context.Resources;
                  
                    CommentWindow?.Create();
                    Bitmap map = TakeScreenShot(GlobalContext);
                    Bitmap fast = FastBlur(map);
                    Drawable draw = new BitmapDrawable(res, fast);
                    CommentWindow?.Window?.SetBackgroundDrawable(draw);
                }
                
                CommentWindow?.SetContentView(Resource.Layout.CommentsLayout);

                InitComponent();
                SetRecyclerViewAdapters();

                StartApiService();

                CommentWindow?.Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                IconClose = CommentWindow?.FindViewById<TextView>(Resource.Id.IconColse);

                RootView = CommentWindow?.FindViewById<RelativeLayout>(Resource.Id.root);
                EmojisIcon = CommentWindow?.FindViewById<ImageView>(Resource.Id.emojiicon);
                EmojisIconEditTextView = CommentWindow?.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = CommentWindow?.FindViewById<CircleButton>(Resource.Id.sendButton);
                MRecycler = CommentWindow?.FindViewById<RecyclerView>(Resource.Id.recyler);
                SwipeRefreshLayout = CommentWindow?.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                ProgressBarLoader = CommentWindow?.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                EmptyStateLayout = CommentWindow?.FindViewById<ViewStub>(Resource.Id.viewStub);

                SendButton.Click += SendButtonOnClick;
                IconClose.Click += IconCloseOnClick;

                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayout_Refresh;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconClose, FontAwesomeIcon.Times);

                ProgressBarLoader.Visibility = ViewStates.Visible;

                var emojis = new EmojIconActions(ActivityContext, RootView, EmojisIconEditTextView, EmojisIcon);
                emojis.ShowEmojIcon();
                EmojisIconEditTextView.RequestFocus();

               
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SwipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            try
            {
                SwipeRefreshLayout.Refreshing = true;
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new CommentsAdapter(ActivityContext , "Song") { CommentList = new ObservableCollection<CommentsDataObject>() };
                MAdapter.OnItemLongClick += MAdapterOnOnItemLongClick;
                MAdapter.OnAvatarClick += CommentsAdapterOnAvatarClick;
                MAdapter.OnLikeClick += MAdapterOnOnLikeClick;

                LayoutManager = new LinearLayoutManager(ActivityContext);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentsDataObject>(ActivityContext, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //OpenProfile
        private void CommentsAdapterOnAvatarClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        GlobalContext.OpenProfile(item.UserData.Id, item.UserData);

                        CommentWindow?.Hide();
                        CommentWindow?.Dismiss();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                CommentWindow?.Hide();
                CommentWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void MAdapterOnOnLikeClick(object sender, CommentAdapterClickEventArgs e)
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
                    var position = e.Position;
                    if (position > -1)
                    {
                        var item = MAdapter.GetItem(position);
                        if (item != null)
                        {
                            if (e.Holder.LikeButton?.Tag?.ToString() == "1")
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, e.Holder.LikeiconView, IonIconsFonts.IosHeartEmpty);
                                e.Holder.LikeiconView.SetTextColor(Color.White);
                                e.Holder.LikeButton.Tag = "0";
                                item.IsLikedComment = false;

                                if (!e.Holder.LikeNumber.Text.Contains("K") && !e.Holder.LikeNumber.Text.Contains("M"))
                                {
                                    double x = Convert.ToDouble(e.Holder.LikeNumber.Text);
                                    if (x > 0)
                                        x--;
                                    else
                                        x = 0;
                                    e.Holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
                                    item.CountLiked = Convert.ToInt32(x);
                                }
                                 
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.LikeUnLikeCommentAsync(item.Id.ToString(), false) });
                            }
                            else
                            {
                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, e.Holder.LikeiconView, IonIconsFonts.IosHeart);

                                e.Holder.LikeiconView.SetTextColor(Color.ParseColor("#ed4856"));
                                e.Holder.LikeButton.Tag = "1";
                                item.IsLikedComment = true;

                                if (!e.Holder.LikeNumber.Text.Contains("K") && !e.Holder.LikeNumber.Text.Contains("M"))
                                {
                                    double x = Convert.ToDouble(e.Holder.LikeNumber.Text);
                                    x++;
                                    e.Holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
                                    item.CountLiked = Convert.ToInt32(x);
                                }
                                 
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.LikeUnLikeCommentAsync(item.Id.ToString(), true) });
                            }
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
         
        //Report / Delete / Copy comment 
        private void MAdapterOnOnItemLongClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            { 
                var position = e.Position;
                if (position > -1)
                {
                    ItemComments = MAdapter.GetItem(position);
                    if (ItemComments != null)
                    { 
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        if (ItemComments.Owner != null && (!ItemComments.Owner.Value && UserDetails.IsLogin))
                            arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Report));  //wael add check if Reported or no 

                        if (ItemComments.Owner != null && (ItemComments.Owner.Value && UserDetails.IsLogin))
                            arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Delete));
                          
                        arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Copy));

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show(); 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SendButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Login), ActivityContext.GetText(Resource.String.Lbl_Message_Sorry_signin), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (string.IsNullOrEmpty(EmojisIconEditTextView.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Comment Code 
                    string time = Methods.Time.TimeAgo(DateTime.Now , false);

                    UnixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                    //remove \n in a string
                    string replacement = Regex.Replace(EmojisIconEditTextView.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

                    CommentsDataObject comment = new CommentsDataObject
                    {
                        Id = UnixTimestamp,
                        TrackId = DataObject.Id,
                        UserId = UserDetails.UserId,
                        Value = replacement,
                        Songseconds = 0,
                        Songpercentage = 0,
                        Time = UnixTimestamp,
                        Posted = time,
                        SecondsFormated = TimeComment,
                        Owner = true,
                        UserData = ListUtils.MyUserInfoList?.FirstOrDefault(),
                        IsLikedComment = false,
                        CountLiked = 0,
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapter.NotifyItemInserted(index); 
                    }

                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                     
                     var (apiStatus, respond) = await RequestsAsync.Comments.CreateCommentAsync(DataObject.AudioId, TimeComment, replacement).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateCommentObject result)
                        {
                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == UnixTimestamp);
                            if (date != null)
                            {
                                date.Id = result.Data.CommentId;

                                index = MAdapter.CommentList.IndexOf(comment);
                                if (index > -1)
                                {
                                    ActivityContext.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            MAdapter.NotifyItemChanged(index);
                                            MRecycler.ScrollToPosition(index);
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });  
                                }
                            }
                             
                            Console.WriteLine(date);
                        }
                    }
                    else
                    {
                        MainScrollEvent.IsLoading = false;
                        Methods.DisplayReportResult(ActivityContext, respond);
                    }

                    //Hide keyboard
                    EmojisIconEditTextView.Text = "";
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

        #endregion

        #region Load Comments 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadComments(offset) });
        }
         
        private async Task LoadComments(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.CommentList.Count;
                 var (apiStatus, respond) = await RequestsAsync.Comments.GetCommentAsync(DataObject.Id.ToString(), "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is CommentsObject result)
                    {
                        var respondList = result.DataComments?.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.DataComments?.Data let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.CommentList.Add(item);
                                }

                                ActivityContext.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.CommentList = new ObservableCollection<CommentsDataObject>(result.DataComments?.Data);
                                ActivityContext.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }

                ActivityContext.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.CommentList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
                ProgressBarLoader.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                ProgressBarLoader.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        private static Bitmap TakeScreenShot(Activity activity)
        {
            try
            { 
                View view = activity.Window?.DecorView;
                if (view != null)
                { 
                    //view.DrawingCacheEnabled = true;
                    //view.BuildDrawingCache();
                     
                    //Bitmap b1 = view.DrawingCache;
                    Bitmap b1 = BitmapUtil.LoadBitmapFromView(view);

                    Rect frame = new Rect();
                    activity.Window?.DecorView.GetWindowVisibleDisplayFrame(frame);
                    int statusBarHeight = frame.Top;

                    var metrics = Resources.System.DisplayMetrics;
                    int height = metrics.HeightPixels;
                    int width = metrics.WidthPixels;

                    Bitmap b = Bitmap.CreateBitmap(b1, 0, statusBarHeight, width, height - statusBarHeight);
                    //view.DestroyDrawingCache();
                    return b;
                }
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            } 
        }

        private Bitmap FastBlur(Bitmap sentBitmap)
        {
            try
            {
                if (null == sentBitmap) return null!;
                Bitmap outputBitmap = Bitmap.CreateBitmap(sentBitmap);

#pragma warning disable CS0618
                RenderScript renderScript = RenderScript.Create(ActivityContext);
                Allocation tmpIn = Allocation.CreateFromBitmap(renderScript, sentBitmap);
                Allocation tmpOut = Allocation.CreateFromBitmap(renderScript, outputBitmap);
                //Intrinsic Gausian blur filter
                ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(renderScript, Element.U8_4(renderScript));
                theIntrinsic.SetRadius(AppSettings.BlurRadiusComment);
                theIntrinsic.SetInput(tmpIn);
                theIntrinsic.ForEach(tmpOut);
                tmpOut.CopyTo(outputBitmap);
#pragma warning restore CS0618

                return outputBitmap;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == ActivityContext.GetText(Resource.String.Lbl_Report))
                {                       
                    //wael add check if Reported or no 
                    if (Methods.CheckConnectivity())
                    {
                        var dialogBuilder = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialogBuilder.Title(Resource.String.Lbl_Report).TitleColorRes(Resource.Color.primary);
                        dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                        {
                            try
                            {
                                if (s.Length <= 0) return;
                                if (Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_received_your_report), ToastLength.Short)?.Show();

                                    //Sent Api >>
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.ReportUnReportCommentAsync(ItemComments?.Id.ToString(), s, true) });
                                }
                                else
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                        dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                        dialogBuilder.PositiveText(GlobalContext.GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialogBuilder.NegativeText(GlobalContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                        dialogBuilder.Build().Show(); 
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_Delete))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var data = MAdapter.CommentList.FirstOrDefault(a => a.Id == ItemComments?.Id);
                        if (data != null)
                        {
                            MAdapter.CommentList.Remove(ItemComments);

                            int index = MAdapter.CommentList.IndexOf(data);
                            if (index >= 0)
                                MAdapter.NotifyItemRemoved(index);
                        }

                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();

                        //Sent Api Report
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.DeleteCommentAsync(ItemComments?.Id.ToString()) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    } 
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_Copy))
                {
                    GlobalContext?.SoundController?.ClickListeners?.OnMenuCopyOnClick(ItemComments?.Value);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
    }
}