# questripag
A library for implementing GET requests returning filtered &amp; ordered paged responses in .NET.

## Simple example
Say you have a collection of movies in your database. You'd like to build an `GET` endpoint returning paginated view into the data. The endpoint should support filtering & ordering by certain properties of the movies (which might or might not be the same properties the endpoint returns).
A request for the top 50 best rated comedy or romantic movies released in the year 2023 would correspond to a request like  
`GET /movie?page=1@50&order=-rating&released=2023-01-01..2023-12-31&category=romantic|comedy`.  
Let's see how you would satisfy such request using this library & EFCore.

```cs
builder.Services
    .AddControllers(options => options.AddQueryBinder());

[Route("movie"), ApiController]
public class MovieController(DbContext dbContext, Queryer queryer): ControllerBase
{
  private DbContext _dbContext = dbContext;

  private Expression<Func<MovieEntity, MovieDto>> _projection = x => new MovieDto {
    Name = x.Name,
    Released = x.Released,
    Rating = x.Rating
    Category = x.Category
  }
  [HttpGet]
  // Query object is automatically bound to all the data in the querystring of the GET
  public Task<Page<MovieDto>> GetMovies(Query<MovieQueryFilter, MovieQueryOrder> query) 
      => _dbContext.Movies.Where(query).OrderBy(query).Select(_projection).ToPageAsync(query);
}

// Source data
public class MovieEntity {
  public string Name {get;}
  public DateOnly Released {get;}
  public double Rating {get;}
  public MovieCategory Category {get;}
}

// Definition of the properties available for filtering
public interface MovieQueryFilter {
  public Filter<string>? Name {get;set;}
  public Filter<DateOnly>? Released {get;set;}
  public Filter<double>? Rating {get;set;}
  public Filter<MovieCategory>? Category {get;set;}
}

// Definition of the properties available for ordering
public interface MovieQueryOrder {
  public Order? Name {get;set;}
  public Order? Released {get;set;}
  public Order? Rating {get;set;}
  public Order? Category {get;set;}
}

// Response data
public class MovieDto {
  public string Name {get;set;}
  public DateOnly Released {get;set;}
  public double Rating {get;set;}
  public MovieCategory Category {get;set;}
}

public enum MovieCategory {Comedy, Scifi, Thriller, Romantic}

// EF configuration & other project setup ommited
```

This is a very minimal setup. The type argument of `Query` defines which properties are available for filtering and sorting. By default, each filter / order operation is applied to the source data type (in this case `MovieEntity`).

## Available operations
The page is 1-indexed and is specified like `page=2`, or `page2@50` which also specifies the page size.
The result can be ordered by a property such as `order=count` or as descending with `order=-count`. Ordering by multiple coordinates is supported by either repeating `order`, such as `order=-released&order=name` or by simply concatenating the selectors, such as `order=-released+name`.
For each property, the query can restrict the result to the items for which the property admits a certain value or values. Ranges are supported by delimiting the bounds with `..`. Any bound can be ommited. Multiple values are supported by delimiting with `|`. Verbatim `..`/`|`/`\` are to be escaped using `\`. As an example, `released=2023-01-01..2023-12-31` restricts to 2023 movies, while `category=romantic|comedy` restricts to romantic or comedy movies.
Both scalar and collection data are supported in the source.

### Renaming the fields
`JsonPropertyName` is respected.

### Nested fields
Filtering/ordering by nested fields is supported by default.

### Custom filtering/ordering
The form shown above
```cs
_dbContext.Movies.Where(query).OrderBy(query).Select(_projection).ToPageAsync(query)
```
is good for simple cases where everything can be mapped automatically. However there is a more verbose option which allows for handling more complex cases:

```cs
_dbContext.Movies
  .Where(x => x.Name, query.Filter.Name)
  .Where(x => x.Released, query.Filter.Released)
  .Where(x => x.Rating, query.Filter.Rating)
  .Where(x => x.Category, query.Filter.Category)
  .OrderBy(x => x.Name, query.Order.Name)
  .OrderBy(x => x.Released, query.Order.Released)
  .OrderBy(x => x.Rating, query.Order.Rating)
  .OrderBy(x => x.Category, query.Order.Category)
  .Select(_projection).ToPageAsync(query)
```


## Typescript support
This library includes two features helping integrating the rest APIs created using this library using a javascript frontend.
1. Functions for (de)serializing a query request to/from a query string. (TODO)
2. Javascript generator exposing the endpoints configuration, meaning which properties are available for filtering & ordering. This is useful when building a generic Table view. The table needs to know which columns to offer for filtering/orderign to the user.
