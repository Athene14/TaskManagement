using System.ComponentModel.DataAnnotations;

namespace TaskManagementServices.Shared.AuthService.DTO
{
    public class LoginRequest
    {
        /// <summary>
        /// Email
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
