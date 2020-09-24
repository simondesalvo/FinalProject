using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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

        [Authorize]
        public IActionResult Recommended()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList(); //list of movies wathced by a user

            string mostViewedGenre = GetMostWatchedGenre();
            List<UserMovie> movieByGenre = new List<UserMovie>();

            List<Genre> allGenres = _context.Genre.Where(g => g.Genre1 == mostViewedGenre).ToList();  //gets every instance of the most wathced genre
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

            Dictionary<UserMovie, double> moviesWithAvgRating = new Dictionary<UserMovie, double>();
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

                    List<Genre> userGenres = allGenres.Where(m => m.Imdbid == watchedMovies[u].MovieId).ToList(); //variable that finds the genre of the wathced movie at u
                    for (int wg = 0; wg < userGenres.Count; wg++) //goes through every genre associated with a single wathced movie
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

        [Authorize]
        public IActionResult GetMoviesOfDecade(string year, string movieId)
        {
            //finds first year if it's a listed range, first three characters
            string strDecadeFind = year.Substring(0, 3);
            //adds zero to first three chracters, making it the start of a decade
            strDecadeFind = strDecadeFind + "0";
            int startYear = int.Parse(strDecadeFind);
            //makes first three chracters and a 9, to the end of the decade
            int endYear = startYear + 9;

            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //everything in our database apart from the title the user was just looking at
            List<string> movieIdListOfDecade = _context.MovieYear.Where(umw => Convert.ToInt32(umw.Year) >= startYear && Convert.ToInt32(umw.Year) <= endYear && umw.Imdbid != movieId).ToList().Select(m => m.Imdbid).Distinct().ToList();
            //Get all the movies of that decade 
            List<UserMovie> moviesOfDecade = new List<UserMovie>();
            foreach (string movId in movieIdListOfDecade)
            {
                UserMovie findMovie = _context.UserMovie.Where(um => um.MovieId == movId).FirstOrDefault();
                if (findMovie != null)
                {
                    moviesOfDecade.Add(findMovie);
                }
            }

            // Get all movies that the login user has watched             
            List<string> watchedMovieIds = _context.UserMovie.Where(um => um.UserId == id && um.Watched == true).Select(i => i.MovieId).ToList();
            // Among these make a list of movies that fall in the search decade
            List<string> watchedMovieIdsOfDecade = new List<string>();
            foreach (string movieid in watchedMovieIds)
            {
                if (movieIdListOfDecade.Contains(movieid))
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

            //exclude the movies the user has already watched
            List<UserMovie> recommendedMovies = moviesOfDecade.Except(watchedMovies).ToList();

            Dictionary<UserMovie,double> viewMovies = GetMovieWithScore(recommendedMovies);

            return View(viewMovies);
        }
        [Authorize]
        public IActionResult GetMoviesByDirector(string name)
        {
            List<DictionaryVM> vmList = new List<DictionaryVM>();
            string[] directors = name.Split(',');
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            foreach (string director in directors)
            {
                List<MovieDirector> directorWorks = _context.MovieDirector.Where(m => director.Contains(m.Director)).ToList();
                List<UserMovie> directorMovies = new List<UserMovie>();
                for (int i = 0; i < directorWorks.Count; i++)
                {
                    List<UserMovie> addMovies = _context.UserMovie.Where(m => m.MovieId == directorWorks[i].Imdbid).ToList();
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
                List<MovieDirector> directorlist = directorWorks.Distinct().ToList();
                Dictionary<UserMovie, double> directorMovieByHighest = moviesWithAvgRating.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                DictionaryVM dictionaries = new DictionaryVM();
                dictionaries.Actor = director;
                dictionaries.MoviesWithScore = directorMovieByHighest;
                vmList.Add(dictionaries);
            }

            return View(vmList);
        }
        [Authorize]
        public IActionResult GetOtherUsersWatchedMovie(string movieId)
        {
            // Get the login user
            string loginUserid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            // Get the user ids who have watched this other than the login user;
            List<UserMovie> othersList = _context.UserMovie.Where(um => um.MovieId == movieId && um.UserId != loginUserid).ToList();

            // create a VM of user name and the usermovie related to the user
            List<UserNameUserMovieVM> unameUMovieList = new List<UserNameUserMovieVM>();
            // for each user id in the list 
            foreach (UserMovie um in othersList)
            {
                // get the user name from AspNetUsers table
                string userNameFromDB = _context.AspNetUsers.Where(au => au.Id == um.UserId).ToList().Select(aum => aum.UserName).FirstOrDefault();

                //extract the first part of the email since the username is stored as the email
                int indexOfAtChar = userNameFromDB.IndexOf('@');
                string userName = userNameFromDB.Substring(0, indexOfAtChar);

                //create a new VM object for each user and add the info
                UserNameUserMovieVM uNameuMovie = new UserNameUserMovieVM();
                uNameuMovie.UserName = userName;
                uNameuMovie.userMovie = um;

                // Add this to the list 
                unameUMovieList.Add(uNameuMovie);
            }

            return View(unameUMovieList);
        }
        
        public double GetAverageRating(UserMovie movie)
        {
            List<int> ratings = new List<int>();
            List<UserMovie> allMovies = _context.UserMovie.Where(u => u.MovieId == movie.MovieId).ToList();
            foreach (UserMovie m in allMovies)
            {
                if (m.UserRating > 0) 
                { 
                 ratings.Add(m.UserRating);       
                }
            }
            if(ratings.Count>0)
            {
            double average = ratings.Average();
            double roundedAverage = Math.Round(average, 0);
            return roundedAverage;
            }
            else
            {
                return 0;
            }

        }

        [Authorize]
        public IActionResult GetMoviesByActor(string name)
        {

            List<DictionaryVM> vmList = new List<DictionaryVM>();
            string[] actors = name.Split(',');
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            foreach (string actor in actors)
            {
                List<MovieActor> actorWorks = _context.MovieActor.Where(m => actor.Contains(m.Actor)).ToList(); //makes sure the instances of actors match the actor we are checking for
                List<UserMovie> actorMovies = new List<UserMovie>();
                for (int i = 0; i < actorWorks.Count; i++)  //goes through all the instances of an actor and adds those movies to a list
                {
                    List<UserMovie> addMovies = _context.UserMovie.Where(m => m.MovieId == actorWorks[i].Imdbid).ToList();
                    actorMovies.Add(addMovies[0]);
                }
                List<UserMovie> recommendedMovies = actorMovies.Except(watchedMovies).ToList();
                recommendedMovies.Select(x => x.MovieId).Distinct();
                Dictionary<UserMovie, double> moviesWithAvgRating = new Dictionary<UserMovie, double>();
                for (int am = 0; am < recommendedMovies.Count(); am++) //goes through every movie in recommendedMovies and finds the average user rating
                {                                                      // and adds that movie and score to a dictionary
                    double averageRating = GetAverageRating(recommendedMovies[am]);
                    moviesWithAvgRating.Add(recommendedMovies[am], averageRating);
                }
                // List<MovieActor> distinctActors = actorWorks.Distinct().ToList();
                Dictionary<UserMovie, double> actorMoviesByHighest = moviesWithAvgRating.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                DictionaryVM dictionaries = new DictionaryVM();
                dictionaries.Actor = actor;
                dictionaries.MoviesWithScore = actorMoviesByHighest;
                vmList.Add(dictionaries);
            }

            return View(vmList);
        }
        [Authorize]
        public IActionResult OurList()
        {
            return View();
        }
        [Authorize]
        public IActionResult SadMovies()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();

            List<UserMovie> allUM = _context.UserMovie.ToList();
            List<UserMovie> moviesNotWatched = allUM.Except(watchedMovies).ToList();
            List<UserMovie> distinctMovies = GetDistinctMovies(moviesNotWatched);

            List<UserMovie> sadMovies = new List<UserMovie>();
            foreach (UserMovie um in distinctMovies)
            {
                List<UserMovie> sameMovies = _context.UserMovie.Where(u => u.MovieId == um.MovieId).ToList(); //creates a list of all the instances of this movie
                List<string> reviews = new List<string>();
                foreach (UserMovie sm in sameMovies)
                {
                    if (sm.UserReview != null)
                    {
                        reviews.Add(sm.UserReview);
                    }
                }
                int sadnessCount = 0;
                foreach (string review in reviews)
                {
                    string[] words = review.ToLower().Split(" ");
                    foreach (string word in words) //goes through all the words in all of the reviews for this movie and  
                    {                              //looks to see how many times the keyword is in the review
                        if (Regex.IsMatch(word, @"\b(sad)\b")|| Regex.IsMatch(word, @"\b(saddest)\b"))
                        {
                            sadnessCount++;
                        }
                    }
                }
                if (sadnessCount > 1)
                {
                    sadMovies.Add(um);
                }
            }
            Dictionary<UserMovie, double> dictonary = GetMovieWithScore(sadMovies);
            return View(dictonary);
        }
        [Authorize]
        public IActionResult FamilyMovies()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();

            List<UserMovie> allUM = _context.UserMovie.ToList();
            List<UserMovie> moviesNotWatched = allUM.Except(watchedMovies).ToList();
            List<UserMovie> distinctMovies = GetDistinctMovies(moviesNotWatched);

            List<UserMovie> famMovies = new List<UserMovie>();
            foreach (UserMovie um in distinctMovies)
            {
                List<UserMovie> sameMovies = _context.UserMovie.Where(u => u.MovieId == um.MovieId).ToList(); //creates a list of all the instances of this movie
                List<string> reviews = new List<string>();
                foreach (UserMovie sm in sameMovies)
                {
                    if (sm.UserReview != null)
                    {
                        reviews.Add(sm.UserReview);
                    }
                }
                int wholesomeCount = 0;
                foreach (string review in reviews)
                {
                    string[] words = review.ToLower().Split(" ");
                    foreach (string word in words) //goes through all the words in all of the reviews for this movie and  
                    {                              //looks to see how many times the keyword is in the review
                        if (Regex.IsMatch(word, @"\b(family)\b"))
                        {
                            wholesomeCount++;
                        }
                    }
                }
                if (wholesomeCount > 1)
                {
                    famMovies.Add(um);
                }
            }
            Dictionary<UserMovie, double> dictonary = GetMovieWithScore(famMovies);
            return View(dictonary);
        }
        [Authorize]
        public IActionResult FunnyMovies()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<UserMovie> watchedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();

            List<UserMovie> allUM = _context.UserMovie.ToList();
            List<UserMovie> moviesNotWatched = allUM.Except(watchedMovies).ToList();
            List<UserMovie> distinctMovies = GetDistinctMovies(moviesNotWatched);

            List<UserMovie> funnyMovies = new List<UserMovie>();
            foreach (UserMovie um in distinctMovies)
            {
                List<UserMovie> sameMovies = _context.UserMovie.Where(u => u.MovieId == um.MovieId).ToList(); //creates a list of all the instances of this movie
                List<string> reviews = new List<string>();
                foreach (UserMovie sm in sameMovies)
                {
                    if (sm.UserReview != null)
                    {
                        reviews.Add(sm.UserReview);
                    }
                }
                int chuckleCount = 0;
                foreach (string review in reviews)
                {
                    string[] words = review.ToLower().Split(" ");
                    foreach (string word in words) //goes through all the words in all of the reviews for this movie and  
                    {                              //looks to see how many times the keyword is in the review
                        if (Regex.IsMatch(word, @"\b(funny)\b")|| Regex.IsMatch(word, @"\b(hilarious)\b")|| Regex.IsMatch(word, @"\b(funniest)\b"))
                        {
                            chuckleCount++;
                        }
                    }
                }
                if (chuckleCount > 1)
                {
                    funnyMovies.Add(um);
                }
            }
            Dictionary<UserMovie, double> dictonary = GetMovieWithScore(funnyMovies);
            return View(dictonary);
        }

        public List<UserMovie> GetDistinctMovies(List<UserMovie> moviesNotWathced)
        {
            List<UserMovie> distinctMovies = new List<UserMovie>();
            distinctMovies.Add(moviesNotWathced[0]);
            foreach (UserMovie m in moviesNotWathced) //gets distinct movie list
            {
                int check = 0;
                foreach (UserMovie u in distinctMovies)
                {
                    if (m.MovieId == u.MovieId)
                    {
                        check++;
                    }
                }
                if (check == 0)
                {
                    distinctMovies.Add(m);
                }
            }
            return distinctMovies;
        }
        public Dictionary<UserMovie, double> GetMovieWithScore(List<UserMovie> movies)
        {
            Dictionary<UserMovie, double> movieWithRating = new Dictionary<UserMovie, double>();

            foreach (UserMovie um in movies)
            {
                if (um.UserRating>0)
                {
                    double average = GetAverageRating(um);
                    movieWithRating.Add(um, average);
                }
            }
            return movieWithRating;
        }

    }
}

