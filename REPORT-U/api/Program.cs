using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportU.Configuration;
using ReportU.Data;
using ReportU.Middleware;
using ReportU.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// .env local (opcional): si existe .env junto al binario y la variable aún no
// está definida en el entorno, se importa. En producción mandan las variables
// reales del entorno (podman/docker secrets, App Service, etc.).
// ---------------------------------------------------------------------------
LoadDotEnv(builder.Environment.ContentRootPath);

// ---------------------------------------------------------------------------
// Configuración (toda por variables de entorno, ver .env.example):
//   ConnectionStrings__DefaultConnection, Jwt__Key, Jwt__Issuer,
//   Jwt__Audience, Jwt__ExpiresMinutes, AzureBlob__ConnectionString,
//   AzureBlob__Container
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Configura Jwt__Key con al menos 32 caracteres (ver .env.example).");

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.Configure<BlobOptions>(
    builder.Configuration.GetSection(BlobOptions.SectionName));

// ---------------------------------------------------------------------------
// MVC + JSON: enums como strings ("Incidencia", no 0) para un contrato legible
// en la app móvil.
// ---------------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------------------
// EF Core + PostgreSQL.
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<ReportUDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------
// Servicios de la app.
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPostImageService, PostImageService>();

var blobConnectionString = builder.Configuration["AzureBlob:ConnectionString"];
if (!string.IsNullOrWhiteSpace(blobConnectionString))
    builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
else
    builder.Services.AddSingleton<IBlobStorageService, LocalFileStorageService>();

// ---------------------------------------------------------------------------
// Autenticación JWT (Bearer). El 401/403 sale como ProblemDetails JSON para
// que la app móvil lo maneje igual que el resto de errores.
// ---------------------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        o.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/problem+json";
                return context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = 401,
                    Title = "No autenticado",
                    Detail = "Envía el token como 'Authorization: Bearer {token}'.",
                    Extensions =
                    {
                        ["code"] = ErrorCodes.Unauthorized,
                        ["traceId"] = context.HttpContext.TraceIdentifier,
                    },
                });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/problem+json";
                return context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = 403,
                    Title = "Prohibido",
                    Detail = "No tienes permiso para este recurso.",
                    Extensions =
                    {
                        ["code"] = ErrorCodes.ForbiddenNotOwner,
                        ["traceId"] = context.HttpContext.TraceIdentifier,
                    },
                });
            },
        };
    });
builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Swagger (Swashbuckle) con documentación por endpoint (XML comments) y botón
// "Authorize" para probar con JWT.
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ReportU API",
        Version = "v1",
        Description = "Comunidad universitaria: publicaciones, imágenes (Azure Blob Storage), apoyos, comentarios y guardados.",
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xml)) o.IncludeXmlComments(xml);

    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el accessToken obtenido en /api/auth/login o /api/auth/register.",
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

// Convención global de errores: application/problem+json con {code, traceId}.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportU API v1");
    o.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Crea el contenedor de Azure si se configuró conexión (best-effort: sin Azure
// la app sigue funcionando con almacenamiento local).
if (!string.IsNullOrWhiteSpace(blobConnectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var azure = scope.ServiceProvider.GetRequiredService<IBlobStorageService>() as AzureBlobStorageService;
        if (azure is not null) await azure.EnsureContainerAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo crear/verificar el contenedor de Azure Blob Storage. La subida de imágenes fallará hasta configurar AzureBlob__ConnectionString.");
    }
}

app.Run();

// Carga mínima de .env (KEY=VALUE, ignora comentarios y comillas).
static void LoadDotEnv(string contentRoot)
{
    var path = Path.Combine(contentRoot, ".env");
    if (!File.Exists(path)) return;
    foreach (var raw in File.ReadAllLines(path))
    {
        var line = raw.Trim();
        if (line.Length == 0 || line.StartsWith('#')) continue;
        var sep = line.IndexOf('=');
        if (sep <= 0) continue;
        var key = line[..sep].Trim();
        var value = line[(sep + 1)..].Trim().Trim('"').Trim('\'');
        if (!string.IsNullOrEmpty(key) && Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

// Necesario para WebApplicationFactory en futuros tests de integración.
public partial class Program;
