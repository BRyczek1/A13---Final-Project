using AutoMapper;
using MovieLibraryEntities.Models;
using MovieLibraryOO.Dto;
using System.Collections.Generic;

namespace MovieLibraryOO.Mappers
{
    public class MovieMapper : IMovieMapper
    {
        private readonly IMapper _mapper;

        public MovieMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<MovieDto> Map(IEnumerable<Movie> movies)
        {
            IEnumerable<MovieDto> dto = _mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDto>>(movies);
            return dto;
        }

        public MovieDto Map(Movie movie)
        {
            MovieDto dto = _mapper.Map<Movie, MovieDto>(movie);
            return dto;
        }
    }
}
