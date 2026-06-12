using System.ComponentModel.DataAnnotations;

namespace FlowerNursery.Models
{
    public class Greenhouse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Greenhouse Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]
        public string? Location { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        // Owner (user isolation)
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public ICollection<FlowerGroup> FlowerGroups { get; set; } = new List<FlowerGroup>();
    }
}
