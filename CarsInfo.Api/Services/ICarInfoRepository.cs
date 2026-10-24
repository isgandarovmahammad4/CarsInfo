using CarsInfo.Api.Entities;

namespace CarsInfo.Api.Services
{
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCarsAsync();
        Task<(IEnumerable<Car>, PaginationMetadata)> GetCarsAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
        Task<Car?> GetCarAsync(int carId, bool poi);
        Task<bool> CarExist(int carId);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestsAsync(int carId);
        Task<PointOfInterest?> GetPointOfInterestsAsync(int carId, int poiId);
        Task AddPointOfInterestForCityAsync(int carId, PointOfInterest pointOfInterest);
        Task<bool> SaveChangesAsync();
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
    }
}
