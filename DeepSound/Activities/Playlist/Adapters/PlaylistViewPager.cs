using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Playlist.Adapters
{
    public class PlaylistViewPager : PagerAdapter 
    { 
        private readonly Activity ActivityContext;
        private readonly ObservableCollection<PlaylistDataObject> PlaylistList;
        private readonly LayoutInflater Inflater;

        public PlaylistViewPager(Activity context, PlaylistDataObject playlistList)
        {
            try
            {
                ActivityContext = context;
                PlaylistList = new ObservableCollection<PlaylistDataObject> {playlistList, playlistList};
                Inflater = LayoutInflater.From(context);
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
                View layout = null!;
                if (position == 0)
                {
                    //ImageView 
                    layout = Inflater.Inflate(Resource.Layout.Style_PlaylistImageCoursalVeiw, view, false); 
                    var image = layout.FindViewById<ImageView>(Resource.Id.image);
                    var boxLayout = layout.FindViewById<LinearLayout>(Resource.Id.boxLayout);
                   
                    boxLayout.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#efefef"));

                    GlideImageLoader.LoadImage(ActivityContext, PlaylistList[position].ThumbnailReady, image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                }
                else if (position == 1)
                {                 
                    //TextView 
                    layout = Inflater.Inflate(Resource.Layout.Style_PlaylistTextCoursalVeiw, view, false); 
                    var countSongs = layout.FindViewById<TextView>(Resource.Id.countSongs);
                    var timeCreated = layout.FindViewById<TextView>(Resource.Id.timeCreated);
                    var boxLayout = layout.FindViewById<LinearLayout>(Resource.Id.boxLayout);
                    
                    boxLayout.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.ParseColor("#efefef"));

                    countSongs.Text = PlaylistList[position].Songs.ToString();
                    timeCreated.Text = Methods.Time.TimeAgo(PlaylistList[position].Time,false);

                    var line = layout.FindViewById<View>(Resource.Id.line);
                    line.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.line_verticle_white : Resource.Drawable.line_verticle_black);
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