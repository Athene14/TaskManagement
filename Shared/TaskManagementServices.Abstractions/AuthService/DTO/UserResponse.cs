using System.ComponentModel.DataAnnotations;

namespace TaskManagementServices.Shared.AuthService.DTO
{
    public class UserResponse
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Полное имя
        /// </summary>
        [Required]
        public string FullName { get; set; }
    }
}
