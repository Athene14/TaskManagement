using System.ComponentModel.DataAnnotations;

namespace TaskManagementServices.Shared.AuthService.DTO
{
    public class RegisterRequest
    {
        /// <summary>
        /// Полное имя
        /// </summary>
        [Required]
        public string FullName { get; set; }

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
