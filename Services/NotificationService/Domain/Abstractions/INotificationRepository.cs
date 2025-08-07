using NotificationService.Domain.Models;

namespace NotificationService.Domain.Abstractions
{
    public interface INotificationRepository : IDisposable
    {
        Task<NotificationDomainModel> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(NotificationDomainModel notification);
        Task<IEnumerable<NotificationDomainModel>> GetManyByIds(params Guid[] ids);
    }
}
