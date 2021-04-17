using System.Linq;
using API.Dtos;
using API.Models;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>().
                ForMember(dest => dest.PhotosUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url)).
                ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<User, UserForDetailedDto>().
                ForMember(dest => dest.PhotosUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url)).
                ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
        }
    }
}