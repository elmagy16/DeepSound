using System;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using DeepSound.Library.Anjo.IntegrationRecyclerView;

namespace DeepSound.Helpers.Utils
{
    public class TemplateRecyclerInflater
    {
        public LinearLayout MainLinear;
        public TextView TitleText, IconTitle, TextMore;
        public RecyclerView Recyler;
        public dynamic LayoutManager;

        public enum TypeLayoutManager
        {
            LinearLayoutManagerVertical,
            LinearLayoutManagerHorizontal,
            GridLayoutManagerVertical,
            GridLayoutManagerHorizontal,
            StaggeredGridLayoutManagerVertical,
            StaggeredGridLayoutManagerHorizontal
        }

        private void InitComponent(View inflated)
        {
            try
            {
                MainLinear = (LinearLayout) inflated.FindViewById(Resource.Id.mainLinear);
                TitleText = (TextView) inflated.FindViewById(Resource.Id.textTitle);
                IconTitle = (TextView) inflated.FindViewById(Resource.Id.iconTitle);
                TextMore = (TextView) inflated.FindViewById(Resource.Id.textMore);
               // DescriptionText = (TextView) inflated.FindViewById(Resource.Id.textSecondery);
                Recyler = (RecyclerView) inflated.FindViewById(Resource.Id.recyler);

                //FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.AngleLeft);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InflateLayout<T>(Activity activity, View inflated, dynamic mAdapter, TypeLayoutManager manager,int spanCount = 0, bool showTitle = true, string titleText = "", bool showMore = true) where T : class
        {
            try
            {
                InitComponent(inflated);

                if (showTitle)
                {
                    MainLinear.Visibility = ViewStates.Visible;

                    if (string.IsNullOrEmpty(titleText))
                    { TitleText.Visibility = ViewStates.Gone; IconTitle.Visibility = ViewStates.Gone;}
                    else
                        TitleText.Text = titleText;
                     
                    if (showMore)
                        TextMore.Visibility = ViewStates.Visible;
                    else
                        TextMore.Visibility = ViewStates.Gone;
                }
                else
                    MainLinear.Visibility = ViewStates.Gone;

                switch (manager)
                {
                    case TypeLayoutManager.LinearLayoutManagerHorizontal:
                        LayoutManager = new LinearLayoutManager(activity, LinearLayoutManager.Horizontal, false);
                        Recyler.NestedScrollingEnabled = false;
                        break;
                    case TypeLayoutManager.LinearLayoutManagerVertical:
                        LayoutManager = new LinearLayoutManager(activity);
                        break;
                    case TypeLayoutManager.GridLayoutManagerVertical:
                        LayoutManager = new GridLayoutManager(activity, spanCount);
                        break;
                    case TypeLayoutManager.GridLayoutManagerHorizontal:
                        LayoutManager = new GridLayoutManager(activity, spanCount, LinearLayoutManager.Horizontal, false);
                        Recyler.NestedScrollingEnabled = false;
                        break;
                    case TypeLayoutManager.StaggeredGridLayoutManagerVertical:
                        LayoutManager = new StaggeredGridLayoutManager(spanCount, LinearLayoutManager.Vertical);
                        break;
                    case TypeLayoutManager.StaggeredGridLayoutManagerHorizontal:
                        LayoutManager = new StaggeredGridLayoutManager(spanCount, LinearLayoutManager.Horizontal);
                        Recyler.NestedScrollingEnabled = false;
                        break;
                    default:
                        LayoutManager = new LinearLayoutManager(activity);
                        break;
                }

                Recyler.SetLayoutManager(LayoutManager);
                Recyler.SetItemViewCacheSize(20);
                Recyler.HasFixedSize = true;
                Recyler.SetItemViewCacheSize(10);
                Recyler.GetLayoutManager().ItemPrefetchEnabled = true;

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<T>(activity, mAdapter, sizeProvider, 10);
                Recyler.AddOnScrollListener(preLoader);

                Recyler.SetAdapter(mAdapter); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}