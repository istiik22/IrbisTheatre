using System.Data;

namespace IrbisTheatre.Models;

public class Play
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TargetAudience { get; set; }
    public DateTime? PremiereDate { get; set; }  // ? ИСПРАВЛЕНО: DateTime вместо DateTimeOffset

    // Внешние ключи
    public int AuthorId { get; set; }
    public int GenreId { get; set; }

    // Навигационные свойства
    public Author? Author { get; set; }
    public Genre? Genre { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Performance> Performances { get; set; } = new List<Performance>();
}