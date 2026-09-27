using RECETASMD.Data;
using RECETASMD.Models;
using RECETASMD.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RECETASMD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAzureBlobStorageService _blobStorageService;

        public RecipesController(AppDbContext context, IAzureBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RecipeResponseDto>>> GetRecipes()
        {
            var recipes = await _context.Recipes
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = recipes.Select(r => new RecipeResponseDto
            {
                Id = r.Id,
                Title = r.Title,
                AuthorName = r.AuthorName,
                ImageUrl = r.ImageUrl,
                Ingredients = JsonSerializer.Deserialize<List<string>>(r.IngredientsJson) ?? new(),
                PreparationSteps = JsonSerializer.Deserialize<List<string>>(r.PreparationJson) ?? new(),
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeResponseDto>> GetRecipeById(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "La receta no existe." });
            }

            return Ok(new RecipeResponseDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                AuthorName = recipe.AuthorName,
                ImageUrl = recipe.ImageUrl,
                Ingredients = JsonSerializer.Deserialize<List<string>>(recipe.IngredientsJson) ?? new(),
                PreparationSteps = JsonSerializer.Deserialize<List<string>>(recipe.PreparationJson) ?? new(),
                CreatedAt = recipe.CreatedAt
            });
        }

        [HttpPost("upload-image")]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Por favor selecciona un archivo de imagen válido." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Solo se permiten imágenes con extensión JPG, PNG o WEBP." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "El tamaño de la imagen no debe exceder los 5 MB." });
            }

            try
            {
                var imageUrl = await _blobStorageService.UploadImageAsync(file);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al procesar la imagen: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RecipeResponseDto>> CreateRecipe([FromBody] CreateRecipeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Parsear ingredientes por línea
            var ingredientsList = dto.IngredientsText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();

            // Parsear preparación por línea
            var preparationList = dto.PreparationText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var defaultImage = "https://images.unsplash.com/photo-1495521821757-a1efb6729352?q=80&w=800&auto=format&fit=crop";

            var recipe = new Recipe
            {
                Title = dto.Title.Trim(),
                AuthorName = string.IsNullOrWhiteSpace(dto.AuthorName) ? "Ana López" : dto.AuthorName.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? defaultImage : dto.ImageUrl,
                IngredientsJson = JsonSerializer.Serialize(ingredientsList),
                PreparationJson = JsonSerializer.Serialize(preparationList),
                CreatedAt = DateTime.UtcNow
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, new RecipeResponseDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                AuthorName = recipe.AuthorName,
                ImageUrl = recipe.ImageUrl,
                Ingredients = ingredientsList,
                PreparationSteps = preparationList,
                CreatedAt = recipe.CreatedAt
            });
        }
    }
}
