using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerNursery.Models
{
    public class WateringSchedule
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Scheduled date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Foreign key
        [Required]
        [Display(Name = "Flower Group")]
        public int FlowerGroupId { get; set; }

        // Navigation property
        [ForeignKey("FlowerGroupId")]
        public FlowerGroup? FlowerGroup { get; set; }
    }
}
