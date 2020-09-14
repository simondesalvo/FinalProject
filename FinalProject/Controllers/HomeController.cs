using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinalProject.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieDAL _movieDAL;
        private readonly string _apikey;
        string loginUserId;
        private readonly MovieTrackerDbContext _context;

        public HomeController(IConfiguration configuration)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
            loginUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }


        public async Task<IActionResult> SearchResult(string userInput)
        {
            List<MovieSearch> movies = await _movieDAL.GetMovies($"{userInput}");            
            return View("SearchResult", movies);
        }

        public async Task<IActionResult> MovieSelection(string id)
        {
            var selection = await _movieDAL.SecondGetMovieInfo($"{id}");
            return View(selection);

        }


        public async Task<PopcornMovie> SecondApiSearch()
        {
            PopcornMovie movie = await _movieDAL.SecondGetMovieInfo("tt1375666");
            return movie;
        }

        public IActionResult AddtoWatchedList(PopcornMovie popcornMovie)
        {
            UserMovie userMovie = new UserMovie();
            userMovie.UserId = loginUserId;
            userMovie.MovieId = popcornMovie.imdbID;
            userMovie.Watched = false;
            try
            {
                _context.UserMovie.Add(userMovie);
                _context.SaveChanges();
                AddtoGenre(popcornMovie);
            }
            catch
            {

            }
            return RedirectToAction("DisplayList");
        }

        public bool AddtoGenre(PopcornMovie popcornMovie)
        {            
            Genre newEntry = new Genre();
            newEntry.Imdbid = popcornMovie.imdbID;
            newEntry.Genre1 = popcornMovie.Genre;
            UniqueGenre uGenre = new UniqueGenre();
            try
            {
                _context.Genre.Add(newEntry);
                _context.SaveChanges();
                if(!uGenre.uniqueGenres.Contains(popcornMovie.Genre))
                {
                    uGenre.uniqueGenres.Add(popcornMovie.Genre);
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
