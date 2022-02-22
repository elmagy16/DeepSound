using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.App;
using DeepSound.Helpers.Model;
using DeepSoundClient;
using DeepSoundClient.Classes.Global;

namespace DeepSound.Helpers.Utils
{
    public static class DeepSoundTools
    {
        private static readonly string[] CountriesArray = Application.Context.Resources?.GetStringArray(Resource.Array.countriesArray);

        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (dataUser == null)
                    return "";

                if (!string.IsNullOrEmpty(dataUser.Name) && !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) && !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return Methods.FunString.DecodeString(dataUser.Username);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }
         
        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (dataUser == null)
                    return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);

                if (!string.IsNullOrEmpty(dataUser.AboutDecoded) && !string.IsNullOrWhiteSpace(dataUser.AboutDecoded))
                    return Methods.FunString.DecodeString(dataUser.AboutDecoded);

                return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
        }
         
        public static string GetCountry(long codeCountry)
        {
            try
            { 
                if (codeCountry > -1)
                {
                    string name = CountriesArray[codeCountry];
                    return name;
                }
                return "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static string GetGender(string type)
        {
            try
            {
                string text;
                switch (type)
                {
                    case "Male":
                    case "male":
                        text = Application.Context.GetText(Resource.String.Lbl_Male);
                        break;
                    case "Female":
                    case "female":
                        text = Application.Context.GetText(Resource.String.Lbl_Female);
                        break;
                    default:
                        text = "";
                        break;
                }
                return text;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static List<SoundDataObject> ListFilter(List<SoundDataObject> list)
        {
            try
            {
                if (list == null || list?.Count == 0)
                    return new List<SoundDataObject>();

                const string sDuration = "0:0";
                const string mDuration = "00:00";
                const string hDuration = "00:00:00";

                list.RemoveAll(a => a.Duration is sDuration or mDuration or hDuration || string.IsNullOrEmpty(a.AudioId));
                list.RemoveAll(a => a.Availability == 1 && a.UserId != UserDetails.UserId);

                foreach (var sound in list)
                {
                    if (!sound.Thumbnail.StartsWith("http"))
                    {
                        sound.Thumbnail = GetTheFinalLink(sound.Thumbnail);
                    }

                    if (!sound.AudioLocation.StartsWith("http"))
                    {
                        sound.AudioLocation = GetTheFinalLink(sound.AudioLocation);
                    } 
                }

                return new List<SoundDataObject>(list);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                if (list == null || list?.Count == 0)
                    return new List<SoundDataObject>();
                else
                    return new List<SoundDataObject>(list);
            }
        }

        /// <summary>
        /// var ImageUrl = !item.Thumbnail.Contains(DeepSoundClient.InitializeDeepSound.WebsiteUrl) ? DeepSoundTools.GetTheFinalLink(item.Thumbnail) : item.Thumbnail;
        /// ['amazone_s3'] == 1   >> https://bucket.s3.amazonaws.com . '/' . $media;
        /// ['ftp_upload'] == 1   >> "http://".$wo['config']['ftp_endpoint'] . '/' . $media;
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static string GetTheFinalLink(string media)
        {
            try
            {
                var path = media;

                if (!media.Contains(InitializeDeepSound.WebsiteUrl))
                    path = InitializeDeepSound.WebsiteUrl + "/" + media;
                 
                var config = ListUtils.SettingsSiteList;
                if (!string.IsNullOrEmpty(config?.S3Upload) && config?.S3Upload == "on")
                {
                    return "https://" + config.S3BucketName + ".s3.amazonaws.com"  + "/" + media;
                }
                  
                if (!string.IsNullOrEmpty(config?.FtpUpload) && config?.FtpUpload == "on")
                {
                    return config.FtpEndpoint + "/" + media;
                }
                 
                if (!string.IsNullOrEmpty(config?.Spaces) && config?.Spaces == "on")
                {

                    if (string.IsNullOrEmpty(config?.SpacesKey) || string.IsNullOrEmpty(config?.SpacesSecret) || string.IsNullOrEmpty(config?.SpaceRegion) || string.IsNullOrEmpty(config?.SpaceName))
                    {
                        return InitializeDeepSound.WebsiteUrl + "/" + media;
                    }

                    return "https://" + config?.SpaceName + "." + config?.SpaceRegion + ".digitaloceanspaces.com/" + media;
                }
                
                return path;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return media;
            }
        }


        public static bool CheckAllowedFileUpload()
        {
            try
            {
                var dataSettings = ListUtils.SettingsSiteList;
                if (dataSettings?.WhoCanUpload == "admin") //just admin 
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Admin;
                    if (dataUser == 0) // Not Admin
                    {
                        return false;
                    }
                }
                else if (dataSettings?.WhoCanUpload == "artist") //just artist user  
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Artist;
                    if (dataUser == 0) // Not Artist 
                    {
                        return false;
                    }
                }
                else  //"all"
                {
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
        }
         
        public static bool GetStatusAds()
        {
            try
            {
                switch (AppSettings.ShowAds)
                {
                    case ShowAds.AllUsers:
                        return true;
                    case ShowAds.UnProfessional:
                    {
                        var isPro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro ?? 0;
                        if (isPro == 0)
                            return true;
                        else
                            return false;
                    }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckWallet()
        {
            try
            {
                var wallet = ListUtils.MyUserInfoList.FirstOrDefault()?.Wallet ?? "0";
                if (!string.IsNullOrEmpty(wallet) && wallet != "0")
                { 
                    var number = Convert.ToInt64(wallet);
                    if (number > 0)
                    { 
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static Dictionary<string, string> GetTimezonesList()
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"Pacific/Midway", "(GMT-11:00) Midway Island"},
                    {"US/Samoa", "(GMT-11:00) Samoa"},
                    {"US/Hawaii", "(GMT-10:00) Hawaii"},
                    {"US/Alaska", "(GMT-09:00) Alaska"},
                    {"US/Pacific", "(GMT-08:00) Pacific Time (US &amp; Canada)"},
                    {"America/Tijuana", "(GMT-08:00) Tijuana"},
                    {"US/Arizona", "(GMT-07:00) Arizona"},
                    {"US/Mountain", "(GMT-07:00) Mountain Time (US &amp; Canada)"},
                    {"America/Chihuahua", "(GMT-07:00) Chihuahua"},
                    {"America/Mazatlan", "(GMT-07:00) Mazatlan"},
                    {"America/Mexico_City", "(GMT-06:00) Mexico City"},
                    {"America/Monterrey", "(GMT-06:00) Monterrey"},
                    {"Canada/Saskatchewan", "(GMT-06:00) Saskatchewan"},
                    {"US/Central", "(GMT-06:00) Central Time (US &amp; Canada)"},
                    {"US/Eastern", "(GMT-05:00) Eastern Time (US &amp; Canada)"},
                    {"US/East-Indiana", "(GMT-05:00) Indiana (East)"},
                    {"America/Bogota", "(GMT-05:00) Bogota"},
                    {"America/Lima", "(GMT-05:00) Lima"},
                    {"America/Caracas", "(GMT-04:30) Caracas"},
                    {"Canada/Atlantic", "(GMT-04:00) Atlantic Time (Canada)"},
                    {"America/La_Paz", "(GMT-04:00) La Paz"},
                    {"America/Santiago", "(GMT-04:00) Santiago"},
                    {"Canada/Newfoundland", "(GMT-03:30) Newfoundland"},
                    {"America/Buenos_Aires", "(GMT-03:00) Buenos Aires"},
                    {"Greenland", "(GMT-03:00) Greenland"},
                    {"Atlantic/Stanley", "(GMT-02:00) Stanley"},
                    {"Atlantic/Azores", "(GMT-01:00) Azores"},
                    {"Atlantic/Cape_Verde", "(GMT-01:00) Cape Verde Is."},
                    {"Africa/Casablanca", "(GMT) Casablanca"},
                    {"Europe/Dublin", "(GMT) Dublin"},
                    {"Europe/Lisbon", "(GMT) Lisbon"},
                    {"Europe/London", "(GMT) London"},
                    {"Africa/Monrovia", "(GMT) Monrovia"},
                    {"Europe/Amsterdam", "(GMT+01:00) Amsterdam"},
                    {"Europe/Belgrade", "(GMT+01:00) Belgrade"},
                    {"Europe/Berlin", "(GMT+01:00) Berlin"},
                    {"Europe/Bratislava", "(GMT+01:00) Bratislava"},
                    {"Europe/Brussels", "(GMT+01:00) Brussels"},
                    {"Europe/Budapest", "(GMT+01:00) Budapest"},
                    {"Europe/Copenhagen", "(GMT+01:00) Copenhagen"},
                    {"Europe/Ljubljana", "(GMT+01:00) Ljubljana"},
                    {"Europe/Madrid", "(GMT+01:00) Madrid"},
                    {"Europe/Paris", "(GMT+01:00) Paris"},
                    {"Europe/Prague", "(GMT+01:00) Prague"},
                    {"Europe/Rome", "(GMT+01:00) Rome"},
                    {"Europe/Sarajevo", "(GMT+01:00) Sarajevo"},
                    {"Europe/Skopje", "(GMT+01:00) Skopje"},
                    {"Europe/Stockholm", "(GMT+01:00) Stockholm"},
                    {"Europe/Vienna", "(GMT+01:00) Vienna"},
                    {"Europe/Warsaw", "(GMT+01:00) Warsaw"},
                    {"Europe/Zagreb", "(GMT+01:00) Zagreb"},
                    {"Europe/Athens", "(GMT+02:00) Athens"},
                    {"Europe/Bucharest", "(GMT+02:00) Bucharest"},
                    {"Africa/Cairo", "(GMT+02:00) Cairo"},
                    {"Africa/Harare", "(GMT+02:00) Harare"},
                    {"Europe/Helsinki", "(GMT+02:00) Helsinki"},
                    {"Europe/Istanbul", "(GMT+02:00) Istanbul"},
                    {"Asia/Jerusalem", "(GMT+02:00) Jerusalem"},
                    {"Europe/Kiev", "(GMT+02:00) Kyiv"},
                    {"Europe/Minsk", "(GMT+02:00) Minsk"},
                    {"Europe/Riga", "(GMT+02:00) Riga"},
                    {"Europe/Sofia", "(GMT+02:00) Sofia"},
                    {"Europe/Tallinn", "(GMT+02:00) Tallinn"},
                    {"Europe/Vilnius", "(GMT+02:00) Vilnius"},
                    {"Asia/Baghdad", "(GMT+03:00) Baghdad"},
                    {"Asia/Kuwait", "(GMT+03:00) Kuwait"},
                    {"Africa/Nairobi", "(GMT+03:00) Nairobi"},
                    {"Asia/Riyadh", "(GMT+03:00) Riyadh"},
                    {"Europe/Moscow", "(GMT+03:00) Moscow"},
                    {"Asia/Tehran", "(GMT+03:30) Tehran"},
                    {"Asia/Baku", "(GMT+04:00) Baku"},
                    {"Europe/Volgograd", "(GMT+04:00) Volgograd"},
                    {"Asia/Muscat", "(GMT+04:00) Muscat"},
                    {"Asia/Tbilisi", "(GMT+04:00) Tbilisi"},
                    {"Asia/Yerevan", "(GMT+04:00) Yerevan"},
                    {"Asia/Kabul", "(GMT+04:30) Kabul"},
                    {"Asia/Karachi", "(GMT+05:00) Karachi"},
                    {"Asia/Tashkent", "(GMT+05:00) Tashkent"},
                    {"Asia/Kolkata", "(GMT+05:30) Kolkata"},
                    {"Asia/Kathmandu", "(GMT+05:45) Kathmandu"},
                    {"Asia/Yekaterinburg", "(GMT+06:00) Ekaterinburg"},
                    {"Asia/Almaty", "(GMT+06:00) Almaty"},
                    {"Asia/Dhaka", "(GMT+06:00) Dhaka"},
                    {"Asia/Novosibirsk", "(GMT+07:00) Novosibirsk"},
                    {"Asia/Bangkok", "(GMT+07:00) Bangkok"},
                    {"Asia/Jakarta", "(GMT+07:00) Jakarta"},
                    {"Asia/Krasnoyarsk", "(GMT+08:00) Krasnoyarsk"},
                    {"Asia/Chongqing", "(GMT+08:00) Chongqing"},
                    {"Asia/Hong_Kong", "(GMT+08:00) Hong Kong"},
                    {"Asia/Kuala_Lumpur", "(GMT+08:00) Kuala Lumpur"},
                    {"Australia/Perth", "(GMT+08:00) Perth"},
                    {"Asia/Singapore", "(GMT+08:00) Singapore"},
                    {"Asia/Taipei", "(GMT+08:00) Taipei"},
                    {"Asia/Ulaanbaatar", "(GMT+08:00) Ulaan Bataar"},
                    {"Asia/Urumqi", "(GMT+08:00) Urumqi"},
                    {"Asia/Irkutsk", "(GMT+09:00) Irkutsk"},
                    {"Asia/Seoul", "(GMT+09:00) Seoul"},
                    {"Asia/Tokyo", "(GMT+09:00) Tokyo"},
                    {"Australia/Adelaide", "(GMT+09:30) Adelaide"},
                    {"Australia/Darwin", "(GMT+09:30) Darwin"},
                    {"Asia/Yakutsk", "(GMT+10:00) Yakutsk"},
                    {"Australia/Brisbane", "(GMT+10:00) Brisbane"},
                    {"Australia/Canberra", "(GMT+10:00) Canberra"},
                    {"Pacific/Guam", "(GMT+10:00) Guam"},
                    {"Australia/Hobart", "(GMT+10:00) Hobart"},
                    {"Australia/Melbourne", "(GMT+10:00) Melbourne"},
                    {"Pacific/Port_Moresby", "(GMT+10:00) Port Moresby"},
                    {"Australia/Sydney", "(GMT+10:00) Sydney"},
                    {"Asia/Vladivostok", "(GMT+11:00) Vladivostok"},
                    {"Asia/Magadan", "(GMT+12:00) Magadan"},
                    {"Pacific/Auckland", "(GMT+12:00) Auckland"},
                    {"Pacific/Fiji", "(GMT+12:00) Fiji"},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

    }
     
    #region MaterialDialog

    public class MyMaterialDialog : Java.Lang.Object, MaterialDialog.ISingleButtonCallback
    {
        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    #endregion

}