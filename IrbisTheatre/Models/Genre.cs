namespace IrbisTheatre.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство
    public ICollection<Play> Plays { get; set; } = new List<Play>();
}