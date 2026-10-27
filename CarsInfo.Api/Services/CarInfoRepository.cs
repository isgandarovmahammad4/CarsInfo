using CarsInfo.Api.DbContexts;
using CarsInfo.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CarsInfo.Api.Services
{
    public class CarInfoRepository : ICarInfoRepository
    {
        private readonly CarContext _context;
        public CarInfoRepository(CarContext carContext)
        {
            _context = carContext;
        }
        public Task<Car?> GetCarAsync(int carId, bool poi)
        {
            if (poi)
            {
                return _context.Cars.Include(c => c.PointsOfInterests).Where(p => p.Id == carId).FirstOrDefaultAsync();
            }
            return _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
        }

        public async Task<IEnumerable<Car>> GetCarsAsync()
        {

            return await _context.Cars.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<Car>,PaginationMetadata)> GetCarsAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var collection = _context.Cars as IQueryable<Car>;

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(c => c.Name.Contains(searchQuery) || (c.Description != null && c.Description.Contains(searchQuery)));
            }

            var totalItemCount = await collection.CountAsync();

            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);
            
            var collectionToReturn = await collection.OrderBy(c => c.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn,paginationMetadata);
        }

        public async Task<bool> CarExist(int carId)
        {
            return await _context.Cars.AnyAsync(c => c.Id == carId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestsAsync(int carId, int poiId)
        {
            return await _context.PointsOfInterest.Where(p => p.CarId == carId && p.Id == poiId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestsAsync(int carId)
        {
            return await _context.PointsOfInterest.Where(p => p.CarId == carId).ToListAsync();
        }

        public async Task AddPointOfInterestForCarAsync(int carId, PointOfInterest pointOfInterest)
        {
            var car = await GetCarAsync(carId,false);
            if (car != null) 
            {
                car.PointsOfInterests.Add(pointOfInterest);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> CarNameMatchesCarID(string? carName, int carId)
        {
            return await _context.Cars.AnyAsync(c=>c.Id == carId && c.Name == carName);
        }
    }
}
