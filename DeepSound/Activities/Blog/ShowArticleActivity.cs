using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Bumptech.Glide.Util;
using DeepSound.Activities.Base;
using DeepSound.Activities.Comments.Adapters;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSoundClient.Classes.Blog;
using DeepSoundClient.Classes.Comments;
using DeepSoundClient.Requests;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Newtonsoft.Json;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Blog
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ShowArticleActivity : BaseActivity , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageBlog;
        private TextView TxtTitle, TxtDescription, TxtViews;
        private WebView TxtHtml;
        private ImageButton BtnMore;
        private ArticleDataObject ArticleData; 
        private CoordinatorLayout RootView;
        private TextView CategoryName, ClockIcon, DateTimeTextView;
        private string ArticleId, DataWebHtml;

        private LinearLayoutManager LayoutManager;
        public CommentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private RecyclerViewOnScrollListener MainScrollEvent;

        private ImageView EmojisIcon;
        private EmojiconEditText TxtComment;
        private CircleButton BtnSentCommentl;

        private CommentsDataObject ItemComments;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                 
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.ArticlesViewLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataArticles(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = FindViewById<CoordinatorLayout>(Resource.Id.root);

                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog);
                ClockIcon = FindViewById<TextView>(Resource.Id.ClockIcon);
                DateTimeTextView = FindViewById<TextView>(Resource.Id.DateTime);
                CategoryName = FindViewById<TextView>(Resource.Id.CategoryName);
                TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtViews = FindViewById<TextView>(Resource.Id.views);
                BtnMore = FindViewById<ImageButton>(Resource.Id.more);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                EmojisIcon = FindViewById<ImageView>(Resource.Id.emojiicon);
                TxtComment = FindViewById<EmojiconEditText>(Resource.Id.commenttext);
                BtnSentCommentl = FindViewById<CircleButton>(Resource.Id.sendButton);

                MRecycler.Visibility = ViewStates.Gone;
                TxtComment.Text = "";
                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ClockIcon, IonIconsFonts.Time);
                  
                var emojis = new EmojIconActions(this, RootView, TxtComment, EmojisIcon);
                emojis.ShowEmojIcon();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = "";
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new CommentsAdapter(this , "Blog")
                {
                    CommentList = new ObservableCollection<CommentsDataObject>()
                };

                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentsDataObject>(this, MAdapter, sizeProvider, 10);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -= 
                if (addEvent)
                {
                    BtnSentCommentl.Click += BtnSentCommentlOnClick;
                    MAdapter.OnItemLongClick += MAdapterOnOnItemLongClick;
                    MAdapter.OnLikeClick += MAdapterOnOnLikeClick;
                    BtnMore.Click += BtnMoreOnClick;
                }
                else
                {
                    BtnSentCommentl.Click -= BtnSentCommentlOnClick;
                    MAdapter.OnItemLongClick -= MAdapterOnOnItemLongClick;
                    MAdapter.OnLikeClick -= MAdapterOnOnLikeClick;
                    BtnMore.Click -= BtnMoreOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopyLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
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
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
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

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.LikeUnLikeCommentAsync(item.Id.ToString(), false) });
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

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.LikeUnLikeCommentAsync(item.Id.ToString(), true) });
                            }
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Events

        private async void BtnSentCommentlOnClick(object sender, EventArgs e)
        {
            try
            { 
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }


                if (string.IsNullOrEmpty(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    CommentsDataObject comment = new CommentsDataObject
                    {
                        Id = unixTimestamp,
                        ArticleId = ArticleId,
                        UserId = UserDetails.UserId,
                        Value = TxtComment.Text,
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsLikedComment = false,
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapter.NotifyItemInserted(index);
                            break;
                    }

                    MRecycler.Visibility = ViewStates.Visible;
                     
                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Articles.CreateCommentsAsync(ArticleId, text);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateCommentsArticlesObject result:
                                        {
                                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data?.Id);
                                            if (date != null)
                                            {
                                                date = result.Data;
                                                date.Id = result.Data.Id;

                                                index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp));
                                                switch (index)
                                                {
                                                    case > -1:
                                                        MAdapter.CommentList[index] = result.Data;

                                                        //MAdapter.NotifyItemChanged(index);
                                                        MRecycler.ScrollToPosition(index);
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        if (UserDetails.IsLogin)
                            arrayAdapter.Add(GetText(Resource.String.Lbl_Report));

                        if (ItemComments.Owner != null && (ItemComments.Owner.Value && UserDetails.IsLogin))
                            arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
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
        
        #endregion
         
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

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetText(Resource.String.Lbl_Report))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialogBuilder.Title(Resource.String.Lbl_Report).TitleColorRes(Resource.Color.primary);
                        dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                        {
                            try
                            {
                                if (s.Length <= 0) return;
                                if (Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_received_your_report), ToastLength.Short)?.Show();

                                    //Sent Api Report
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.ReportUnReportCommentAsync(ItemComments?.Id.ToString(), s.ToString(), true) }); 
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                        dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                        dialogBuilder.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                        dialogBuilder.Build().Show(); 
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Delete))
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

                        Toast.MakeText(this, GetText(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();

                        //Sent Api Report
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.DeleteCommentAsync(ItemComments?.Id.ToString()) });
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(this, ItemComments?.Value);
                } 
                else if (text == GetText(Resource.String.Lbl_CopyLink))
                {
                    Methods.CopyToClipboard(this, ArticleData?.Url);
                } 
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as Song
                    if (!CrossShare.IsSupported)
                    {
                        return;
                    }

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = Methods.FunString.DecodeString(ArticleData?.Title),
                        Text = "",
                        Url = ArticleData?.Url
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private void GetDataArticles()
        {
            try
            {
                ArticleData = JsonConvert.DeserializeObject<ArticleDataObject>(Intent?.GetStringExtra("itemObject") ?? "");
                if (ArticleData != null)
                {
                    ArticleId = ArticleData.Id;

                    GlideImageLoader.LoadImage(this, ArticleData.Thumbnail, ImageBlog, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    SupportActionBar.Title = Methods.FunString.DecodeString(ArticleData.Title);

                    TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(ArticleData.Description);
                    TxtViews.Text = ArticleData.View + " " + GetText(Resource.String.Lbl_Views);

                    //SharedCount.Text = ArticleData.Shared.ToString();
                    DateTimeTextView.Text = ArticleData.CreatedAt;

                    CategoryName.Text = GetText(Resource.String.Lbl_Category) + " : "  + CategoriesController.Get_Translate_Categories_Communities(ArticleData.Category?.ToString(), "", "Blog");  

                    string style = AppSettings.SetTabDarkTheme ? "<style type='text/css'>body{color: #fff; background-color: #282828; line-height: 1.42857143;}</style>" : "<style type='text/css'>body{color: #444; background-color: #FFFEFE; line-height: 1.42857143;}</style>";
                    string imageFullWidthStyle = "<style>img{display: inline;height: auto;max-width: 100%;}</style>";

                    var content = Html.FromHtml(ArticleData.Content, FromHtmlOptions.ModeCompact)?.ToString();
                    DataWebHtml = "<!DOCTYPE html>";
                    DataWebHtml += "<head><title></title>" + style + imageFullWidthStyle + "</head>";
                    DataWebHtml += "<body>" + content + "</body>";
                    DataWebHtml += "</html>";

                    TxtHtml.SetWebViewClient(new MyWebViewClient(this));
                    TxtHtml.Settings.LoadsImagesAutomatically = true;
                    TxtHtml.Settings.JavaScriptEnabled = true;
                    TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                    TxtHtml.Settings.DomStorageEnabled = true;
                    TxtHtml.Settings.AllowFileAccess = true;
                    TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

                    TxtHtml.Settings.UseWideViewPort = true;
                    TxtHtml.Settings.LoadWithOverviewMode = true;

                    TxtHtml.Settings.SetSupportZoom(false);
                    TxtHtml.Settings.BuiltInZoomControls = false;
                    TxtHtml.Settings.DisplayZoomControls = false;

                    //int fontSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 18, Resources?.DisplayMetrics);
                    //TxtHtml.Settings.DefaultFontSize = fontSize;

                    TxtHtml.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.GetBlogByIdAsync(ArticleId) });

                    if (UserDetails.IsLogin)
                        StartApiService();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment(offset) });
        }

        private async Task LoadDataComment(string offset)
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var countList = MAdapter.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Articles.GetCommentsAsync(ArticleId, "25", offset);
                if (apiStatus != 200 || respond is not GetCommentsArticlesObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    switch (respondList)
                    {
                        case > 0 when countList > 0:
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.CommentList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                                break;
                            }
                        case > 0:
                            MAdapter.CommentList = new ObservableCollection<CommentsDataObject>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            break;
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false; 

                if (MAdapter.CommentList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible; 
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone; 
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyWebViewClient : WebViewClient
        {
            private readonly ShowArticleActivity Activity;
            public MyWebViewClient(ShowArticleActivity mActivity)
            {
                Activity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                new IntentController(Activity).OpenBrowserFromApp(request.Url.ToString());
                view.LoadDataWithBaseURL(null, Activity.DataWebHtml, "text/html", "UTF-8", null);
                return true;
            }
        }

    }
}   