using MovieLibraryEntities.Models;
using MovieLibraryOO.Dto;
using System.Collections.Generic;

namespace MovieLibraryOO.Mappers
{
    public interface IMovieMapper
    {
        IEnumerable<MovieDto> Map(IEnumerable<Movie> movies);
    }
}