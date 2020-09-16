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
        public IActionResult MovieSelection(string id)
        {
            var selection = _movieDAL.SecondGetMovieInfo($"{id}");
            return View(selection);

        }

        [Authorize]
        public IActionResult Index()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> savedMovies = _context.UserMovie.Where(x => x.UserId == userId).ToList();
            
            List<PopcornMovie> popList = new List<PopcornMovie>();

            UserPopcorn userPop = new UserPopcorn();

            List<UserPopcorn> upList = new List<UserPopcorn>();
            
            foreach (UserMovie u in savedMovies)
            {
                PopcornMovie pop = new PopcornMovie();
                pop = (PopcornMovie)MovieSelection($"{u.MovieId}");
                popList.Add(pop);
            }

            userPop.UserMovies = savedMovies;
            userPop.PopcornMovies = popList;


            //foreach loop getting list of popcorn movie
            //combined class of UserPopcorn, make list of?
            //in view display, titel, user rating, popcorn metacritic rating, if higher
            //say "Y'all have weird taste, maybe watch some AFI top 100 and try again"
            //if lower "critics were wrong I guess"


            return View(userPop);
        }
    }
}
