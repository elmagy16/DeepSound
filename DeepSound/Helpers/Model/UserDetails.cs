using System;
using Android.App;
using DeepSound.Helpers.Utils;
using DeepSoundClient;

namespace DeepSound.Helpers.Model
{
    public static class UserDetails
    {
        public static string AccessToken = string.Empty;
        public static long UserId;
        public static string Username = string.Empty;
        public static string FullName = string.Empty;
        public static string Password = string.Empty;
        public static string Email = string.Empty;
        public static string Cookie = string.Empty;
        public static string Status = string.Empty;
        public static string Avatar = string.Empty;
        public static string Cover = string.Empty;
        public static string DeviceId = string.Empty;
        public static string Lang = string.Empty;
        public static string IsPro = string.Empty;
        public static string Url = string.Empty; 
        public static string Lat = string.Empty;
        public static string Lng = string.Empty;
        public static string LangName = string.Empty;
        public static bool IsLogin = false; 
        public static string FilterGenres = "1", FilterPrice = "1";

        public static string ProductCategory = "", ProductPriceMin = "", ProductPriceMax = "";
        public static int CountNotificationsStatic = 0;

        public static int UnixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public static string Time = UnixTimestamp.ToString();

        public static string AndroidId = Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);

        public static bool IsOptimizationApp = false;

        public static void ClearAllValueUserDetails()
        {
            try
            {
                AccessToken = string.Empty;
                UserId = 0;
                Username = string.Empty;
                FullName = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Cookie = string.Empty;
                Status = string.Empty;
                Avatar = string.Empty;
                Cover = string.Empty;
                DeviceId = string.Empty;
                Lang = string.Empty;
                Lat = string.Empty;
                Lng = string.Empty;
                LangName = string.Empty;

                Current.AccessToken = string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}