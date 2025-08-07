using FluentMigrator;
using TaskService.Domain.Models;

namespace TaskService.Infra.Data.Migrations
{
    [Migration(20250803111400)]
    public class Date_20250803111400_TaskInit : Migration
    {
        public override void Up()
        {
            Create.Table(TableNameConstants.TaskTableName)
            .WithColumn(nameof(TaskDomainModel.TaskId).ToLower()).AsGuid().PrimaryKey()
            .WithColumn(nameof(TaskDomainModel.Title).ToLower()).AsString(255).NotNullable()
            .WithColumn(nameof(TaskDomainModel.Description).ToLower()).AsString().Nullable()
            .WithColumn(nameof(TaskDomainModel.CreatedAt).ToLower()).AsInt64().NotNullable()
            .WithColumn(nameof(TaskDomainModel.UpdatedAt).ToLower()).AsInt64().Nullable()
            .WithColumn(nameof(TaskDomainModel.IsActive).ToLower()).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(TaskDomainModel.CreatedBy).ToLower()).AsGuid().NotNullable()
            .WithColumn(nameof(TaskDomainModel.AssignedUserId).ToLower()).AsGuid().Nullable();

        }

        public override void Down()
        {
            Delete.Table(TableNameConstants.TaskTableName);
        }

    }
}
