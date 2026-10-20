using CarsInfo.Api.Entities;

namespace CarsInfo.Api.Services
{
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCarsAsync();
        Task<Car?> GetCarAsync(int carId, bool poi);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestsAsync(int carId);
        Task<PointOfInterest?> GetPointOfInterestsAsync(int carId, int poiId);

    }
}
