namespace IrbisTheatre.Models;

public class Ticket
{
    public int Id { get; set; }
    public string UniqueNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Status { get; set; } // free, sold, reserved

    // Внешние ключи
    public int PerformanceId { get; set; }
    public int SeatId { get; set; }

    // Навигационные свойства
    public Performance? Performance { get; set; }
    public Seat? Seat { get; set; }
}