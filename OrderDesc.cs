﻿using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LSolr
{
    public class OrderDesc<T>
    {
        public string SelectFields { get; set; }
        public string _order = "";

        public OrderDesc(Expression<Func<T, object>> exp, List<FieldMap> map)
        {
            orderdesc(exp, map);
        }
        public OrderDesc<T> orderdesc(Expression<Func<T, object>> func, List<FieldMap> map)
        {
            string tempString = GetSelectString(func, map);
            _order = tempString == "" ? "" : "&sort=" + tempString;
            return this;
        }

        public string GetSelectString(Expression func, List<FieldMap> map)
        {
            string tempOrderString = "";
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
                                tempOrderString += field.SolrField + " DESC,";
                            else
                                tempOrderString += field.EntityField + " DESC,";
                        }
                    }
                }
            }
            if (func.NodeType == ExpressionType.MemberAccess)
            {
                var expN = func as MemberExpression;
                var field = map.Find(a => a.EntityField == expN.Member.Name);
                if (field != null)
                {
                    if (!string.IsNullOrEmpty(field.SolrField))
                        tempOrderString += field.SolrField + " DESC,";
                    else
                        tempOrderString += field.EntityField + " DESC,";
                }
                else
                    return expN.Member.Name.ToString() + " DESC,";
            }
            if (func.NodeType == ExpressionType.Convert)
            {
                var expN = func as UnaryExpression;
                return GetSelectString(expN.Operand, map);
            }

            return tempOrderString.Trim(',');
        }
    }
}
