using System.ComponentModel.DataAnnotations;

namespace IrbisTheatre.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите ФИО")]
    [Display(Name = "ФИО")]
    [StringLength(200, ErrorMessage = "ФИО не должно превышать 200 символов")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]

    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Введите номер телефона")]      
    [Phone(ErrorMessage = "Некорректный номер телефона")]    
    [Display(Name = "Телефон")]                              
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите должность")]
    [Display(Name = "Должность")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите отдел")]
    [Display(Name = "Отдел")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите, что вы сотрудник")]
    public bool IsEmployee { get; set; }

    [Required(ErrorMessage = "Введите код подтверждения")]
    [Display(Name = "Код подтверждения")]
    public string VerificationCode { get; set; } = string.Empty;
}