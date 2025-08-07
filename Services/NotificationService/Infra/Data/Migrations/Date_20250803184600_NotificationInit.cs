using FluentMigrator;
using NotificationService.Domain.Models;

namespace NotificationService.Infra.Data.Migrations
{
    [Migration(20250803184600)]
    public class Date_20250803184600_NotificationInit : Migration
    {
        public override void Up()
        {
            Create.Table(TableNameConstants.NotificationTableName)
                .WithColumn(nameof(NotificationDomainModel.Id).ToLower()).AsGuid().PrimaryKey()
                .WithColumn(nameof(NotificationDomainModel.CreatedTimestamp).ToLower()).AsInt64().NotNullable()
                .WithColumn(nameof(NotificationDomainModel.Message).ToLower()).AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(TableNameConstants.NotificationTableName);
        }

    }
}
