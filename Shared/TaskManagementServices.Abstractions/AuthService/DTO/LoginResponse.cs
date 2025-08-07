using System.ComponentModel.DataAnnotations;

namespace TaskManagementServices.Shared.AuthService.DTO
{
    public class LoginResponse
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// JWT токен
        /// </summary>
        [Required]
        public string Token { get; set; }

    }
}
