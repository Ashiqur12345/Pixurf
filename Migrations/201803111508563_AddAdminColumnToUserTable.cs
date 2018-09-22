namespace Pixurf.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdminColumnToUserTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "Admin", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "Admin");
        }
    }
}
