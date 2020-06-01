using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{
    public static class SolrStrExted
    {
        public static string SolrReplace(this string s)
        {
            return s.Replace("(", "\\(").Replace(")", "\\)")
                 .Replace("\"", "").Replace(":", "\\:")
                 .Replace(" ", "\\ ").Replace("+", "%2B")
                 .Replace("*", "\\*").Replace("~", "\\~")
                 .Replace("!", "\\!").Replace("%", "\\%")
                 .Replace("@", "\\@").Replace("^", "\\^")
                 .Replace("?", "\\?").Replace("&", "%26")
                  .Replace("[", "\\[").Replace("]", "\\]");
        }
    }
}
