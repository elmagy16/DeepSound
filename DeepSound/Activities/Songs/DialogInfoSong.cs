using System;
using Android.App;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using String = Java.Lang.String;

namespace DeepSound.Activities.Songs
{
    public class DialogInfoSong
    {
        #region Variables Basic

        private readonly Activity ActivityContext;
        private Dialog InfoSongWindow;
        private ImageView ImageSong, ImageCover;
        private TextView IconClose, TxtNameSong, IconGenres, TxtGenres, TxtPublisherName, TxtDate, TxtAgeRestriction, TxtDuration, TxtPurchased;
        private SuperTextView TxtAbout, TxtTags, TxtLyrics;
        private TextView IconLike, CountLike, IconStars, CountStars, IconViews, CountViews, IconShare, CountShare, IconComment, CountComment;

        //private LinearLayout LayoutGenres, LayoutPublisher, LayoutAddedOn;
        private SoundDataObject DataObject;
        private PublisherAdView PublisherAdView;
        public StReadMoreOption ReadMoreOption { get; }

        #endregion

        public DialogInfoSong(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                 
                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(activity.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(activity.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Display(SoundDataObject dataObject)
        {
            try
            {
                DataObject = dataObject;

                InfoSongWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                InfoSongWindow?.SetContentView(Resource.Layout.DialogInfoSongLayout);

                InitComponent();
                AddOrRemoveEvent(true);
                SetDataSong();

                InfoSongWindow?.Show();
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
                IconClose = InfoSongWindow?.FindViewById<TextView>(Resource.Id.IconColse);
                ImageSong = InfoSongWindow?.FindViewById<ImageView>(Resource.Id.image);
                ImageCover = InfoSongWindow?.FindViewById<ImageView>(Resource.Id.image_Cover);

                TxtNameSong = InfoSongWindow?.FindViewById<TextView>(Resource.Id.nameSong);

                //LayoutGenres = InfoSongWindow?.FindViewById<LinearLayout>(Resource.Id.LayoutGenres);
                IconGenres = InfoSongWindow?.FindViewById<TextView>(Resource.Id.IconGenres);
                TxtGenres = InfoSongWindow?.FindViewById<TextView>(Resource.Id.GenresText);

                //LayoutPublisher = InfoSongWindow?.FindViewById<LinearLayout>(Resource.Id.LayoutPublisher);
                TxtPublisherName = InfoSongWindow?.FindViewById<TextView>(Resource.Id.publisherText);


                //LayoutAddedOn = InfoSongWindow?.FindViewById<LinearLayout>(Resource.Id.LayoutAddedOn);
                TxtDate = InfoSongWindow?.FindViewById<TextView>(Resource.Id.dateText);

                IconLike = InfoSongWindow?.FindViewById<TextView>(Resource.Id.iconLike);
                CountLike = InfoSongWindow?.FindViewById<TextView>(Resource.Id.textView_songLike);
                IconStars = InfoSongWindow?.FindViewById<TextView>(Resource.Id.iconStars);
                CountStars = InfoSongWindow?.FindViewById<TextView>(Resource.Id.textView_totalrate_songlist);
                IconViews = InfoSongWindow?.FindViewById<TextView>(Resource.Id.iconViews);
                CountViews = InfoSongWindow?.FindViewById<TextView>(Resource.Id.textView_views);
                IconShare = InfoSongWindow?.FindViewById<TextView>(Resource.Id.iconShare);
                CountShare = InfoSongWindow?.FindViewById<TextView>(Resource.Id.textView_share);
                IconComment = InfoSongWindow?.FindViewById<TextView>(Resource.Id.iconComment);
                CountComment = InfoSongWindow?.FindViewById<TextView>(Resource.Id.textView_comment);

                TxtAgeRestriction = InfoSongWindow?.FindViewById<TextView>(Resource.Id.AgeRestrictionText);
                TxtPurchased = InfoSongWindow?.FindViewById<TextView>(Resource.Id.PaidText);
                TxtDuration = InfoSongWindow?.FindViewById<TextView>(Resource.Id.DurationText);

                TxtAbout = InfoSongWindow?.FindViewById<SuperTextView>(Resource.Id.aboutText);
                TxtTags = InfoSongWindow?.FindViewById<SuperTextView>(Resource.Id.tagText);
                TxtLyrics = InfoSongWindow?.FindViewById<SuperTextView>(Resource.Id.lyricsText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconClose, FontAwesomeIcon.Times);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLike, IonIconsFonts.Heart);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconStars, IonIconsFonts.Star);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconViews, IonIconsFonts.Play);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconShare, IonIconsFonts.ShareAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconComment, FontAwesomeIcon.CommentDots);

                PublisherAdView = InfoSongWindow?.FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    IconClose.Click += IconCloseOnClick;
                    TxtPublisherName.Click += TxtPublisherNameOnClick;
                }
                else
                {
                    IconClose.Click -= IconCloseOnClick;
                    TxtPublisherName.Click -= TxtPublisherNameOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
           
        //Open profile 
        private void TxtPublisherNameOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoSongWindow?.Hide();
                InfoSongWindow?.Dismiss();

                HomeActivity.GetInstance()?.OpenProfile(DataObject.Publisher.Id, DataObject.Publisher);
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
                PublisherAdView?.Destroy();

                InfoSongWindow?.Hide();
                InfoSongWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void SetDataSong()
        {
            try
            {
                if (DataObject != null)
                {
                    var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                    var fullGlideRequestBuilder = Glide.With(ActivityContext).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
                    fullGlideRequestBuilder.Load(DataObject.Thumbnail).Into(ImageCover);

                    GlideImageLoader.LoadImage(ActivityContext, DataObject.Thumbnail, ImageSong, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtNameSong.Text = DataObject.Title;
                    TxtGenres.Text = DataObject.CategoryName;
                    TxtDate.Text = DataObject.TimeFormatted;
                    TxtPublisherName.Text = DeepSoundTools.GetNameFinal(DataObject.Publisher);
                    TxtDuration.Text = DataObject.Duration;
                    TxtAgeRestriction.Text = ActivityContext.GetString(DataObject.AgeRestriction == 0 ? Resource.String.Lbl_AgeRestrictionText0 : Resource.String.Lbl_AgeRestrictionText1);
                    TxtPurchased.Text = ActivityContext.GetString(DataObject.Price != null && Math.Abs(DataObject.Price.Value) > 0 ? Resource.String.Lbl_Yes : Resource.String.Lbl_No);

                    CountLike.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountLikes));
                    CountStars.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountFavorite));
                    CountViews.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountViews.Replace("K", "").Replace("M", "")));
                    CountShare.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountShares));
                    CountComment.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataObject.CountComment));

                    TextSanitizer aboutSanitizer = new TextSanitizer(TxtAbout, ActivityContext);
                    aboutSanitizer.Load(Methods.FunString.DecodeString(DataObject.Description));

                    TextSanitizer tagsSanitizer = new TextSanitizer(TxtTags, ActivityContext);
                    tagsSanitizer.Load(Methods.FunString.DecodeString(DataObject.Tags.Replace(",", " #")));

                    ReadMoreOption.AddReadMoreTo(TxtLyrics, new String(DataObject.Lyrics)); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}