namespace DeepSound.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string FormatNoWrapHtml(this string input)
        {
            return input.Replace(" ", "&nbsp;");
        }

        public static string AddHtmlBoldStyle(this string input)
        {
            return $"<b>{input}</b>";
        }
    }
}