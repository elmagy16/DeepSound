using System.Collections.ObjectModel;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Activities.Playlist;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using Newtonsoft.Json;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Tabbes.Adapters
{
    public class PlayListViewPagerAdapter : PagerAdapter
    {
        private readonly Activity ActivityContext;
        private readonly ObservableCollection<PlaylistDataObject> PlaylistList;
        private readonly LayoutInflater Inflater;
        private readonly RequestBuilder FullGlideRequestBuilder;

        public PlayListViewPagerAdapter(Activity context, ObservableCollection<PlaylistDataObject> playlistList)
        {
            try
            {
                ActivityContext = context;
                PlaylistList = playlistList;
                Inflater = LayoutInflater.From(context);
                var glideRequestOptions = new RequestOptions().Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder).SetDiskCacheStrategy(DiskCacheStrategy.All).SetPriority(Priority.High);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Transition(new BitmapTransitionOptions().CrossFade(100));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                View layout = Inflater.Inflate(Resource.Layout.PlayListCoursalLayoutView, view, false);
                var mainFeaturedImage = layout.FindViewById<ImageView>(Resource.Id.image);
                var title = layout.FindViewById<TextView>(Resource.Id.titleText);
                var seconderText = layout.FindViewById<TextView>(Resource.Id.seconderyText);
                var thirdText = layout.FindViewById<TextView>(Resource.Id.thirdText);

                if (PlaylistList[position] != null)
                {
                    var d = PlaylistList[position].Name.Replace("<br>", "");
                    title.Text = Methods.FunString.DecodeString(d);
                    seconderText.Text = PlaylistList[position].Songs + " " + ActivityContext.GetText(Resource.String.Lbl_Songs) + " "; 


                    if (PlaylistList[position].Privacy == 0)
                        thirdText.Text = ActivityContext.GetText(Resource.String.Lbl_Public);
                    else
                        thirdText.Text = ActivityContext.GetText(Resource.String.Lbl_Private);
                                         
                    FullGlideRequestBuilder.Load(PlaylistList[position].ThumbnailReady).Into(mainFeaturedImage);
                }

                if (!layout.HasOnClickListeners)
                {
                    layout.Click += (sender, args) =>
                    {
                        try
                        {
                            var item = PlaylistList[position];
                            if (item != null)
                            {
                                Bundle bundle = new Bundle();
                                bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                                bundle.PutString("PlaylistId", item.Id.ToString());

                                var playlistProfileFragment = new PlaylistProfileFragment
                                {
                                    Arguments = bundle
                                };

                                ((HomeActivity)ActivityContext)?.FragmentBottomNavigator.DisplayFragment(playlistProfileFragment);
                            }
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

        public override int Count => PlaylistList?.Count ?? 0;

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
}