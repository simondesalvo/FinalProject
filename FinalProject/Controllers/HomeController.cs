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
            List<UserMovie> userList = new List<UserMovie>();
            foreach (UserMovie m in savedMovies)
            {
                userList.Add(m);
            }
            return View(userList);
        }


        //delete movie from watch list
        [Authorize]
        public IActionResult DeleteMovie(int id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserMovie movie = new UserMovie();
            Genre genre = new Genre();
            try
            {
                movie.UserId = userId;
                movie = _context.UserMovie.Where(x => x.Id == id).First();
                //movie.MovieId = genre.Imdbid;
                List<Genre> genreList = _context.Genre.Where(x => x.Imdbid == movie.MovieId).ToList();
                foreach (Genre g in genreList)
                {
                    _context.Genre.Remove(g);
                    _context.SaveChanges();
                }
                _context.UserMovie.Remove(movie);
                _context.SaveChanges();
                return RedirectToAction("DisplayList");
            }
            catch
            {
                return RedirectToAction("DisplayList");
            }
        }

        
       


        //add movie to list
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
            
            //Add the new object to the table 
            if(ModelState.IsValid)
            {
                UserMovie userMovieExisting = _context.UserMovie.Where(um=>um.MovieId == id).FirstOrDefault();
                if (userMovieExisting == null)
                {
                    ViewBag.MovieExitsInWatchedList = false;
                    _context.UserMovie.Add(userMovie);
                    _context.SaveChanges();
                    AddtoGenre(selectedMovie);
                }
                else
                {
                    ViewBag.MovieExitsInWatchedList = true;
                }                
            }
            return RedirectToAction("DisplayList");
        }


        //adds genre to genre table (for recommendations)
        public bool AddtoGenre(PopcornMovie popcornMovie)
        {     
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
                    }
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

        // Returns a list of unique Genres stored in the db
        public List<string> GetDistinctGenres()
        {
            List<string> distinctGenres = new List<string>();            
            distinctGenres = _context.Genre.ToList().Select(p => p.Genre1).Distinct().ToList();

            return distinctGenres;
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
