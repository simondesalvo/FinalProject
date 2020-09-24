using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace FinalProject.Controllers
{
    public class UserCritic : Controller
    {
        private readonly MovieDAL _movieDAL;
        private readonly MovieTrackerDbContext _context;
        private readonly string _apikey;

        public UserCritic(IConfiguration configuration, MovieTrackerDbContext context)
        {
            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
            _movieDAL = new MovieDAL(_apikey);
            _context = context;

        }

        //api call
        public async Task<PopcornMovie> MovieSelection(string id)
        {
            var selection = await _movieDAL.SecondGetMovieInfo($"{id}");
            return selection;

        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //maybe a dictionary with movie title and a list of ints
            Dictionary<string, int[]> userPop = new Dictionary<string, int[]>();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> savedMovies = _context.UserMovie.Where(x => x.UserId == userId).ToList();

            for (int i = 0; i< savedMovies.Count; i++)
            {
                PopcornMovie pop = new PopcornMovie();
                pop = await MovieSelection($"{savedMovies[i].MovieId}");
                int metaScore;
                bool isValid = int.TryParse(pop.Metascore, out metaScore);
                if (isValid == true)
                {
                    int[] scores = new int[] { savedMovies[i].UserRating, metaScore };
                    userPop.Add(savedMovies[i].Title, scores);
                }
            }     

            return View(userPop);
        }
    }
}
