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

            //string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            //List<UserMovie> savedMovies = _context.UserMovie.Where(x => x.UserId == userId).ToList();

            //List<PopcornMovie> popList = new List<PopcornMovie>();

            //UserPopcorn userPop = new UserPopcorn();

            //List<UserPopcorn> upList = new List<UserPopcorn>();

            //foreach (UserMovie u in savedMovies)
            //{
            //    PopcornMovie pop = new PopcornMovie();
            //    pop = await MovieSelection($"{u.MovieId}");
            //    //breaks on second call
            //    int intScore;
            //    bool isValid = int.TryParse(pop.Metascore.ToString(), out intScore);
            //    if (isValid == true)
            //    {
            //        popList.Add(pop);
            //    }
            //    else
            //    {
            //        savedMovies.Remove(u);
            //    }

            //}

            //userPop.UserMovies = savedMovies;
            //userPop.PopcornMovies = popList;




            //foreach loop getting list of popcorn movie
            //combined class of UserPopcorn, make list of?
            //int try parse if/else statement on metacritic rating
            //in view display, titel, user rating, popcorn metacritic rating, if higher
            //say "Y'all have weird taste, maybe watch some AFI top 100 and try again"
            //if lower "critics were wrong I guess"


            return View(userPop);
        }
    }
}
