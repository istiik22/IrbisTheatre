namespace IrbisTheatre.Models;

public class Employer
{
    public int Id { get; set; }
    public string Fio { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Contacts { get; set; } 
    public string? Email { get; set; }    
    public string? Password { get; set; }  
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public string? Status { get; set; }

    public ICollection<ProductionTeam> ProductionTeams { get; set; } = new List<ProductionTeam>();
    public ICollection<EmployerProperty> EmployerProperties { get; set; } = new List<EmployerProperty>();
}