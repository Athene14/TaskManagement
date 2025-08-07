using System.ComponentModel.DataAnnotations;

namespace TaskManagementServices.Shared.TaskService.DTO
{
    public class PagedResponse<T>
    {
        /// <summary>
        /// Текущая страница
        /// </summary>
        [Required]
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы
        /// </summary>
        [Required]
        public int PageSize { get; set; }

        /// <summary>
        /// Общее число элементов
        /// </summary>
        [Required]
        public int TotalCount { get; set; }

        [Required]
        public List<T> Items { get; set; }

        public PagedResponse(List<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
