using Questripag;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApiDocument(x =>
{
    x.Title = "Test API";
    x.AddQueryMapping();    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi()
    .UseSwaggerUi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
