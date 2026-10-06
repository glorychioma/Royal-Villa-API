using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings")["SecretKey"]);


builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token"
            }
        };

        document.Security = [
            new OpenApiSecurityRequirement{
                {new OpenApiSecuritySchemeReference ("Bearer"), new List<string>() }
            }
            ];
        return Task.CompletedTask;
    });
    });

builder.Services.AddAutoMapper(o =>
    {
        o.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
         o.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
        o.CreateMap<Villa, VillaDTO>().ReverseMap();
        o.CreateMap<User, UserDTO>().ReverseMap();
        o.CreateMap<VillaAmenities, VillaAmenitiesCreateDTO>().ReverseMap();
        o.CreateMap<VillaAmenities, VillaAmenitiesUpdateDTO>().ReverseMap();
        o.CreateMap<VillaAmenities, VillaAmenitiesDTO>()
        .ForMember(dest => dest.VillaName, opt => opt.MapFrom(src => src.Villa != null ? src.Villa.Name : null));
        o.CreateMap<VillaAmenitiesDTO, VillaAmenities>().ForMember(dest => dest.Id, opt => opt.Ignore());

    }
);

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
