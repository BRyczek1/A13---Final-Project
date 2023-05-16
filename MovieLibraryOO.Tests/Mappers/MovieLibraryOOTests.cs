using AutoMapper;
using MovieLibraryEntities.Models;
using MovieLibraryOO.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MovieLibraryOO.Tests.Mappers
{
    public class MovieLibraryOOTests
    {
        private readonly Mapper _mapper;

        public MovieLibraryOOTests()
        {
            var mapperProfile = new MovieProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mapperProfile));
            _mapper = new Mapper(configuration);
        }

        [Fact]
        public void Mapper_ValidInput_MappedSuccessfully()
        {
            // ARRANGE
            MovieMapper mapper = new MovieMapper(_mapper);

            var genre =  new Genre() { Id = 1, Name = "Action" };
            var movieGenres = new List<MovieGenre> { new MovieGenre() { Id = 1, Genre = genre } };
            var movies = new List<Movie>();
            movies.Add(new Movie() { Id = 1, Title = "Test Title", ReleaseDate = DateTime.Now, MovieGenres = movieGenres });

            // ACT
            var movieDtos = mapper.Map(movies);

            // ASSERT
            Assert.NotEmpty(movieDtos);
            Assert.Single(movieDtos);

            var dto = movieDtos.First();
            Assert.Equal(1, dto.Id);
            Assert.InRange(dto.ReleaseDate, DateTime.Now.AddSeconds(-10), DateTime.Now.AddSeconds(10));
            Assert.Equal("Test Title", dto.Title);
        }
    }
}
