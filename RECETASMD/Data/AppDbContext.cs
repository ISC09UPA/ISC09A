using RECETASMD.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RECETASMD.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario Semilla (Ana López y Diego Ruíz)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Ana López",
                    Email = "ana@correo.com",
                    Password = "password123",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Name = "Diego Ruíz",
                    Email = "diego@correo.com",
                    Password = "password123",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Recetas Semilla coincidiendo exactamente con el prototipo
            var pastaIngredients = JsonSerializer.Serialize(new List<string>
            {
                "200 g de pasta",
                "250 g de champiñones",
                "150 ml de crema",
                "1 diente de ajo",
                "30 g de parmesano",
                "1 cda. de aceite",
                "Sal, pimienta y perejil al gusto"
            });

            var pastaSteps = JsonSerializer.Serialize(new List<string>
            {
                "1. Cuece la pasta: Cocina según el paquete. Reserva un poco del agua y escurre.",
                "2. Dora los champiñones: Calienta el aceite y saltea el ajo y los champiñones durante 6 minutos.",
                "3. Mezcla y sirve: Añade crema, pasta y parmesano. Ajusta la sal y termina con perejil."
            });

            var tacosIngredients = JsonSerializer.Serialize(new List<string>
            {
                "300 g de pechuga de pollo deshebrada",
                "8 tortillas de maíz calientitas",
                "2 limones jugosos",
                "1/2 cebolla picada y cilantro",
                "Salsa verde o roja al gusto"
            });

            var tacosSteps = JsonSerializer.Serialize(new List<string>
            {
                "1. Sazona el pollo: Saltea el pollo deshebrado con sal, pimienta y un toque de limón.",
                "2. Prepara los tacos: Coloca el pollo sobre las tortillas de maíz bien calientes.",
                "3. Agrega toppings: Decora con cebolla, cilantro picado, exprime limón y sirve con tu salsa favorita."
            });

            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Id = 1,
                    Title = "Pasta cremosa con champiñones",
                    AuthorName = "Ana López",
                    ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3d5d6281313?q=80&w=800&auto=format&fit=crop",
                    IngredientsJson = pastaIngredients,
                    PreparationJson = pastaSteps,
                    CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new Recipe
                {
                    Id = 2,
                    Title = "Tacos de pollo con limón",
                    AuthorName = "Diego Ruíz",
                    ImageUrl = "https://images.unsplash.com/photo-1565299585323-38d6b0865b47?q=80&w=800&auto=format&fit=crop",
                    IngredientsJson = tacosIngredients,
                    PreparationJson = tacosSteps,
                    CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
