using KIAN.Data;
using KIAN.Models;
using KIAN.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Base de datos (Azure SQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios
builder.Services.AddSingleton<CurrentUserService>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<SpacedRepetitionService>();
builder.Services.AddScoped<CardService>();

// CORS (solo desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Usuario único: usa el primero que exista o crea uno
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = app.Configuration;

    var user = await db.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
    if (user is null)
    {
        user = new User
        {
            Name = config["DefaultUser:Name"] ?? "Kian",
            Email = config["DefaultUser:Email"] ?? "kian@local"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    app.Services.GetRequiredService<CurrentUserService>().Set(user.Id);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Dev");
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();