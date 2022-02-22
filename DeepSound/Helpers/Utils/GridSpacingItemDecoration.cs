using System;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace DeepSound.Helpers.Utils
{
    public class GridSpacingItemDecoration : RecyclerView.ItemDecoration
    {
        private readonly int SpanCount;
        private readonly int Spacing;
        private readonly bool IncludeEdge;

        public GridSpacingItemDecoration(int spanCount, int spacing, bool includeEdge)
        {
            try
            {
                SpanCount = spanCount;
                Spacing = spacing;
                IncludeEdge = includeEdge;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }


        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            try
            {
                base.GetItemOffsets(outRect, view, parent, state);
                int position = parent.GetChildAdapterPosition(view);
                int column = position % SpanCount;

                if (IncludeEdge)
                {
                    outRect.Left = Spacing - column * Spacing / SpanCount;
                    outRect.Right = (column + 1) * Spacing / SpanCount;

                    if (position < SpanCount)
                    {
                        outRect.Top = Spacing;
                    }
                    outRect.Bottom = Spacing;
                }
                else
                {
                    outRect.Left = column * Spacing / SpanCount;
                    outRect.Right = Spacing - (column + 1) * Spacing / SpanCount;

                    if (position >= SpanCount)
                    {
                        outRect.Top = Spacing;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
    }
}