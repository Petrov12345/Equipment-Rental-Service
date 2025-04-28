namespace EquipmentRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEquipmentApprovalAndImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Equipment", "ImagePath", c => c.String(maxLength: 200));
            AlterColumn("dbo.User", "Password", c => c.String(nullable: false, maxLength: 200));
            DropColumn("dbo.Equipment", "PicturePath");
            DropColumn("dbo.Rental", "IsApproved");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Rental", "IsApproved", c => c.Boolean(nullable: false));
            AddColumn("dbo.Equipment", "PicturePath", c => c.String(maxLength: 255));
            AlterColumn("dbo.User", "Password", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Equipment", "ImagePath");
        }
    }
}
