using System;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Webkit;
using Java.IO;
using Java.Lang;
using Console = System.Console;
using Exception = System.Exception;

namespace DeepSound.Helpers.Utils
{
    /// <summary>
    /// Converts HTML to PDF.
    /// <para>
    /// Can convert only one task at a time, any requests to do more conversions before
    /// ending the current task are ignored.
    /// </para>
    /// </summary>
    public class PdfConverter : Java.Lang.Object, IRunnable
    {

        private const string Tag = "PdfConverter";
        private static PdfConverter SInstance;

        private Context MContext;
        private string MHtmlString;
        private File MPdfFile;
        private PrintAttributes MPdfPrintAttrs;
        private bool MIsCurrentlyConverting;
        private WebView MWebView;

        public static PdfConverter Instance
        {
            get
            {
                lock (typeof(PdfConverter))
                {
                    if (SInstance == null)
                    {
                        SInstance = new PdfConverter();
                    }

                    return SInstance;
                }
            }
        }

        public void Run()
        {
            MWebView = new WebView(MContext);
            MWebView.SetWebViewClient(new WebViewClientAnonymousInnerClass(this));
            MWebView.LoadData(MHtmlString, "text/HTML", "UTF-8");
        }

        private class WebViewClientAnonymousInnerClass : WebViewClient
        {
            private readonly PdfConverter OuterInstance;

            public WebViewClientAnonymousInnerClass(PdfConverter outerInstance)
            {
                OuterInstance = outerInstance;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            { 
                try
                {
                    return base.ShouldOverrideUrlLoading(view, request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return base.ShouldOverrideUrlLoading(view, request);
                }
            }

            public override void OnLoadResource(WebView view, string url)
            {
                try
                {
                    base.OnLoadResource(view, url);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); 
                } 
            }

            public override void OnPageFinished(WebView view, string url)
            {
                try
                {
                    base.OnPageFinished(view, url);
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                    {
                        throw new Exception("call requires API level 19");
                    }
                    else
                    {
                        // Get a PrintManager instance
                        PrintManager printManager = (PrintManager)Instance.MContext.GetSystemService(Context.PrintService);
                        
                        string jobName = AppSettings.ApplicationName + " Document";

                        // Get a print adapter instance
                        PrintDocumentAdapter printAdapter;

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            printAdapter = view.CreatePrintDocumentAdapter(jobName);
                        }
                        else
                        {
#pragma warning disable CS0618
                            printAdapter = view.CreatePrintDocumentAdapter();
#pragma warning restore CS0618
                        }

                        // Create a print job with name and adapter instance 
                        //PrintJob printJob = printManager.Print(jobName, printAdapter, new PrintAttributes.Builder().Build());

                        // Save the job object for later status checking
                        //Console.WriteLine(printJob);
                        if (printManager != null)
                        {
                            var ddd = printManager.Print(jobName, printAdapter, OuterInstance.PdfPrintAttrs); 
                        }

                        //printAdapter.OnLayout(null, OuterInstance.PdfPrintAttrs, null, null, null);
                        //printAdapter.OnWrite(new PageRange[] { PageRange.AllPages }, OuterInstance.OutputFileDescriptor, null, null);
                        Instance?.Destroy();
                    }
                }
                catch (Exception exception)
                {
                    //Failed to open ParcelFileDescriptor
                    Methods.DisplayReportResultTrack(exception);
                    Instance?.Destroy();
                }
            }


            //public class LayoutResultCallbackAnonymousInnerClass : PrintDocumentAdapter.LayoutResultCallback
            //{
            //    public LayoutResultCallbackAnonymousInnerClass(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            //    {
                    
            //    }
            //}

            //private class WriteResultCallbackAnonymousInnerClass : PrintDocumentAdapter.WriteResultCallback
            //{
            //    public override void OnWriteFinished(PageRange[] pages)
            //    {
            //        try
            //        {
            //            base.OnWriteFinished(pages);
            //            Instance?.Destroy();
            //        }
            //        catch (Exception exception)
            //        {
            //            //Failed to open ParcelFileDescriptor
            //            Methods.DisplayReportResultTrack(exception);
            //        }
            //    }

            //    public WriteResultCallbackAnonymousInnerClass(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            //    {
            //    }
            //}
        }

        public virtual PrintAttributes PdfPrintAttrs
        {
            get
            {
                return MPdfPrintAttrs ?? DefaultPrintAttrs;
            }
            set
            {
                MPdfPrintAttrs = value;
            }
        }


        public virtual void Convert(Context context, string htmlString, File file)
        {
            if (context == null)
            {
                throw new ArgumentException("context can't be null");
            }
            if (ReferenceEquals(htmlString, null))
            {
                throw new ArgumentException("htmlString can't be null");
            }
            if (file == null)
            {
                throw new ArgumentException("file can't be null");
            }

            try
            {
                if (MIsCurrentlyConverting)
                {
                    return;
                }

                MContext = context;
                MHtmlString = htmlString;
                MPdfFile = file;
                MIsCurrentlyConverting = true;
               RunOnUiThread(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private ParcelFileDescriptor OutputFileDescriptor
        {
            get
            {
                try
                {
                    MPdfFile.CreateNewFile();
                    return ParcelFileDescriptor.Open(MPdfFile, ParcelFileMode.Truncate | ParcelFileMode.ReadWrite);
                }
                catch (Exception exception)
                {
                    //Failed to open ParcelFileDescriptor
                    Methods.DisplayReportResultTrack(exception);
                }
                return null;
            }
        }

        private PrintAttributes DefaultPrintAttrs
        {
            get
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                {
                    return null;
                }

                return (new PrintAttributes.Builder()).SetMediaSize(PrintAttributes.MediaSize.NaGovtLetter)
                    .SetResolution(new PrintAttributes.Resolution("RESOLUTION_ID", "RESOLUTION_ID", 600, 600))
                    .SetMinMargins(PrintAttributes.Margins.NoMargins).Build();

            }
        }

        private void RunOnUiThread(IRunnable runnable)
        {
            try
            {
                Handler handler = new Handler(MContext.MainLooper);
                handler.Post(runnable);
            }
            catch (Exception exception)
            {
                //Failed to open ParcelFileDescriptor
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Destroy()
        {
            try
            {
                MContext = null;
                MHtmlString = null;
                MPdfFile = null;
                MPdfPrintAttrs = null;
                MIsCurrentlyConverting = false;
                MWebView = null;
            }
            catch (Exception exception)
            {
                //Failed to open ParcelFileDescriptor
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
} 