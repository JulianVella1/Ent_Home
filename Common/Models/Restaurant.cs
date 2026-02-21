using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Restaurant : IItemValidating
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string OwnerEmail { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
        public string? ImagePath { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; }
        public string GetCardPartial()
        {
            return "_RestaurantCard";
        }

        public List<string> GetValidators()
        {
            throw new NotImplementedException();
        }
    }
}
