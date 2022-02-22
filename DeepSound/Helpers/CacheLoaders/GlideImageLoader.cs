using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Palette.Graphics;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Engine.BitmapRecycle;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Load.Resource.Drawable;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using DeepSound.Helpers.Utils;
using Java.IO;
using Java.Lang;
using Java.Security;
using Exception = System.Exception;
using Math = System.Math;

namespace DeepSound.Helpers.CacheLoaders
{
    public enum ImageStyle
    {
        CenterCrop, CircleCrop, RoundedCrop, FitCenter, CircleCropWithBorder, Blur, PaletteBitmapColor
    }

    public enum ImagePlaceholders
    {
        Color, Drawable
    }

    public static class GlideImageLoader
    {
        public static void LoadImage(Activity activity, string imageUri, ImageView image, ImageStyle style, ImagePlaceholders imagePlaceholders, bool compress = false, RequestOptions options = null)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUri) || string.IsNullOrWhiteSpace(imageUri) || image == null || activity?.IsDestroyed != false)
                    return;

                imageUri = imageUri.Replace(" ", "");

                var newImage = Glide.With(activity);

                options ??= GetOptions(style, imagePlaceholders);
                 
                if (compress) 
                    options.Override(200);

                if (style == ImageStyle.Blur)
                {
                    newImage.Load(imageUri).Apply(options)
                        .Transition(DrawableTransitionOptions.WithCrossFade())
                        .Transform(new BlurTransformation(activity), new CenterCrop())
                        .Override(150)
                        .Into(image);
                    return;
                }

                //if (style == ImageStyle.PaletteBitmapColor)
                //{
                //    newImage.Load(imageUri).Apply(options) 
                //        .Into(new ColorGenerate(activity ,image));
                //     return;
                //}

                if (imageUri.Contains("no_profile_image") || imageUri.Contains("blackdefault") || imageUri.Contains("no_profile_image_circle")
                    || imageUri.Contains("ImagePlacholder") || imageUri.Contains("ImagePlacholder_circle") || imageUri.Contains("Grey_Offline") 
                    || imageUri.Contains("d-avatar") || imageUri.Contains("addImage"))
                {
                    if (imageUri.Contains("no_profile_image_circle"))
                        newImage.Load(Resource.Drawable.no_profile_image_circle).Apply(options).Into(image);
                    else if (imageUri.Contains("no_profile_image") || imageUri.Contains("d-avatar"))
                        newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
                    else if (imageUri.Contains("ImagePlacholder"))
                        newImage.Load(Resource.Drawable.ImagePlacholder).Apply(options).Into(image);
                    else if (imageUri.Contains("ImagePlacholder_circle"))
                        newImage.Load(Resource.Drawable.ImagePlacholder_circle).Apply(options).Into(image);
                    else if (imageUri.Contains("blackdefault"))
                        newImage.Load(Resource.Drawable.blackdefault).Apply(options).Into(image);
                    else if (imageUri.Contains("Grey_Offline"))
                        newImage.Load(Resource.Drawable.Grey_Offline).Apply(options).Into(image);
                    else if (imageUri.Contains("addImage"))
                        newImage.Load(Resource.Drawable.addImage).Apply(options).Into(image);
                }
                else if (!string.IsNullOrEmpty(imageUri) && imageUri.Contains("http"))
                {
                    newImage.Load(imageUri).Apply(options).Into(image);
                }
                else if (!string.IsNullOrEmpty(imageUri) && (imageUri.Contains("file://") ||
                                                             imageUri.Contains("content://") ||
                                                             imageUri.Contains("storage") ||
                                                             imageUri.Contains("/data/user/0/")))
                {
                    File file2 = new File(imageUri);
                    var photoUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", file2);
                    RequestOptions option = style == ImageStyle.CircleCrop
                        ? new RequestOptions().CircleCrop()
                        : new RequestOptions();
                    Glide.With(activity).Load(photoUri).Apply(option).Into(image);
                }
                else
                {
                    newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void LoadImage(Activity activity, int imageUri, ImageView image, ImageStyle style, ImagePlaceholders imagePlaceholders, bool compress = false, RequestOptions options = null)
        {
            try
            {
                if (image == null || activity?.IsDestroyed != false)
                    return;

                var newImage = Glide.With(activity);

                options ??= GetOptions(style, imagePlaceholders);

                if (compress && style != ImageStyle.RoundedCrop) options.Override(200);

                newImage.Load(imageUri).Apply(options).Into(image);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static RequestOptions GetOptions(ImageStyle style, ImagePlaceholders imagePlaceholders)
        {
            try
            {
                RequestOptions options = new RequestOptions();

                switch (style)
                {
                    case ImageStyle.Blur or ImageStyle.CenterCrop:
                        options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                            .CenterCrop()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone()
                            .Error(Resource.Drawable.ImagePlacholder)
                            .Placeholder(Resource.Drawable.ImagePlacholder));
                        break;
                    case ImageStyle.FitCenter:
                        options = new RequestOptions().Apply(RequestOptions.CenterCropTransform().AutoClone()
                            .FitCenter()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder)
                            .Placeholder(Resource.Drawable.ImagePlacholder));
                        break;
                    case ImageStyle.CircleCrop:
                        options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
                            .CenterCrop().CircleCrop()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder_circle)
                            .Placeholder(Resource.Drawable.ImagePlacholder_circle));
                        break;
                    case ImageStyle.CircleCropWithBorder:
                        options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
                            .CenterCrop().CircleCrop()
                            .Transform(new GlideCircleWithBorder(1, Color.Black))
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder_circle)
                            .Placeholder(Resource.Drawable.ImagePlacholder_circle));
                        break;
                    case ImageStyle.RoundedCrop:
                        options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
                            .CenterCrop()
                            .Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(20)))
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder_circle)
                            .Placeholder(Resource.Drawable.ImagePlacholder_circle));
                        break;
                    default:
                        options.CenterCrop();
                        break;
                }

                if (imagePlaceholders == ImagePlaceholders.Color)
                {
                    var color = Methods.FunString.RandomColor().Item1;
                    options.Placeholder(new ColorDrawable(Color.ParseColor(color)))
                        .Fallback(new ColorDrawable(Color.ParseColor(color)));
                }
                else if (imagePlaceholders == ImagePlaceholders.Drawable)
                {
                    if (style is ImageStyle.CircleCrop or ImageStyle.CircleCropWithBorder)
                        options.Placeholder(Resource.Drawable.ImagePlacholder_circle)
                            .Fallback(Resource.Drawable.ImagePlacholder_circle);
                    else
                        options.Placeholder(Resource.Drawable.ImagePlacholder)
                            .Fallback(Resource.Drawable.ImagePlacholder);
                }

                return options;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new RequestOptions().CenterCrop();
            }
        }

        public static RequestBuilder GetPreLoadRequestBuilder(Activity activityContext, string url, ImageStyle style)
        {
            try
            {
                if (url == null || string.IsNullOrEmpty(url))
                    return null!;

                var options = GetOptions(style, ImagePlaceholders.Drawable);

                if (url.Contains("avatar"))
                    options.CircleCrop();

                options.Override(200);

                options.SetDiskCacheStrategy(DiskCacheStrategy.All);

                return Glide.With(activityContext)
                    .Load(url)
                    .Apply(options);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

        }

        public static RequestOptions GetRequestOptions(ImageStyle style, ImagePlaceholders imagePlaceholders)
        {
            try
            {
                var options = new RequestOptions();


                if (style == ImageStyle.CenterCrop)
                {
                    options.CenterCrop();
                }
                else if (style == ImageStyle.FitCenter)
                {
                    options.FitCenter();
                }
                else if (style == ImageStyle.CircleCrop)
                {
                    options.CircleCrop();
                }
                else if (style == ImageStyle.CircleCropWithBorder)
                {
                    options.CircleCrop();
                    options.Transform(new GlideCircleWithBorder(2, Color.White));
                }
                else if (style == ImageStyle.RoundedCrop)
                {
                    options.Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(20)));
                }
                else
                {
                    options.CenterCrop();
                }


                if (imagePlaceholders == ImagePlaceholders.Color)
                {
                    var color = Methods.FunString.RandomColor().Item1;
                    options.Placeholder(new ColorDrawable(Color.ParseColor(color)))
                        .Fallback(new ColorDrawable(Color.ParseColor(color)));
                }
                else if (imagePlaceholders == ImagePlaceholders.Drawable)
                {
                    options.Placeholder(Resource.Drawable.ImagePlacholder).Fallback(Resource.Drawable.ImagePlacholder);
                }

                return options;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new RequestOptions();
            }

        }
    }

    public class GlideCircleWithBorder : BitmapTransformation
    {
        private readonly Paint MBorderPaint;
        private readonly float MBorderWidth;

        public GlideCircleWithBorder(int borderWidth, Color borderColor)
        {
            try
            {
                if (Resources.System?.DisplayMetrics != null)
                    MBorderWidth = Resources.System.DisplayMetrics.Density * borderWidth;

                MBorderPaint = new Paint { Dither = true, AntiAlias = true, Color = borderColor };
                MBorderPaint.SetStyle(Paint.Style.Stroke);
                MBorderPaint.StrokeWidth = MBorderWidth;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        protected override Bitmap Transform(IBitmapPool pool, Bitmap toTransform, int outWidth, int outHeight)
        {
            return CircleCrop(pool, toTransform);
        }

        public override void UpdateDiskCacheKey(MessageDigest p0)
        {

        }

        private Bitmap CircleCrop(IBitmapPool pool, Bitmap source)
        {
            try
            {
                if (source == null) return null;

                int size = (int)(Math.Min(source.Width, source.Height) - MBorderWidth / 2);
                int x = (source.Width - size) / 2;
                int y = (source.Height - size) / 2;
                Bitmap squared = Bitmap.CreateBitmap(source, x, y, size, size);
                Bitmap result = pool.Get(size, size, Bitmap.Config.Argb8888);
                if (result == null) result = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);

                //Create a brush Canvas Manually draw a border
                Canvas canvas = new Canvas(result);
                Paint paint = new Paint();
                paint.SetShader(new BitmapShader(squared, Shader.TileMode.Clamp, Shader.TileMode.Clamp));
                paint.AntiAlias = true;
                float r = size / 2f;
                canvas.DrawCircle(r, r, r, paint);
                if (MBorderPaint != null)
                {
                    float borderRadius = r - MBorderWidth / 2;
                    canvas.DrawCircle(r, r, borderRadius, MBorderPaint);
                }
                return result;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
    }

    public class BlurTransformation : BitmapTransformation
    {
#pragma warning disable CS0618
        private readonly RenderScript RenderScript;
#pragma warning restore CS0618

        public BlurTransformation(Context context)
        {
#pragma warning disable CS0618
            RenderScript = RenderScript.Create(context);
#pragma warning restore CS0618
        }

        protected override Bitmap Transform(IBitmapPool pool, Bitmap toTransform, int outWidth, int outHeight)
        {
            Bitmap blurredBitmap = toTransform.Copy(Bitmap.Config.Argb8888, true);

            Bitmap outputBitmap = Bitmap.CreateBitmap(blurredBitmap);

#pragma warning disable CS0618
            Allocation tmpIn = Allocation.CreateFromBitmap(RenderScript, blurredBitmap);
            Allocation tmpOut = Allocation.CreateFromBitmap(RenderScript, outputBitmap);
            //Intrinsic Gausian blur filter
            ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(RenderScript, Element.U8_4(RenderScript));
            theIntrinsic.SetRadius(25);
            theIntrinsic.SetInput(tmpIn);
            theIntrinsic.ForEach(tmpOut);
            tmpOut.CopyTo(outputBitmap);
#pragma warning restore CS0618
            return outputBitmap;
        }

        public override void UpdateDiskCacheKey(MessageDigest p0)
        {

        }
    }

    public class ColorGenerate : CustomTarget, Palette.IPaletteAsyncListener
    {
        private readonly ImageView Image;
        private readonly Activity Context;

        public ColorGenerate(Activity context, ImageView image)
        {
            try
            {
                Context = context;
                Image = image;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override void OnResourceReady(Java.Lang.Object resource, ITransition transition)
        {
            try
            {
                if (resource is BitmapDrawable bitmapDrawable)
                {
                    Palette.From(bitmapDrawable.Bitmap).MaximumColorCount(2).Generate(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLoadCleared(Drawable p0) { }
        public void OnGenerated(Palette palette)
        {
            try
            {
                if (Context?.IsDestroyed != false)
                    return;

                var metrics = Resources.System.DisplayMetrics;
                int height = metrics.HeightPixels;
                int width = metrics.HeightPixels;

                if (palette.Swatches.Count == 2)
                {
                    string hex1 = "#" + Integer.ToHexString(palette.Swatches[0].Rgb).Remove(0, 2);
                    string hex2 = "#" + Integer.ToHexString(palette.Swatches[1].Rgb).Remove(0, 2);

                    int[] color = { Color.ParseColor(hex1), Color.ParseColor(hex2) };

                    var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, width, height, false, true);
                    if (bitmap != null)
                    {
                        Glide.With(Context).Load(bitmap).Apply(new RequestOptions().Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(25)))).Into(Image);
                    }
                }
                else if (palette.Swatches.Count > 0)
                {
                    string hex1 = "#" + Integer.ToHexString(palette.Swatches[0].Rgb).Remove(0, 2);

                    int[] color = { Color.ParseColor(hex1), Color.ParseColor("#444444") };

                    var (gradient, bitmap) = ColorUtils.GetGradientDrawable(color, width, height, false, true);
                    if (bitmap != null)
                    {
                        Glide.With(Context).Load(bitmap).Apply(new RequestOptions().Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(25)))).Into(Image);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}