namespace EquipmentRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rental", "Status", c => c.String(maxLength: 20));
            AddColumn("dbo.Rental", "ApproverID", c => c.Int());
            CreateIndex("dbo.Rental", "ApproverID");
            AddForeignKey("dbo.Rental", "ApproverID", "dbo.User", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Rental", "ApproverID", "dbo.User");
            DropIndex("dbo.Rental", new[] { "ApproverID" });
            DropColumn("dbo.Rental", "ApproverID");
            DropColumn("dbo.Rental", "Status");
        }
    }
}
