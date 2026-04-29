namespace IrbisTheatre.Models;

public class Property
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство
    public ICollection<EmployerProperty> EmployerProperties { get; set; } = new List<EmployerProperty>();
}