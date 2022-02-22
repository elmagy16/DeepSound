using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Webkit;
using Android.Widget; 
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Requests;
using Exception = System.Exception;

namespace DeepSound.Payment
{
    public class InitPayStackPayment 
    {
        private readonly Activity ActivityContext;
        private Dialog PayStackWindow;
        private WebView HybridView;
        private string Url, Price;
        public InitPayStackPayment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayPayStackPayment(string url, string price)
        {
            try
            {
                Url = url;
                Price = price;

                PayStackWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                PayStackWindow.SetContentView(Resource.Layout.PaymentWebViewLayout);

                var title = (TextView)PayStackWindow.FindViewById(Resource.Id.toolbar_title);
                if (title != null)
                    title.Text = ActivityContext.GetText(Resource.String.Lbl_PayWith) + " " + ActivityContext.GetText(Resource.String.Lbl_PayStack);

                var closeButton = (TextView)PayStackWindow.FindViewById(Resource.Id.toolbar_close);
                if (closeButton != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, closeButton, IonIconsFonts.Close);

                    closeButton.SetTextSize(ComplexUnitType.Sp, 20f);
                    closeButton.Click += CloseButtonOnClick;
                }

                HybridView = PayStackWindow.FindViewById<WebView>(Resource.Id.LocalWebView);

                //Set WebView
                if (HybridView != null)
                {
                    HybridView.SetWebViewClient(new MyWebViewClient(this));
                    if (HybridView.Settings != null)
                    {
                        HybridView.Settings.LoadsImagesAutomatically = true;
                        HybridView.Settings.JavaScriptEnabled = true;
                        HybridView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                        HybridView.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                        HybridView.Settings.DomStorageEnabled = true;
                        HybridView.Settings.AllowFileAccess = true;
                        HybridView.Settings.DefaultTextEncodingName = "utf-8";

                        HybridView.Settings.UseWideViewPort = true;
                        HybridView.Settings.LoadWithOverviewMode = true;

                        HybridView.Settings.SetSupportZoom(false);
                        HybridView.Settings.BuiltInZoomControls = false;
                        HybridView.Settings.DisplayZoomControls = false;
                    }

                    //Load url to be rendered on WebView
                    HybridView.LoadUrl(Url);
                }

                PayStackWindow.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                PayStackWindow.Hide();
                PayStackWindow.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void StopPayStack()
        {
            try
            {
                if (PayStackWindow != null)
                {
                    PayStackWindow.Hide();
                    PayStackWindow.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string reference , string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"reference", reference}, 
                    };

                    var priceInt = Convert.ToInt32(Price) * 100;

                    keyValues.Add("price", priceInt.ToString());

                    var (apiStatus, respond) = await RequestsAsync.Payments.PayStackAsync(request , keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                            ActivityContext.Finish();
                            break;
                        default:
                            Methods.DisplayReportResult(ActivityContext, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private class MyWebViewClient : WebViewClient 
        {
            private readonly InitPayStackPayment MActivity;
            public MyWebViewClient(InitPayStackPayment mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                try
                {
                    if (string.IsNullOrEmpty(request?.Url?.ToString()))
                        return false;

                    if (request.Url.ToString() == MActivity.Url)
                    {
                        view.LoadUrl(request.Url.ToString());
                    }
                    else if (request.Url.ToString().Contains("reference"))
                    {
                        //https://demo.deepsoundscript.com/endpoints/paystack/pay?type=wallet&amount=20&trxref=61dd6ca1d67e8&reference=61dd6ca1d67e8

                        var reference = request.Url.ToString()?.Split("&reference=")?.Last();
                        if (string.IsNullOrEmpty(reference))
                            return false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PayStack(reference, "pay") });
                    } 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return false;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);

                    if (view.Settings != null)
                    {
                        view.Settings.JavaScriptEnabled = true;
                        view.Settings.DomStorageEnabled = true;
                        view.Settings.AllowFileAccess = true;
                        view.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                        view.Settings.UseWideViewPort = true;
                        view.Settings.LoadWithOverviewMode = true;
                        view.Settings.SetSupportZoom(false);
                        view.Settings.BuiltInZoomControls = false;
                        view.Settings.DisplayZoomControls = false;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            } 
        }  
    }
}