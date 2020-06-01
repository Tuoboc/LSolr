using LSolr.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LSolr
{
    public class Where<T>
    {
        public string _where = "";
        private readonly List<FieldMap> fieldmap;
        public Where(Expression<Func<T, object>> func, List<FieldMap> map)
        {
            fieldmap = map;
            where(func);
        }
        private Where<T> where(Expression<Func<T, object>> func)
        {
            string fq = GetWhereString(func);
            _where = "&fq=" + fq;
            return this;
        }

        private string GetWhereString(Expression func)
        {
            if (func == null)
            {
                return "";
            }

            if (func is LambdaExpression)
            {
                var exp = func as LambdaExpression;
                return GetWhereString(exp.Body);
            }
            if (func is UnaryExpression)
            {
                var exp = func as UnaryExpression;
                return GetWhereString(exp.Operand);
            }
            if (func is BinaryExpression)
            {
                var exp = func as BinaryExpression;
                return VisitBinaryExpression(exp);
            }
            if (func is MemberExpression)
            {
                var exp = func as MemberExpression;
                return VisitMemberExpression(exp);
            }
            if (func is MethodCallExpression)
            {
                var exp = func as MethodCallExpression;
                return VisitMethodCallExpression(exp);
            }
            return "";
        }



        private string VisitBinaryExpression(BinaryExpression func)
        {
            var left = "";
            var right = "";
            var leftFunc = func.Left;
            var leftType = ExpressionEnum.CheckExpressionType(func.Left);
            string result = "";
            switch (leftType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    left = VisitBinaryExpression(func.Left as BinaryExpression); break;
                case ExpressionEnum.EnumNodeType.Constant:
                    left = VisitConstantExpression(func.Left as ConstantExpression); break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    left = VisitMemberExpression(func.Left as MemberExpression); break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    left = VisitUnaryExpression(func.Left as UnaryExpression); break;
                case ExpressionEnum.EnumNodeType.Call:
                    left = VisitMethodCallExpression(func.Left as MethodCallExpression); break;
            }

            var rightFunc = func.Right;
            var rightType = ExpressionEnum.CheckExpressionType(func.Right);
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    right = VisitBinaryExpression(func.Right as BinaryExpression); break;
                case ExpressionEnum.EnumNodeType.Constant:
                    right = VisitConstantExpression(func.Right as ConstantExpression); break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    right = VisitValueMemberExpression(func.Right as MemberExpression); break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    right = VisitValueUnaryExpression(func.Right as UnaryExpression); break;
                case ExpressionEnum.EnumNodeType.Call:
                    right = VisitMethodCallExpression(func.Right as MethodCallExpression); break;
            }
            var operaType = ExpressionTypeToString(func.NodeType);

            if (right == "null")
            {
                if (operaType == ":")
                    return "!" + left + ":*";
                else if (operaType == "<>")
                    return left + ":*";
            }
            else
            {
                switch (operaType)
                {
                    case ":":
                        result = left + ":" + right;
                        break;
                    case "<>":
                        result = "!" + left + ":" + right;
                        break;
                    case "AND":
                        result = "(" + left + " AND " + right + ")";
                        break;
                    case "OR":
                        result = "(" + left + " OR " + right + ")";
                        break;
                    case ">=":
                        result = left + ":[" + right + " TO *]";
                        break;
                    case "<=":
                        result = left + ":[* TO " + right + "]";
                        break;
                    case ">":
                        result = left + ":{" + right + " TO *}";
                        break;
                    case "<":
                        result = left + ":{* TO " + right + "}";
                        break;
                }
            }

            return result;
        }

        private string VisitUnaryExpression(UnaryExpression func, ref string Type)
        {

            var funcType = ExpressionEnum.CheckExpressionType(func.Operand);
            switch (funcType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    return VisitBinaryExpression(func.Operand as BinaryExpression);
                case ExpressionEnum.EnumNodeType.Constant:
                    return VisitConstantExpression(func.Operand as ConstantExpression);

                case ExpressionEnum.EnumNodeType.UndryOperator:
                    return VisitUnaryExpression(func.Operand as UnaryExpression);
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    return VisitMemberExpression(func.Operand as MemberExpression, ref Type);

            }
            return "";
        }

        private string VisitUnaryExpression(UnaryExpression func)
        {
            var funcType = ExpressionEnum.CheckExpressionType(func.Operand);
            switch (funcType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    return VisitBinaryExpression(func.Operand as BinaryExpression);
                case ExpressionEnum.EnumNodeType.Constant:
                    return VisitConstantExpression(func.Operand as ConstantExpression);

                case ExpressionEnum.EnumNodeType.UndryOperator:
                    return VisitUnaryExpression(func.Operand as UnaryExpression);
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    return VisitMemberExpression(func.Operand as MemberExpression);

            }
            return "";
        }
        private string VisitValueUnaryExpression(UnaryExpression func)
        {
            var funcType = ExpressionEnum.CheckExpressionType(func.Operand);
            switch (funcType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    return VisitBinaryExpression(func.Operand as BinaryExpression);
                case ExpressionEnum.EnumNodeType.Constant:
                    return VisitConstantExpression(func.Operand as ConstantExpression);

                case ExpressionEnum.EnumNodeType.UndryOperator:
                    return VisitUnaryExpression(func.Operand as UnaryExpression);
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    return VisitValueMemberExpression(func.Operand as MemberExpression);

            }
            return "";
        }

        private string VisitMemberExpression(MemberExpression func, ref string Type)
        {
            if (func.Type.IsGenericType && func.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type = func.Type.GetGenericArguments()[0].Name;
            }
            else if (func.Type.IsGenericType && func.Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type = func.Type.GetGenericArguments()[0].Name;
            }
            else
                Type = func.Type.Name;
            return VisitMemberExpression(func);
        }

        private string VisitMemberExpression(MemberExpression func)
        {

            var field = fieldmap.Find(a => a.EntityField == func.Member.Name);
            if (field != null)
            {
                if (!string.IsNullOrEmpty(field.SolrField))
                    return field.SolrField;
                else
                    return field.EntityField;
            }
            else
                return func.Member.Name.ToString();
        }

        private string VisitValueMemberExpression(MemberExpression func)
        {
            object value;
            if (func.Type.IsGenericType && func.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var typename = func.Type.GetGenericArguments()[0].Name;
                switch (typename)
                {
                    case "Int64":
                        {
                            var getter = Expression.Lambda<Func<long?>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "Int32":
                        {
                            var getter = Expression.Lambda<Func<int?>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "DateTime":
                        {
                            var getter = Expression.Lambda<Func<DateTime?>>(func).Compile();
                            value = getter();
                            DateTime time = (DateTime)value;
                            if (!string.IsNullOrEmpty(Helper.setting.solrtimezone) && Helper.setting.solrtimezone.ToLower() == "true")
                                time = time.AddHours(-8);
                            return time.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                    case "Decimal":
                        {
                            var getter = Expression.Lambda<Func<Decimal?>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "Double":
                        {
                            var getter = Expression.Lambda<Func<double?>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    default:
                        {
                            var getter = Expression.Lambda<Func<object>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                }
            }
            else
            {
                switch (func.Type.Name)
                {
                    case "Int64":
                        {
                            var getter = Expression.Lambda<Func<long>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "Int32":
                        {
                            var getter = Expression.Lambda<Func<int>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "String":
                        {
                            var getter = Expression.Lambda<Func<string>>(func).Compile();
                            value = getter();
                            return "\"" + value.ToString() + "\"";
                        }
                    case "DateTime":
                        {
                            var getter = Expression.Lambda<Func<DateTime>>(func).Compile();
                            value = getter();
                            DateTime time = (DateTime)value;
                            if (!string.IsNullOrEmpty(Helper.setting.solrtimezone) && Helper.setting.solrtimezone.ToLower() == "true")
                                time = time.AddHours(-8);
                            return time.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                    case "Decimal":
                        {
                            var getter = Expression.Lambda<Func<Decimal>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    case "Double":
                        {
                            var getter = Expression.Lambda<Func<double>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                    default:
                        {
                            var getter = Expression.Lambda<Func<object>>(func).Compile();
                            value = getter();
                            return value.ToString();
                        }
                }
            }

        }

        private static string VisitConstantExpression(ConstantExpression func)
        {
            object vaule = func.Value;
            string v_str = string.Empty;
            if (vaule == null)
            {
                return "null";
            }
            if (vaule is string)
            {
                if (vaule.ToString() == "")
                    v_str = "\"\"";
                else
                    v_str = string.Format("{0}", vaule.ToString().SolrReplace());
            }
            else if (vaule is DateTime)
            {
                DateTime time = (DateTime)vaule;
                if (!string.IsNullOrEmpty(Helper.setting.solrtimezone) && Helper.setting.solrtimezone.ToLower() == "true")
                    time = time.AddHours(-8);
                v_str = string.Format("{0}", time.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
            else
            {
                v_str = vaule.ToString();
            }
            return v_str;
        }

        private static string ExpressionTypeToString(ExpressionType func)
        {
            switch (func)
            {
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Equal: return ":";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Not: return "NOT";
                case ExpressionType.Add: return "+";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Convert: return "";
                default: return "unknown";
            }
        }

        private string VisitMethodCallExpression(MethodCallExpression func)
        {

            switch (func.Method.Name)
            {
                case "SolrLike":
                case "Like":
                    return CreateLikeMethodWhereString(func);
                case "SolrNotLike":
                case "NotLike":
                    return CreateNotLikeMethodWhereString(func);
                case "SolrIn":
                case "In":
                    return CreateInMethodWhereString(func);
                case "SolrNotIn":
                case "NotIn":
                    return CreateNotInMethodWhereString(func);
                case "SolrInLike":
                    return CreateInLikeMethodWhereString(func);
                case "Contains":
                    return CreateContainsMethodWhereString(func);
            }
            return "";
        }

        private string CreateLikeMethodWhereString(MethodCallExpression func)
        {
            var caller = VisitMemberExpression(func.Arguments[0] as MemberExpression);
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[1]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[1] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[1] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[1] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[1] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            var match = VisitConstantExpression(func.Arguments[2] as ConstantExpression);
            if (match.ToLower() == "left")
                return caller + ":*" + value;
            else if (match.ToLower() == "right")
                return caller + ":" + value + "*";
            else
                return caller + ":*" + value + "*";
        }

        private string CreateContainsMethodWhereString(MethodCallExpression func)
        {
            var caller = VisitMemberExpression(func.Object as MemberExpression);
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[0]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[0] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[0] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[0] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[0] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            return caller + ":" + value + "";
        }

        private string CreateNotLikeMethodWhereString(MethodCallExpression func)
        {
            var caller = VisitMemberExpression(func.Arguments[0] as MemberExpression);
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[1]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[1] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[1] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[1] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[1] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            var match = VisitConstantExpression(func.Arguments[2] as ConstantExpression);
            if (match.ToLower() == "left")
                return "!" + caller + ":*" + value;
            else if (match.ToLower() == "right")
                return "!" + caller + ":" + value + "*";
            else
                return "!" + caller + ":*" + value + "*";
        }

        private string CreateInMethodWhereString(MethodCallExpression func)
        {
            var leftType = ExpressionEnum.CheckExpressionType(func.Arguments[0]);
            string caller = "";
            string MemberType = "";
            switch (leftType)
            {
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    caller = VisitMemberExpression(func.Arguments[0] as MemberExpression, ref MemberType); break;
                case ExpressionEnum.EnumNodeType.Call:
                    caller = VisitMethodCallExpression(func.Arguments[0] as MethodCallExpression); break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    caller = VisitUnaryExpression(func.Arguments[0] as UnaryExpression, ref MemberType); break;
            }
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[1]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[1] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[1] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[1] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[1] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            string InString = "";
            if (MemberType == "String")
            {
                foreach (var item in value.Split(','))
                {
                    InString += "\"" + item + "\",";
                }
            }
            else
            {
                foreach (var item in value.Split(','))
                {
                    InString += item + " ";
                }
            }
            if (InString != "")
                return caller + ":(" + InString.Trim(',') + ")";
            else
                return "";
        }

        private string CreateInLikeMethodWhereString(MethodCallExpression func)
        {
            var leftType = ExpressionEnum.CheckExpressionType(func.Arguments[0]);
            string caller = "";
            string MemberType = "";
            switch (leftType)
            {
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    caller = VisitMemberExpression(func.Arguments[0] as MemberExpression, ref MemberType); break;
                case ExpressionEnum.EnumNodeType.Call:
                    caller = VisitMethodCallExpression(func.Arguments[0] as MethodCallExpression); break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    caller = VisitUnaryExpression(func.Arguments[0] as UnaryExpression, ref MemberType); break;
            }
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[1]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[1] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[1] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[1] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[1] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            string InString = "";
            if (MemberType == "String")
            {
                foreach (var item in value.Split(','))
                {
                    InString += "*" + item + "* ";
                }
            }
            else
            {
                foreach (var item in value.Split(','))
                {
                    InString += item + " ";
                }
            }
            if (InString != "")
                return caller + ":(" + InString.Trim(',') + ")";
            else
                return "";
        }

        private string CreateNotInMethodWhereString(MethodCallExpression func)
        {
            var leftType = ExpressionEnum.CheckExpressionType(func.Arguments[0]);
            string caller = "";
            string MemberType = "";
            switch (leftType)
            {
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    caller = VisitMemberExpression(func.Arguments[0] as MemberExpression, ref MemberType); break;
                case ExpressionEnum.EnumNodeType.Call:
                    caller = VisitMethodCallExpression(func.Arguments[0] as MethodCallExpression); break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    caller = VisitUnaryExpression(func.Arguments[0] as UnaryExpression, ref MemberType); break;
            }
            var rightType = ExpressionEnum.CheckExpressionType(func.Arguments[1]);
            var value = "";
            switch (rightType)
            {
                case ExpressionEnum.EnumNodeType.BinaryOperator:
                    value = VisitBinaryExpression(func.Arguments[1] as BinaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.Constant:
                    value = VisitConstantExpression(func.Arguments[1] as ConstantExpression);
                    break;
                case ExpressionEnum.EnumNodeType.UndryOperator:
                    value = VisitUnaryExpression(func.Arguments[1] as UnaryExpression);
                    break;
                case ExpressionEnum.EnumNodeType.MemberAccess:
                    value = VisitValueMemberExpression(func.Arguments[1] as MemberExpression);
                    break;
            }
            value = value.SolrReplace();
            string InString = "";
            if (MemberType == "String")
            {
                foreach (var item in value.Split(','))
                {
                    InString += "\"" + item + "\",";
                }
            }
            else
            {
                foreach (var item in value.Split(','))
                {
                    InString += item + " ";
                }
            }
            if (InString != "")
                return "!" + caller + ":(" + InString.Trim(',') + ")";
            else
                return "";
        }



        private string AnalysisExpression(Expression exp)
        {
            string TextSql = "";
            switch (exp.NodeType)
            {
                case ExpressionType.Call:
                    {
                        MethodCallExpression mce = exp as MethodCallExpression;
                        Console.WriteLine("The Method Is {0}", mce.Method.Name);
                        Console.WriteLine("The Method TypeOf {0}", mce.Method.DeclaringType);
                        if (mce.Method.DeclaringType == typeof(string))
                        {
                            break;
                        }

                        for (int i = 0; i < mce.Arguments.Count; i++)
                        {

                            TextSql += AnalysisExpression(mce.Arguments[i]);
                        }
                    }
                    break;
                case ExpressionType.Quote:
                    {
                        UnaryExpression ue = exp as UnaryExpression;
                        TextSql += AnalysisExpression(ue.Operand);
                    }
                    break;
                case ExpressionType.Lambda:
                    {
                        LambdaExpression le = exp as LambdaExpression;
                        AnalysisExpression(le.Body);

                    }
                    break;
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                    {
                        BinaryExpression be = exp as BinaryExpression;
                        Console.WriteLine("The Method Is {0}", exp.NodeType.ToString());
                        TextSql += AnalysisExpression(be.Left);
                        TextSql += AnalysisExpression(be.Right);
                    }
                    break;
                case ExpressionType.Constant:
                    {
                        ConstantExpression ce = exp as ConstantExpression;
                        Console.WriteLine("The Value Type Is {0}", ce.Value.ToString());
                    }
                    break;
                case ExpressionType.Parameter:
                    {
                        ParameterExpression pe = exp as ParameterExpression;
                        Console.WriteLine("The Parameter Is {0}", pe.Name);
                    }
                    break;
                default:
                    {
                        Console.Write("UnKnow");
                    }
                    break;
            }
            return TextSql;
        }


    }
}
