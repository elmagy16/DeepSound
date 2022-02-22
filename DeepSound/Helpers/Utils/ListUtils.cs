using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;

namespace DeepSound.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static OptionsObject.Data SettingsSiteList;
        public static ObservableCollection<UserDataObject> MyUserInfoList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GenresObject.DataGenres> GenresList = new ObservableCollection<GenresObject.DataGenres>();
        public static ObservableCollection<PricesObject.DataPrice> PriceList = new ObservableCollection<PricesObject.DataPrice>();
        public static ObservableCollection<PlaylistDataObject> PlaylistList = new ObservableCollection<PlaylistDataObject>();
        public static ObservableCollection<DataConversation> ChatList = new ObservableCollection<DataConversation>();
        public static List<SoundDataObject> FavoritesList = new List<SoundDataObject>();
        public static List<SoundDataObject> LikedSongs = new List<SoundDataObject>();

        public static void ClearAllList()
        {
            try
            {
                Constant.ArrayListPlay.Clear();
                DataUserLoginList.Clear();
                SettingsSiteList = null!;
                MyUserInfoList.Clear();
                GenresList.Clear();
                PriceList.Clear();
                FavoritesList.Clear();
                LikedSongs.Clear();
            }
            catch (Exception e)
            {
               Methods.DisplayReportResultTrack(e);
            }
        }

        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
               Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

    }
}