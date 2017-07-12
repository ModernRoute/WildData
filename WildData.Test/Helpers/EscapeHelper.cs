namespace ModernRoute.WildData.Test.Helpers
{
    static class EscapeHelper
    {
        public static string EscapeString(string value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Replace("\\","\\\\").Replace("\"", "\\\"");
        }
    }
}
