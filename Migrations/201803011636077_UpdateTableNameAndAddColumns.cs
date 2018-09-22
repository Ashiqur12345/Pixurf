namespace Pixurf.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTableNameAndAddColumns : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AspNetUsers", newName: "User");
            RenameColumn(table: "dbo.User", name: "Id", newName: "User ID");
            AddColumn("dbo.User", "About_Me", c => c.String());
            AddColumn("dbo.User", "Joining_Date", c => c.DateTime());
            AddColumn("dbo.User", "Country", c => c.String());
            AddColumn("dbo.User", "Pro_Pic_ID", c => c.String());
            AddColumn("dbo.User", "Status", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "Status");
            DropColumn("dbo.User", "Pro_Pic_ID");
            DropColumn("dbo.User", "Country");
            DropColumn("dbo.User", "Joining_Date");
            DropColumn("dbo.User", "About_Me");
            RenameColumn(table: "dbo.User", name: "User ID", newName: "Id");
            RenameTable(name: "dbo.User", newName: "AspNetUsers");
        }
    }
}
