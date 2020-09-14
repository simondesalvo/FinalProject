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
        private readonly MovieTrackerDbContext _movieDB;

        public HomeController(IConfiguration configuration)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
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
                return RedirectToAction("DisplayWatchedMovies");
            }
            catch
            {
                return RedirectToAction("DisplayWatchedMovies");
            }
        }




        //search for second API, not used yet
        public async Task<PopcornMovie> SecondApiSearch()
        {
            PopcornMovie movie = await _movieDAL.SecondGetMovieInfo("tt1375666");
            return movie;
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
