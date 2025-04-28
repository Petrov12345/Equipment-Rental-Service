using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace EquipmentRental.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(200)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        public virtual ICollection<Equipment> Equipments { get; set; }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return System.BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
