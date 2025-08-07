using AuthService.Domain.Models;
using FluentMigrator;

namespace AuthService.Infra.Data.Migrations
{
    [Migration(20250802210200)]
    public class Date_20250802210200_UserInit : Migration
    {
        public override void Up()
        {
            Create.Table(TableNameConstants.UserTableName)
                .WithColumn(nameof(UserDomain.Id).ToLower()).AsGuid().PrimaryKey()
                .WithColumn(nameof(UserDomain.Email).ToLower()).AsString(255).NotNullable().Unique()
                .WithColumn(nameof(UserDomain.PasswordHash).ToLower()).AsString(500).NotNullable()
                .WithColumn(nameof(UserDomain.FullName).ToLower()).AsString(255).NotNullable()
                .WithColumn(nameof(UserDomain.CreatedAt).ToLower()).AsInt64().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(TableNameConstants.UserTableName);
        }

    }
}
