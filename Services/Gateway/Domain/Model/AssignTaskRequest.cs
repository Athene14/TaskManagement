namespace Gateway.Domain.Model
{
    public class AssignTaskRequest
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        public required Guid AssignedUserId { get; set; }
    }

}

