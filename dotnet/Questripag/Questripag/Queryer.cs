﻿using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Questripag;

public interface IQueryer
{
    public IQueryable<TSource> Filter<TSource, TQuery>(IQueryable<TSource> source, IFiltering<TQuery> filtering);
    public IQueryable<TSource> Order<TSource, TQuery>(IQueryable<TSource> source, IOrdering<TQuery> ordering);
}

internal class Queryer(QueryerOptions? options = null) : IQueryer
{
    public QueryerOptions Options { get; set; } = options ?? new();

    public IQueryable<TSource> Filter<TSource, TQuery>(IQueryable<TSource> source, IFiltering<TQuery> filtering)
    {
        foreach(var filter in filtering.Filters)
        {
            source = source.Where(GetFilterPredicate<TSource, TQuery>(filter));
        }
        return source;
    }

    public Expression<Func<TSource, bool>> GetFilterPredicate<TSource, TQuery>(FilterCoordinate<dynamic> filter)
    {
        // TODO Support custom prop expressions instead of calling `GetDefaultPropExpression`
        var propExpr = GetDefaultPropExpression<TSource>(filter.Key);
        // TODO support cases where propExpr points to a collection
        var propType = propExpr.ReturnType;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            throw new NotImplementedException();
        }
        LambdaExpression filterPredicate;

        var y = Expression.Parameter(propType, "y");

        // TODO support multiple values
        var values = filter.Value.ToList();
        if (values.Count > 1) throw new NotImplementedException();

        if (!(values.First() is ScalarFilterValue<object>))
        {
            // TODO support range filter
            throw new NotImplementedException();
        }
        var scalarValue = ((ScalarFilterValue<object>)values.First()).Value;

        var equalsMethod = propType.GetMethod("Equals", [typeof(object)]);
        filterPredicate = Expression.Lambda(Expression.Call(Expression.Constant(scalarValue), equalsMethod, Expression.Convert(y, typeof(object))), y);
        // filterPredicate = y => y.Equals((object)dynamicValue)

        return (Expression<Func<TSource, bool>>)filterPredicate.ComposeByInlining(propExpr);
    }


    public IQueryable<TSource> Order<TSource, TQuery>(IQueryable<TSource> source, IOrdering<TQuery> ordering)
    {
        // Support custom prop expressions instead of calling `GetDefaultPropExpression`
        var order = ordering.Orders.ToList();
        if (order.Any())
        {
            var expr0 = (Expression<Func<TSource, object>>)GetDefaultPropExpression<TSource>(order[0].Key);
            var ordered = order[0].IsDescending ? source.OrderByDescending(expr0) : source.OrderBy(expr0);
            foreach (var o in order.Skip(1))
            {
                var expr = (Expression<Func<TSource, object>>)GetDefaultPropExpression<TSource>(order[0].Key);
                ordered = o.IsDescending ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr);
            }
            return ordered;
        }
        return source;
    }

    private LambdaExpression GetDefaultPropExpression<TSource>(string propName)
    {
        var param = Expression.Parameter(typeof(TSource), "x");
        Expression body = param;
        foreach(var part in propName.Split("."))
        {
            body = Expression.Property(body, part);
        }
        return Expression.Lambda(body, param);
    }
}

public class QueryerOptions
{
    public StringFilterOperation StringOperation { get; set; } = StringFilterOperation.StartsWith;
    public DateTimeFilterPrecision DateTimePrecision { get; set; } = DateTimeFilterPrecision.Days;
    public enum StringFilterOperation { Equals, StartsWith, Contains }
    public enum DateTimeFilterPrecision { Exact, Seconds, Minutes, Days }
}
