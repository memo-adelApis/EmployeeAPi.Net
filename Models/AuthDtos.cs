using System.ComponentModel.DataAnnotations;

namespace EmployeeApi.Models
{
    // النموذج المستخدم لطلب التسجيل
    public record RegisterModel(
        [Required] string Username,
        [Required][EmailAddress] string Email,
        [Required] string Password);

    // النموذج المستخدم لطلب تسجيل الدخول
    public record LoginModel(
        [Required] string Username,
        [Required] string Password);

    // النموذج المستخدم لإرجاع الرد بعد تسجيل الدخول
    public record AuthResponse(string Token, DateTime Expiration);
    public record CreateRoleModel([Required] string RoleName);
}