using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquipmentRental.Models
{
    [Table("Rental")]
    public class Rental
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int EquipmentID { get; set; }

        [Required]
        public int RenterID { get; set; }

        [Required(ErrorMessage = "Rental Start Date is required")]
        public DateTime RentalStartDate { get; set; }

        [Required(ErrorMessage = "Rental End Date is required")]
        public DateTime RentalEndDate { get; set; }

        [Range(0, 999999, ErrorMessage = "Total Cost must be non‑negative")]
        public decimal TotalCost { get; set; }

        [StringLength(20)]
        public string Status { get; set; }


        public int? ApproverID { get; set; }


        public virtual Equipment Equipment { get; set; }
        public virtual User Renter { get; set; }
        public virtual User Approver { get; set; }
    }
}