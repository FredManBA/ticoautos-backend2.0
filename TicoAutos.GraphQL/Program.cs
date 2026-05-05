using TicoAutos.GraphQL;
using TicoAutos.GraphQL.Extensions;
using TicoAutos.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGraphQlServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MIDDLEWARE ERROR: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw;
    }
});

app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.MapGraphQL();

app.Run();