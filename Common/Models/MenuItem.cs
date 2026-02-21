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
    internal class MenuItem : IItemValidating
    {
        [Key]
        public Guid Id { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        public double Price { get; set; }
        [Required, MaxLength(30)]
        [ForeignKey("Restaurant")]
        public int RestId {get; set; }
        public virtual Restaurant? Restaurant { get; set; }
        public bool Status { get; set; } = false;
        public string? ImagePath { get; set; }


        public string GetCardPartial()
        {
            return "_MenuItemCard";
        }

        public List<string> GetValidators()
        {
            var validators= new List<string>();

            if (Restaurant != null && !string.IsNullOrWhiteSpace(Restaurant.OwnerEmail)) { 
                validators.Add(Restaurant.OwnerEmail);
            }
            return validators;
        }
    }
}
