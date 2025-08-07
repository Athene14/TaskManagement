using FluentMigrator;
using System.Data;
using TaskService.Domain.Models;

namespace TaskService.Infra.Data.Migrations
{
    [Migration(20250803125200)]
    public class Date_20250803125200_TaskHistoryInit : Migration
    {
        public override void Up()
        {
            Create.Table(TableNameConstants.TaskHistoryTableName)
            .WithColumn(nameof(TaskSnapshotDomainModel.SnapshotId).ToLower()).AsGuid().PrimaryKey()
            .WithColumn(nameof(TaskSnapshotDomainModel.TaskId).ToLower()).AsGuid().NotNullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.ChangedBy).ToLower()).AsGuid().NotNullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.ChangeTime).ToLower()).AsInt64().NotNullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.Title).ToLower()).AsString(255).NotNullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.Description).ToLower()).AsString().Nullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.IsActive).ToLower()).AsBoolean().NotNullable()
            .WithColumn(nameof(TaskSnapshotDomainModel.AssignedUserId).ToLower()).AsGuid().Nullable();

            Create.ForeignKey($"FK_{TableNameConstants.TaskHistoryTableName}_{TableNameConstants.TaskTableName}")
                .FromTable(TableNameConstants.TaskHistoryTableName).ForeignColumn(nameof(TaskSnapshotDomainModel.TaskId).ToLower())
                .ToTable(TableNameConstants.TaskTableName).PrimaryColumn(nameof(TaskDomainModel.TaskId).ToLower())
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.ForeignKey($"FK_{TableNameConstants.TaskHistoryTableName}_{TableNameConstants.TaskTableName}").OnTable(TableNameConstants.TaskHistoryTableName);
            Delete.Table(TableNameConstants.TaskHistoryTableName);
        }

    }
}
