using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsInfo.Api.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [MaxLength(200)]
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
        public int CarId { get; set; }

        public PointOfInterest(string name)
        {
            Name = name;
        }
    }
}