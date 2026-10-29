using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;
using Riok.Mapperly.Abstractions;

namespace CarsInfo.Api.Mapping
{
    /// <summary>
    /// Compile-time generated mappings between <see cref="PointOfInterest"/> and its DTOs.
    /// </summary>
    [Mapper]
    public static partial class PointOfInterestMapper
    {
        [MapperIgnoreSource(nameof(PointOfInterest.Car))]
        [MapperIgnoreSource(nameof(PointOfInterest.CarId))]
        public static partial PointOfInterestDto ToDto(this PointOfInterest pointOfInterest);

        public static partial IEnumerable<PointOfInterestDto> ToDtos(this IEnumerable<PointOfInterest> pointsOfInterest);

        [MapperIgnoreTarget(nameof(PointOfInterest.Id))]
        [MapperIgnoreTarget(nameof(PointOfInterest.Car))]
        [MapperIgnoreTarget(nameof(PointOfInterest.CarId))]
        public static partial PointOfInterest ToEntity(this PointOfInterestForCreationDto dto);

        [MapperIgnoreSource(nameof(PointOfInterest.Id))]
        [MapperIgnoreSource(nameof(PointOfInterest.Car))]
        [MapperIgnoreSource(nameof(PointOfInterest.CarId))]
        public static partial PointOfInterestForUpdateDto ToUpdateDto(this PointOfInterest pointOfInterest);

        /// <summary>
        /// Copies the updatable fields from <paramref name="dto"/> onto an existing entity.
        /// </summary>
        [MapperIgnoreTarget(nameof(PointOfInterest.Id))]
        [MapperIgnoreTarget(nameof(PointOfInterest.Car))]
        [MapperIgnoreTarget(nameof(PointOfInterest.CarId))]
        public static partial void ApplyTo(this PointOfInterestForUpdateDto dto, PointOfInterest pointOfInterest);
    }
}
