using CarsInfo.Api.Entities;

namespace CarsInfo.Api.Services
{
    public interface ICarInfoRepository
    {
        Task<IEnumerable<Car>> GetCars();

    }
}
