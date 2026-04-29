using System.Net.Sockets;

namespace IrbisTheatre.Models;

public class Performance
{
    public int Id { get; set; }
    public DateTime Datetime { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsPremiere { get; set; }
    public string? Status { get; set; }

    // Внешние ключи
    public int PlayId { get; set; }

    // Навигационные свойства
    public Play? Play { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<PerformanceGroup> PerformanceGroups { get; set; } = new List<PerformanceGroup>();
}