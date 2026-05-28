using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MasterMind.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required, Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public string? Pincode { get; set; }

        public string? TransactionId { get; set; }

        public string? OrderId { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public string OrderStatus { get; set; } = "Pending";
        public List<OrderItem> OrderItems { get; set; }
        public bool IsCancelled { get; set; } = false;

        public string? CancellationReason { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? TrackingId { get; set; } // for courier tracking
        public DateTime? EstimatedDeliveryDate { get; set; } // nullable in case it's not assigned yet


    }
}