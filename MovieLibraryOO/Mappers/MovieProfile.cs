using AutoMapper;
using MovieLibraryEntities.Models;
using MovieLibraryOO.Dto;

namespace MovieLibraryOO.Mappers
{
    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<Movie, MovieDto>();
            // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
        }
    }
}
