using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Requests;
using MaterialDialogsCore;
using Newtonsoft.Json;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Event
{
    public class EventProfileFragment : Fragment, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private ImageView ImageCover, ImageUser;
        private TextView IconBack, IconMore , TxtName, TxtUsername, TxtTickets, TxtJoined, TxtDate, TxtDescription;
        private SuperTextView TxtLocation;
        private LinearLayout VideoTrailerLayout;

        private LinearLayout ButtonLayout;
        private AppCompatButton ButtonJoin, ButtonBuyTicket;
        private string EventId;
        private EventDataObject EventObject;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.EventProfileLayout, container, false);
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

                EventId = Arguments.GetString("EventId") ?? "";
                EventObject = JsonConvert.DeserializeObject<EventDataObject>(Arguments.GetString("ItemData") ?? "");

                InitComponent(view);

                StartApiService();
                AdsGoogle.Ad_Interstitial(Activity);
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

        public override void OnResume()
        {
            try
            {
                base.OnResume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
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
                ImageCover = view.FindViewById<ImageView>(Resource.Id.imageCover);
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack); 
                IconMore = view.FindViewById<TextView>(Resource.Id.IconMore);

                ImageUser = view.FindViewById<ImageView>(Resource.Id.imageUser);
                TxtName = view.FindViewById<TextView>(Resource.Id.name);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.username);

                TxtTickets = view.FindViewById<TextView>(Resource.Id.tickets);
                TxtJoined = view.FindViewById<TextView>(Resource.Id.joined);
                TxtLocation = view.FindViewById<SuperTextView>(Resource.Id.location); 
                TxtDate = view.FindViewById<TextView>(Resource.Id.date);

                VideoTrailerLayout = view.FindViewById<LinearLayout>(Resource.Id.videoTrailerLayout);
                TxtDescription = view.FindViewById<TextView>(Resource.Id.description);

                ButtonLayout = view.FindViewById<LinearLayout>(Resource.Id.ButtonLayout);
                ButtonJoin = view.FindViewById<AppCompatButton>(Resource.Id.ButtonJoin);
                ButtonBuyTicket = view.FindViewById<AppCompatButton>(Resource.Id.ButtonBuyTicket);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ArrowForward : IonIconsFonts.ArrowBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMore, IonIconsFonts.More);

                VideoTrailerLayout.Visibility = ViewStates.Gone;

                //wael add count after update 
                TxtJoined.Visibility = ViewStates.Invisible;

                IconBack.Click += BackIconOnClick;
                IconMore.Click += IconMoreOnClick;
                VideoTrailerLayout.Click += VideoTrailerLayoutOnClick;
                ButtonJoin.Click += ButtonJoinOnClick;
                ButtonBuyTicket.Click += ButtonBuyTicketOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
            {
                switch (item.ItemId)
                {
                    case Android.Resource.Id.Home:
                        GlobalContext.FragmentNavigatorBack();
                        return true;
                }
                return base.OnOptionsItemSelected(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        #endregion

        #region Event

        private void ButtonBuyTicketOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DeepSoundTools.CheckWallet())
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        return;
                    }

                    var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(GetText(Resource.String.Lbl_BuyTicket));
                    dialogBuilder.Content(GetText(Resource.String.Lbl_AreYouSureBuyTicket));
                    dialogBuilder.PositiveText(GetText(Resource.String.Lbl_Buy)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.BuyTicketEventAsync(EventObject?.Id?.ToString()) });

                            Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                            //wael after update and get id from api
                            //var (apiStatus, respond) = await RequestsAsync.Event.DownloadTicketEventAsync("398");
                            //if (apiStatus == 200)
                            //{
                            //    if (respond is DownloadTicketEventObject result)
                            //    {
                            //        Activity.RunOnUiThread(() =>
                            //        {
                            //            try
                            //            {
                            //                PdfConverter converter = PdfConverter.Instance;
                            //                File file = new File(Methods.Path.FolderDcimMyApp, EventObject.Name + ".pdf");

                            //                //var content = Html.FromHtml(result.Html, FromHtmlOptions.ModeCompact)?.ToString();
                            //                var DataWebHtml = "<!DOCTYPE html>";
                            //                DataWebHtml += "<head><title></title></head>";
                            //                DataWebHtml += "<body>" + result.Html + "</body>";
                            //                DataWebHtml += "</html>";

                            //                converter.Convert(Context, DataWebHtml, file);
                            //                // By now the pdf has been printed in the file.
                                             
                            //            }
                            //            catch (Exception exception)
                            //            {
                            //                Methods.DisplayReportResultTrack(exception);
                            //            }
                            //        });
                            //    }
                            //}
                            //else Methods.DisplayReportResult(Activity, respond); 
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.Build().Show(); 
                }
                else
                {
                    var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(GetText(Resource.String.Lbl_Wallet));
                    dialogBuilder.Content(GetText(Resource.String.Lbl_Error_NoWallet));
                    dialogBuilder.PositiveText(GetText(Resource.String.Lbl_AddWallet)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            Activity.StartActivity(new Intent(Activity, typeof(WalletActivity)));
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.Build().Show(); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ButtonJoinOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }
                 
                switch (ButtonJoin?.Tag?.ToString())
                {
                    case "false":
                        ButtonJoin.Text = GetText(Resource.String.Lbl_Joined);
                        ButtonJoin.Tag = "true";
                        ButtonJoin.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        ButtonJoin.SetTextColor(Color.White);

                        EventObject.IsJoined = 1;
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.JoinEventAsync(EventObject.Id?.ToString(), "join") });
                        break;
                    default:
                        ButtonJoin.Text = GetText(Resource.String.Lbl_Join);
                        ButtonJoin.Tag = "false";
                        ButtonJoin.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        ButtonJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                        EventObject.IsJoined = 0;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.JoinEventAsync(EventObject.Id?.ToString(), "unjoin") });
                        break;
                }

                var list = GlobalContext?.PlaylistFragment?.EventFragment?.MAdapter?.EventsList;
                var dataEvent = list?.FirstOrDefault(a => a.Id == EventObject.Id);
                if (dataEvent != null)
                {
                    dataEvent.IsJoined = EventObject.IsJoined;
                    GlobalContext?.PlaylistFragment?.EventFragment?.MAdapter.NotifyItemChanged(list.IndexOf(dataEvent));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void VideoTrailerLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(EventObject?.Video))
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(EventObject.Video));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (EventObject.UserId == UserDetails.UserId && UserDetails.IsLogin)
                {
                    //arrayAdapter.Add(GetText(Resource.String.Lbl_DeleteEvent)); wael Next Update
                    arrayAdapter.Add(GetText(Resource.String.Lbl_EditEvent));
                }

                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                dialogList.Title(GetText(Resource.String.Lbl_Event));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
           
        #endregion
         
        #region Load Data Event 

        private void StartApiService()
        {
            SetDataEvent();

            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadEventById });
        }

        private async Task LoadEventById()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Event.GetEventByIdAsync(EventId);
                if (apiStatus == 200)
                {
                    if (respond is GetEventDataObject result)
                    {
                        EventObject = result.Data;
                        Activity.RunOnUiThread(SetDataEvent);
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);
            }
            else
            {
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        private void SetDataEvent()
        {
            try
            {
                if (EventObject != null)
                {   
                    GlideImageLoader.LoadImage(Activity, EventObject.Image, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    if (EventObject.UserData != null)
                    {
                        GlideImageLoader.LoadImage(Activity, EventObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUsername.Text = "@" + EventObject.UserData.Username;
                    }

                    TxtName.Text = Methods.FunString.DecodeString(EventObject.Name);
                    TxtTickets.Text = EventObject.AvailableTickets + " " + GetText(Resource.String.Lbl_AvailableTickets);

                    //wael add count after update 
                    //TxtJoined.Text = "0" + " " + GetText(Resource.String.Lbl_JoinedPeople);

                    if (!string.IsNullOrEmpty(EventObject.OnlineUrl))
                    {
                        var descriptionAutoLink = new TextSanitizer(TxtLocation, Activity);
                        descriptionAutoLink.Load(EventObject.OnlineUrl);
                    }
                    else if (!string.IsNullOrEmpty(EventObject.RealAddress))
                    {
                        TxtLocation.Text = EventObject.RealAddress;
                    }

                    // 2021-11-24 (04:06)  •  2021-12-24 (05:00)
                    TxtDate.Text = EventObject.StartDate + " (" + EventObject.StartTime + ")  •  " + EventObject.EndDate + " (" + EventObject.EndTime + ")";
                    TxtDescription.Text = Methods.FunString.DecodeString(EventObject.Desc);

                    if (EventObject.UserId == UserDetails.UserId)
                    {
                        ButtonLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        ButtonLayout.Visibility = ViewStates.Visible;
                        if (EventObject.IsJoined is 1)
                        {
                            ButtonJoin.Text = GetText(Resource.String.Lbl_Joined);
                            ButtonJoin.Tag = "true";
                            ButtonJoin.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                            ButtonJoin.SetTextColor(Color.White);
                        }
                        else
                        {
                            ButtonJoin.Text = GetText(Resource.String.Lbl_Join);
                            ButtonJoin.Tag = "false";
                            ButtonJoin.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            ButtonJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        }

                        if (EventObject.TicketPrice is > 0 && EventObject.TicketPrice is > 0)
                        {
                            ButtonBuyTicket.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            ButtonBuyTicket.Visibility = ViewStates.Gone;
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string text)
        {
            try
            {
                if (text == GetText(Resource.String.Lbl_DeleteEvent))
                {
                    if (!UserDetails.IsLogin)
                    {
                        PopupDialogController dialogController = new PopupDialogController(Activity, null, "Login");
                        dialogController.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                        return;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialogBuilder.Title(GetText(Resource.String.Lbl_DeleteEvent));
                        dialogBuilder.Content(GetText(Resource.String.Lbl_AreYouSureDeleteEvent));
                        dialogBuilder.PositiveText(GetText(Resource.String.Lbl_YesButKeepSongs)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                var dataEventFragment = GlobalContext?.PlaylistFragment.EventFragment;
                                var list2 = dataEventFragment?.MAdapter?.EventsList;
                                var dataMyEvent = list2?.FirstOrDefault(a => a.Id == EventObject?.Id);
                                if (dataMyEvent != null)
                                {
                                    int index = list2.IndexOf(dataMyEvent);
                                    if (index >= 0)
                                    {
                                        list2?.Remove(dataMyEvent);
                                        dataEventFragment?.MAdapter?.NotifyItemRemoved(index);
                                    }
                                }

                                Toast.MakeText(Activity, GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                                //Sent Api >>
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.DeleteEventAsync(EventObject?.Id.ToString()) });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialogBuilder.NegativeText(GetText(Resource.String.Lbl_YesDeleteEverything)).OnNegative(new MyMaterialDialog());
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                        dialogBuilder.Build().Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == GetText(Resource.String.Lbl_EditEvent))
                {
                    if (!UserDetails.IsLogin)
                    {
                        PopupDialogController dialogController = new PopupDialogController(Activity, null, "Login");
                        dialogController.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                        return;
                    }

                    var intent = new Intent(Activity, typeof(EditEventActivity));
                    intent.PutExtra("EventView", JsonConvert.SerializeObject(EventObject));
                    Activity.StartActivity(intent);
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    //Share Plugin same as Song
                    if (!CrossShare.IsSupported)
                    {
                        return;
                    }

                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = EventObject?.Name,
                        Text = "",
                        Url = EventObject?.Url
                    });
                }
                else if (text == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(Activity, EventObject?.Url);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}