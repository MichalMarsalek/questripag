# questripag
A library for implementing GET requests returning filtered &amp; ordered paged responses in .NET.

## Simple example
Say you have a collection of movies in your database. You'd like to build an `GET` endpoint returning paginated view into the data. The endpoint should support filtering & ordering by certain properties of the movies.
A request for the top 50 best rated comedy or romantic movies released in the year 2023 would correspond to a request like  
`GET /movie?page=1@50&order=-rating&released=2023-01-01..2023-12-31&category=romantic|comedy`.  
Let's see how you would satisfy such request using this library & EFCore.

```cs
builder.Services
    .AddQueryer()
    .AddControllers(x => x.AddQueryBinder());
    .AddOpenApiDocument(x => x.AddQueryMapping());

[Route("movie"), ApiController]
public class MovieController(DbContext dbContext, Queryer queryer): ControllerBase
{
  private DbContext _dbContext = dbContext;
  private Queryer _queryer = queryer;

  private Expression<Func<MovieEntity, MovieDto>> _projection = x => new MovieDto {
    Name = x.Name,
    Released = x.Released,
    Rating = x.Rating
    Category = x.Category
  }
  [HttpGet]
  // Query object is automatically bound to all the data in the querystring of the GET
  public Task<Page<MovieDto>> GetMovies(Query<IMovieQueryOptions> query) 
      => _queryer.QueryAsync(_dbContext.Movies, query, _projection);
}

// Definition of the properties available for filtering/ordering
public interface IMovieQueryOptions {
  public string Name {get;set;}
  public DateOnly Released {get;set;}
  public double Rating {get;set;}
  public MovieCategory Category {get;set;}
}

// Source data
public class MovieEntity: IMovieQueryOptions {
  public string Name {get;}
  public DateOnly Released {get;}
  public double Rating {get;}
  public MovieCategory Category {get;}
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

This is a very minimal setup. The type argument of `Query` defines which properties are available for filtering and sorting. By default each property is avaialble for both, but this can be configured with attributes. By default, each filter / order operation is applied to the source data type (in this case `MovieEntity`), that's why the source data should implement the query options interface. Instead of the default mapping of the query prop to the source prop of the same name, each query prop can have a custom behaviour defined.

## Available operations
The page is 1-indexed and is specified like `page=2`, or `page2@50` which also specifies the page size.
The result can be ordered by a property such as `order=count` or as descending with `order=-count`. Ordering by multiple coordinates is supported by either repeating `order`, such as `order=-released&order=name` or by simply concatenating the selectors, such as `order=-released+name`.
For each property, the query can restrict the result to the items for which the property admits a certain value or values. Ranges are supported by delimiting the bounds with `..`. Any bound can be ommited. Multiple values are supported by delimiting with `|`. Verbatim `..`/`|`/`\` are to be escaped using `\`. As an example, `released=2023-01-01..2023-12-31` restricts to 2023 movies, while `category=romantic|comedy` restricts to romantic or comedy movies.
Both scalar and collection data are supported in the source.

To disable filtering or ordering using a certain prop, use the `NoFilter` or `NoOrder` attribute.

### Renaming the fields
`JsonPropertyName` is respected.

### Nested fields
Filtering/ordering by nested fields is supported by default. To allow a filter like `nested.property=4`, create a `public int Nested_Property {get;set;}` property on your query options interface or use `[JsonPropertyName("nested.property")]` attribute.

### Custom filtering/ordering
In certain situations, the default generated `Expression`s can either be undesirable or untranslatable by EF. In these cases you can specify a custom behaviour of each prop in the context of filtering and ordering. (TODO)

### Type safety with nested & custom properties
Due to the nominal nature of C#'s type system, it is impossible to validate a correspondence between the source data type and the query options interface for nested props, let alone for props that have a custom filter/order behaviour defined. Therefore, in these cases we recommend having a base interface for the basic non-nested default behaving props that the source data type is validated against and then an inherited interface that's used as the actual type parameter of `Query`.

## Nswag support
This library customizes (TODO) the openAPI generation for the `Query<T>` type.

## Typescript support
This library includes two features helping integrating the rest APIs created using this library using a javascript frontend.
1. Functions for (de)serializing a query request to/from a query string. (TODO)
2. Javascript generator exposing the endpoints configuration, meaning which properties are available for filtering & ordering. This is useful when building a generic Table view. The table needs to know which columns to offer for filtering/orderign to the user.
