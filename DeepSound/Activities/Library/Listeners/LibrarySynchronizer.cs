using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Exception = System.Exception;

namespace DeepSound.Activities.Library.Listeners
{
    public class LibrarySynchronizer : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly HomeActivity GlobalContext;
        private readonly Activity ActivityContext;
        private MorePlaylistClickEventArgs MorePlaylistArgs;
        private MoreAlbumsClickEventArgs MoreAlbumsArgs;
        private string TypeDialog = "", OptionDialog = "";

        public LibrarySynchronizer(Activity activityContext)
        {
            try
            { 
                ActivityContext = activityContext;
                GlobalContext = HomeActivity.GetInstance();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToLiked(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "1");
                if (item == null) return;
                item.SongsCount = count != 0 ? count : item.SongsCount + 1;

                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(0, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveFromLiked(SoundDataObject song)
        {
            try
            {
                var removeItem = ListUtils.LikedSongs.Select(x => x).FirstOrDefault(x => x.Id == song.Id);
                ListUtils.LikedSongs.Remove(removeItem);

                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "1");
                if (item == null) return;
                item.SongsCount = item.SongsCount > 0 ? item.SongsCount - 1 : 0;

                var thumbnail = ListUtils.LikedSongs.Count > 0
                    ? ListUtils.LikedSongs[^1]?.Thumbnail
                    : "blackdefault";
                item.BackgroundImage = thumbnail;

                GlobalContext.LibraryFragment.MAdapter.LibraryList[0] = item;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(0, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToRecentlyPlayed(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "2");
                if (item == null) return;

                item.SongsCount = count != 0 ? count : item.SongsCount + 1;

                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(1, "picture");
                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToFavorites(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "3");
                if (item == null) return;
                item.SongsCount = count != 0 ? count : item.SongsCount + 1;

                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(2, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveFromFavorites(SoundDataObject song)
        {
            try
            {
                var removeItem = ListUtils.FavoritesList.Select(x => x).FirstOrDefault(x => x.Id == song.Id);
                ListUtils.FavoritesList.Remove(removeItem);

                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "3");
                if (item == null) return;
                item.SongsCount = item.SongsCount > 0 ? item.SongsCount - 1 : 0;

                var thumbnail = ListUtils.FavoritesList.Count > 0
                    ? ListUtils.FavoritesList[^1]?.Thumbnail
                    : "blackdefault";
                item.BackgroundImage = thumbnail;

                GlobalContext.LibraryFragment.MAdapter.LibraryList[2] = item;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(2, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveRecentlyPlayed()
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "2");
                if (item == null) return;
                item.SongsCount = 0;
                item.BackgroundImage = "blackdefault";
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(1, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToLatestDownloads(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "4");
                if (item == null) return;
                item.SongsCount = count != 0 ? count : item.SongsCount + 1;
                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(3, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToShareSong(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "5");
                if (item == null) return;

                item.SongsCount = count != 0 ? count : item.SongsCount + 1;
                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(4, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void AddToPurchases(SoundDataObject song, int count = 0)
        {
            try
            {
                var item = GlobalContext?.LibraryFragment?.MAdapter?.LibraryList?.FirstOrDefault(a => a.SectionId == "6");
                if (item == null) return;
                item.SongsCount = count != 0 ? count : item.SongsCount + 1;

                item.BackgroundImage = song.Thumbnail;
                GlobalContext?.LibraryFragment?.MAdapter?.NotifyItemChanged(0, "picture");

                var sqlEntity = new SqLiteDatabase();
                sqlEntity.InsertLibraryItem(item);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

        public void PlaylistMoreOnClick(MorePlaylistClickEventArgs args)
        {
            try
            {
                OptionDialog = "Playlist";
                MorePlaylistArgs = args;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (args.PlaylistClass.IsOwner && UserDetails.IsLogin)
                {
                    arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_DeletePlaylist));
                    arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_EditPlaylist));
                }

                arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_Copy));

                dialogList.Title(GlobalContext.GetText(Resource.String.Lbl_Playlist));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GlobalContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AlbumsOnMoreClick(MoreAlbumsClickEventArgs args)
        {
            try
            {
                OptionDialog = "Albums";
                MoreAlbumsArgs = args;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (args.AlbumsClass.IsOwner != null && (args.AlbumsClass.IsOwner.Value && UserDetails.IsLogin))
                {
                    arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_DeleteAlbum));
                    arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_EditAlbum));
                }

                arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GlobalContext.GetText(Resource.String.Lbl_Copy));

                dialogList.Title(GlobalContext.GetText(Resource.String.Lbl_Albums));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GlobalContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
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
                if (text == GlobalContext.GetText(Resource.String.Lbl_DeletePlaylist))
                {
                    OnMenuDeletePlaylistOnClick();
                }
                else if (text == GlobalContext.GetText(Resource.String.Lbl_EditPlaylist))
                {
                    OnMenuEditPlaylistOnClick();
                }
                else if (text == GlobalContext.GetText(Resource.String.Lbl_Share))
                {
                    switch (OptionDialog)
                    {
                        case "Playlist":
                            SharePlaylist();
                            break;
                        case "Albums":
                            ShareAlbums();
                            break;
                    }
                }
                else if (text == GlobalContext.GetText(Resource.String.Lbl_Copy))
                {
                    ClipboardManager clipboard = (ClipboardManager)GlobalContext.GetSystemService(Context.ClipboardService);
                    ClipData clip = null!;
                    switch (OptionDialog)
                    {
                        case "Playlist":
                            clip = ClipData.NewPlainText("clipboard", MorePlaylistArgs?.PlaylistClass?.Url);
                            break;
                        case "Albums":
                            clip = ClipData.NewPlainText("clipboard", MoreAlbumsArgs?.AlbumsClass?.Url);
                            break;
                    }
                    clipboard.PrimaryClip = clip;

                    Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
                }
                else if (text == GlobalContext.GetText(Resource.String.Lbl_DeleteAlbum))
                {
                    OnMenuDeleteAlbumOnClick();
                }
                else if (text == GlobalContext.GetText(Resource.String.Lbl_EditAlbum))
                {
                    OnMenuEditAlbumOnClick();
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
                if (TypeDialog == "DeletePlaylist")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        GlobalContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                var dataPlaylist = ListUtils.PlaylistList?.FirstOrDefault(a => a.Id == MorePlaylistArgs?.PlaylistClass?.Id);
                                if (dataPlaylist != null)
                                {
                                    ListUtils.PlaylistList.Remove(dataPlaylist);
                                }
                                //wael
                                //var dataPlaylistFragment = GlobalContext?.PlaylistFragment;
                                //dataPlaylistFragment?.UpdateMyPlaylist();

                                //var dataMyPlaylistFragment = GlobalContext?.PlaylistFragment?.MyPlaylistFragment?.PlaylistAdapter;
                                //var list2 = dataMyPlaylistFragment?.PlaylistList;
                                //var dataMyPlaylist = list2?.FirstOrDefault(a => a.Id == MorePlaylistArgs?.PlaylistClass?.Id);
                                //if (dataMyPlaylist != null)
                                //{
                                //    int index = list2.IndexOf(dataMyPlaylist);
                                //    if (index >= 0)
                                //    {
                                //        list2?.Remove(dataMyPlaylist);
                                //        dataMyPlaylistFragment?.NotifyItemRemoved(index);
                                //    }
                                //}

                                Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_PlaylistSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.DeletePlaylistAsync(MorePlaylistArgs?.PlaylistClass?.Id.ToString()) });
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
                if (TypeDialog == "DeleteAlbum")
                {
                    if (p1 == DialogAction.Positive) //Yes, But Keep The Songs
                    {
                        GlobalContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                var dataAlbumFragment = GlobalContext?.BrowseFragment.AlbumsAdapter;
                                var list2 = dataAlbumFragment?.AlbumsList;
                                var dataMyAlbum = list2?.FirstOrDefault(a => a.Id == MoreAlbumsArgs?.AlbumsClass?.Id);
                                if (dataMyAlbum != null)
                                {
                                    int index = list2.IndexOf(dataMyAlbum);
                                    if (index >= 0)
                                    {
                                        list2?.Remove(dataMyAlbum);
                                        dataAlbumFragment?.NotifyItemRemoved(index);
                                    }
                                }

                                Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Albums.DeleteAlbumAsync("single", MoreAlbumsArgs?.AlbumsClass?.Id.ToString()) });
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    else if (p1 == DialogAction.Negative) //Yes, Delete Everything
                    {
                        GlobalContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                var dataAlbumFragment = GlobalContext?.BrowseFragment.AlbumsAdapter;
                                var list2 = dataAlbumFragment?.AlbumsList;
                                var dataMyAlbum = list2?.FirstOrDefault(a => a.Id == MoreAlbumsArgs?.AlbumsClass?.Id);
                                if (dataMyAlbum != null)
                                {
                                    int index = list2.IndexOf(dataMyAlbum);
                                    if (index >= 0)
                                    {
                                        list2?.Remove(dataMyAlbum);
                                        dataAlbumFragment?.NotifyItemRemoved(index);
                                    }
                                }

                                Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Albums.DeleteAlbumAsync("all", MoreAlbumsArgs?.AlbumsClass?.Id.ToString()) });
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
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

        private void OnMenuDeleteAlbumOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteAlbum";

                    var dialog = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(GlobalContext.GetText(Resource.String.Lbl_DeleteAlbum));
                    dialog.Content(GlobalContext.GetText(Resource.String.Lbl_AreYouSureDeleteAlbum));
                    dialog.PositiveText(GlobalContext.GetText(Resource.String.Lbl_YesButKeepSongs)).OnPositive(this);
                    dialog.NegativeText(GlobalContext.GetText(Resource.String.Lbl_YesDeleteEverything)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuEditAlbumOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                Intent intent = new Intent(GlobalContext, typeof(EditAlbumActivity));
                intent.PutExtra("ItemData", JsonConvert.SerializeObject(MoreAlbumsArgs.AlbumsClass));
                GlobalContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuDeletePlaylistOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeletePlaylist";

                    var dialog = new MaterialDialog.Builder(GlobalContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(GlobalContext.GetText(Resource.String.Lbl_DeletePlaylist));
                    dialog.Content(GlobalContext.GetText(Resource.String.Lbl_AreYouSureDeletePlaylist));
                    dialog.PositiveText(GlobalContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GlobalContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuEditPlaylistOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                    dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                    return;
                }

                Intent intent = new Intent(GlobalContext, typeof(EditPlaylistActivity));
                intent.PutExtra("ItemData", JsonConvert.SerializeObject(MorePlaylistArgs.PlaylistClass));
                GlobalContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void SharePlaylist()
        {
            try
            {
                //Share Plugin same as Song
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = MorePlaylistArgs?.PlaylistClass?.Name,
                    Text = "",
                    Url = MorePlaylistArgs?.PlaylistClass?.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void ShareAlbums()
        {
            try
            {
                //Share Plugin same as Song
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = MoreAlbumsArgs?.AlbumsClass?.Title,
                    Text = "",
                    Url = MoreAlbumsArgs?.AlbumsClass?.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }

    public class MorePlaylistClickEventArgs
    {
        public PlaylistDataObject PlaylistClass { get; set; }
    }

    public class MoreAlbumsClickEventArgs
    {
        public View View { get; set; }
        public DataAlbumsObject AlbumsClass { get; set; }
    }
}