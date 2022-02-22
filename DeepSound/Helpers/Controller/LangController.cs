using System;
using System.Globalization;
using System.Threading;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using DeepSound.Activities.SettingsUser;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Java.Util;

namespace DeepSound.Helpers.Controller
{
    public class LangController : ContextWrapper
    {
        private Context Context;
        public static string Language = "";

        protected LangController(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LangController(Context context) : base(context)
        {
            Context = context;
        }

        //public static Context SetAppLanguage(Context activityContext)
        //{
        //    try
        //    {
        //        var res = activityContext.Resources; // Get the string 

        //        Configuration config = activityContext.Resources?.Configuration;

        //        var locale = activityContext.Resources?.Configuration.Locale;

        //        Configuration conf = res.Configuration;
        //        conf.SetLocale(locale);

        //        Locale.Default = locale;

        //        if ((int)Build.VERSION.SdkInt > 17)
        //            conf.SetLayoutDirection(locale);

        //        DisplayMetrics dm = res.DisplayMetrics;

        //        if (config.Locale.Language.Contains("ar"))
        //        {
        //            AppSettings.Lang = "ar";
        //            AppSettings.FlowDirectionRightToLeft = true;
        //        }
        //        else
        //        {
        //            AppSettings.FlowDirectionRightToLeft = false;
        //        }

        //        if ((int)Build.VERSION.SdkInt >= 24)
        //        {
        //            LocaleList localeList = new LocaleList(locale);
        //            LocaleList.Default = localeList;
        //            conf.Locales = localeList;

        //            //Locale.SetDefault(Locale.Category.Display, locale);
        //            activityContext = activityContext.CreateConfigurationContext(conf);
        //            return activityContext;
        //        }

        //        conf.Locale = locale;
        //        res.UpdateConfiguration(conf, dm);

        //        Wrap(activityContext, config.Locale.Language);

        //        return activityContext;
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //        return activityContext;
        //    }
        //}

        //public static ContextWrapper Wrap(Context context, string language)
        //{
        //    try
        //    {
        //        Language = language;

        //        Configuration config = context.Resources?.Configuration;

        //        var sysLocale = config.Locales.Get(0);

        //        if (!language.Equals("") && !sysLocale.Language.Equals(language))
        //        {
        //            sysLocale = new Locale(language);
        //            Locale.Default = sysLocale;
        //        }
        //        CultureInfo myCulture = new CultureInfo(language);
        //        CultureInfo.DefaultThreadCurrentCulture = myCulture;
        //        config.SetLocale(sysLocale);

        //        var ss = context.Resources?.Configuration.Locale;
        //        Console.WriteLine(ss);
        //        //SharedPref.SharedData?.Edit()?.PutString("Lang_key", language)?.Commit();

        //        //context = context.CreateConfigurationContext(config);
        //        context.Resources?.UpdateConfiguration(config, null);

        //        return new LangController(context);
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //        return new LangController(context);
        //    }
        //}

        //public static void SetDefaultAppSettings()
        //{
        //    try
        //    {
        //        //SharedPref.Edit().PutString("Lang_key", "Auto")?.Commit();
        //        if (AppSettings.Lang != "")
        //        {
        //            if (AppSettings.Lang == "ar")
        //            {
        //                //SharedPref.SharedData?.Edit()?.PutString("Lang_key", "ar")?.Commit();
        //                AppSettings.Lang = "ar";
        //                AppSettings.FlowDirectionRightToLeft = true;
        //            }
        //            else
        //            {
        //                // SharedPref.SharedData?.Edit()?.PutString("Lang_key", AppSettings.Lang)?.Commit();
        //                AppSettings.FlowDirectionRightToLeft = false;
        //            }
        //        }
        //        else
        //        {
        //            AppSettings.FlowDirectionRightToLeft = false;

        //            //var Lang = SharedPref.SharedData.GetString("Lang_key", AppSettings.Lang);
        //            //if (Lang == "ar")
        //            //{
        //            //    SharedPref.SharedData?.Edit()?.PutString("Lang_key", "ar")?.Commit();
        //            //    AppSettings.Lang = "ar";
        //            //    AppSettings.FlowDirectionRightToLeft = true;
        //            //}
        //            //else if (Lang == "Auto")
        //            //{
        //            //    SharedPref.SharedData?.Edit()?.PutString("Lang_key", "Auto")?.Commit();
        //            //}
        //            //else
        //            //{
        //            //    SharedPref.SharedData?.Edit()?.PutString("Lang_key", Lang)?.Commit();
        //            //}
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        public static Context SetApplicationLang(Context context, string language)
        {
            try
            {
                Locale newLocale = new Locale(language);

                Locale locale = newLocale;
                Locale.Default = (locale);

                Resources res = context.Resources;
                Configuration configuration = res.Configuration;


                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    configuration.SetLocale(newLocale);

                    LocaleList localeList = new LocaleList(newLocale);
                    LocaleList.Default = (localeList);
                    Locale.SetDefault(Locale.Category.Display, newLocale);
                    configuration.Locales = (localeList);
                    configuration.SetLayoutDirection(newLocale);
                    try
                    {
                        CultureInfo myCulture = new CultureInfo(language);
                        CultureInfo.DefaultThreadCurrentCulture = myCulture;
                    }
                    catch 
                    {
                         //
                    }
                   
                    context = context.CreateConfigurationContext(configuration);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    configuration.Locale = newLocale;
#pragma warning restore CS0618 // Type or member is obsolete

                    configuration.SetLocale(newLocale);

                    try
                    {
                        CultureInfo myCulture = new CultureInfo(language);
                        CultureInfo.DefaultThreadCurrentCulture = myCulture;
                    }
                    catch
                    {
                        //
                    }

                    context = context.CreateConfigurationContext(configuration);
                    //res.UpdateConfiguration(configuration, res.DisplayMetrics);
                }
                else
                {
#pragma warning disable 618
                    configuration.Locale = newLocale;
                   // res.UpdateConfiguration(configuration, res.DisplayMetrics);
#pragma warning restore 618
                }

#pragma warning disable 618
                res.UpdateConfiguration(configuration, res.DisplayMetrics);
#pragma warning restore 618

                UserDetails.LangName = language;
                AppSettings.Lang = language;
                 
                AppSettings.FlowDirectionRightToLeft = language.Contains("ar") || language.Contains("ku");
                SetCulture(language);
                SharedPref.SharedData?.Edit()?.PutString("Lang_key", language)?.Commit();

                return context;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return context;
            }
        }

        private static void SetCulture(string language)
        {
            try
            {
                CultureInfo myCulture = new CultureInfo(language);
                CultureInfo.DefaultThreadCurrentCulture = myCulture;
                Thread.CurrentThread.CurrentCulture = myCulture;
                Thread.CurrentThread.CurrentUICulture = myCulture;

                new ChineseLunisolarCalendar();
                new HebrewCalendar();
                new HijriCalendar();
                new JapaneseCalendar();
                new JapaneseLunisolarCalendar();
                new KoreanCalendar();
                new KoreanLunisolarCalendar();
                new PersianCalendar();
                new TaiwanCalendar();
                new TaiwanLunisolarCalendar();
                new ThaiBuddhistCalendar();
                new UmAlQuraCalendar();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}