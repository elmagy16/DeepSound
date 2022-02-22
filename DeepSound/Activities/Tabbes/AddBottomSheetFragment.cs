using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Linq;
using DeepSound.Activities.Event;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Product;
using DeepSound.Activities.Stations;
using DeepSound.Helpers.Utils;
using Google.Android.Material.BottomSheet;

namespace DeepSound.Activities.Tabbes
{
    public class AddBottomSheetFragment : BottomSheetDialogFragment
    {
        #region Variables Basic
         
        private LinearLayout UploadSongLayout, UploadAlbumLayout, ImportSongLayout, CreatePlaylistLayout, CreateStationsLayout, CreateEventLayout, CreateProductLayout;
      
        private HomeActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = HomeActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetAddLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                //Get Value And Set Toolbar
                InitComponent(view); 
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                UploadSongLayout = view.FindViewById<LinearLayout>(Resource.Id.UploadSongLayout);
                UploadAlbumLayout = view.FindViewById<LinearLayout>(Resource.Id.UploadAlbumLayout);
                CreatePlaylistLayout = view.FindViewById<LinearLayout>(Resource.Id.CreatePlaylistLayout);
                CreateStationsLayout = view.FindViewById<LinearLayout>(Resource.Id.CreateStationsLayout);
                ImportSongLayout = view.FindViewById<LinearLayout>(Resource.Id.ImportSongLayout);
                CreateEventLayout = view.FindViewById<LinearLayout>(Resource.Id.CreateEventLayout);
                CreateProductLayout = view.FindViewById<LinearLayout>(Resource.Id.CreateProductLayout);

                if (!AppSettings.ShowButtonUploadSingleSong)
                    UploadSongLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonUploadAlbum)
                    UploadAlbumLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowButtonImportSong)
                    ImportSongLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowStations)
                    CreateStationsLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.ShowPlaylist)
                    CreatePlaylistLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.EnableEvent)
                    CreateEventLayout.Visibility = ViewStates.Gone;
                 
                if (!AppSettings.EnableProduct)
                    CreateProductLayout.Visibility = ViewStates.Gone;
                 
                UploadSongLayout.Click += UploadSongLayoutOnClick;
                UploadAlbumLayout.Click += UploadAlbumLayoutOnClick;
                ImportSongLayout.Click += ImportSongLayoutOnClick;
                CreatePlaylistLayout.Click += CreatePlaylistLayoutOnClick;
                CreateStationsLayout.Click += CreateStationsLayoutOnClick;
                CreateEventLayout.Click += CreateEventLayoutOnClick;
                CreateProductLayout.Click += CreateProductLayoutOnClick;

                var artist = ListUtils.MyUserInfoList?.FirstOrDefault()?.Artist ?? 0; 
                if (artist == 0)
                {
                    CreateEventLayout.Visibility = ViewStates.Gone;
                    CreateProductLayout.Visibility = ViewStates.Gone;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events


        private void CreatePlaylistLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(CreatePlaylistActivity)));
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CreateStationsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(CreateStationsActivity)));
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CreateProductLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivityForResult(new Intent(Activity, typeof(CreateProductActivity)), 3500);
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void CreateEventLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivityForResult(new Intent(Activity, typeof(CreateEventActivity)), 4500);
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImportSongLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.BtnImportSongOnClick();

                Dismiss(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UploadAlbumLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.BtnUploadAnAlbumOnClick(); 
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UploadSongLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.BtnUploadSingleSongOnClick();  
                Dismiss(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}