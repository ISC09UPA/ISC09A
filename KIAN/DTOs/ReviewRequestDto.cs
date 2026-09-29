using System.ComponentModel.DataAnnotations;

namespace KIAN.DTOs;

public class ReviewRequestDto
{
    [Required]
    public Guid CardId { get; set; }

    /// <summary>0 = Otra vez, 1 = Difícil, 2 = Bien, 3 = Fácil</summary>
    [Range(0, 3)]
    public int Quality { get; set; }
}