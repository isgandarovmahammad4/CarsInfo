using AutoMapper;
using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;

namespace CarsInfo.Api.Profiles
{
    public class PointOfInterestProfile:Profile
    {
        public PointOfInterestProfile()
        {
            CreateMap<PointOfInterest,PointOfInterestDto>();
            CreateMap<PointOfInterestForCreationDto,PointOfInterest>();
            CreateMap<PointOfInterestForUpdateDto,PointOfInterest>();
            CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();

        }
    }
}
