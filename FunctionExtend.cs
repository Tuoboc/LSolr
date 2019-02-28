using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{
    public static class FunctionExtend
    {
        public static bool SolrLike(this object obj, string str, string match = "all")
        {
            return true;
        }
        public static bool SolrNotLike(this object obj, string str, string match = "all")
        {
            return true;
        }
        public static bool SolrIn(this object obj, string ValueList)
        {
            return true;
        }
        public static bool SolrNotIn(this object obj, string ValueList)
        {
            return true;
        }

    }
}
