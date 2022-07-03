using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace Reflection.Differentiation
{
    class Algebra
    {
        public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> func)
        {
            var function = func.Body;
            var x = func.Parameters;

            if (function is ConstantExpression)
                return z => 0;

            if (function is ParameterExpression)
                return z => 1;

            if (function is BinaryExpression binExpr)
            {
                if (binExpr.NodeType == ExpressionType.Add || binExpr.NodeType == ExpressionType.Subtract)
                    return DifferentiateAddSubstract(binExpr, x);

                if (binExpr.NodeType == ExpressionType.Multiply)
                    return DifferentiateMultiply(binExpr, x);
            }

            if (function is MethodCallExpression e && (e.Method.Name == "Sin" || e.Method.Name == "Cos"))
                return DifferentiateSinCos(e, x);

            if (function.ToString().Contains("ToString") || function.ToString().Contains("Max"))
                throw new ArgumentException(function.ToString() + "is not differentiable");

            return z => 0;
        }

        private static Expression<Func<double, double>> DifferentiateAddSubstract(BinaryExpression binExpr, ReadOnlyCollection<ParameterExpression> z)
        {
            var Left = Differentiate(Expression.Lambda<Func<double, double>>(binExpr.Left, z)).Body;
            var Right = Differentiate(Expression.Lambda<Func<double, double>>(binExpr.Right, z)).Body;

            return Expression.Lambda<Func<double, double>>(
                   System.Linq.Expressions.Expression.MakeBinary(binExpr.NodeType,
                   Left, Right), z);
        }

        private static Expression<Func<double, double>> DifferentiateMultiply(BinaryExpression binExpr, ReadOnlyCollection<ParameterExpression> z)
        {
            Expression Left = System.Linq.Expressions.Expression.MakeBinary(ExpressionType.Multiply,
                   Differentiate(Expression.Lambda<Func<double, double>>(binExpr.Left, z)).Body, binExpr.Right);

            Expression Right = System.Linq.Expressions.Expression.MakeBinary(ExpressionType.Multiply,
                   Differentiate(Expression.Lambda<Func<double, double>>(binExpr.Right, z)).Body, binExpr.Left);

            return Expression.Lambda<Func<double, double>>(
                   System.Linq.Expressions.Expression.MakeBinary(ExpressionType.Add, Left, Right), z);
        }

        private static Expression<Func<double, double>> DifferentiateSinCos(MethodCallExpression e, ReadOnlyCollection<ParameterExpression> z)
        {
            var innerDiff = Differentiate(Expression.Lambda<Func<double, double>>(e.Arguments[0], z));
            Expression resBody = Expression.Call(typeof(Math).GetMethod("Sin"), z);

            if (e.Method.Name == "Sin")
                resBody = Expression.Multiply(Expression.Call(typeof(Math).GetMethod("Cos"), e.Arguments[0]), innerDiff.Body);

            if (e.Method.Name == "Cos")
                resBody = Expression.Negate(Expression.Multiply(Expression.Call(typeof(Math).GetMethod("Sin"), e.Arguments[0]), innerDiff.Body));

            return Expression.Lambda<Func<double, double>>(resBody, z);
        }
    }

}
