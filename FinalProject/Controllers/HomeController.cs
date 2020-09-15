using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinalProject.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieDAL _movieDAL;
        private readonly string _apikey;
        private readonly MovieTrackerDbContext _context;
        private readonly MovieTrackerDbContext _movieDB;

        public HomeController(IConfiguration configuration, MovieTrackerDbContext context)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        //basic search result list from first API
        public async Task<IActionResult> SearchResult(string userInput)
        {
            List<MovieSearch> movies = await _movieDAL.GetMovies($"{userInput}");            
            return View("SearchResult", movies);
        }

        //selection from search result, second API call
        public async Task<IActionResult> MovieSelection(string id)
        {
            var selection = await _movieDAL.SecondGetMovieInfo($"{id}");
            return View(selection);

        }


        //display watched movie list
        [Authorize]
        public IActionResult DisplayList()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> savedMovies = _movieDB.UserMovie.Where(x => x.UserId == id).ToList();
            return View(savedMovies);
        }

        //delete movie from watch list
        [Authorize]
        public IActionResult DeleteMovie(string id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserMovie movie = new UserMovie();
            try
            {
                movie.UserId = userId;
                movie = _movieDB.UserMovie.Where(x => x.MovieId == id).First();
                _movieDB.UserMovie.Remove(movie);
                _movieDB.SaveChanges();
                return RedirectToAction("DisplayList");
            }
            catch
            {
                return RedirectToAction("DisplayList");
            }
        }

        //search for second API, not used yet
        public async Task<PopcornMovie> SecondApiSearch()
        {
            PopcornMovie movie = await _movieDAL.SecondGetMovieInfo("tt1375666");
            return movie;
        }

        public async Task<IActionResult> AddtoWatchedList(string id)
        {
            // Get movie info from the API using the IMDB id
            PopcornMovie selectedMovie = await _movieDAL.SecondGetMovieInfo($"{id}");
            // Get the login user id
            string loginUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            // Create a new UserMovie object and fill in the details
            UserMovie userMovie = new UserMovie();
            userMovie.UserId = loginUserId;
            userMovie.MovieId = selectedMovie.imdbID;
            userMovie.Title = selectedMovie.Title;
            userMovie.Watched = false;
            //try
            //{
            //Add the new object to the table 
                if(ModelState.IsValid)
            {
                _context.UserMovie.Add(userMovie);
                _context.SaveChanges();
                AddtoGenre(selectedMovie);
            }
                
            //}
            //catch
            //{

            //}
            return RedirectToAction("DisplayList");
        }

        public bool AddtoGenre(PopcornMovie popcornMovie)
        {
            // Create a new object to store a list of unique genres
            UniqueGenre uGenre = new UniqueGenre();

            try
            {
                //split genre value to get individual genres
                string genreString = popcornMovie.Genre.Replace(",", null);
                string[] genreArray = genreString.Split(" ");
                for (int i=0; i<genreArray.Length;i++)
                {
                    // Create a new genre object and fill in the details
                    Genre newEntry = new Genre();
                    newEntry.Imdbid = popcornMovie.imdbID;
                    newEntry.Genre1 = genreArray[i];

                    if(ModelState.IsValid)
                    {
                        //Add the new genre to the table 
                        _context.Genre.Add(newEntry);
                        _context.SaveChanges();

                        // If the genre is not already present in the unique list, add it.

                        var listOfGenres = _context.Genre.GroupBy(x => x.Genre1).Select(x => x.FirstOrDefault());
                        var details = _context.Genre.GroupBy(x => x.Genre1).Select(y => y.First()).Distinct();

                        if (uGenre.uniqueGenres == null)
                        {
                            uGenre.uniqueGenres = new List<string>();
                            uGenre.uniqueGenres.Add(genreArray[i]);
                        }
                        else if (!uGenre.uniqueGenres.Contains(genreArray[i]))
                        {
                            uGenre.uniqueGenres.Add(genreArray[i]);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult UpdateWatchedList(UserMovie userMovie)
        {
            UserMovie userMovieToUpdate = _context.UserMovie.Find(userMovie.Id);
            userMovieToUpdate.Watched = userMovie.Watched;
            userMovieToUpdate.WatchLocation = userMovie.WatchLocation;
            userMovieToUpdate.WatchYear = userMovie.WatchYear;
            userMovieToUpdate.UserRating = userMovie.UserRating;
            userMovieToUpdate.UserReview = userMovie.UserReview;
            // TO BE IMPLEMENTED LATER
            //userMovieToUpdate.WatchedTogetherId = ;     
            _context.UserMovie.Update(userMovieToUpdate);
            _context.SaveChanges();

            return RedirectToAction("DisplayList");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
