using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IrbisTheatre.Models;

public class Rehearsal
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Datetime { get; set; }

    [Required]
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty; // Место проведения (Большая сцена, Малая сцена, Репетиционный зал)

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? Status { get; set; } = "запланирована";

    // Внешние ключи
    public int PlayId { get; set; }

    // Навигационные свойства
    [ForeignKey("PlayId")]
    public virtual Play? Play { get; set; }

    // Связь с сотрудниками (кто участвует в репетиции)
    public virtual ICollection<RehearsalParticipant> Participants { get; set; } = new List<RehearsalParticipant>();
}