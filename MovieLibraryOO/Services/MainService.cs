using Castle.Core.Internal;
using ConsoleTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieLibraryEntities.Dao;
using MovieLibraryEntities.Models;
using MovieLibraryOO.Dao;
using MovieLibraryOO.Dto;
using MovieLibraryOO.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieLibraryOO.Services
{
    public class MainService : IMainService
    {
        private readonly ILogger<MainService> _logger;
        private readonly IMovieMapper _movieMapper;
        private readonly IRepository _repository;
        private readonly IUserService _users;

        public MainService(ILogger<MainService> logger, IMovieMapper movieMapper, IRepository repository, IUserService users)
        {
            _logger = logger;
            _movieMapper = movieMapper;
            _repository = repository;
            _users = users;
        }
        private void ViewMovie(Movie movie)
        {
            if (movie == null)
            {
                _logger.LogInformation($"Tried to view invalid movie");
            }
            else
            {
                Console.WriteLine($"Title: {movie.Title}");
                Console.WriteLine($"Release Date: {movie.ReleaseDate}");

                if (movie.MovieGenres.IsNullOrEmpty())
                {
                    Console.WriteLine("No genres given");
                }
                else
                {
                    Console.Write("Genres: ");
                    foreach (var g in movie.MovieGenres)
                    {
                        Console.Write($"{g.Genre.Name}, ");
                    }
                    Console.WriteLine();
                }

                if (movie.UserMovies.IsNullOrEmpty())
                {
                    Console.WriteLine("Average Rating: Unrated");
                }
                else
                {
                    long sum = 0;

                    foreach (var um in movie.UserMovies)
                    {
                        sum += um.Rating;
                    }

                    var avg = (float)sum / (float)movie.UserMovies.Count;

                    Console.WriteLine($"Average Rating: {avg}");

                    foreach (var um in movie.UserMovies)
                    {
                        Console.WriteLine($"User #{um.User.Id} gave a rating of #{um.Rating}");
                    }
                }

                Console.WriteLine("\n");
            }
        }

        public void Invoke()
        {
            var menu = new Menu();

            Menu.MenuOptions menuChoice;

            do
            {
                menuChoice = menu.ChooseAction();

                switch (menuChoice)
                {
                    case Menu.MenuOptions.CreateMovie:
                        if (_users.LoggedInAs != null)
                        {
                            var title = menu.GetUserResponse("Movie", "name:");

                            var genre = "";
                            var genres = new List<Genre>();

                            while (true)
                            {
                                genre = menu.GetUserResponse("Add movie", "genre (or type 'done' to continue):");

                                if (genre == "done") break;

                                var entity = _repository.GetGenre(genre);

                                if (entity == null)
                                {
                                    genre = menu.GetUserResponse("Invalid genre, try again:");
                                }
                                else
                                {
                                    genres.Add(entity);
                                }
                            }

                            var dateStr = menu.GetUserResponse("Movie", "release date:");
                            DateTime date;

                            while (!DateTime.TryParse(dateStr, out date))
                            {
                                menu.GetUserResponse("Invalid date, try again:");
                            }

                            var movie = _repository.AddMovie(title, genres, date);

                            Console.WriteLine($"Movie ID: {movie.Id}");
                            Console.WriteLine($"Movie Title: {movie.Title}");
                            Console.WriteLine($"Movie Genres: {movie.MovieGenres}");
                            Console.WriteLine($"Movie Release Date: {movie.ReleaseDate}");

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} added movie #{movie.Id} to the database");
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.UpdateMovie:
                        if (_users.LoggedInAs != null)
                        {
                            var idStr = menu.GetUserResponse("Movie", "id:");
                            Int64 id = 0;
                            Movie movie = null;

                            if (Int64.TryParse(idStr, out id))
                            {
                                movie = _repository.GetMovie(id);
                            }

                            if (movie == null)
                            {
                                _logger.LogInformation($"Tried to edit movie with invalid id {id}");
                            }
                            else
                            {
                                movie.Title = menu.GetUserResponseWithDefault("Movie", "name:", movie.Title);

                                if(movie.MovieGenres.IsNullOrEmpty())
                                {
                                    movie.MovieGenres = new List<MovieGenre>();
                                }

                                foreach (var g in movie.MovieGenres)
                                {
                                    var yn = "";

                                    while (yn != "y" && yn != "n")
                                    {
                                        yn = menu.GetUserResponse($"Keep genre '{g.Genre.Name}'?", "(y/n):");
                                    }

                                    if (yn == "n") movie.MovieGenres.Remove(g);
                                }

                                string genre = "";

                                while (true)
                                {
                                    genre = menu.GetUserResponse("Add movie", "genre (or type 'done' to continue):");

                                    if (genre == "done") break;

                                    var entity = _repository.GetGenre(genre);

                                    if (entity == null)
                                    {
                                        genre = menu.GetUserResponse("Invalid genre, try again:");
                                    }
                                    else
                                    {
                                        _repository.AddMovieGenre(movie, entity);
                                    }
                                }

                                var dateStr = menu.GetUserResponseWithDefault("Movie", "release date:", movie.ReleaseDate.ToString());
                                DateTime date;

                                while (!DateTime.TryParse(dateStr, out date))
                                {
                                    menu.GetUserResponse("Invalid date, try again:");
                                }

                                movie.ReleaseDate = date;
                                _repository.SaveChanges();

                                _logger.LogInformation($"User #{_users.LoggedInAs.Id} updated movie #{id} in the database");
                            }
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.DeleteMovie:
                        if (_users.LoggedInAs != null)
                        {
                            var idStr = menu.GetUserResponse("Movie", "id:");
                            Int64 id = 0;
                            Movie movie = null;

                            if (Int64.TryParse(idStr, out id))
                            {
                                movie = _repository.GetMovie(id);
                            }

                            if (movie == null)
                            {
                                _logger.LogInformation($"Tried to delete movie with invalid id {id}");
                            }
                            else
                            {
                                _repository.DeleteMovie(id);
                            }

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} deleted movie #{id} from the database");
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                        break;
                    case Menu.MenuOptions.SearchMovies:
                        if (_users.LoggedInAs != null)
                        {
                            var name = menu.GetUserResponse("Enter movie", "name:");

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} searched for movies from database");
                            var results = _repository.SearchMovies(name);
                            var movies = _movieMapper.Map(results);
                            ConsoleTable.From<MovieDto>(movies).Write();
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.ShowAllMovies:
                        if (_users.LoggedInAs != null)
                        {
                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} listed all movies in database");
                            var results = _repository.GetAllMovies();
                            var movies = _movieMapper.Map(results);
                            ConsoleTable.From<MovieDto>(movies).Write();
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.ViewMovie:
                        if (_users.LoggedInAs != null)
                        {
                            var idStr = menu.GetUserResponse("Movie", "id:");
                            Int64 id = 0;
                            Movie movie = null;

                            if (Int64.TryParse(idStr, out id))
                            {
                                movie = _repository.GetMovie(id);
                            }

                            if (movie == null)
                            {
                                _logger.LogInformation($"Tried to view invalid movie with invalid id {id}");
                            }
                            else
                            {
                                ViewMovie(movie);
                            }

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} viewed movie #{movie.Id}");

                            string rate = "";

                            while (rate != "y" && rate != "n")
                            {
                                rate = menu.GetUserResponse("Would you like to rate this movie?", "(y/n):").ToLower();
                            }

                            if(rate == "y")
                            {
                                string ratingStr = menu.GetUserResponse("Enter", "rating (1-5):");
                                int rating = 0;

                                while (!Int32.TryParse(ratingStr, out rating))
                                {
                                    ratingStr = menu.GetUserResponse("Invalid rating, try again:");
                                }

                                _repository.RateMovie(_users.LoggedInAs, movie, rating);
                            }
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.TopMovieByAge:
                        if (_users.LoggedInAs != null)
                        {
                            var ageStr = menu.GetUserResponse("Minimum", "age:");
                            int min = 0, max = 0;

                            while (!Int32.TryParse(ageStr, out min))
                            {
                                ageStr = menu.GetUserResponse("Invalid age, try again:");
                            }

                            ageStr = menu.GetUserResponse("Maximum", "age:");
                            while (!Int32.TryParse(ageStr, out max))
                            {
                                ageStr = menu.GetUserResponse("Invalid age, try again:");
                            }

                            var movie = _repository.TopMovie(min, max);
                            ViewMovie(movie);

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} viewed top movie for people of age {min}-{max}");
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.TopMovieByOccupation:
                        if (_users.LoggedInAs != null)
                        {
                            string occupationStr = menu.GetUserResponse("Enter", "occupation:");
                            Occupation occupation = _repository.GetOccupation(occupationStr);

                            while (occupation == null)
                            {
                                occupationStr = menu.GetUserResponse("Invalid occupation, try again:", "");
                                occupation = _repository.GetOccupation(occupationStr);
                            }

                            var movie = _repository.TopMovie(occupation);
                            ViewMovie(movie);

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} viewed top movie for people with occupation {occupation}");
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.GenrePopulation:
                        if (_users.LoggedInAs != null)
                        {
                            var pop = new Dictionary<string, int>();
                            var movies = _repository.GetAllMovies();

                            foreach(var m in movies)
                            {
                                foreach(var g in m.MovieGenres)
                                {
                                    if (!pop.ContainsKey(g.Genre.Name)) pop[g.Genre.Name] = 1;
                                    else pop[g.Genre.Name]++;
                                }
                            }

                            _logger.LogInformation($"User #{_users.LoggedInAs.Id} viewed genre population");

                            Console.WriteLine("There are...");

                            foreach(var item in pop)
                            {
                                Console.WriteLine($"{item.Value} {item.Key} movies");
                            }

                            Console.WriteLine('\n');
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.LogIn:
                        {
                            var idStr = menu.GetUserResponse("User", "id:");
                            Int64 id = 0;
                            User user = null;

                            if (Int64.TryParse(idStr, out id))
                            {
                                user = _repository.GetUser(id);
                            }

                            if (user == null)
                            {
                                _logger.LogInformation($"Tried to login as user with invalid id {id}");
                            }
                            else
                            {
                                _users.LoggedInAs = user;
                                _logger.LogInformation($"Logged in as user #{_users.LoggedInAs.Id}");
                            }
                        }
                        break;
                    case Menu.MenuOptions.LogOut:
                        if (_users.LoggedInAs != null)
                        {
                            _logger.LogInformation($"Logged out of user #{_users.LoggedInAs.Id}");
                            _users.LoggedInAs = null;
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                    case Menu.MenuOptions.CreateUser:
                        {
                            string ageStr = menu.GetUserResponse("Enter user", "age:");
                            int age = 0;

                            while (!Int32.TryParse(ageStr, out age) || age <= 0)
                            {
                                ageStr = menu.GetUserResponse("Invalid age, try again:");
                            }

                            string gender = menu.GetUserResponse("Enter user", "gender:").ToUpper();

                            while (gender != "M" && gender != "F" && gender != "X")
                            {
                                gender = menu.GetUserResponse("Gender must be M, F, or X; try again:").ToUpper();
                            }

                            string zipStr = menu.GetUserResponse("Enter user", "zip code:");
                            int zip = 0;

                            while (!Int32.TryParse(zipStr, out zip) || zip <= 0)
                            {
                                zipStr = menu.GetUserResponse("Invalid zipcode, try again:");
                            }

                            string occupationStr = menu.GetUserResponse("Enter user", "occupation:");
                            Occupation occupation = _repository.GetOccupation(occupationStr);

                            while (occupation == null)
                            {
                                occupationStr = menu.GetUserResponse("Invalid occupation, try again:", "");
                                occupation = _repository.GetOccupation(occupationStr);
                            }

                            var user = _repository.AddUser(age, gender, zipStr, occupation);

                            Console.WriteLine($"User ID: {user.Id}");
                            Console.WriteLine($"User age: {user.Age}");
                            Console.WriteLine($"User gender: {user.Gender}");
                            Console.WriteLine($"User zip-code: {user.ZipCode}");
                            Console.WriteLine($"User occupation: {user.Occupation.Name}");

                            string login = "";

                            while (login != "y" && login != "n")
                            {
                                login = menu.GetUserResponse("Login as this user?", "(y/n):").ToLower();
                            }

                            _logger.LogInformation($"Created user #{user.Id}");

                            if (login == "y")
                            {
                                _logger.LogInformation($"Logged in as user #{user.Id}");
                                _users.LoggedInAs = user;
                            }
                        }
                        break;
                    case Menu.MenuOptions.ViewUser:
                        if (_users.LoggedInAs != null)
                        {
                            var idStr = menu.GetUserResponse("User", "id:");
                            Int64 id = 0;
                            User user = null;

                            if (Int64.TryParse(idStr, out id))
                            {
                                user = _repository.GetUser(id);
                            }

                            if (user == null)
                            {
                                _logger.LogInformation($"Tried to login as user with invalid id {id}");
                            }
                            else
                            {
                                Console.WriteLine($"User ID: {user.Id}");
                                Console.WriteLine($"User age: {user.Age}");
                                Console.WriteLine($"User gender: {user.Gender}");
                                Console.WriteLine($"User zip-code: {user.ZipCode}");
                                Console.WriteLine($"User occupation: {user.Occupation.Name}");

                                foreach(var um in user.UserMovies)
                                {
                                    Console.WriteLine($"Rated {um.Movie.Title} with score of {um.Rating} on {um.RatedAt}");
                                }

                                _logger.LogInformation($"User #{_users.LoggedInAs.Id} viewed user #{user.Id}'s info");
                            }
                        }
                        else Console.WriteLine("Must be logged in to perform that action.");
                        break;
                }
            }
            while (menuChoice != Menu.MenuOptions.Exit);

            menu.Exit();


            Console.WriteLine("\nThanks for using the Movie Library!");

        }
    }
}