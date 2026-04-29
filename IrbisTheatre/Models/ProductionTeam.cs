namespace IrbisTheatre.Models;

public class ProductionTeam
{
    public int Id { get; set; }
    public string? ParticipationType { get; set; }
    public string? ProductionPosition { get; set; }

    // Внешние ключи
    public int EmployerId { get; set; }
    public int RoleId { get; set; }

    // Навигационные свойства
    public Employer? Employer { get; set; }
    public Role? Role { get; set; }
    public ICollection<PerformanceGroup> PerformanceGroups { get; set; } = new List<PerformanceGroup>();
}