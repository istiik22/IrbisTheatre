namespace IrbisTheatre.Models;

public class Hall
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационные свойства
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}