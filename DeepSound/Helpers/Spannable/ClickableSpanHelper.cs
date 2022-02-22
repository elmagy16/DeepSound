using System;
using Android.Text;
using Android.Text.Style;
using Android.Views;

namespace DeepSound.Helpers.Spannable
{
    public class ClickableSpanHelper : ClickableSpan
    {
        private readonly Action Action;
        public ClickableSpanHelper(Action action)
        {
            Action = action;
        }

        public override void OnClick(View view)
        {
            Action.Invoke();
        }

        public override void UpdateDrawState(TextPaint textPaint)
        {
            textPaint.UnderlineText = false;
        }
    }
}