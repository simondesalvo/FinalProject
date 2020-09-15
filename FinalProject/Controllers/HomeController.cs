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


        //display movie list
        [Authorize]
        public IActionResult DisplayList()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> savedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            return View(savedMovies);
        }

        //delete movie from watch list
        [Authorize]
        public IActionResult DeleteMovie(int id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserMovie movie = new UserMovie();
            try
            {
                movie.UserId = userId;
                movie = _context.UserMovie.Where(x => x.Id == id).First();
                _context.UserMovie.Remove(movie);
                _context.SaveChanges();
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


        //add movie to list
        public async Task<IActionResult> AddtoWatchedList(string id)
        {
            PopcornMovie selectedMovie = await _movieDAL.SecondGetMovieInfo($"{id}");
            string loginUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserMovie userMovie = new UserMovie();
            userMovie.UserId = loginUserId;
            userMovie.MovieId = selectedMovie.imdbID;
            userMovie.Title = selectedMovie.Title;
            userMovie.Watched = false;
            //try
            //{
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


        //adds genre to genre table (for recommendations)
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


        //checks to see if user's movie is still in database before going to update view
        public IActionResult UpdateMovie (int id)
        {
            UserMovie movie = _context.UserMovie.Find(id);
            if (movie == null)
            {
                return RedirectToAction("DisplayList");
            }
            else
            {
                return View(movie);
            }
        }
        //update movie in list
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
            _context.Entry(userMovieToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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
