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
        public string UpdateData = "";
        private List<FieldMap> FieldMap;

        private Dictionary<string, object> UpdateFieldsDic = new Dictionary<string, object>();

        public Update(Expression<Func<T, object>> exp, List<FieldMap> map)
        {
            FieldMap = map;
            update(exp, map);
        }
        public Update<T> update(Expression<Func<T, object>> func, List<FieldMap> map)
        {
            string tempString = GetUpdateString(func, map);
            UpdateData = tempString;
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
            if (func.NodeType == ExpressionType.MemberInit)
            {
                var exp0 = func as MemberInitExpression;
                var t = exp0.NewExpression.Type;
                if (exp0.Bindings.Count > 0)
                {
                    foreach (var item in exp0.Bindings)
                    {
                        var itemexp = item as MemberAssignment;
                        var exp = itemexp.Expression;
                        var field = FieldMap.Find(a => a.EntityField == item.Member.Name);
                        object value = new object();
                        var type = ExpressionEnum.CheckExpressionType(exp);
                        switch (type)
                        {
                            case ExpressionEnum.EnumNodeType.Constant:
                                var val = exp as ConstantExpression;
                                value = val.Value;
                                break;
                            case ExpressionEnum.EnumNodeType.MemberAccess:
                                UnaryExpression cast = Expression.Convert(exp, typeof(object));
                                value = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
                                break;
                            case ExpressionEnum.EnumNodeType.UndryOperator:
                                var unaryExpression = exp as UnaryExpression;
                                if (unaryExpression != null)
                                {
                                    var uexp = unaryExpression.Operand;
                                    switch (ExpressionEnum.CheckExpressionType(uexp))
                                    {
                                        case ExpressionEnum.EnumNodeType.Constant:
                                            var uval = uexp as ConstantExpression;
                                            value = uval.Value;

                                            break;
                                        case ExpressionEnum.EnumNodeType.MemberAccess:
                                            UnaryExpression ucast = Expression.Convert(uexp, typeof(object));
                                            value = Expression.Lambda<Func<object>>(ucast).Compile().Invoke();
                                            break;
                                    }
                                }

                                break;
                            case ExpressionEnum.EnumNodeType.Call:
                                var mexp = (exp as MethodCallExpression).Object as MemberExpression;
                                if (mexp == null) break;
                                UnaryExpression mcast = Expression.Convert(mexp, typeof(object));
                                value = Expression.Lambda<Func<object>>(mcast).Compile().Invoke();
                                break;
                        };
                        UpdateFieldsDic.Add(field.EntityField, value);
                    }
                    var keyList = FieldMap.FindAll(a => a.IsKey == true);
                    if (keyList.Count == 0)
                        throw new Exception("没有找到solr的主键字段,检查SolrField是否有字段的IsKey为true");
                    else if (keyList.Count > 1)
                        throw new Exception("只能有一个字段作为solr的主键,检查SolrField是否多个字段的IsKey为true");
                    if (!UpdateFieldsDic.ContainsKey(keyList[0].EntityField))
                    {
                        throw new Exception("solr更新必须根据主键字段进行更新,检查更新的对象中是否有[" + keyList[0].EntityField + "]字段");
                    }
                    else
                    {
                        List<string> updateResult = new List<string>();
                        foreach (var item in UpdateFieldsDic)
                        {
                            FieldMap field = FieldMap.Find(a => a.EntityField == item.Key);
                            string key = string.IsNullOrEmpty(field.SolrField) ? field.EntityField : field.SolrField;
                            string type = field.EntityType;
                            object value = UpdateFieldsDic[field.EntityField];
                            bool iskey = field.IsKey;
                            string keystring = key + ":";
                            string valueString = "";
                            switch (type)
                            {
                                case "String":
                                    valueString = "\"" + value.ToString() + "\"";
                                    break;
                                case "Double":
                                case "Float":
                                case "Decimal":
                                case "Int64":
                                case "Int32":
                                    valueString = value.ToString();
                                    break;
                            }
                            if (iskey)
                                keystring += valueString;
                            else
                                keystring += "{\"set\":" + valueString + "}";
                            updateResult.Add(keystring);
                        }
                        tempUpdateString = "[{" + string.Join(",", updateResult) + "}]";
                    }
                }
            }

            if (func.NodeType == ExpressionType.MemberAccess)
            {
                var exp0 = func as MemberExpression;
                UnaryExpression mcast = Expression.Convert(exp0, exp0.Type);
                object model = Expression.Lambda<Func<object>>(mcast).Compile().Invoke();
                List<string> updateResult = new List<string>();
                int KeyCount = 0;
                foreach (var item in map)
                {
                    object value = Helper.GetPropertyValue(model, item.EntityField);
                    if (value != null)
                    {
                        string valueString = "";
                        string key = string.IsNullOrEmpty(item.SolrField) ? item.EntityField : item.SolrField;
                        string keystring = key + ":";
                        switch (item.EntityType)
                        {
                            case "String":
                                valueString = "\"" + value.ToString() + "\"";
                                break;
                            case "Double":
                            case "Float":
                            case "Decimal":
                            case "Int64":
                            case "Int32":
                                valueString = value.ToString();
                                break;
                        }
                        if (item.IsKey)
                        {
                            KeyCount++;
                            keystring += valueString;
                        }
                        else
                            keystring += "{\"set\":" + valueString + "}";
                        updateResult.Add(keystring);
                    }
                }
                tempUpdateString = "[{" + string.Join(",", updateResult) + "}]";
                if (KeyCount == 0)
                    throw new Exception("没有找到solr的主键字段,检查SolrField是否有字段的IsKey为true");
                else if (KeyCount > 1)
                    throw new Exception("只能有一个字段作为solr的主键,检查SolrField是否多个字段的IsKey为true");
            }
            return tempUpdateString;
        }
    }
}
