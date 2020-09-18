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
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList(); //list of movies wathced by a user
           
            string mostViewedGenre= GetMostWatchedGenre();
            List<UserMovie> movieByGenre = new List<UserMovie>();

            List<Genre> allGenres = _context.Genre.Where(g => g.Genre1==mostViewedGenre).ToList();  //gets every instance of the most wathced genre
            for (int g = 0; g < allGenres.Count; g++)  //goes through each instance of the most watched genre
            {
                bool watched = false;
                Genre watchedGenre = allGenres[g];
                List<UserMovie> findMovie = _context.UserMovie.Where(r => r.MovieId == watchedGenre.Imdbid).ToList();   //gets the user movies associated with that instance of the most watched genre
                foreach (UserMovie m in watchedMovies)
                {
                    if (findMovie[0].MovieId == m.MovieId)    //makes sure movies that the user has already watch don't get added to recommended movie list
                    {
                        watched = true;
                    }
                }
                    if (!watched) 
                    {
                    
                        movieByGenre.Add(findMovie[0]);
                    }
            }
            List<UserMovie> recommendedMovies = movieByGenre.Except(watchedMovies).ToList();    //excludes the movies the user has already watched
            recommendedMovies.Select(x => x.MovieId).Distinct();  //makes sure the same movie doesn't show up multiple times
           
                Dictionary<UserMovie,double>moviesWithAvgRating = new Dictionary<UserMovie, double>();
            for (int um = 0; um < recommendedMovies.Count(); um++)
            {

                double averageRating = GetAverageRating(recommendedMovies[um]);
                moviesWithAvgRating.Add(recommendedMovies[um], averageRating);

            }
            Dictionary<UserMovie, double> sortetDictionary = moviesWithAvgRating.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            ViewBag.Genre = mostViewedGenre;
            return View(sortetDictionary);
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

        public IActionResult GetMoviesOfDecade(string year, string movieId)
        {
            string strDecadeFind = year.Substring(0, 3);
            strDecadeFind = strDecadeFind + "0";
            int startYear = int.Parse(strDecadeFind);
            int endYear = startYear + 9;

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<string> movieIdListOfDecade = _context.MovieYear.Where(umw => Convert.ToInt32(umw.Year) >= startYear && Convert.ToInt32(umw.Year) <= endYear && umw.Imdbid != movieId).ToList().Select(m=>m.Imdbid).Distinct().ToList();

            //Get all the movies of that decade 
            List<UserMovie> moviesOfDecade = new List<UserMovie>();
            foreach(string movId in movieIdListOfDecade)
            {
                UserMovie findMovie = _context.UserMovie.Where(um => um.MovieId == movId).FirstOrDefault();
                if(findMovie != null)
                {
                    moviesOfDecade.Add(findMovie);
                }
            }

            // Get all movies that the login user has watched 
            //List<UserMovie> watchedMovies = _context.UserMovie.Where(um => um.UserId == id && um.Watched ==true && um.MovieId != movieId).ToList();
            List<string> watchedMovieIds = _context.UserMovie.Where(um => um.UserId == id && um.Watched == true).Select(i=>i.MovieId).ToList();
            // Among these make a list of movies that fall in the search decade
            List<string> watchedMovieIdsOfDecade = new List<string>();
            foreach(string movieid in watchedMovieIds)
            {
                if(movieIdListOfDecade.Contains(movieid))
                {
                    watchedMovieIdsOfDecade.Add(movieid);
                }
            }

            //Get the movie details of the watched movies
            List<UserMovie> watchedMovies = new List<UserMovie>();
            foreach (string mId in watchedMovieIdsOfDecade)
            {
                UserMovie findWatchedMovie = _context.UserMovie.Where(um => um.MovieId == mId).FirstOrDefault();
                if (findWatchedMovie != null)
                {
                    watchedMovies.Add(findWatchedMovie);
                }
            }
            //List<UserMovie> watchedMovies = _context.UserMovie.Where(um => um.UserId == id && um.Watched == true).ToList();


            //exclude the movies the user has already watched
            List<UserMovie> recommendedMovies = moviesOfDecade.Except(watchedMovies).ToList();
            return View(recommendedMovies);
        }

        public IActionResult GetMoviesByDirector(string name)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<MovieDirector> directorWorks = _context.MovieDirector.Where(m => name.Contains(m.Director)).ToList();
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            List<UserMovie>directorMovies= new List<UserMovie>();
            for(int i=0; i < directorWorks.Count; i++)
            {
                List<UserMovie> addMovies = _context.UserMovie.Where(m => m.MovieId ==directorWorks[i].Imdbid).ToList();
                directorMovies.Add(addMovies[0]);
            }
            List<UserMovie> recommendedMovies = directorMovies.Except(watchedMovies).ToList();
            recommendedMovies.Select(x => x.MovieId).Distinct();
            Dictionary<UserMovie, double> moviesWithAvgRating = new Dictionary<UserMovie, double>();
            for (int dm = 0; dm < recommendedMovies.Count(); dm++)
            {

                double averageRating = GetAverageRating(recommendedMovies[dm]);
                moviesWithAvgRating.Add(recommendedMovies[dm], averageRating);
            }
            List<MovieDirector> directors = new List<MovieDirector>();
            Dictionary<UserMovie, double>directorMovieByHighest=moviesWithAvgRating.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach(UserMovie m in directorMovieByHighest.Keys)
            {
                List<MovieDirector> placeholder = _context.MovieDirector.Where(w => w.Imdbid == m.MovieId).ToList();
                foreach (MovieDirector d in placeholder)
                {
                    directors.Add(d);
                }
            }
            ViewBag.directors = directors;
            return View(directorMovieByHighest);
        }       

        //public async Task<IActionResult> SearchResultByIMDBId(List<string> imdbIdList)
        //{
        //    List<MovieSearch> movies = new List<MovieSearch>();
        //    if(imdbIdList != null && imdbIdList.Count > 0)
        //    {
        //        foreach(string movieId in imdbIdList)
        //        {
        //            APIMovie movie = await _movieDAL.GetMovieInfo($"{movieId}");
        //            if(movie != null)
        //            {
        //                movies.Add(movie);
        //            }
        //        }
        //    }
            
        //    return View("SearchResult", movies);
        //}              

        public double GetAverageRating(UserMovie movie)
        {
            List<int> ratings = new List<int>();
            List<UserMovie> allMovies = _context.UserMovie.Where(u => u.MovieId == movie.MovieId).ToList();
            foreach (UserMovie m in allMovies)
            {
                ratings.Add(m.UserRating);
            }
            double average = ratings.Average();
            double roundedAverage = Math.Round(average, 0);
            return roundedAverage;
            
        }
    }
}
