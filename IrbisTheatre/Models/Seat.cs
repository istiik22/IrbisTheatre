using System.Net.Sockets;

namespace IrbisTheatre.Models;

public class Seat
{
    public int Id { get; set; }
    public short RowNumber { get; set; }
    public short SeatNumber { get; set; }
    public string? Category { get; set; }

    // Внешние ключи
    public int HallId { get; set; }

    // Навигационные свойства
    public Hall? Hall { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}