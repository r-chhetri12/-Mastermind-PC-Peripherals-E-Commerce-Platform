using System.ComponentModel.DataAnnotations;

namespace MasterMind.Models
{
    public class UserProfile
    {
        [Key]
        public string Id { get; set; } // Matches IdentityUser.Id
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? PhoneNumber { get; set; }
      public string? Email { get; set; }
    }
}
