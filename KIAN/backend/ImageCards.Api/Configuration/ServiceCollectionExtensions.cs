using System.Text.Json.Serialization;
using ImageCards.Api.Data;
using ImageCards.Api.HealthChecks;
using ImageCards.Api.Middleware;
using ImageCards.Api.Services.Cards;
using ImageCards.Api.Services.CurrentUser;
using ImageCards.Api.Services.Images;
using ImageCards.Api.Services.Reviews;
using ImageCards.Api.Services.SpacedRepetition;
using ImageCards.Api.Services.Storage;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Configuration;

/// <summary>Service registration grouped by concern, so Program.cs stays small.</summary>
public static class ServiceCollectionExtensions
{
    public const string ConnectionStringName = "DefaultConnection";

    // Room for the multipart boundaries and the translation fields around the image.
    private const long MultipartOverheadBytes = 64 * 1024;

    public static IServiceCollection AddImageCardsApi(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

        services.AddProblemDetails(o =>
            o.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath))
            {
                o.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddImageCardsDatabase(this IServiceCollection services)
    {
        services.AddDbContext<ImageCardsDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is not configured. " +
                    $"Set ConnectionStrings__{ConnectionStringName} or use dotnet user-secrets.");
            }

            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
        });

        return services;
    }

    public static IServiceCollection AddImageCardsStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection(AzureStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<ImageUploadOptions>()
            .Bind(configuration.GetSection(ImageUploadOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FormOptions>()
            .Configure<IOptions<ImageUploadOptions>>((form, upload) =>
                form.MultipartBodyLengthLimit = upload.Value.MaxBytes + MultipartOverheadBytes);

        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IImageValidator, ImageValidator>();
        return services;
    }

    public static IServiceCollection AddImageCardsServices(this IServiceCollection services)
    {
        // Swap this registration to change the algorithm (e.g. FsrsSpacedRepetitionScheduler).
        services.AddSingleton<ISpacedRepetitionScheduler, Sm2SpacedRepetitionScheduler>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IReviewService, ReviewService>();
        return services;
    }

    /// <summary>
    /// Development uses a fixed local user. Every other environment requires an authenticated principal,
    /// so nothing works outside Development until real authentication is configured.
    /// </summary>
    public static IServiceCollection AddImageCardsCurrentUser(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddOptions<DevelopmentUserOptions>()
                .Bind(configuration.GetSection(DevelopmentUserOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<ICurrentUserService, DevelopmentCurrentUserService>();
        }
        else
        {
            services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();
        }

        return services;
    }

    public static IServiceCollection AddImageCardsHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ImageCardsDbContext>("sql")
            .AddCheck<BlobStorageHealthCheck>("blob-storage", timeout: TimeSpan.FromSeconds(10));
        return services;
    }
}
