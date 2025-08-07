using Moq;
using TaskManagementServices.Shared.Exceptions;
using TaskManagementServices.Shared.TaskService.DTO;
using TaskService.App.Exceptions;
using TaskService.Domain.Abstractions;
using TaskService.Domain.Models;

namespace TaskService.Tests
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<ITaskHistoryRepository> _historyRepoMock;
        private readonly App.Service.TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepoMock = new Mock<ITaskRepository>();
            _historyRepoMock = new Mock<ITaskHistoryRepository>();
            _taskService = new App.Service.TaskService(
                _taskRepoMock.Object,
                _historyRepoMock.Object
            );
        }

        [Fact]
        public async Task GetTaskByIdAsync_ExistingTask_ReturnsTaskResponse()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Test Task",
                Description = "Test Description",
                IsActive = true
            };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            var result = await _taskService.GetTaskByIdAsync(taskId);

            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal(task.Title, result.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_NonExistingTask_ThrowsNotFoundException()
        {
            var taskId = Guid.NewGuid();
            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync((TaskDomainModel)null);

            await Assert.ThrowsAsync<NotFoundException>(()=> _taskService.GetTaskByIdAsync(taskId));
        }

        [Fact]
        public async Task GetTaskByIdAsync_EmptyGuid_ThrowsInvalidArgumentException()
        {

            var emptyGuid = Guid.Empty;
            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(emptyGuid))
                .ReturnsAsync((TaskDomainModel)null);

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _taskService.GetTaskByIdAsync(emptyGuid));
        }

        // GetWithFilterAsync
        [Fact]
        public async Task GetWithFilterAsync_ValidFilter_ReturnsPagedResponse()
        {

            var filter = new TaskFilter();
            var page = 1;
            var pageSize = 10;
            var tasks = new List<TaskDomainModel>
            {
                new TaskDomainModel { TaskId = Guid.NewGuid(), Title = "Task 1" },
                new TaskDomainModel { TaskId = Guid.NewGuid(), Title = "Task 2" }
            };

            var pagedTasks = new PagedResponse<TaskDomainModel>(tasks, page, pageSize, 2);

            _taskRepoMock.Setup(repo => repo.GetTasksAsync(filter, page, pageSize))
                .ReturnsAsync(pagedTasks);


            var result = await _taskService.GetWithFilterAsync(filter, page, pageSize);


            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetWithFilterAsync_EmptyResult_ReturnsEmptyPagedResponse()
        {

            var filter = new TaskFilter { Title = "NonExisting" };
            var page = 1;
            var pageSize = 10;
            var emptyTasks = new PagedResponse<TaskDomainModel>(
                new List<TaskDomainModel>(), page, pageSize, 0);

            _taskRepoMock.Setup(repo => repo.GetTasksAsync(filter, page, pageSize))
                .ReturnsAsync(emptyTasks);


            var result = await _taskService.GetWithFilterAsync(filter, page, pageSize);


            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetTaskHistoryAsync_NoHistory_ThrowsNotFoundException()
        {
            var taskId = Guid.NewGuid();
            _historyRepoMock.Setup(repo => repo.GetSnapshotsByTaskIdAsync(taskId))
                .ReturnsAsync(new List<TaskSnapshotDomainModel>());

            await Assert.ThrowsAsync<NotFoundException>(()=> _taskService.GetTaskHistoryAsync(taskId));
        }

        [Fact]
        public async Task CreateTaskAsync_ValidRequest_CreatesTaskAndHistory()
        {
            var userId = Guid.NewGuid();
            var request = new CreateTaskRequest
            {
                Title = "New Task",
                Description = "Description",
                AssignedUserId = Guid.NewGuid()
            };

            _taskRepoMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(Guid.NewGuid());

            var result = await _taskService.CreateTaskAsync(userId, request);

            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            _taskRepoMock.Verify(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()), Times.Once);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_CreationFails_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var request = new CreateTaskRequest
            {
                Title = "New Task",
                Description = "desc"
            };

            _taskRepoMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(Guid.Empty);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(userId, request));
        }

        [Fact]
        public async Task CreateTaskAsync_NullRequest_ThrowsInvalidArgumentException()
        {
            var userId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidArgumentException>(() =>
                _taskService.CreateTaskAsync(userId, null));
        }

        // UpdateTaskAsync
        [Fact]
        public async Task UpdateTaskAsync_ValidUpdate_UpdatesTaskAndCreatesHistory()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Old Title",
                Description = "Old Description",
                AssignedUserId = Guid.NewGuid(),
                IsActive = true
            };

            var request = new UpdateTaskRequest
            {
                Title = "New Title",
                Description = "New Description",
                AssignedUserId = Guid.NewGuid()
            };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _taskRepoMock.Setup(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(true);


            var result = await _taskService.UpdateTaskAsync(userId, taskId, request);


            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            _taskRepoMock.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()), Times.Once);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_NoChanges_ReturnsTaskWithoutUpdate()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Existing Title",
                Description = "Existing Description",
                AssignedUserId = Guid.NewGuid(),
                IsActive = true
            };

            var request = new UpdateTaskRequest
            {
                Title = "Existing Title",
                Description = "Existing Description",
                AssignedUserId = existingTask.AssignedUserId
            };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);


            var result = await _taskService.UpdateTaskAsync(userId, taskId, request);


            Assert.NotNull(result);
            _taskRepoMock.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()), Times.Never);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_InactiveTask_ThrowsInactiveUpdateTaskException()
        {

            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                IsActive = false
            };

            var request = new UpdateTaskRequest { Title = "New Title", Description = "desc" };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            await Assert.ThrowsAsync<InactiveUpdateTaskException>(() =>
                _taskService.UpdateTaskAsync(userId, taskId, request));
        }

        [Fact]
        public async Task UpdateTaskAsync_NonExistingTask_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new UpdateTaskRequest { Title = "New Title", Description = "desc" };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync((TaskDomainModel)null);

            await Assert.ThrowsAsync<NotFoundException>(()=>_taskService.UpdateTaskAsync(userId, taskId, request));
        }

        [Fact]
        public async Task UpdateTaskAsync_UpdateFails_ThrowsInvalidOperationException()
        {

            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Old Title",
                IsActive = true
            };

            var request = new UpdateTaskRequest { Title = "New Title", Description = "desc" };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _taskRepoMock.Setup(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskService.UpdateTaskAsync(userId, taskId, request));
        }

        // DeleteTaskAsync
        [Fact]
        public async Task DeleteTaskAsync_ValidTask_DeletesAndCreatesHistory()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                IsActive = true
            };

            _taskRepoMock.Setup(repo => repo.SoftDeleteTaskAsync(taskId))
                .ReturnsAsync(true);

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);


            var result = await _taskService.DeleteTaskAsync(userId, taskId);


            Assert.True(result);
            _taskRepoMock.Verify(repo => repo.SoftDeleteTaskAsync(taskId), Times.Once);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_DeleteFails_ThrowsException()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(new TaskDomainModel()
                {
                    AssignedUserId = userId,
                    TaskId = taskId
                });

            _taskRepoMock.Setup(repo => repo.SoftDeleteTaskAsync(taskId))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<Exception>(() =>
                _taskService.DeleteTaskAsync(userId, taskId));
        }

        [Fact]
        public async Task DeleteTaskAsync_AlreadyDeleted_ThrowsNotActiveUpdateTaskException()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var deletedTask = new TaskDomainModel
            {
                TaskId = taskId,
                IsActive = false
            };

            _taskRepoMock.Setup(repo => repo.SoftDeleteTaskAsync(taskId))
                .ReturnsAsync(true);

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(deletedTask);

            await Assert.ThrowsAsync<InactiveUpdateTaskException>(()=> _taskService.DeleteTaskAsync(userId, taskId));
        }


        [Fact]
        public async Task CreateTaskAsync_LongValues_HandlesCorrectly()
        {

            var userId = Guid.NewGuid();
            var longString = new string('a', 10000);
            var request = new CreateTaskRequest
            {
                Title = longString,
                Description = longString
            };

            _taskRepoMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(Guid.NewGuid());


            var result = await _taskService.CreateTaskAsync(userId, request);


            Assert.NotNull(result);
            Assert.Equal(longString, result.Title);
        }

        // создание таски без описани€ - валидный случай
        [Fact]
        public async Task CreateTaskAsync_CreatingWithoutDescription_ShouldBeFine()
        {
            var userId = Guid.NewGuid();
            var request = new CreateTaskRequest
            {
                Title = "New Task",
            };

            _taskRepoMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(Guid.NewGuid());


            var result = await _taskService.CreateTaskAsync(userId, request);


            Assert.NotNull(result);
            Assert.Equal(request.Title, result.Title);
            _taskRepoMock.Verify(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()), Times.Once);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_PartialUpdate_UpdatesOnlyChangedFields()
        {

            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Original Title",
                Description = "Original Description",
                AssignedUserId = Guid.NewGuid(),
                IsActive = true
            };

            var request = new UpdateTaskRequest
            {
                Title = "Updated Title",
                AssignedUserId = existingTask.AssignedUserId // Ќе мен€ем
            };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);

            _taskRepoMock.Setup(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(true)
                .Callback<TaskDomainModel>(updatedTask =>
                {
                    // ѕровер€ем, что изменились только нужные пол€
                    Assert.Equal("Updated Title", updatedTask.Title);
                    Assert.Equal("Original Description", updatedTask.Description);
                    Assert.Equal(existingTask.AssignedUserId, updatedTask.AssignedUserId);
                });


            var result = await _taskService.UpdateTaskAsync(userId, taskId, request);


            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_EmptyUpdate_DoesNothing()
        {

            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var existingTask = new TaskDomainModel
            {
                TaskId = taskId,
                Title = "Original Title",
                Description = "Original Description",
                AssignedUserId = Guid.NewGuid(),
                IsActive = true
            };

            var request = new UpdateTaskRequest
            {
                Title = "Original Title",
                Description = "Original Description",
                AssignedUserId = existingTask.AssignedUserId
            };

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync(existingTask);


            var result = await _taskService.UpdateTaskAsync(userId, taskId, request);


            Assert.NotNull(result);
            _taskRepoMock.Verify(repo => repo.UpdateTaskAsync(It.IsAny<TaskDomainModel>()), Times.Never);
            _historyRepoMock.Verify(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTaskAsync_NonExistingTask_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            _taskRepoMock.Setup(repo => repo.GetTaskByIdAsync(taskId))
                .ReturnsAsync((TaskDomainModel)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _taskService.DeleteTaskAsync(userId, taskId));
        }

        [Fact]
        public async Task GetTaskHistoryAsync_LargeHistory_ReturnsAllItems()
        {

            var taskId = Guid.NewGuid();
            var largeHistory = Enumerable.Range(1, 1000)
                .Select(i => new TaskSnapshotDomainModel
                {
                    SnapshotId = Guid.NewGuid(),
                    TaskId = taskId,
                    ChangeTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                })
                .ToList();

            _historyRepoMock.Setup(repo => repo.GetSnapshotsByTaskIdAsync(taskId))
                .ReturnsAsync(largeHistory);


            var result = await _taskService.GetTaskHistoryAsync(taskId);


            Assert.NotNull(result);
            Assert.Equal(1000, result.Count());
        }

        [Fact]
        public async Task CreateHistorySnapshot_OnCreate_CorrectSnapshotData()
        {

            var userId = Guid.NewGuid();
            var request = new CreateTaskRequest
            {
                Title = "Task with Snapshot",
                Description = "Snapshot check"
            };

            TaskSnapshotDomainModel createdSnapshot = null;
            _taskRepoMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskDomainModel>()))
                .ReturnsAsync(Guid.NewGuid());

            _historyRepoMock.Setup(repo => repo.AddSnapshotAsync(It.IsAny<TaskSnapshotDomainModel>()))
                .Callback<TaskSnapshotDomainModel>(snapshot => createdSnapshot = snapshot)
                .Returns(Task.CompletedTask);


            await _taskService.CreateTaskAsync(userId, request);


            Assert.NotNull(createdSnapshot);
            Assert.Equal(request.Title, createdSnapshot.Title);
            Assert.Equal(request.Description, createdSnapshot.Description);
            Assert.True(createdSnapshot.IsActive);
            Assert.Equal(userId, createdSnapshot.ChangedBy);
        }
    }
}