using System.Collections.Generic;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Widget;
using DeepSound.Helpers.Spannable;

namespace DeepSound.Helpers.Extensions
{
    public static class TextViewExtensions
    {
        public static void ConvertToSpannableTextView(this TextView textView, Context context, List<SpannableText> spannableTextList)
        {
            string spannableTextString = string.Empty;
            spannableTextList.ForEach(x =>
            {
                var spannableText = x.TextString;
                spannableText = x.IsBold ? spannableText.AddHtmlBoldStyle() : spannableText;
                spannableText = x.IsNoWrap ? spannableText.FormatNoWrapHtml() : spannableText;
                spannableTextString += spannableText;
            });
            var spannableString = new SpannableString(Html.FromHtml(spannableTextString , FromHtmlOptions.ModeLegacy));
            var currentCharPointer = 0;
            spannableTextList.ForEach(x =>
            {
                if (x.HasAction)
                {
                    spannableString.SetSpan(
                        new ClickableSpanHelper(x.Action.Invoke),
                        currentCharPointer,
                        currentCharPointer + x.TextString.Length,
                        SpanTypes.ExclusiveExclusive);
                }

                if (x.UpdateTextColor)
                {
                    spannableString.SetSpan(
                        new ForegroundColorSpan(x.TextColor),
                        currentCharPointer,
                        currentCharPointer + x.TextString.Length,
                        SpanTypes.ExclusiveExclusive);
                }

                if (x.IsUnderLine)
                {
                    spannableString.SetSpan(
                        new UnderlineSpan(),
                        currentCharPointer,
                        currentCharPointer + x.TextString.Length,
                        SpanTypes.ExclusiveExclusive);
                }

                currentCharPointer += x.TextString.Length;
            });

            textView.TextFormatted = spannableString;
            textView.MovementMethod = new LinkMovementMethod();
        }
    }
}