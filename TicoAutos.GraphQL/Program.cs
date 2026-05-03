using TicoAutos.GraphQL;
using TicoAutos.GraphQL.Extensions;
using TicoAutos.Infrastructure;

// Entry point for the TicoAutos GraphQL service.
// This is a standalone ASP.NET Core project separate from the REST API,
// sharing the same database and JWT authentication system.
var builder = WebApplication.CreateBuilder(args);

// Infrastructure: registers DbContext, UnitOfWork and repositories
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication — same secret/issuer/audience as TicoAutos.WebApi
builder.Services.AddJwtAuthentication(builder.Configuration);

// HotChocolate GraphQL server with all query types
builder.Services.AddGraphQlServer();

// CORS for Angular dev client
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

app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();

// GraphQL endpoint — accessible at /graphql
app.MapGraphQL();

app.Run();