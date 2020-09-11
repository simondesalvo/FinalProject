using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinalProject.Models;
using Microsoft.Extensions.Configuration;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieDAL _movieDAL;
        private readonly string _apikey;        

        public HomeController(IConfiguration configuration)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<List<MovieSearch>> SearchMovie()
        {            
            List<MovieSearch> movies = await _movieDAL.GetMovie("inception");
            return movies;
        }
        public async Task<PopcornMovie> SearchMovieDetails()
        {
            PopcornMovie movie = await _movieDAL.GetMovieInfo("tt1375666");
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
