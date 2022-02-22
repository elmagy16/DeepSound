using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using Com.Onesignal;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif.Models;
using DeepSoundClient.Classes.Global;
using Newtonsoft.Json;

namespace DeepSound.Library.OneSignalNotif
{
    public class OneSignalNotification : Java.Lang.Object, OneSignal.IOSNotificationWillShowInForegroundHandler, OneSignal.IOSNotificationOpenedHandler, IOSSubscriptionObserver
    {
        //Force your app to Register Notification directly without loading it from server (For Best Result)

        public static string Type;
        public static string Userid, TrackId, TypeText = "User";
        public static UserDataObject UserData; 

        private static volatile OneSignalNotification InstanceRenamed;
        public static OneSignalNotification Instance
        {
            get
            {
                OneSignalNotification localInstance = InstanceRenamed;
                if (localInstance == null)
                {
                    lock (typeof(OneSignalNotification))
                    {
                        localInstance = InstanceRenamed;
                        if (localInstance == null)
                        {
                            InstanceRenamed = localInstance = new OneSignalNotification();
                        }
                    }
                }
                return localInstance;

            }
        }

        public void RegisterNotificationDevice(Context context)
        {
            try
            {
                if (AppSettings.ShowNotification)
                {
                    if (!string.IsNullOrEmpty(AppSettings.OneSignalAppId) || !string.IsNullOrWhiteSpace(AppSettings.OneSignalAppId))
                    {
                        //The following options are available with increasingly more information:
                        //NONE, FATAL, ERROR, WARN, INFO, DEBUG, VERBOSE
                        OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.Verbose, OneSignal.LOG_LEVEL.None);

                        // OneSignal Initialization
                        OneSignal.SetAppId(AppSettings.OneSignalAppId);
                        OneSignal.InitWithContext(context);

                        // OneSignal Methods
                        OneSignal.SetNotificationWillShowInForegroundHandler(this);
                        OneSignal.SetNotificationOpenedHandler(this);

                        OneSignal.UnsubscribeWhenNotificationsAreDisabled(true);
                        OneSignal.PauseInAppMessages(true);
                        OneSignal.LocationShared = false;

                        // OneSignal Register
                        OneSignal.AddSubscriptionObserver(this);
                        OneSignal.DisablePush(false);
                    }
                }
                else
                {
                    UnRegisterNotificationDevice();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void UnRegisterNotificationDevice()
        {
            try
            {
                OneSignal.RemoveExternalUserId();

                OneSignal.ClearOneSignalNotifications();
                OneSignal.DisablePush(true);
                OneSignal.UnsubscribeWhenNotificationsAreDisabled(false);

                AppSettings.ShowNotification = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void IdsAvailable()
        {
            try
            {
                OSDeviceState device = OneSignal.DeviceState;

                if (device != null)
                {
                    //string email = device.EmailAddress;
                    //string emailId = device.EmailUserId;
                    //string pushToken = device.PushToken;
                    string userId = device.UserId;

                    //bool enabled = device.AreNotificationsEnabled();
                    bool subscribed = device.IsSubscribed;
                    //bool subscribedToOneSignal = device.IsEmailSubscribed;

                    if (subscribed && !string.IsNullOrEmpty(userId))
                        UserDetails.DeviceId = userId;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void NotificationWillShowInForeground(OSNotificationReceivedEvent result)
        {
            try
            {
                //var jsonObject = result.ToJSONObject().ToString();
                //Console.WriteLine(jsonObject);

                //var notification = JsonConvert.DeserializeObject<OsObject.OsNotificationReceivedObject>(jsonObject);

                //string title = notification.Notification.Title;
                //string message = notification.Notification.Body;
                //Dictionary<string, object> RawPayload = notification.Notification.AdditionalData;

                // OneSignal.ClearOneSignalNotifications();    
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.ToString(), ToastLength.Long)?.Show(); //Allen
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void NotificationOpened(OSNotificationOpenedResult result)
        {
            try
            {
                //string actionId = result.Action.ActionId;
                //OSNotificationAction.ActionType type = result.Action.Type; // "ActionTaken" | "Opened"

                var jsonObject = result.ToJSONObject().ToString();
                Console.WriteLine(jsonObject);
                var notification = JsonConvert.DeserializeObject<OsObject.OsNotificationReceivedObject>(jsonObject);

                string title = notification.Notification.Title;
                string message = notification.Notification.Body;
                Dictionary<string, object> additionalData = notification.Notification.AdditionalData;
                  
                if (additionalData?.Count > 0)
                {
                    TypeText = "User";
                    foreach (var item in additionalData)
                    {
                        switch (item.Key)
                        {
                            case "user_id":
                                Userid = item.Value.ToString();
                                break;
                            case "track":
                            {
                                TrackId = item.Value.ToString();
                                if (!string.IsNullOrEmpty(TrackId))
                                    TypeText = "Track";
                                break;
                            }
                            case "user_data":
                                UserData = JsonConvert.DeserializeObject<UserDataObject>(item.Value.ToString());
                                break;
                            case "url":
                            {
                                string url = item.Value.ToString();
                                Console.WriteLine(url);
                                break;
                            }
                        }
                    }
                }

                //to : do
                //go to activity or fragment depending on data 
                Intent intent = new Intent(Application.Context, typeof(HomeActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.SingleTop);
                intent.SetAction(Intent.ActionView);
                intent.PutExtra("TypeNotification", Type);
                Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void OnOSSubscriptionChanged(OSSubscriptionStateChanges p0)
        {
            try
            {
                var jsonObject = p0.ToJSONObject().ToString();
                var notification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(jsonObject);
                Console.WriteLine(notification);

                if (notification?.To.IsSubscribed != null && notification.To.IsSubscribed.Value && !string.IsNullOrEmpty(notification.To.UserId))
                    UserDetails.DeviceId = notification.To.UserId;

                IdsAvailable();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        } 
    }
}