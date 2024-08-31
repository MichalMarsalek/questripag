using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Questripag;

public class Queryer
{
    public IQueryable<TSource> Filter<TSource, TQuery>(IQueryable<TSource> source, IFiltering<TQuery> filtering)
    {
        foreach(var filter in filtering.Filters)
        {
            source = Filter<TSource, TQuery>(source, filter);
        }
        return source;
    }

    public IQueryable<TSource> Filter<TSource, TQuery>(IQueryable<TSource> source, FilterCoordinate<object> filter)
    {
        // TODO Support custom prop expressions instead of calling `GetDefaultPropExpression`
        var propExpr = GetDefaultPropExpression<TSource>(filter.Key);
        // TODO support cases where propExpr points to a collection
        var propType = propExpr.ReturnType;
        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            throw new NotImplementedException();
        }
        Expression<Func<dynamic, bool>> filterPredicate;

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
        filterPredicate = (Expression.Lambda(Expression.Call(Expression.Constant(scalarValue), equalsMethod, Expression.Convert(y, typeof(object))), y) as Expression<Func<dynamic, bool>>)!;
        // filterPredicate = y => y.Equals((object)dynamicValue)

        return source.Where(filterPredicate.ComposeByInlining(propExpr));
    }


    public IQueryable<TSource> Order<TSource, TQuery>(IQueryable<TSource> source, IOrdering<TQuery> ordering)
    {
        // Support custom prop expressions instead of calling `GetDefaultPropExpression`
        var order = ordering.Orders.ToList();
        if (order.Any())
        {
            var expr0 = GetDefaultPropExpression<TSource>(order[0].Key);
            var ordered = order[0].IsDescending ? source.OrderByDescending(expr0) : source.OrderBy(expr0);
            foreach (var o in order.Skip(1))
            {
                var expr = GetDefaultPropExpression<TSource>(order[0].Key);
                ordered = o.IsDescending ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr);
            }
            return ordered;
        }
        return source;
    }

    private Expression<Func<TSource, dynamic>> GetDefaultPropExpression<TSource>(string propName)
    {
        var param = Expression.Parameter(typeof(TSource), "x");
        return Expression.Lambda<Func<TSource, dynamic>>(Expression.Property(param, propName), param);
    }
}
