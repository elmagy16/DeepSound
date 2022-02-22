//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using Environment = Android.OS.Environment;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SoundDownloadAsyncController
    {
        private readonly DownloadManager Downloadmanager;
        private readonly DownloadManager.Request Request;
        private readonly string FilePath = Android.OS.Environment.DirectoryDownloads;
        private readonly string Filename;
        private long DownloadId;
        private string FromActivity;
        private SoundDataObject SoundData;
        private static Activity ActivityContext;

        public SoundDownloadAsyncController(string url, string filename, Activity contextActivity)
        {
            try
            {
                ActivityContext = HomeActivity.GetInstance();

                if (!filename.Contains(".mp3"))
                    Filename = filename + ".mp3";

                Downloadmanager = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
                Request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StartDownloadManager(string title, SoundDataObject sound, string fromActivity)
        {
            try
            {
                if (sound != null && !string.IsNullOrEmpty(title))
                {
                    SoundData = sound;
                    FromActivity = fromActivity;

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Insert_LatestDownloadsSound(sound);

                    Request.SetTitle(title);
                    Request.SetAllowedNetworkTypes(DownloadNetwork.Mobile | DownloadNetwork.Wifi);

                    Request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, Filename);

                    Request.SetNotificationVisibility(DownloadVisibility.Visible);
                    Request.SetAllowedOverRoaming(true);
                    DownloadId = Downloadmanager.Enqueue(Request);

                    var onDownloadComplete = new OnDownloadComplete
                    {
                        ActivityContext = ActivityContext,
                        TypeActivity = fromActivity,
                        Sound = sound
                    };

                    ActivityContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Download_failed), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopDownloadManager()
        {
            try
            {
                Downloadmanager.Remove(DownloadId);
                RemoveDiskSoundFile(Filename, SoundData.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool RemoveDiskSoundFile(string filename, long id)
        {
            try
            {
                string path = new Java.IO.File(Environment.DirectoryDownloads + "/" + filename + ".mp3").Path;

                if (File.Exists(path))
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Remove_LatestDownloadsSound(id);
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool CheckDownloadLinkIfExits()
        {
            try
            {
                if (File.Exists(FilePath + Filename))
                    return true;

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public static string GetDownloadedDiskVideoUri(string filename)
        {
            try
            {
                Java.IO.File file;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    var directories = Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);
                    file = new Java.IO.File(directories, filename + ".mp3");
                }
                else
                {
                    file = new Java.IO.File(Methods.Path.GetDirectoryDcim() + "/" + Environment.DirectoryDownloads, filename + ".mp3");
                }

                //Hbh14ktZ3i4frTd  
                if (file.Exists())
                {
                    return file.Path;
                }

                return "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "";
            }
        }

        [BroadcastReceiver(Exported = false)]
        [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
        public class OnDownloadComplete : BroadcastReceiver
        {
            public Context ActivityContext;
            public string TypeActivity;
            public SoundDataObject Sound;

            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (intent.Action == DownloadManager.ActionDownloadComplete)
                    {
                        if (ActivityContext == null)
                            return;

                        DownloadManager downloadManagerExcuter = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
                        long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                        DownloadManager.Query query = new DownloadManager.Query();
                        query.SetFilterById(downloadId);
                        ICursor c = downloadManagerExcuter.InvokeQuery(query);
                        var sqlEntity = new SqLiteDatabase();

                        if (c.MoveToFirst())
                        {
                            int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
                            if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
                            {
                                string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

                                ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
                                ActivityManager.GetMyMemoryState(appProcessInfo);
                                if (appProcessInfo.Importance == Importance.Foreground || appProcessInfo.Importance == Importance.Background)
                                {
                                    sqlEntity.Update_LatestDownloadsSound(Sound.Id, downloadedPath);
                                    if (TypeActivity == "Main")
                                    {
                                        if (ActivityContext is HomeActivity tabbedMain)
                                        {
                                            tabbedMain.SoundController.BtnIconDownload.Tag = "Downloaded";
                                            tabbedMain.SoundController.BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                            tabbedMain.SoundController.BtnIconDownload.SetColorFilter(Color.Red);

                                            tabbedMain.SoundController.ProgressBarDownload.Visibility = ViewStates.Invisible;
                                            tabbedMain.SoundController.BtnIconDownload.Visibility = ViewStates.Visible;

                                            tabbedMain.LibrarySynchronizer.AddToLatestDownloads(Sound);
                                        }
                                    }
                                }
                                else
                                { 
                                    sqlEntity.Update_LatestDownloadsSound(Sound.Id, downloadedPath);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }
    }
}