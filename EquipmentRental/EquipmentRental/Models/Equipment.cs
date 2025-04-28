using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentRental.Models
{
    [Table("Equipment")]
    public class Equipment
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Equipment Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50)]
        public string Category { get; set; }

        [Required(ErrorMessage = "Rental Price is required")]
        [Range(0.01, 999999, ErrorMessage = "Price must be greater than 0")]
        public decimal RentalPrice { get; set; }

        [StringLength(100)]
        public string AvailabilityStatus { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int OwnerID { get; set; }
        public virtual User Owner { get; set; }


        [StringLength(200)]
        public string ImagePath { get; set; }


        public bool IsApproved { get; set; }
    }
}
