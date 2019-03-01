﻿using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LSolr
{
    public class Group<T>
    {
        public string GroupStr = "";

        public Group(Expression<Func<T, object>> exp, List<FieldMap> map)
        {
            group(exp, map);
        }
        public Group<T> group(Expression<Func<T, object>> func, List<FieldMap> map)
        {
            string tempString = GetSelectString(func, map);
            GroupStr = "facet=on&facet.pivot=" + tempString;
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this;
        }

        public string GetSelectString(Expression func, List<FieldMap> map)
        {
            string tempSelectString = "";
            if (func == null)
            {
                return "";
            }

            if (func is LambdaExpression)
            {
                var exp = func as LambdaExpression;
                return GetSelectString(exp.Body, map);
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
                                tempSelectString += field.SolrField + ",";
                            else
                                tempSelectString += field.EntityField + ",";
                        }
                    }
                }
            }
            return tempSelectString.Trim(',');
        }
    }
}
