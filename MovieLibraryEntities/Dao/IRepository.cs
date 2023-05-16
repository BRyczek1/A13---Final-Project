using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Dao
{
    public interface IRepository
    {
        public void SaveChanges();

        public Movie AddMovie(string title, ICollection<Genre> genres, DateTime releaseDate);
        IEnumerable<Movie> GetAllMovies();
        IEnumerable<Movie> SearchMovies(string searchString);
        public Movie? GetMovie(Int64 id);
        public void DeleteMovie(Int64 id);
        public void RateMovie(User user, Movie movie, int rating);
        public Movie? TopMovie(int ageMin, int ageMax);
        public Movie? TopMovie(Occupation occupation);
        public Genre? GetGenre(string name);
        public MovieGenre AddMovieGenre(Movie movie, Genre genre);
        public User AddUser(int age, string gender, string zipcode, Occupation occupation);
        public User? GetUser(Int64 id);
        public Occupation? GetOccupation(string name);
    }
}
