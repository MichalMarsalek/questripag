using System.Linq.Expressions;

namespace Questripag;

public static class ExpressionExtensions
{
    public static Expression Substitute(this Expression expr, ParameterExpression target, Expression replacement)
        => new ParameterReplacementExpressionVisitor(target, replacement).Visit(expr);

    public static Expression<Func<T1, T3>> ComposeByInlining<T1, T2, T3>(this Expression<Func<T2, T3>> outer, Expression<Func<T1, T2>> inner)
        => Expression.Lambda<Func<T1, T3>>(outer.Body.Substitute(outer.Parameters[0], inner.Body), inner.Parameters[0]);

    public static LambdaExpression ComposeByInlining(this LambdaExpression outer, LambdaExpression inner)
        => Expression.Lambda(outer.Body.Substitute(outer.Parameters[0], inner.Body), inner.Parameters[0]);

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, right.Body.Substitute(right.Parameters[0], left.Parameters[0])), left.Parameters[0]);

    public static LambdaExpression Or(this LambdaExpression left, LambdaExpression right)
        => Expression.Lambda(Expression.OrElse(left.Body, right.Body.Substitute(right.Parameters[0], left.Parameters[0])), left.Parameters[0]);

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, right.Body.Substitute(right.Parameters[0], left.Parameters[0])), left.Parameters[0]);

    public static LambdaExpression And(this LambdaExpression left, LambdaExpression right)
        => Expression.Lambda(Expression.AndAlso(left.Body, right.Body.Substitute(right.Parameters[0], left.Parameters[0])), left.Parameters[0]);

    private class ParameterReplacementExpressionVisitor : ExpressionVisitor
    {
        public ParameterExpression target { get; private set; }
        public Expression replacement { get; private set; }

        public ParameterReplacementExpressionVisitor(ParameterExpression target, Expression replacement)
        {
            this.target = target;
            this.replacement = replacement;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == target ? replacement : node;
        }
    }
}

