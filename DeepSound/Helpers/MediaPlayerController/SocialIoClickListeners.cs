using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Tabbes;
using DeepSound.Activities.Upload;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SocialIoClickListeners : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback 
    {
        private readonly Activity MainContext;
        private readonly HomeActivity GlobalContext;
        private string TypeDialog, NamePage;
        private MoreSongClickEventArgs MoreSongArgs;

        public SocialIoClickListeners(Activity context)
        {
            try
            {
                MainContext = context;
                GlobalContext = (HomeActivity)MainContext ?? HomeActivity.GetInstance();
                TypeDialog = string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Add Like Or Remove 
        public void OnLikeSongsClick(LikeSongsClickEventArgs e, string name = "")
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var refs = SetLike(e.LikeButton);
                    e.SongsClass.IsLiked = refs;

                    //add to Liked
                    if (refs)
                    {
                        ListUtils.LikedSongs.Add(e.SongsClass);
                        GlobalContext?.LibrarySynchronizer.AddToLiked(e.SongsClass);
                    }
                    else
                    {
                        GlobalContext?.LibrarySynchronizer.RemoveFromLiked(e.SongsClass);
                    }

                    if (name == "AlbumsFragment")
                    {
                        var list = AlbumsFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            AlbumsFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PlaylistProfileFragment")
                    {  
                        var list = PlaylistProfileFragment.Instance?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            PlaylistProfileFragment.Instance?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "SongsByGenresFragment")
                    {
                        var list = GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PlaylistProfileFragment")
                    {
                        var list = GlobalContext?.PlaylistFragment?.PlaylistProfileFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.PlaylistFragment?.PlaylistProfileFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "SearchSongsFragment")
                    {
                        var list = GlobalContext?.MainFragment.SearchFragment?.SongsTab?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.MainFragment.SearchFragment?.SongsTab?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "FavoritesFragment")
                    {
                        var list = GlobalContext?.LibraryFragment.FavoritesFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.FavoritesFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "LatestDownloadsFragment")
                    {
                        var list = GlobalContext?.LibraryFragment.LatestDownloadsFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.LatestDownloadsFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "LikedFragment")
                    {
                        var list = GlobalContext?.LibraryFragment.LikedFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.LikedFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PurchasesFragment")

                    {
                        var list = GlobalContext?.LibraryFragment.PurchasesFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.PurchasesFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "RecentlyPlayedFragment")
                    {
                        var list = GlobalContext?.LibraryFragment.RecentlyPlayedFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.RecentlyPlayedFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "SharedFragment")
                    {
                        var list = GlobalContext?.LibraryFragment.SharedFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.LibraryFragment.SharedFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "SongsByTypeFragment")
                    {
                        var list = GlobalContext?.MainFragment.SongsByTypeFragment?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.MainFragment.SongsByTypeFragment?.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "UserLikedFragment")
                    {
                        var list = GlobalContext?.ProfileFragment.LikedFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.ProfileFragment.LikedFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "UserSongsFragment")
                    {
                        var list = GlobalContext?.ProfileFragment.SongsFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsLiked = refs;
                            int index = list.IndexOf(dataSong);
                            GlobalContext?.ProfileFragment.SongsFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.LikeUnLikeTrackAsync(e.SongsClass.AudioId) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Dislike Or Remove 
        public void OnDislikeSongsClick(LikeSongsClickEventArgs e, string name = "")
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var refs = SetDislike(e.LikeButton);
                    e.SongsClass.IsDisLiked = refs;

                    //add to Disliked
                    //if (refs)
                    //    GlobalContext?.LibrarySynchronizer.AddToLiked(e.SongsClass);

                    if (name == "AlbumsFragment")
                    {
                        var list = AlbumsFragment.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsDisLiked = refs;
                            int index = list.IndexOf(dataSong);
                            AlbumsFragment.MAdapter?.NotifyItemChanged(index);
                        }
                    }
                    else if (name == "PlaylistProfileFragment")
                    { 
                        var list = PlaylistProfileFragment.Instance?.MAdapter?.SoundsList;
                        var dataSong = list?.FirstOrDefault(a => a.Id == e.SongsClass.Id);
                        if (dataSong != null)
                        {
                            dataSong.IsDisLiked = refs;
                            int index = list.IndexOf(dataSong);
                            PlaylistProfileFragment.Instance?.MAdapter?.NotifyItemChanged(index);
                        }
                    }

                    //Sent Api 
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.DislikeUnDislikeTrackAsync(e.SongsClass.AudioId) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Favorite Or Remove 
        public void OnFavoriteSongsClick(FavSongsClickEventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                if (Methods.CheckConnectivity())
                {
                    var refs = SetFav(e.FavButton);
                    e.SongsClass.IsFavoriated = refs;

                    //add to Favorites
                    if (refs)
                    {
                        ListUtils.FavoritesList.Add(e.SongsClass);
                        GlobalContext?.LibrarySynchronizer.AddToFavorites(e.SongsClass);
                    }
                    else
                    {
                        GlobalContext?.LibrarySynchronizer.RemoveFromFavorites(e.SongsClass);
                    }

                    //Sent Api
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.AddOrRemoveFavoriteTrackAsync(e.SongsClass.AudioId) });

                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        public async void OnShareClick(ShareSongClickEventArgs args)
        {
            try
            {
                if (!CrossShare.IsSupported)
                    return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = args.SongsClass.Title,
                    Text = args.SongsClass.Description,
                    Url = args.SongsClass.Url
                });

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdate_SharedSound(args.SongsClass);

                if (UserDetails.IsLogin)
                    GlobalContext?.LibrarySynchronizer?.AddToShareSong(args.SongsClass);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Show More :  DeleteSong , EditSong , GoSong , Copy Link  , Report .. 
        public void OnMoreClick(MoreSongClickEventArgs args, string namePage = "")
        {
            try
            {
                NamePage = namePage;
                MoreSongArgs = args;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (MoreSongArgs.SongsClass.UserId == UserDetails.UserId && UserDetails.IsLogin)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_DeleteSong));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_EditSong));
                }

                if (UserDetails.IsLogin)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_RePost));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportSong));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportCopyright));
                }

                var showDeleteOptionForOtherUsers = MoreSongArgs.SongsClass.UserId != UserDetails.UserId && AppSettings.AllowDeletingDownloadedSongs && NamePage == "LatestDownloadsFragment";
                if (showDeleteOptionForOtherUsers)
                {
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_DeleteSong));
                }

                arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_Copy));

                dialogList.Title(MainContext.GetText(Resource.String.Lbl_Songs));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == MainContext.GetText(Resource.String.Lbl_DeleteSong))
                {
                    OnMenuDeleteSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_EditSong))
                {
                    OnMenuEditSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_RePost))
                {
                    OnMenuRePostOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportSong))
                {
                    OnMenuReportSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportCopyright))
                {
                    OnMenuReportCopyrightSongOnClick(MoreSongArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_Copy))
                {
                    OnMenuCopyOnClick(MoreSongArgs.SongsClass.Url);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "DeleteSong")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    SoundDataObject dataSong = null!;
                                    dynamic mAdapter = null!;

                                    switch (NamePage)
                                    {
                                        //Delete Song from list
                                        case "FavoritesFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.FavoritesFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.FavoritesFragment?.MAdapter;
                                            break;
                                        case "LatestDownloadsFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.LatestDownloadsFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.LatestDownloadsFragment?.MAdapter;
                                            break;
                                        case "LikedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.LikedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.LikedFragment?.MAdapter;
                                            break;
                                        case "RecentlyPlayedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.RecentlyPlayedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.RecentlyPlayedFragment?.MAdapter;
                                            break;
                                        case "SharedFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.SharedFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.SharedFragment?.MAdapter;
                                            break;
                                        case "PurchasesFragment":
                                            dataSong = GlobalContext?.LibraryFragment?.PurchasesFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.LibraryFragment?.PurchasesFragment?.MAdapter;
                                            break;
                                        case "SongsByGenresFragment":
                                            dataSong = GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.MainFragment?.SongsByGenresFragment?.MAdapter;
                                            break;
                                        case "SongsByTypeFragment":
                                            dataSong = GlobalContext?.MainFragment?.SongsByTypeFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.MainFragment?.SongsByTypeFragment?.MAdapter;
                                            break;
                                        case "SearchSongsFragment":
                                            dataSong = GlobalContext?.BrowseFragment?.SearchFragment?.SongsTab?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == MoreSongArgs.SongsClass.Id);
                                            mAdapter = GlobalContext?.BrowseFragment?.SearchFragment?.SongsTab?.MAdapter;
                                            break;
                                    }

                                    if (mAdapter != null && dataSong != null)
                                    {
                                        int index = mAdapter.SoundsList.IndexOf(dataSong);
                                        mAdapter.SoundsList.Remove(dataSong);

                                        if (index >= 0)
                                        {
                                            mAdapter.NotifyItemRemoved(index);
                                        }
                                    }

                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_SongSuccessfullyDeleted), ToastLength.Short)?.Show();

                                    var showingDeleteOptionForOtherUsers = MoreSongArgs.SongsClass.UserId != UserDetails.UserId &&
                                                                        AppSettings.AllowDeletingDownloadedSongs &&
                                                                        NamePage == "LatestDownloadsFragment";
                                    if (showingDeleteOptionForOtherUsers)
                                    {
                                        RemoveDiskSoundFile(dataSong.Title, dataSong.Id);
                                    }
                                    else
                                    {
                                        //Sent Api >>
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.DeleteTrackAsync(MoreSongArgs.SongsClass.Id.ToString()) });
                                    }
                                }
                                else
                                {
                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool RemoveDiskSoundFile(string fileName, long id)
        {
            try
            {
                var filePath = "";

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    /*
                     * this changes is due to scoped storage introduce in Android 10
                     * https://developer.android.com/preview/privacy/storage
                     */

                    var file = Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryMusic);
                    filePath = Methods.MultiMedia.CheckFileIfExits(file + "/" + fileName);
                }
                else
                {
                    filePath = Methods.Path.FolderDcimSound + fileName;
                }

                if (File.Exists(filePath))
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Remove_LatestDownloadsSound(id);

                    File.Delete(filePath);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        #endregion

        //DeleteSong
        private void OnMenuDeleteSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteSong";
                    MoreSongArgs = song;

                    var showDeleteFromCacheDialog = MoreSongArgs.SongsClass.UserId != UserDetails.UserId && AppSettings.AllowDeletingDownloadedSongs && NamePage == "LatestDownloadsFragment";
                    var content = showDeleteFromCacheDialog ? MainContext.GetText(Resource.String.Lbl_Do_You_want_to_remove_Song) : MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteSong);

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteSong));
                    dialog.Content(content);
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Edit Song 
        private void OnMenuEditSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }


                Intent intent = new Intent(MainContext, typeof(EditSongActivity));
                intent.PutExtra("ItemDataSong", JsonConvert.SerializeObject(song.SongsClass));
                intent.PutExtra("NamePage", NamePage);
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //RePost
        private void OnMenuRePostOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_RePost_Message), ToastLength.Short)?.Show();
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.RePostTrackAsync(song.SongsClass.Id.ToString()) });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //ReportSong
        private void OnMenuReportSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialogBuilder.Title(Resource.String.Lbl_ReportSong).TitleColorRes(Resource.Color.primary);
                dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;

                        if (Methods.CheckConnectivity())
                        {
                            Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportSong), ToastLength.Short)?.Show();
                            //Sent Api >>
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.ReportUnReportTrackAsync(song.SongsClass.Id.ToString(), s.ToString(), true) });
                        }
                        else
                        {
                            Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e); 
                    }
                });
                dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                dialogBuilder.PositiveText(MainContext.GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialogBuilder.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialogBuilder.AlwaysCallSingleChoiceCallback();
                dialogBuilder.Build().Show(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Report Copyright Song
        private void OnMenuReportCopyrightSongOnClick(MoreSongClickEventArgs song)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                    dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialogBuilder.Title(Resource.String.Lbl_ReportCopyright).TitleColorRes(Resource.Color.primary);
                dialogBuilder.Input(0, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;

                        if (!materialDialog.PromptCheckBoxChecked)
                        {
                            Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_copyright_Error), ToastLength.Short)?.Show();
                        }
                        else
                        {
                            if (Methods.CheckConnectivity())
                            {
                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportSong), ToastLength.Short)?.Show();
                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Common.CreateCopyrightAsync(song.SongsClass.Id.ToString(), s.ToString()) });
                            }
                            else
                            {
                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }
                        } 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                dialogBuilder.CheckBoxPromptRes(Resource.String.Lbl_copyright_Des, false, null);
                dialogBuilder.PositiveText(MainContext.GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialogBuilder.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                dialogBuilder.AlwaysCallSingleChoiceCallback();
                dialogBuilder.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Copy
        public void OnMenuCopyOnClick(string urlClipboard)
        {
            try
            {
                ClipboardManager clipboard = (ClipboardManager)MainContext.GetSystemService(Context.ClipboardService);
                ClipData clip = ClipData.NewPlainText("clipboard", urlClipboard);
                clipboard.PrimaryClip = clip;

                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool SetLike(ImageView likeButton)
        {
            try
            {
                if (likeButton?.Tag?.ToString() == "Liked")
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_heart);
                    likeButton.ClearColorFilter();
                    likeButton.Tag = "Like";
                    return false;
                }
                else
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_heart_filled_post_vector);
                    likeButton.SetColorFilter(Color.ParseColor("#f55a4e"));
                    likeButton.Tag = "Liked";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool SetDislike(ImageView likeButton)
        {
            try
            {
                if (likeButton?.Tag?.ToString() == "Disliked")
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_dislike);
                    likeButton.SetColorFilter(Color.Argb(255, 255, 255, 255));
                    likeButton.Tag = "Dislike";
                    return false;
                }
                else
                {
                    likeButton.SetImageResource(Resource.Drawable.icon_player_dislike);
                    likeButton.SetColorFilter(Color.ParseColor("#f55a4e"));
                    likeButton.Tag = "Disliked";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool SetFav(ImageView favButton)
        {
            try
            {
                if (favButton?.Tag?.ToString() == "Added")
                {
                    favButton.SetImageResource(Resource.Drawable.icon_player_star);
                    favButton.SetColorFilter(Color.Argb(255, 255, 255, 255));
                    favButton.Tag = "Add";
                    return false;
                }
                else
                {
                    favButton.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                    favButton.SetColorFilter(Color.ParseColor("#ffa142"));
                    favButton.Tag = "Added";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

    }

    public class MoreSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }

    public class ShareSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }

    public class FavSongsClickEventArgs : EventArgs
    {
        public SoundDataObject SongsClass { get; set; }
        public ImageView FavButton { get; set; }
    }

    public class CommentSongClickEventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
    }

    public class LikeSongsClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public SoundDataObject SongsClass { get; set; }
        public ImageView LikeButton { get; set; }
    }
}