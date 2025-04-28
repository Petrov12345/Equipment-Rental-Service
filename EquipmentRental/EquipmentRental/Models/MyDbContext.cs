using System.Data.Entity;

namespace EquipmentRental.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("MyEquipmentRentalConnection")
        {
        }

        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipment>()
                .HasRequired(e => e.Owner)
                .WithMany(u => u.Equipments)
                .HasForeignKey(e => e.OwnerID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Rental>()
                .HasRequired(r => r.Equipment)
                .WithMany()
                .HasForeignKey(r => r.EquipmentID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Rental>()
                .HasRequired(r => r.Renter)
                .WithMany()
                .HasForeignKey(r => r.RenterID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
