using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LSolr
{
    public class Update<T>
    {
        public string UpdateFields { get; set; }
        public string _select = "";

        public Update(Expression<Func<T, object>> exp, List<FieldMap> map)
        {
            select(exp, map);
        }
        public Update<T> select(Expression<Func<T, object>> func, List<FieldMap> map)
        {
            string tempString = GetUpdateString(func, map);
            _select = tempString == "" ? "" : "&fl=" + tempString;
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this;
        }

        public string GetUpdateString(Expression func, List<FieldMap> map)
        {
            string tempUpdateString = "";
            if (func == null)
            {
                return "";
            }

            if (func is LambdaExpression)
            {
                var exp = func as LambdaExpression;
                return GetUpdateString(exp.Body, map);
            }

            if (func.NodeType == ExpressionType.Call)
            {
                var callexp = func as MethodCallExpression;
                if (callexp.Method.Name == "All")
                    return "";
            }
            if (func.NodeType == ExpressionType.New)
            {
                var expN = func as NewExpression;
                if (expN.Arguments.Count > 0)
                {
                    for (int i = 0; i < expN.Arguments.Count; i++)
                    {
                        var memberexp = expN.Arguments[i] as MemberExpression;
                        if (memberexp != null)
                        {
                            FieldMap field = map.Find(a => a.EntityField == memberexp.Member.Name);
                            if (!string.IsNullOrEmpty(field.SolrField))
                                tempUpdateString += field.SolrField + ",";
                            else
                                tempUpdateString += field.EntityType + ",";
                        }
                    }
                }
            }
            return tempUpdateString.Trim(',');
        }
    }
}
