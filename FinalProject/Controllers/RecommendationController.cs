using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FinalProject.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly MovieDAL _movieDAL;
        private readonly MovieTrackerDbContext _context;
        private readonly string _apikey;

        //public RecommendationController(IConfiguration configuration)
        //{
        //    _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
        //    _movieDAL = new MovieDAL(_apikey);
        //    _context = new MovieTrackerDbContext();
        //}
        public RecommendationController(IConfiguration configuration, MovieTrackerDbContext context)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
            _context = context;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Recommended()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
           
            string mostViewedGenre= GetMostWatchedGenre();
            List<UserMovie> movieByGenre = new List<UserMovie>();

            List<Genre> allGenres = _context.Genre.Where(g => g.Genre1==mostViewedGenre).ToList();
            for (int g = 0; g < allGenres.Count; g++)
            {
                Genre watchedGenre = allGenres[g];
                List<UserMovie> findMovie = _context.UserMovie.Where(r => r.MovieId == watchedGenre.Imdbid).ToList();
                movieByGenre.Add(findMovie[0]);
            }
            List<UserMovie> recommendedMovies = movieByGenre.Except(watchedMovies).ToList();
            recommendedMovies.Select(x => x.MovieId).Distinct();
            return View(recommendedMovies);
        }

        public string GetMostWatchedGenre()
        {
            List<string> distinctGenres = GetDistinctGenres();
           List<Genre> allGenres = _context.Genre.ToList();
            
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            List<int> genreCounts = new List<int>();
            for (int i = 0; i < distinctGenres.Count; i++)  //iterates through every genre available / i= the genre we are checking
            {
                int counter = 0;
                string checkedGenre = distinctGenres[i];
                for (int u = 0; u < watchedMovies.Count; u++) //iterates through every movie wathced by the user
                {
                    
                    List <Genre> userGenres = allGenres.Where(m => m.Imdbid == watchedMovies[u].MovieId).ToList(); //variable that finds the genre of the wathced movie at u
                    for (int wg=0; wg<userGenres.Count; wg++) //goes through every genre associated with a single wathced movie
                    if (userGenres[wg].Genre1.Contains(checkedGenre))   //increases count if the watched movie's genre is the same as the genre we are checking (i)
                    {
                        counter++;
                    }
                }
                genreCounts.Add(counter);   //adds the count of that genre to a list

            }
            int genreIndex = genreCounts.IndexOf(genreCounts.Max());  //finds the index of the genre with the highest count (Most watched Genre)
            return distinctGenres[genreIndex];
        }
        public List<string> GetDistinctGenres()
        {
            List<string> distinctGenres = new List<string>();
            distinctGenres = _context.Genre.ToList().Select(p => p.Genre1).Distinct().ToList();

            return distinctGenres;
        }


    }
}
