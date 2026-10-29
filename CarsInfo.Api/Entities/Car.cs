using CarsInfo.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsInfo.Api.Entities
{
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
        public ICollection<PointOfInterest> PointsOfInterests { get; set; } = new List<PointOfInterest>();

        public Car(string name)
        {
            Name = name;    
        }

    }
}
