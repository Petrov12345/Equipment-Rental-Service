namespace EquipmentRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Equipment",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Category = c.String(nullable: false, maxLength: 50),
                        RentalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvailabilityStatus = c.String(maxLength: 100),
                        Description = c.String(maxLength: 500),
                        OwnerID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.User", t => t.OwnerID, cascadeDelete: true)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        Address = c.String(maxLength: 200),
                        Telephone = c.String(maxLength: 20),
                        Password = c.String(nullable: false, maxLength: 100),
                        Role = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Rental",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EquipmentID = c.Int(nullable: false),
                        RenterID = c.Int(nullable: false),
                        RentalStartDate = c.DateTime(nullable: false),
                        RentalEndDate = c.DateTime(nullable: false),
                        TotalCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Equipment", t => t.EquipmentID)
                .ForeignKey("dbo.User", t => t.RenterID)
                .Index(t => t.EquipmentID)
                .Index(t => t.RenterID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Rental", "RenterID", "dbo.User");
            DropForeignKey("dbo.Rental", "EquipmentID", "dbo.Equipment");
            DropForeignKey("dbo.Equipment", "OwnerID", "dbo.User");
            DropIndex("dbo.Rental", new[] { "RenterID" });
            DropIndex("dbo.Rental", new[] { "EquipmentID" });
            DropIndex("dbo.Equipment", new[] { "OwnerID" });
            DropTable("dbo.Rental");
            DropTable("dbo.User");
            DropTable("dbo.Equipment");
        }
    }
}
