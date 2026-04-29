namespace IrbisTheatre.Models;

public class EmployerProperty
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    // Внешние ключи
    public int EmployerId { get; set; }
    public int PropertyId { get; set; }

    // Навигационные свойства
    public Employer? Employer { get; set; }
    public Property? Property { get; set; }
}