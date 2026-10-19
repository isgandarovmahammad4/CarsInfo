using CarsInfo.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarsInfo.Api.DbContexts
{
    public class CarContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        //We can now connect to a SQLite database.
        //That being said, we're not there yet.
        // First way of doing this is overriding the onConfiguring method. This has a dbcontext options builder as a parameter.That  provides us with a method UseSqlite.
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite("connectionstring");
        //    base.OnConfiguring(optionsBuilder);
        //}

        public CarContext(DbContextOptions<CarContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .HasData(
                new Car("Car 1")
                {
                    Id = 1,
                    Name = "Car 1",
                    Description = "Description of Car 1"
                },
                new Car("Car 2")
                {
                    Id = 2,
                    Name = "Car 2",
                    Description = "Description of Car 2"
                },
                new Car("Car 3")
                {
                    Id = 3,
                    Name = "Car 3",
                    Description = "Description of Car 3"
                }
            );
            modelBuilder.Entity<PointOfInterest>().HasData(
                new PointOfInterest("Point of Interest 1")
                {
                    Id = 1,
                    CarId = 1,
                    Name = "Point of Interest 1",
                    Description = "Description of Point of Interest 1"
                },
                new PointOfInterest("Point of Interest 2")
                {
                    Id = 2,
                    CarId = 1,
                    Name = "Point of Interest 2",
                    Description = "Description of Point of Interest 2"
                },
                new PointOfInterest("Point of Interest 3")
                {
                    Id = 3,
                    CarId = 2,
                    Name = "Point of Interest 3",
                    Description = "Description of Point of Interest 3"
                }
            );
            base.OnModelCreating(modelBuilder);
            
        }

        //second way is doing htis via constructor of decompiled DbContext class. That constructor takes a DbContextOptions parameter, dbcontextOptions. We can use that to pass in the connection string. This is the preferred way of doing it.
    }   // So lets add a constructor to our CarContext that calls this constructor overload. Now we can provide these options at the moment when we register our DbContext in the program cs dependency injection container. So let's do that in the program.cs file.

}
