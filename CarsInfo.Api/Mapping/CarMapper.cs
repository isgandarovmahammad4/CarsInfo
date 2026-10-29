using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;
using Riok.Mapperly.Abstractions;

namespace CarsInfo.Api.Mapping
{
    /// <summary>
    /// Compile-time generated mappings between <see cref="Car"/> and its DTOs.
    /// </summary>
    [Mapper]
    [UseStaticMapper(typeof(PointOfInterestMapper))]
    public static partial class CarMapper
    {
        [MapProperty(nameof(Car.PointsOfInterests), nameof(CarDto.PointsOfInterest))]
        public static partial CarDto ToCarDto(this Car car);

        [MapperIgnoreSource(nameof(Car.PointsOfInterests))]
        public static partial CarWithoutPointsOfInterestDto ToCarWithoutPointsOfInterestDto(this Car car);

        public static partial IEnumerable<CarWithoutPointsOfInterestDto> ToCarWithoutPointsOfInterestDtos(this IEnumerable<Car> cars);
    }
}
