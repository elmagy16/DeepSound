using System;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using DeepSound.Helpers.Fonts;

namespace DeepSound.Helpers.Utils
{
   public class EmptyStateInflater
   {
        public AppCompatButton EmptyStateButton;
        public TextView EmptyStateIcon;
        public TextView DescriptionText;
        public TextView TitleText;

        public enum Type
        {
            NoConnection,
            NoSearchResult,
            SomeThingWentWrong,
            NoUsers,
            NoNotifications,
            NoMessage,
            NoBlock,
            NoPlaylist,
            NoAlbums,
            NoFavorites,
            NoLiked,
            NoArtists,
            NoRecentlyPlayed,
            NoSound,
            NoComments,
            NoPurchases,
            NoActivity,  
            NoStations,
            NoSessions,
            NoArticle,
            NoSoundWithPaid,
            NoProducts,
            NoEvents,
            NoCarts,
        }

        public void InflateLayout(View inflated, Type type)
        {
            try
            {

                EmptyStateIcon = (TextView) inflated.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView) inflated.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView) inflated.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (AppCompatButton) inflated.FindViewById(Resource.Id.button);

                if (type == Type.NoConnection)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon,IonIconsFonts.IosThunderstorm);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_TitleText);
                    DescriptionText.Text =Application.Context.GetText(Resource.String.Lbl_NoConnection_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_Button);
                }
                else if (type == Type.NoSearchResult)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Search);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_TitleText);
                    DescriptionText.Text =Application.Context.GetText(Resource.String.Lbl_NoSearchResult_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_Button);
                }
                else if (type == Type.SomeThingWentWrong)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                    DescriptionText.Text =Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                    EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_Button);
                }
                else if (type == Type.NoUsers)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoMoreUser);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoNotifications)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon,IonIconsFonts.Notifications);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoNotification_TitleText);
                    DescriptionText.Text =Application.Context.GetText(Resource.String.Lbl_NoNotification_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoMessage)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatboxes);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoMessage_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoMessage_DescriptionText) + " " + AppSettings.ApplicationName;
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoBlock)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoBlockUsers);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoPlaylist)
                { 
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.List);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPlaylist_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoAlbums)
                { 
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Albums);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoAlbums_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoFavorites)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoFavorites_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoLiked)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoLiked_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoRecentlyPlayed)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoRecentlyPlayed_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoSound)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoDataUser_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoArtists)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoArtists_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                } 
                else if (type == Type.NoComments)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbubbles);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_TitleText);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments_DescriptionText);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                    TitleText.SetTextColor(Color.White);
                    DescriptionText.SetTextColor(Color.White);
                }
                else if (type == Type.NoPurchases)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPurchasesSongsFound);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoActivity)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoActivitiesFound);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoStations)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.MusicalNote);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoStationsFound);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoSessions)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Fingerprint);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Sessions);
                    DescriptionText.Text = "";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoArticle)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.FileAlt);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Article);
                    DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Article);
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoSoundWithPaid)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.LogoUsd);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoSoundWithPaid_TitleText);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoProducts)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, EmptyStateIcon, FontAwesomeIcon.Store);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Products);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoEvents)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Events);
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
                else if (type == Type.NoCarts)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Cart);
                    EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                    TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Carts); 
                    DescriptionText.Text = " ";
                    EmptyStateButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }
   }
}