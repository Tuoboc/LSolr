using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LSolr.Model
{
    public static class ExpressionEnum
    {
        public enum EnumNodeType
        {

            BinaryOperator = 1,

            UndryOperator = 2,

            Constant = 3,

            MemberAccess = 4,

            Call = 5,

            Unknown = -99,

            NotSupported = -98
        }

        public static EnumNodeType CheckExpressionType(Expression func)
        {
            switch (func.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.NotEqual:
                    return EnumNodeType.BinaryOperator;
                case ExpressionType.Constant:
                    return EnumNodeType.Constant;
                case ExpressionType.MemberAccess:
                    return EnumNodeType.MemberAccess;
                case ExpressionType.Call:
                    return EnumNodeType.Call;
                case ExpressionType.Not:
                case ExpressionType.Convert:
                    return EnumNodeType.UndryOperator;
                default:
                    return EnumNodeType.Unknown;
            }
        }
    }
}
