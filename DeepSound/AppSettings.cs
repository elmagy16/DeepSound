using DeepSound.Helpers.Model;

namespace DeepSound
{
    //For the accuracy of the icon and logo, please use this website " https://appicon.co " and add images according to size in folders " mipmap " 

    internal static class AppSettings
    {
        /// <summary>
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">deepsoundscript.com</string>
        /// </summary>  
        public static string Cert = "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";
        
        //Main Settings >>>>>
        //*********************************************************
        public static string ApplicationName = "RD Music Cloud";
        public static string DatabaseName = "radiodos_rdmusic"; 
        public static string Version = "2.3";

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#27b830";

        public static string BackgroundGradationColor1 = "#07702a";
        public static string BackgroundGradationColor2 = "#0eb376";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = "EN"; //Default language ar_AE

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false;

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "9ff24dee312db450fe77a35cf74583190419a3e2";

        public static int RefreshGetNotification = 60000; // 1 minute

        // WalkThrough Settings >>
        //*********************************************************
        public static bool ShowWalkTroutPage = false; 

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static ShowAds ShowAds = ShowAds.UnProfessional;

        public static bool ShowAdMobBanner = true;
        public static bool ShowAdMobInterstitial = true;
        public static bool ShowAdMobRewardVideo = false;
        public static bool ShowAdMobNative = true;
        public static bool ShowAdMobAppOpen = true;
        public static bool ShowAdMobRewardedInterstitial = false;

        public static string AdInterstitialKey = "ca-app-pub-9262006016408695/1642065796";
        public static string AdRewardVideoKey = "";
        public static string AdAdMobNativeKey = "ca-app-pub-9262006016408695/7385347064";
        public static string AdAdMobAppOpenKey = "";
        public static string AdRewardedInterstitialKey = "ca-app-pub-9262006016408695/2924729743";

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        public static int ShowAdMobNativeCount = 5;  
        public static int ShowAdMobAppOpenCount = 2;  
        public static int ShowAdMobRewardedInterstitialCount = 3;  
        
        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false;  
        public static bool ShowFbRewardVideoAds = false;  
        public static bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";  
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492"; 
        public static string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static bool ShowColonyBannerAds = false; 
        public static bool ShowColonyInterstitialAds = false; 
        public static bool ShowColonyRewardAds = false; 

        public static string AdsColonyAppId = "appc1a3a39f4257436fb0";
        public static string AdsColonyBannerId = "vzf3427a794942477a91";
        public static string AdsColonyInterstitialId = "vz0df8be89b80d41a9ba";
        public static string AdsColonyRewardedId = "vzd163d9467cbc4ab681";
        //*********************************************************   

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file  
        //Facebook >> ../values/analytic.xml .. line 10 - 11
        //Google >> ../values/analytic.xml .. line 15
        //*********************************************************
        public static bool EnableSmartLockForPasswords = true;
        
        public static bool ShowFacebookLogin = false;
        public static bool ShowGoogleLogin = false; 
        public static bool ShowWoWonderLogin = false;

        public static string ClientId = "49464937845-m8juldql15bg7g9qhunu1i7121phjd1n.apps.googleusercontent.com";

        public static string AppNameWoWonder = "WoWonder"; 
         
        //*********************************************************
        public static bool ShowPrice = true;
        public static bool ShowSkipButton = true;

        //in album
        public static bool ShowCountPurchases = true; 

        //Show Title Album Only on song
        public static bool ShowTitleAlbumOnly = false;  

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        public static bool EnableOptimizationApp = false;  

        public static bool ShowSettingsRateApp = true;  
        public static int ShowRateAppCount = 5;

        public static bool ShowSettingsHelp = true;//#New
        public static bool ShowSettingsTermsOfUse = true;//#New
        public static bool ShowSettingsAbout = true;//#New
        public static bool ShowSettingsDeleteAccount = true;//#New
         
        public static bool ShowSettingsUpdateManagerApp = false; 

        public static bool ShowTextWithSpace = false; 
         
        //Set Blur Screen Comment
        //*********************************************************
        public static bool EnableBlurBackgroundComment = true;

        //Set the radius of the Blur. Supported range 0 < radius <= 25
        public static readonly float BlurRadiusComment = 25f;

        //Import && Upload Videos >>  
        //*********************************************************
        public static bool ShowButtonUploadSingleSong { get; set; } = true;
        public static bool ShowButtonUploadAlbum { get; set; } = true;  
        public static bool ShowButtonImportSong { get; set; } = true;

        //Tap profile
        //*********************************************************
        public static bool ShowStore = true;  
        public static bool ShowStations = true;  
        public static bool ShowPlaylist = true;  
         
        //Offline Sound >>  
        //*********************************************************
        public static bool AllowOfflineDownload = true;
         
        //Profile >>  
        //*********************************************************
        public static bool ShowEmail = true; 

        public static bool ShowForwardTrack = true; 
        public static bool ShowBackwardTrack = true; 

        //Settings Page >>  
        //*********************************************************
        public static bool ShowEditPassword = true; 
        public static bool ShowWithdrawals = true; 
        public static bool ShowGoPro = true; 
        public static bool ShowBlockedUsers = true; 
        public static bool ShowBlog = true;  
        public static bool ShowSettingsTwoFactor = true; 
        public static bool ShowSettingsManageSessions = true;  

        //Last_Messages Page >>
        //********************************************************* 
        public static bool RunSoundControl = true; 
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds

        //Set Theme App >> Color - Tab
        //*********************************************************
        public static bool SetTabDarkTheme = false;

        //Bypass Web Erros 
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static bool RenderPriorityFastPostLoad = true;

        //Payment System
        //*********************************************************
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = true;

        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc & https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static bool ShowPaypal = true;
        public static string MerchantAccountId = "live"; //#New

        public static string SandboxTokenizationKey = "AdqD85lE2JtGQ619V0Rvcj4LU2F9S36yj06Pk_FXQjrC0IYOtslSATum9vr5lXJrfN0qsQzNhlPX0_85"; //#New
        public static string ProductionTokenizationKey = "EBwGP5mn-p2d_m1kKCLpny01lME7uBWUtcgTlzMCzTAun6QqD6xN3_kwlT8lTHEoMMw8GS0nnqULmJ4b"; //#New

        public static bool ShowBankTransfer = false;
        public static bool ShowCreditCard = false;//#New
         
        public static bool ShowCashFree = false;

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static string CashFreeCurrency = "INR";//#New

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 24 
        /// </summary>
        public static bool ShowRazorPay = false;//#New

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static string RazorPayCurrency = "USD";//#New

        public static bool ShowPayStack = false;//#New
        public static bool ShowPaySera = false;//#New  //#Next Version  

        public static bool ShowPayUmoney = false;//#New
        public static bool ShowAuthorizeNet = false;//#New
        public static bool ShowSecurionPay = false;//#New

        //********************************************************* 
        public static bool AllowDeletingDownloadedSongs = true;

        public static bool EnableEvent = true; //#New
        public static bool EnableProduct = true; //#New

    }
} 