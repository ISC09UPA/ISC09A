using System.ComponentModel.DataAnnotations;

namespace RECETASMD.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateRecipeDto
    {
        [Required(ErrorMessage = "El título de la receta es obligatorio")]
        [StringLength(80, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del autor es obligatorio")]
        public string AuthorName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Los ingredientes son obligatorios")]
        public string IngredientsText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los pasos de preparación son obligatorios")]
        public string PreparationText { get; set; } = string.Empty;
    }

    public class RecipeResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public List<string> PreparationSteps { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
