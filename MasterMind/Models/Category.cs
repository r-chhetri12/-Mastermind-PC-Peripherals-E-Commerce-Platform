using Microsoft.Build.Framework;

namespace MasterMind.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
