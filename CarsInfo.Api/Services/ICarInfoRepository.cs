using CarsInfo.Api.Entities;

namespace CarsInfo.Api.Services
{
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCarsAsync();
        Task<Car?> GetCarAsync(int cityId, bool poi);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterests(int cityId);
        Task<PointOfInterest?> GetPointOfInterestsAsync(int cityId, int poiId);

    }
}
