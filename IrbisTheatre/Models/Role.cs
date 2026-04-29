namespace IrbisTheatre.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenderRequirement { get; set; } // male, female, any
    public string? AgeRange { get; set; }
    public string? VoiceRequirements { get; set; }

    // Внешние ключи
    public int PlayId { get; set; }

    // Навигационные свойства
    public Play? Play { get; set; }
    public ICollection<ProductionTeam> ProductionTeams { get; set; } = new List<ProductionTeam>();
}