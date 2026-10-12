using CarsInfo.Api.Models;

namespace CarsInfo.Api
{
    public class CarsDataStroe
    {
        public List<CarDto> Cars { get; set; }
        public static CarsDataStroe Current { get; } = new CarsDataStroe();
        public CarsDataStroe()
        {
            Cars = new List<CarDto>
            {
                new CarDto
                {
                    Id = 1,
                    Name = "Toyota Camry",
                    Description = "A reliable and fuel-efficient sedan.",
                    PointsOfInterest = new List<PointOfInterestDto>
                    {
                        new PointOfInterestDto
                        {
                            Id = 1,
                            Name = "Hybrid Engine",
                            Description = "Fuel-efficient hybrid powertrain with strong real-world economy."
                        },
                        new PointOfInterestDto
                        {
                            Id = 2,
                            Name = "Spacious Interior",
                            Description = "Roomy cabin with comfortable seating for five passengers."
                        },
                        new PointOfInterestDto
                        {
                            Id = 3,
                            Name = "Safety Suite",
                            Description = "Includes adaptive cruise control and lane-keeping assist."
                        }
                    }
                },
                new CarDto
                {
                    Id = 2,
                    Name = "Honda Civic",
                    Description = "A compact car known for its performance and style.",
                    PointsOfInterest = new List<PointOfInterestDto>
                    {
                        new PointOfInterestDto
                        {
                            Id = 1,
                            Name = "Turbocharged Engine",
                            Description = "Responsive turbo engine balancing power and efficiency."
                        },
                        new PointOfInterestDto
                        {
                            Id = 2,
                            Name = "Sporty Handling",
                            Description = "Agile chassis tuned for an engaging driving experience."
                        }
                    }
                }
            };
        }
    }
}
