using FluentMigrator;
using NotificationService.Domain.Models;
using System.Data;

namespace NotificationService.Infra.Data.Migrations
{
    [Migration(20250803185700)]
    public class Date_20250803185700_NotificationRecipientInit : Migration
    {
        public override void Up()
        {
            Create.Table(TableNameConstants.NotificationRecipientsTableName)
                .WithColumn(nameof(NotificationRecipientDomainModel.Id).ToLower()).AsGuid().PrimaryKey()
                .WithColumn(nameof(NotificationRecipientDomainModel.NotificationId).ToLower()).AsGuid().NotNullable()
                .WithColumn(nameof(NotificationRecipientDomainModel.RecipientId).ToLower()).AsGuid().NotNullable()
                .WithColumn(nameof(NotificationRecipientDomainModel.IsRead).ToLower()).AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Index($"idx_{TableNameConstants.NotificationRecipientsTableName}_recipient")
                .OnTable(TableNameConstants.NotificationRecipientsTableName)
                .OnColumn(nameof(NotificationRecipientDomainModel.RecipientId).ToLower());

            Create.ForeignKey($"FK_{TableNameConstants.NotificationRecipientsTableName}_{TableNameConstants.NotificationTableName}")
                .FromTable(TableNameConstants.NotificationRecipientsTableName).ForeignColumn(nameof(NotificationRecipientDomainModel.NotificationId).ToLower())
                .ToTable(TableNameConstants.NotificationTableName).PrimaryColumn(nameof(NotificationDomainModel.Id).ToLower())
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.Index($"idx_{TableNameConstants.NotificationRecipientsTableName}_recipient");
            Delete.ForeignKey($"FK_{TableNameConstants.NotificationRecipientsTableName}_{TableNameConstants.NotificationTableName}").OnTable(TableNameConstants.NotificationRecipientsTableName);
            Delete.Table(TableNameConstants.NotificationRecipientsTableName);
        }

    }
}
