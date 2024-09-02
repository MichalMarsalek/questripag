using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using Questripag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
