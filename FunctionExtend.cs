using System;
using System.Collections.Generic;
using System.Text;

namespace LSolr
{
    public static class FunctionExtend
    {
        public static T All<T>(this T obj)
        {
            return obj;
        }
        public static bool Like(this object obj, string str, string match = "all")
        {
            return true;
        }
        public static bool NotLike(this object obj, string str, string match = "all")
        {
            return true;
        }
        public static bool In(this object obj, string ValueList)
        {
            return true;
        }
        public static bool NotIn(this object obj, string ValueList)
        {
            return true;
        }

    }
}
