using System;
using Android.Graphics;

namespace DeepSound.Helpers.Spannable
{
    public class SpannableText
    {
        public string TextString { get; set; }
        public Action Action { get; set; }
        public Color TextColor { get; set; }
        public bool HasAction => Action != null!;
        public bool UpdateTextColor { get; set; }
        public bool IsBold { get; set; }
        public bool IsUnderLine { get; set; }
        public bool IsNoWrap { get; set; }
    }
}