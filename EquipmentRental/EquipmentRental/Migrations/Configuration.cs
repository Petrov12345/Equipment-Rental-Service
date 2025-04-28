
namespace EquipmentRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using EquipmentRental.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<EquipmentRental.Models.MyDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "EquipmentRental.Models.MyDbContext";
        }

        protected override void Seed(EquipmentRental.Models.MyDbContext context)
        {
            /* ----------  USERS  ---------- */
            string p1 = User.HashPassword("password1");
            string p2 = User.HashPassword("password2");
            string p3 = User.HashPassword("password3");
            string p4 = User.HashPassword("password4");
            string p5 = User.HashPassword("password5");

            context.Users.AddOrUpdate(
                u => u.Email,
                new User { Name = "Alice Johnson", Email = "alice@example.com", Address = "123 Main St", Telephone = "1234567890", Password = p1, Role = "User" },
                new User { Name = "Bob Smith", Email = "bob@example.com", Address = "456 Oak Ave", Telephone = "9876543210", Password = p2, Role = "Admin" },
                new User { Name = "Charlie Brown", Email = "charlie@example.com", Address = "789 Pine Rd", Telephone = "5551234567", Password = p3, Role = "User" },
                new User { Name = "Diana Prince", Email = "diana@example.com", Address = "101 Wonder Ln", Telephone = "5559876543", Password = p4, Role = "User" },
                new User { Name = "Edward Norton", Email = "edward@example.com", Address = "202 Hollywood Blvd", Telephone = "5551112222", Password = p5, Role = "Admin" }
            );
            context.SaveChanges();

            var alice = context.Users.Single(u => u.Email == "alice@example.com");
            var bob = context.Users.Single(u => u.Email == "bob@example.com");     // admin
            var charlie = context.Users.Single(u => u.Email == "charlie@example.com");
            var diana = context.Users.Single(u => u.Email == "diana@example.com");
            var edward = context.Users.Single(u => u.Email == "edward@example.com");  // admin

            /* ----------  EQUIPMENT  ---------- */
            context.Equipments.AddOrUpdate(
                e => e.Name,
                new Equipment { Name = "Excavator", Category = "Heavy Machinery", RentalPrice = 1500m, AvailabilityStatus = "Available", Description = "A heavy-duty excavator for construction.", OwnerID = alice.ID, ImagePath = null, IsApproved = true },
                new Equipment { Name = "Camera", Category = "Electronics", RentalPrice = 50m, AvailabilityStatus = "Available", Description = "High‑resolution DSLR camera.", OwnerID = bob.ID, ImagePath = null, IsApproved = true },
                new Equipment { Name = "Drill", Category = "Tools", RentalPrice = 25m, AvailabilityStatus = "Available", Description = "Electric drill for DIY projects.", OwnerID = charlie.ID, ImagePath = null, IsApproved = true },
                new Equipment { Name = "Lawn Mower", Category = "Garden", RentalPrice = 75m, AvailabilityStatus = "Unavailable", Description = "Reliable lawn mower.", OwnerID = diana.ID, ImagePath = null, IsApproved = false },
                new Equipment { Name = "Projector", Category = "Electronics", RentalPrice = 100m, AvailabilityStatus = "Available", Description = "HD projector for events.", OwnerID = edward.ID, ImagePath = null, IsApproved = true }
            );
            context.SaveChanges();

            var excavator = context.Equipments.Single(e => e.Name == "Excavator");
            var camera = context.Equipments.Single(e => e.Name == "Camera");
            var drill = context.Equipments.Single(e => e.Name == "Drill");
            var projector = context.Equipments.Single(e => e.Name == "Projector");

            /* ----------  RENTALS  ---------- */
            // Approved rentals (existing data)
            context.Rentals.AddOrUpdate(
                r => new { r.EquipmentID, r.RenterID, r.RentalStartDate },
                new Rental
                {
                    EquipmentID = excavator.ID,
                    RenterID = bob.ID,
                    RentalStartDate = new DateTime(2025, 4, 1),
                    RentalEndDate = new DateTime(2025, 4, 5),
                    TotalCost = 6000m,
                    Status = "Approved",
                    ApproverID = edward.ID        
                },
                new Rental
                {
                    EquipmentID = camera.ID,
                    RenterID = alice.ID,
                    RentalStartDate = new DateTime(2025, 4, 10),
                    RentalEndDate = new DateTime(2025, 4, 12),
                    TotalCost = 100m,
                    Status = "Approved",
                    ApproverID = edward.ID
                },
                new Rental
                {
                    EquipmentID = drill.ID,
                    RenterID = diana.ID,
                    RentalStartDate = new DateTime(2025, 5, 1),
                    RentalEndDate = new DateTime(2025, 5, 3),
                    TotalCost = 50m,
                    Status = "Approved",
                    ApproverID = bob.ID
                },
                new Rental
                {
                    EquipmentID = projector.ID,
                    RenterID = charlie.ID,
                    RentalStartDate = new DateTime(2025, 5, 15),
                    RentalEndDate = new DateTime(2025, 5, 16),
                    TotalCost = 100m,
                    Status = "Approved",
                    ApproverID = bob.ID
                }
            );

            /* ----------  SAMPLE PENDING REQUESTS  ---------- */
            context.Rentals.AddOrUpdate(
                r => new { r.EquipmentID, r.RenterID, r.RentalStartDate },
                new Rental
                {
                    EquipmentID = camera.ID,
                    RenterID = diana.ID,
                    RentalStartDate = new DateTime(2025, 6, 1),
                    RentalEndDate = new DateTime(2025, 6, 5),
                    TotalCost = 0m,
                    Status = "Pending",
                    ApproverID = null
                },
                new Rental
                {
                    EquipmentID = projector.ID,
                    RenterID = alice.ID,
                    RentalStartDate = new DateTime(2025, 6, 10),
                    RentalEndDate = new DateTime(2025, 6, 12),
                    TotalCost = 0m,
                    Status = "Pending",
                    ApproverID = null
                }
            );

            context.SaveChanges();
        }
    }
}
