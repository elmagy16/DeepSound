using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using SQLite;
//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================
namespace DeepSound.SQLite
{
    public class DataTables
    {
        [Table("LoginTb")]
        public class LoginTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoId { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccessToken { get; set; }
            public string Cookie { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Lang { get; set; }
            public string DeviceId { get; set; }
        }

        [Table("SettingsTb")]
        public class SettingsTb : OptionsObject.Data
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdSettings { get; set; }

            public new string BlogCategories { get; set; } 
            public new string ProductsCategories { get; set; } 
        }

        [Table("InfoUsersTb")]
        public class InfoUsersTb : UserDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdInfoUsers { get; set; }
             
            public new string Followers { get; set; }  
            public new string Following { get; set; }  
            public new string  Albums { get; set; } 
            public new string Playlists { get; set; } 
            public new string Blocks { get; set; } 
            public new string Favourites { get; set; } 
            public new string RecentlyPlayed { get; set; } 
            public new string Liked { get; set; } 
            public new string Activities { get; set; }  
            public new string Latestsongs { get; set; } 
            public new string TopSongs { get; set; } 
            public new string Store { get; set; }
            public new string Stations { get; set; }

        }

        [Table("LibraryItemTb")]
        public class LibraryItemTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdLibrary { get; set; }

            public string SectionId { get; set; }
            public string SectionText { get; set; }
            public int SongsCount { get; set; }
            public string BackgroundImage { get; set; }
        }

        [Table("GenresTb")]
        public class GenresTb : GenresObject.DataGenres
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdGenres { get; set; }
        }

        [Table("PriceTb")]
        public class PriceTb : PricesObject.DataPrice
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdPrice { get; set; }
        }

        [Table("SharedTb")]
        public class SharedTb : SoundDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdShared { get; set; }

            public new string Publisher { get; set; }
            public new string TagsArray { get; set; }
            public new string TagsFiltered { get; set; }
            public new string SongArray { get; set; }
            public new string Comments { get; set; } 
        }

        [Table("LatestDownloadsTb")]
        public class LatestDownloadsTb : SoundDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdaLatestDownloads { get; set; }

            public new string Publisher { get; set; }
            public new string TagsArray { get; set; }
            public new string TagsFiltered { get; set; }
            public new string SongArray { get; set; }
            public new string Comments { get; set; } 
        }

        [Table("LastChatTb")]
        public class LastChatTb : DataConversation
        {
            [PrimaryKey, AutoIncrement] 
            public int AutoIdLastChat { get; set; }

            public string Id { get; set; }
            public new string User { get; set; }
            public new string GetLastMessage { get; set; }
        }

        [Table("MessageTb")]
        public class MessageTb : ChatMessagesDataObject
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMessage { get; set; }
             
            public new string ApiPosition { get; set; }
            public new string ApiType { get; set; }
        }
    }
}