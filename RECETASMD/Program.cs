using RECETASMD.Data;
using RECETASMD.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=recetas.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Inyectar Servicios
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Asegurar que la base de datos SQLite se crea automáticamente con la semilla de datos (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();

// Ruta raíz amigable para el navegador en http://localhost:5000/
app.MapGet("/", () => Results.Ok(new
{
    message = "API RecetasOMG (ASP.NET Core + SQLite) en funcionamiento 🚀",
    endpoints = new
    {
        recipes = "/api/recipes",
        auth = "/api/auth"
    },
    frontendUrl = "http://localhost:5173"
}));

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Servidor API RecetasOMG (ASP.NET Core) iniciado con éxito en http://0.0.0.0:5000");

// Escucha en todas las interfaces (no solo loopback) para que dispositivos
// físicos y emuladores en la misma red puedan conectarse por la IP LAN.
app.Run("http://0.0.0.0:5000");
