using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaskManagementServices.Shared.TaskService;
using TaskManagementServices.Shared.TaskService.DTO;

namespace TaskManagementServices.Shared.Routers
{
    internal class TaskServiceRouter : ITaskService
    {
        private readonly TaskServiceConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TaskServiceRouter> _logger;

        public TaskServiceRouter(
            IHttpClientFactory httpClientFactory,
            TaskServiceConfiguration configuration,
            ILogger<TaskServiceRouter> logger)
        {
            _config = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        private string BaseUrl => $"http://{_config.GetHost()}:{_config.GetPort()}";

        public async Task<TaskResponse> CreateTaskAsync(Guid initiatorUserId, CreateTaskRequest request)
        {
            var url = $"{BaseUrl}?initiatorUserId={initiatorUserId}";
            _logger.LogDebug("Creating task | Initiator: {InitiatorUserId}, Title: '{Title}'", initiatorUserId, request.Title);

            var response = await _httpClient.PostAndReadResponseAsync<TaskResponse>(url, request);

            _logger.LogInformation("Task created | ID: {TaskId}, Initiator: {InitiatorUserId}, Title: '{Title}'",
                response.Id,
                initiatorUserId,
                request.Title);

            return response;
        }

        public async Task<bool> DeleteTaskAsync(Guid initiatorUserId, Guid taskId)
        {
            var url = $"{BaseUrl}/{taskId}?initiatorUserId={initiatorUserId}";
            _logger.LogDebug("Deleting task | Task ID: {TaskId}, Initiator: {InitiatorUserId}", taskId, initiatorUserId);

            await _httpClient.DeleteAndReadResponseAsync(url);

            _logger.LogInformation("Task deleted | Task ID: {TaskId}, Initiator: {InitiatorUserId}", taskId, initiatorUserId);

            return true;
        }

        public async Task<TaskResponse> GetTaskByIdAsync(Guid taskId)
        {
            var url = $"{BaseUrl}/{taskId}";
            _logger.LogDebug("Fetching task by ID: {TaskId}", taskId);

            var response = await _httpClient.GetAndReadResponseAsync<TaskResponse>(url);

            _logger.LogInformation("Retrieved task | ID: {TaskId}, Status: {Status}, Assignee: {AssigneeId}",
                response.Id,
                response.IsActive,
                response.AssignedUserId);

            return response;
        }

        public async Task<IEnumerable<TaskSnapshotResponse>> GetTaskHistoryAsync(Guid taskId)
        {
            var url = $"{BaseUrl}/{taskId}/history";
            _logger.LogDebug("Fetching history for task: {TaskId}", taskId);

            var response = await _httpClient.GetAndReadResponseAsync<List<TaskSnapshotResponse>>(url);

            _logger.LogInformation("Retrieved {Count} history records for task: {TaskId}",
                response.Count,
                taskId);

            return response;
        }

        public async Task<PagedResponse<TaskResponse>> GetWithFilterAsync(TaskFilter filter, int page, int pageSize)
        {
            var baseUrl = $"{BaseUrl}?page={page}&pageSize={pageSize}";
            var url = baseUrl;

            if (filter != null)
            {
                _logger.LogDebug("Applying task filter | Page: {Page}/{PageSize}, Filter: {@Filter}",
                    page,
                    pageSize,
                    filter);

                var queryParams = new List<string>();
                foreach (var prop in filter.GetType().GetProperties())
                {
                    var value = prop.GetValue(filter);
                    if (value != null)
                    {
                        queryParams.Add($"{prop.Name}={WebUtility.UrlEncode(value.ToString())}");
                    }
                }

                if (queryParams.Count > 0)
                {
                    url += "&" + string.Join("&", queryParams);
                }
            }
            else
            {
                _logger.LogDebug("Fetching tasks without filter | Page: {Page}/{PageSize}",
                    page,
                    pageSize);
            }

            var response = await _httpClient.GetAndReadResponseAsync<PagedResponse<TaskResponse>>(url);

            _logger.LogInformation("Task filter results | Page: {Page}/{PageSize}, Total: {TotalCount}, Returned: {ResultCount}",
                page,
                pageSize,
                response.TotalCount,
                response.Items.Count);

            return response;
        }

        public async Task<TaskResponse> UpdateTaskAsync(Guid initiatorUserId, Guid taskId, UpdateTaskRequest request)
        {
            var url = $"{BaseUrl}/{taskId}?initiatorUserId={initiatorUserId}";
            _logger.LogInformation("Updating task | Task ID: {TaskId}, Initiator: {InitiatorUserId}",
                taskId,
                initiatorUserId);

            var response = await _httpClient.PutAndReadResponseAsync<TaskResponse>(url, request);

            _logger.LogInformation("Task updated | ID: {TaskId}, Status: {Status}, Assignee: {AssigneeId}",
                response.Id,
                response.IsActive,
                response.AssignedUserId);

            return response;
        }
    }
}
