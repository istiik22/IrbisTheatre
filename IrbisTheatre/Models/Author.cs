namespace IrbisTheatre.Models;

public class Author
{
    public int Id { get; set; }
    public string Fio { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? YearsOfLife { get; set; }
    public string? Biography { get; set; }

    // Навигационное свойство
    public ICollection<Play> Plays { get; set; } = new List<Play>();
}