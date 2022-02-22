using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using DeepSound.Activities.Search;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSoundClient.Classes.Chat;

namespace DeepSound.Helpers.Controller
{
    public class TextSanitizer : StTools.IXAutoLinkOnClickListener
    {
        private readonly HomeActivity Context;
        private readonly SuperTextView SuperTextView;
        private readonly Activity Activity;

        public TextSanitizer(SuperTextView linkTextView, Activity activity)
        {
            try
            {
                SuperTextView = linkTextView;
                Activity = activity;
                Context = HomeActivity.GetInstance();

                SuperTextView.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Load(string text, ApiPosition position = ApiPosition.Left)
        {
            try
            {
                SuperTextView.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention });
                if (position == ApiPosition.Right)
                {
                    SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModePhone_color)));
                    SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeEmail_color)));
                    SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeHashtag_color)));
                    SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
                    SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeMention_color)));
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color)));
                    SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
                }
                else
                {
                    SuperTextView.SetPhoneModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModePhone_color)));
                    SuperTextView.SetEmailModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeEmail_color)));
                    SuperTextView.SetHashtagModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeHashtag_color)));
                    SuperTextView.SetUrlModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                    SuperTextView.SetMentionModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeMention_color)));
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                    SuperTextView.SetSelectedStateColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.accent)));
                }

                var textt = text.Split('/');
                if (textt.Length > 1)
                {
                    SuperTextView.SetCustomModeColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color)));
                    SuperTextView.SetCustomRegex(@"\b(" + textt.LastOrDefault() + @")\b");
                }

                string laststring = text.Replace(" /", " ");
                if (!string.IsNullOrEmpty(laststring))
                    SuperTextView.SetText(laststring, TextView.BufferType.Spannable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
                if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
                }
                else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    string url = matchedText.Replace(" ", "").Replace("\n", "");
                    if (!matchedText.Contains("http"))
                    {
                        url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
                    }

                    //var intent = new Intent(Activity, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url);
                    //intent.PutExtra("Type", url);
                    //Activity.StartActivity(intent);
                    new IntentController(Activity).OpenBrowserFromApp(url);
                }
                else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {
                    // Show All Post By Hash
                    Bundle bundle = new Bundle();
                    bundle.PutString("Key", matchedText.Replace("#", ""));
                    SearchFragment searchFragment = new SearchFragment
                    {
                        Arguments = bundle
                    };
                    Context?.FragmentBottomNavigator?.DisplayFragment(searchFragment);
                }
                else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    var bundle = new Bundle();
                    bundle.PutString("Key", Methods.FunString.DecodeString(matchedText));

                    var searchFragment = new SearchFragment
                    {
                        Arguments = bundle
                    };
                    Context?.FragmentBottomNavigator?.DisplayFragment(searchFragment);
                }
                else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }

        }
    }
}