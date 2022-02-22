using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Base;
using DeepSound.Activities.Genres.Adapters;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Genres
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GenresActivity : BaseActivity
    {
        #region Variables Basic

        private RecyclerView GenresRecycler;
        private RelativeLayout BtnNext;
        private GenresCheckerAdapter GenresAdapter;
        private GridLayoutManager LinearLayoutManager;
        private TextView TxtSave;
        private string TypeBtn = "";
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.GenresLayout);

                TypeBtn = Intent?.GetStringExtra("Event") ?? "";
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetGenres();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                GenresRecycler = FindViewById<RecyclerView>(Resource.Id.genresRecyclerView);
                BtnNext = FindViewById<RelativeLayout>(Resource.Id.btnNext);
                TxtSave = FindViewById<TextView>(Resource.Id.TextViewSave);

                switch (TypeBtn)
                {
                    case "Save":
                        TxtSave.Text = GetText(Resource.String.Lbl_Save);
                        break;
                    case "Continue":
                        TxtSave.Text = GetText(Resource.String.Btn_Continue);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Genres);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                LinearLayoutManager = new GridLayoutManager(this, 2);
                LinearLayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2 
                GenresRecycler.SetLayoutManager(LinearLayoutManager);
                GenresRecycler.NestedScrollingEnabled = false;
                GenresAdapter = new GenresCheckerAdapter(this) { GenresList = new ObservableCollection<GenresObject.DataGenres>() };
                GenresRecycler.SetAdapter(GenresAdapter);
                GenresRecycler.SetItemViewCacheSize(20);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<GenresObject.DataGenres>(this, GenresAdapter, sizeProvider, 10);
                GenresRecycler.AddOnScrollListener(preLoader);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    GenresAdapter.OnItemClick += GenresAdapterOnOnItemClick;
                    BtnNext.Click += BtnNextOnClick;
                }
                else
                {
                    GenresAdapter.OnItemClick -= GenresAdapterOnOnItemClick;
                    BtnNext.Click -= BtnNextOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SaveInterestedGenres()
        { 
            SharedPref.StoreInterestedGenresValue(GenresAdapter.AlreadySelectedGenres);
        }

        private List<int> LoadInterestedGenres()
        { 
            return SharedPref.GetInterestedGenresValue();
        }

        #endregion

        #region Events

        private async void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }

                string totalIdChecked = "";
                var list = GenresAdapter.GenresList.Where(genres => genres.Checked).ToList();
                if (list.Count > 0)
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    //Get all id 
                    totalIdChecked = list.Aggregate(totalIdChecked, (current, item) => current + (item.Id + ","));

                    //Sent Api
                    if (!string.IsNullOrEmpty(totalIdChecked))
                    {
                         var (apiStatus, respond) = await RequestsAsync.Common.UpdateInterestAsync(UserDetails.UserId.ToString(), totalIdChecked.Remove(totalIdChecked.Length - 1, 1)).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            SaveInterestedGenres();

                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        //AndHUD.Shared.Dismiss(this);
                                        // Toast.MakeText(this, "Subscription has been updated in" + " " + list?.Count + " " + "interests", ToastLength.Long)?.Show();

                                        switch (TypeBtn)
                                        {
                                            case "Save":
                                                Finish();
                                                break;
                                            case "Continue":
                                                StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(HomeActivity)) : new Intent(this, typeof(HomeActivity)));
                                                Finish();
                                                break;
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                        else
                        {
                            Methods.DisplayAndHudErrorResult(this, respond);
                        }
                    }
                }
                else
                {
                    AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_PleaseSelectInterest), MaskType.Clear, TimeSpan.FromSeconds(2));
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GenresAdapterOnOnItemClick(object sender, GenresCheckerAdapterClickEventArgs e)
        {
            try
            {
                var item = GenresAdapter.GetItem(e.Position);
                if (item != null)
                {
                    item.Checked = !item.Checked;

                    if (GenresAdapter.AlreadySelectedGenres.Contains(item.Id))
                    {
                        GenresAdapter.AlreadySelectedGenres.Remove(item.Id);
                    }
                    else
                    {
                        GenresAdapter.AlreadySelectedGenres.Add(item.Id);
                    }
                    
                    GenresAdapter.NotifyItemChanged(e.Position);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private async void GetGenres()
        {
            try
            {
                if (ListUtils.GenresList?.Count == 0)
                    await ApiRequest.GetGenres_Api();

                var sqlEntity = new SqLiteDatabase();
                ListUtils.GenresList = sqlEntity.Get_GenresList();

                if (ListUtils.GenresList?.Count > 0)
                {
                    GenresAdapter.AlreadySelectedGenres = LoadInterestedGenres();
                    GenresAdapter.GenresList = ListUtils.GenresList;
                    GenresAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}