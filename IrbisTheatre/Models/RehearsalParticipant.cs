using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IrbisTheatre.Models;

public class RehearsalParticipant
{
    [Key]
    public int Id { get; set; }

    public int RehearsalId { get; set; }

    public int EmployerId { get; set; }

    [MaxLength(50)]
    public string? Role { get; set; } // режиссёр, актёр, музыкант и т.д.

    [ForeignKey("RehearsalId")]
    public virtual Rehearsal? Rehearsal { get; set; }

    [ForeignKey("EmployerId")]
    public virtual Employer? Employer { get; set; }
}