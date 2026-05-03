using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicoAutos.Application.Validators.Vehicles;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure;
using TicoAutos.Application.Mappings;
using TicoAutos.WebApi.Auth;
using TicoAutos.WebApi.Extensions;




// Defines the entry point of the application and configures services and middleware for the ASP.NET Core Web API.
var builder = WebApplication.CreateBuilder(args);

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is missing!");
var key = Encoding.UTF8.GetBytes(secretKey);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// Dependency Injection
builder.Services.AddInfrastructure(builder.Configuration);

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateVehicleValidator>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
}, typeof(MappingProfile).Assembly);


// Configure JWT Authentication
var authenticationBuilder = builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
})
.AddCookie(ExternalAuthSchemes.GoogleExternalCookie, opt =>
{
    opt.Cookie.Name = "TicoAutos.Google.External";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var googleCallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authenticationBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, opt =>
    {
        opt.ClientId = googleClientId;
        opt.ClientSecret = googleClientSecret;
        opt.SignInScheme = ExternalAuthSchemes.GoogleExternalCookie;

        if (!string.IsNullOrWhiteSpace(googleCallbackPath))
            opt.CallbackPath = googleCallbackPath;
    });
}

builder.Services.AddAuthorization();


// Configuration for CORS to allow requests from the Angular development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();


app.UseCors("AllowAngularDev");       
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
