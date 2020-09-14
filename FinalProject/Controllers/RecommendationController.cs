//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using FinalProject.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;

//namespace FinalProject.Controllers
//{
//    public class RecommendationController : Controller
//    {
//        private readonly MovieDAL _movieDAL;
//        private readonly MovieTrackerDbContext _context;
//        private readonly string _apikey;

//        public RecommendationController(IConfiguration configuration)
//        {
//            _apikey = configuration.GetSection("ApiKeys")["MovieAPIKey"];
//            _movieDAL = new MovieDAL(_apikey);
//            _context = new MovieTrackerDbContext();
//        }
//        public IActionResult Index()
//        {
//            return View();
//        }

//        public IActionResult Recommended()
//        {

//            int count1stMovie = GenreRecommender();
//            int actioncount = ActionGenreCount();

//            return View();
//        }
//        public int GenreRecommender()
//        {
            
//            //Genre genre1 = _context.Genre.Select(x =>x.Genre1 ==watchedMovies[0].MovieId)
//            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

//            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
//            List<UserMovie> allUserMovies = _context.UserMovie.ToList();
//            int genreCount1 = 0;
           
//            for (int i = 0; i < watchedMovies.Count; i++)
//            {
//                Genre genre1st = (Genre)_context.Genre.Where(x => x.Imdbid == watchedMovies[0].MovieId);
//                string genreChecker = _context.Genre.Where(x => x.Imdbid == watchedMovies[i].MovieId).ToString();
//                genre1st.Genre1
                
//                if (genre1 == genreChecker)
//                {
//                    genreCount1++;
//                }


//                //foreach (UserMovie m in watchedMovies)
//                //{
//                //    if (genre1 == m.ToString())
//                //    {
//                //        genreCount1++;
//                //    }
//                //}
//            }
//            return genreCount1;
//        }
//        public int ActionGenreCount()
//        {
//            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

//            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
//            List<UserMovie> allUserMovies = _context.UserMovie.ToList();

//            int genreCount1 = 0;
//            for (int i = 0; i < watchedMovies.Count; i++)
//            {
//                // string genre1 = _context.Genre.Where(x => x.Imdbid == watchedMovies[0].MovieId).ToString();
//                string action = "action";
//                string genreChecker = _context.Genre.Where(x => x.Imdbid == watchedMovies[i].MovieId).ToString().ToLower();
//                if (genreChecker.Contains(action))
//                {
//                    genreCount1++;
//                }
//            }
//            return genreCount1;
//        }
//        public int ActionGenreCount()
//        {
//            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

//            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
//            List<Genre> genres = _context.Genre.Distinct(x=> x.);

//        }
//        public Genre GetMostWatchedGenre()
//        {
//            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
//             List<Genre> allGenres = _context.Genre.Distinct().ToList();
//            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId ==id).ToList();
//            //var uniqueGenres = _context.Genre.Select(g => g.Genre1).Distinct();

//            for (int i = 0; i < allGenres.Count(); i++)
//            {
//                string checkedGenre = allGenres[i].Genre1;
//                for (int u=0; u<watchedMovies.Count(); u++)
//                {
//                Genre userGenres = allGenres.Where(m => m.Imdbid == watchedMovies[u].MovieId);
//                    if ( userGenres== allGenres[i].Genre1)
//                    {

//                    }
//                }
               

//            }
//        }
//    }
//}
