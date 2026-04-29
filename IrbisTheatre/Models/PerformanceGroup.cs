namespace IrbisTheatre.Models;

public class PerformanceGroup
{
    public int Id { get; set; }

    // Внешние ключи
    public int PerformanceId { get; set; }
    public int ProductionTeamId { get; set; }

    // Навигационные свойства
    public Performance? Performance { get; set; }
    public ProductionTeam? ProductionTeam { get; set; }
}