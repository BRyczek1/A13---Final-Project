using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using MovieLibraryEntities.Context;
using MovieLibraryEntities.Models;
using System.Reflection;

namespace MovieLibraryEntities.Dao
{
    public class Repository : IRepository, IDisposable
    {
        private readonly IDbContextFactory<MovieContext> _contextFactory;
        private readonly MovieContext _context;
        private Int64 nextMovie = 0;
        private Int64 nextUser = 0;

        public Repository(MovieContext dbContext)
        {
            _context = dbContext;
        }
        //public Repository(IDbContextFactory<MovieContext> contextFactory)
        //{
        //    _contextFactory = contextFactory;
        //    _context = _contextFactory.CreateDbContext();
        //}
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Movie AddMovie(string title, ICollection<Genre> genres, DateTime releaseDate)
        {
            var movie = new Movie();
            movie.Title = title;
            movie.ReleaseDate = releaseDate;

            if (this.nextMovie == 0)
            {
                var allMovies = _context.Movies;
                var listOfMovies = allMovies.ToList();
                Int64 maxId = 0;

                foreach (var m in listOfMovies)
                {
                    if (m.Id > maxId) maxId = m.Id;
                }

                this.nextMovie = maxId;
            }

            this.nextMovie++;
            movie.Id = this.nextMovie;

            foreach (var g in genres) AddMovieGenre(movie, g);

            var result = _context.Movies.Add(movie);
            _context.SaveChanges();

            return result.Entity;
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            return _context.Movies
                .Include(x => x.MovieGenres)
                .ThenInclude(x => x.Genre)
                .ToList();
        }

        public IEnumerable<Movie> SearchMovies(string searchString)
        {
            var allMovies = _context.Movies;
            var listOfMovies = allMovies.ToList();
            var temp = listOfMovies.Where(x => x.Title.Contains(searchString, StringComparison.CurrentCultureIgnoreCase));

            return temp;
        }

        public Movie? GetMovie(Int64 id)
        {
            var allMovies = _context.Movies
                .Include(x => x.MovieGenres)
                    .ThenInclude(x => x.Genre)
                .Include(x => x.UserMovies);

            var listOfMovies = allMovies.ToList();
            var temp = listOfMovies.Where(x => x.Id == id);

            if (temp.IsNullOrEmpty()) return null;

            return temp.First();
        }

        public void DeleteMovie(Int64 id)
        {
            var allMovies = _context.Movies;
            var listOfMovies = allMovies.ToList();
            var temp = listOfMovies.Where(x => x.Id == id);

            if (temp.IsNullOrEmpty()) return;

            allMovies.Remove(temp.First());
            _context.SaveChanges();
        }

        public void RateMovie(User user, Movie movie, int rating)
        {
            var allUserMovies = _context.UserMovies;
            var listOfUserMovies = allUserMovies.ToList();
            var temp = listOfUserMovies.Where(x => x.User == user && x.Movie == movie);

            if (temp.IsNullOrEmpty()) // rating doesn't exist yet
            {
                var um = new UserMovie();
                um.Rating = rating;
                um.RatedAt = DateTime.Now;
                um.User = user;
                um.Movie = movie;

                Int64 maxId = 0;

                foreach (var m in listOfUserMovies)
                {
                    if (m.Id > maxId) maxId = m.Id;
                }

                um.Id = maxId + 1;

                allUserMovies.Add(um);
            }
            else
            {
                temp.First().Rating = rating;
                temp.First().RatedAt = DateTime.Now;
            }

            _context.SaveChanges();
        }

        public Movie? TopMovie(int ageMin, int ageMax)
        {
            var allMovies = _context.Movies
                .Include(x => x.UserMovies)
                .ThenInclude(x => x.User);

            var listOfMovies = allMovies.ToList();

            if (listOfMovies.IsNullOrEmpty()) return null;

            var temp = listOfMovies
                .OrderBy(x => x.Title)
                .OrderBy(x => x.UserMovies.Count(y => y.User.Age >= ageMin && y.User.Age <= ageMax))
                .OrderByDescending(x => x.UserMovies
                    .Where(y => y.User.Age >= ageMin && y.User.Age <= ageMax)
                    .DefaultIfEmpty(new UserMovie { Rating = 0 })
                    .Average(y => y.Rating)
                    );

            if (temp.IsNullOrEmpty()) return null;

            return temp.First();
        }

        public Movie? TopMovie(Occupation occupation)
        {
            var allMovies = _context.Movies
                .Include(x => x.UserMovies)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.Occupation);

            var listOfMovies = allMovies.ToList();

            if (listOfMovies.IsNullOrEmpty()) return null;

            var temp = listOfMovies
                .OrderBy(x => x.Title)
                .OrderBy(x => x.UserMovies.Count(y => y.User.Occupation == occupation))
                .OrderByDescending(x => x.UserMovies
                    .Where(y => y.User.Occupation == occupation)
                    .DefaultIfEmpty(new UserMovie { Rating = 0 })
                    .Average(y => y.Rating));

            if (temp.IsNullOrEmpty()) return null;

            return temp.First();
        }

        public Genre? GetGenre(string name)
        {
            var allGenres = _context.Genres;
            var listOfGenres = allGenres.ToList();
            var temp = listOfGenres.Where(x => x.Name.ToLower() == name.ToLower());

            if (temp.IsNullOrEmpty()) return null;

            _context.Entry(temp.First())
                .Collection(x => x.MovieGenres)
                .Load();

            return temp.First();
        }

        public MovieGenre AddMovieGenre(Movie movie, Genre genre)
        {
            var mg = new MovieGenre();
            mg.Id = 0;

            var allGenres = _context.MovieGenres;
            var listOfGenres = allGenres.ToList();
            Int64 maxId = 0;

            foreach (var m in listOfGenres)
            {
                if (m.Id > maxId) maxId = m.Id;
            }

            mg.Id = Convert.ToInt32(maxId) + 1;
            mg.Movie = movie;
            mg.Genre = genre;

            return _context.MovieGenres.Add(mg).Entity;
        }

        public User AddUser(int age, string gender, string zipcode, Occupation occupation)
        {
            var user = new User();
            user.Age = age;
            user.Gender = gender;
            user.ZipCode = zipcode;
            user.Occupation = occupation;

            if(this.nextUser == 0)
            {
                var allUsers = _context.Users;
                var listOfUsers = allUsers.ToList();
                Int64 maxId = 0;

                foreach (var u in listOfUsers)
                {
                    if (u.Id > maxId) maxId = u.Id;
                }

                this.nextUser = maxId;
            }

            this.nextUser++;
            user.Id = this.nextUser;

            var result = _context.Users.Add(user);
            _context.SaveChanges();

            return result.Entity;
        }

        public User? GetUser(Int64 id)
        {
            var users = _context.Users
                .Include(x => x.Occupation)
                .Include(x => x.UserMovies)
                    .ThenInclude(x => x.Movie);

            var listOfUsers = users.ToList();
            var temp = listOfUsers.Where(x => x.Id == id);

            if (temp.IsNullOrEmpty()) return null;

            return temp.First();
        }

        public Occupation? GetOccupation(string name)
        {
            var allOccupations = _context.Occupations;
            var listOfOccupations = allOccupations.ToList();
            var temp = listOfOccupations.Where(x => x.Name.ToLower() == name.ToLower());

            if (temp.IsNullOrEmpty()) return null;

            return temp.First();
        }
    }
}
