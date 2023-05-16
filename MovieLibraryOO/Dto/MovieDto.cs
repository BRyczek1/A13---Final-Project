using MovieLibraryEntities.Models;
using System;
using System.Collections.Generic;

namespace MovieLibraryOO.Dto
{
    public class MovieDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
