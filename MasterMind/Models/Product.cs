using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MasterMind.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }  // Nullable

        [Required]
        public string? Specification { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number")]
        public int Quantity { get; set; }


        [Required]
        public decimal Price { get; set; }

        public string? Image { get; set; }  // Nullable to avoid NULL errors

        public int CategoryId { get; set; } // Nullable to prevent errors

        public Category? Category { get; set; } // Navigation property
    }


}
