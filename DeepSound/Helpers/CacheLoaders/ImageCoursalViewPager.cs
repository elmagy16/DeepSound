using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.CacheLoaders
{
    public class ImageCoursalViewPager : PagerAdapter
    {

        private readonly Activity ActivityContext;
        private readonly ObservableCollection<SoundDataObject> PlaylistList;
        private readonly LayoutInflater Inflater;
        private readonly RequestBuilder FullGlideRequestBuilder;
        private readonly RequestOptions GlideRequestOptions;
        public ImageCoursalViewPager(Activity context, ObservableCollection<SoundDataObject> playlistList)
        {
            ActivityContext = context;
            PlaylistList = playlistList;
            Inflater = LayoutInflater.From(context);
            GlideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
            FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(GlideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
        }
         
        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                View layout = Inflater.Inflate(Resource.Layout.ImageCoursalLayout, view, false);
                var mainFeaturedImage = layout.FindViewById<ImageView>(Resource.Id.image);
                var title = layout.FindViewById<TextView>(Resource.Id.titleText);
                var seconderText = layout.FindViewById<TextView>(Resource.Id.seconderyText);
                //var cardView = layout.FindViewById<CardView>(Resource.Id.cardview2);

                if (PlaylistList[position] != null)
                { 
                    title.Text = Methods.FunString.DecodeString(PlaylistList[position].Title);
                    seconderText.Text = PlaylistList[position].CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music);
                     
                     FullGlideRequestBuilder.Load(PlaylistList[position].Thumbnail).Into(mainFeaturedImage);
                }

                if (!layout.HasOnClickListeners)
                {
                    layout.Click += (sender, args) =>
                    {
                        try
                        {
                            Constant.PlayPos = position;
                            ((HomeActivity)ActivityContext)?.SoundController?.StartPlaySound(PlaylistList[position], PlaylistList);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        } 
                    };
                }

                view.AddView(layout);

                return layout;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            } 
        }

        

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count
        {
            get
            {
                if (PlaylistList != null)
                {
                    return PlaylistList.Count;
                }

                return 0;
            }
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                View view = (View)@object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            } 
        } 
    }
    public class CarouselEffectTransformer : Java.Lang.Object, ViewPager.IPageTransformer
    {
        private static readonly float MIN_SCALE = 0.75f;
        public void TransformPage(View view, float position)
        {
            int pageWidth = view.Width;

            if (position < -1)
            {
                view.Alpha = 0;
            }
            else if (position <= 0)
            { 
                view.Alpha = 1;
                view.TranslationX = 0;
                view.ScaleX = 1;
                view.ScaleY = 1;
            }
            else if (position <= 1)
            { 
                view.Alpha = (1 - position);
                view.TranslationX = (pageWidth * -position);
                float scaleFactor = MIN_SCALE + (1 - MIN_SCALE) * (1 - Math.Abs(position));
                view.ScaleX = scaleFactor;
                view.ScaleY = scaleFactor;
            }
            else
            { 
                view.Alpha = 0;
            }
        }
    }

    public class CarouselEffectTransformer2 : Java.Lang.Object, ViewPager.IPageTransformer
    {
        private readonly int maxTranslateOffsetX;
        private ViewPager viewPager;
        public void TransformPage(View view, float position)
        {
            try
            {
                if (viewPager == null)
                    viewPager = view.Parent as ViewPager;

                var leftInScreen = view.Left - viewPager.ScrollX;
                var centerXInViewPager = leftInScreen + view.MeasuredWidth / 2;
                var offsetX = centerXInViewPager - viewPager.MeasuredWidth / 2;
                var offsetRate = (float)offsetX * 0.38f / viewPager.MeasuredWidth;
                var scaleFactor = 1 - Math.Abs(offsetRate);

                if (scaleFactor > 0)
                {
                    view.ScaleX = scaleFactor;
                    view.ScaleY = scaleFactor;
                    view.TranslationX = -maxTranslateOffsetX * offsetRate;

                }
                ViewCompat.SetElevation(view, scaleFactor);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        private int dp2px(Context context, float dipValue)
        {
            var m = context.Resources?.DisplayMetrics.Density;
            return (int)(dipValue * m + 0.5F);
        }

        public CarouselEffectTransformer2(Context context)
        {
            this.maxTranslateOffsetX = this.dp2px(context, 180.0F);

        }
    }
}