using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Google.Android.Material.BottomSheet;
using MaterialDialogsCore;

namespace DeepSound.Activities.Product
{
    public class FilterProductDialogFragment : BottomSheetDialogFragment, MaterialDialog.IListCallback 
    {
        #region Variables Basic

        private TextView IconBack, IconCategory, IconPrice;
        private EditText  TxtCategory, TxtPriceMin, TxtPriceMax;
        private AppCompatButton BtnApply;
        private string TypeDialog, CategoryId;
        private readonly ProductFragment ContextProduct;

        #endregion

        #region General
         
        public FilterProductDialogFragment(ProductFragment productActivity)
        {
            ContextProduct = productActivity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = localInflater?.Inflate(Resource.Layout.BottomSheetProductFilter, container, false);

                InitComponent(view);

                IconBack.Click += IconBackOnClick;
                BtnApply.Click += BtnApplyOnClick;
                TxtCategory.Touch += TxtCategoryOnTouch; 

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                IconCategory = view.FindViewById<TextView>(Resource.Id.IconCategory);
                TxtCategory = view.FindViewById<EditText>(Resource.Id.CategoryEditText);

                IconPrice = view.FindViewById<TextView>(Resource.Id.IconPrice);
                TxtPriceMin = view.FindViewById<EditText>(Resource.Id.MinimumEditText);
                TxtPriceMax = view.FindViewById<EditText>(Resource.Id.MaximumEditText);

                BtnApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowBack);

                IconCategory.Visibility = ViewStates.Gone;
                IconPrice.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMin, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMax, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCategory);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.ProductCategory = CategoryId;
                UserDetails.ProductPriceMin = TxtPriceMin.Text;
                UserDetails.ProductPriceMax = TxtPriceMax.Text;

                ContextProduct.MAdapter.ProductsList.Clear();
                ContextProduct.MAdapter.NotifyDataSetChanged();

                ContextProduct.SwipeRefreshLayout.Refreshing = true;
                ContextProduct.SwipeRefreshLayout.Enabled = true;

                ContextProduct.MainScrollEvent.IsLoading = false;

                ContextProduct.MRecycler.Visibility = ViewStates.Visible;
                ContextProduct.EmptyStateLayout.Visibility = ViewStates.Gone;

                ContextProduct.StartApiService();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Category";

                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(cat => cat.CategoriesName).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Category));
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
         
        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                var text = itemString;
                if (TypeDialog == "Category")
                {
                    CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesName == text)?.CategoriesId;
                    TxtCategory.Text = text;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

    }
}