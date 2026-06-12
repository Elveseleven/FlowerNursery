using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerNursery.Models
{
    public class FlowerGroup
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Species name is required.")]
        [StringLength(150, ErrorMessage = "Species cannot exceed 150 characters.")]
        [Display(Name = "Species / Variety")]
        public string Species { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Range(1, 100000, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity (plants)")]
        public int Quantity { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Foreign key
        [Required(ErrorMessage = "Please select a greenhouse.")]
        [Display(Name = "Greenhouse")]
        public int GreenhouseId { get; set; }

        // Navigation properties
        [ForeignKey("GreenhouseId")]
        public Greenhouse? Greenhouse { get; set; }

        public ICollection<WateringSchedule> WateringSchedules { get; set; } = new List<WateringSchedule>();
    }
}
