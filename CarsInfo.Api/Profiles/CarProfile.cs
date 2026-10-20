using AutoMapper;
using CarsInfo.Api.Entities;
using CarsInfo.Api.Models;
namespace CarsInfo.Api.Profiles
{
    public class CarProfile:Profile
    {
        public CarProfile() 
        {
            CreateMap<Car, CarWithoutPointsOfInterestDto>();
        }
    }
}
