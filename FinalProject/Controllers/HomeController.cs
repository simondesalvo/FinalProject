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
            PopcornMovie movie1 = await _movieDAL.SecondGetMovieInfo($"{id}");
            APIMovie movie2 = await _movieDAL.GetMovieInfo($"{id}");
            APIPopVM selection = new APIPopVM();
            selection.apiMovie = movie2;
            selection.popMovie = movie1;
            return View(selection);

        }
        
        //display movie list
        [Authorize]
        public IActionResult DisplayList()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<UserMovie> savedMovies = _context.UserMovie.Where(x => x.UserId == id).ToList();
            List<UserMovie> userList = savedMovies.OrderBy(m => m.Title).ToList();
            
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
                //deleting a movie from the user's list
                movie.UserId = userId;
                movie = _context.UserMovie.Where(x => x.Id == id).First();
                _context.UserMovie.Remove(movie);
                _context.SaveChanges();


                //deleting the movie from the genre table if another user doesn't have it in their list
                //prevents errors in our recommendation algorithm
                List<string> distinctGenres = new List<string>();
                distinctGenres = _context.Genre.ToList().Select(g => g.Genre1).Distinct().ToList();
                
                List<string> userIdList = new List<string>();
                userIdList=_context.UserMovie.Where(um => um.MovieId == movie.MovieId).Select(p => p.UserId).Distinct().ToList();

                if (userIdList.Count() == 0)
                {
                    List<Genre> genreList = _context.Genre.Where(x => x.Imdbid == movie.MovieId).ToList();
                    foreach (Genre g in genreList)
                    {
                        _context.Genre.Remove(g);
                        _context.SaveChanges();
                    }
                }

                //deleting the director from director table if another user doesn't have that movie in their list
                //prevents errors in our recommendation algorithm
                List<string> distinctDirectors = new List<string>();
                distinctDirectors = _context.MovieDirector.ToList().Select(d => d.Director).Distinct().ToList();
                userIdList = _context.UserMovie.Where(um => um.MovieId == movie.MovieId).Select(p => p.UserId).Distinct().ToList();
                if (userIdList.Count() == 0)
                {
                    List<MovieDirector> directorList = _context.MovieDirector.Where(x => x.Imdbid == movie.MovieId).ToList();
                    foreach (MovieDirector d in directorList)
                    {
                        _context.MovieDirector.Remove(d);
                        _context.SaveChanges();
                    }
                }

                //deleting the actors from actor table if another user doesn't have that movie in their list
                //prevents errors in our recommendation algorithm
                List<string> distinctActors = new List<string>();
                distinctActors = _context.MovieActor.ToList().Select(a => a.Actor).Distinct().ToList();
                userIdList = _context.UserMovie.Where(um => um.MovieId == movie.MovieId).Select(p => p.UserId).Distinct().ToList();
                if (userIdList.Count() == 0)
                {
                    List<MovieActor> actorList = _context.MovieActor.Where(x => x.Imdbid == movie.MovieId).ToList();
                    foreach (MovieActor a in actorList)
                    {
                        _context.MovieActor.Remove(a);
                        _context.SaveChanges();
                    }
                }

                //deleting the year from year table if another user doesn't have that movie in their list
                //prevents errors in our recommendation algorithm
                List<string> distinctYear = new List<string>();
                distinctYear = _context.MovieYear.ToList().Select(y => y.Year).Distinct().ToList();
                userIdList = _context.UserMovie.Where(um => um.MovieId == movie.MovieId).Select(p => p.UserId).Distinct().ToList();
                if (userIdList.Count() == 0)
                {
                    List<MovieYear> yearList = _context.MovieYear.Where(x => x.Imdbid == movie.MovieId).ToList();
                    foreach (MovieYear y in yearList)
                    {
                        _context.MovieYear.Remove(y);
                        _context.SaveChanges();
                    }
                }
                return RedirectToAction("DisplayList");
            }
            catch
            {
                return RedirectToAction("DisplayList");
            }
        }

        [Authorize]
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
            userMovie.UserRating = 0;
            
            //Add the new object to the table 
            if(ModelState.IsValid)
            {
                UserMovie userMovieExisting = _context.UserMovie.Where(um=>um.MovieId == id && um.UserId==loginUserId).FirstOrDefault();

                if (userMovieExisting == null)
                {
                    TempData["MovieExistsCheck"] = "Successfully added!";
                    _context.UserMovie.Add(userMovie);
                    _context.SaveChanges();
                    AddtoGenre(selectedMovie);
                    AddToMovieActor(id, selectedMovie.Actors);
                    AddToMovieDirector(id, selectedMovie.Director);
                    AddToMovieYear(id, selectedMovie.Year);
                }
                else
                {
                    TempData["MovieExistsCheck"] = "Movie already exists in your list!";
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

        //add info to the MovieActor table
        public bool AddToMovieActor(string imdbId, string actors)
        {
            try
            {
                MovieActor movieActorExisting = _context.MovieActor.Where(ma => ma.Imdbid == imdbId).FirstOrDefault();
                if (movieActorExisting == null)
                {
                    string[] actorArray = SplitString(actors);
                    for (int i = 0; i < actorArray.Length; i++)
                    {
                        // Create a new movieactor object and fill in the details
                        MovieActor newEntry = new MovieActor();
                        newEntry.Imdbid = imdbId;
                        newEntry.Actor = actorArray[i].Trim();

                        if (ModelState.IsValid)
                        {
                            //Add the new MovieActor to the table 
                            _context.MovieActor.Add(newEntry);
                            _context.SaveChanges();
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception )
            {
                return false;
            }
        }

        public bool AddToMovieDirector(string imdbId, string directors)
        {
            try
            {
                MovieDirector movieDirectorExisting = _context.MovieDirector.Where(md => md.Imdbid == imdbId).FirstOrDefault();
                if (movieDirectorExisting == null)
                {
                    string[] directorArray = SplitString(directors);
                    for (int i = 0; i < directorArray.Length; i++)
                    {
                        // Create a new MovieDirector object and fill in the details
                        MovieDirector newEntry = new MovieDirector();
                        newEntry.Imdbid = imdbId;
                        newEntry.Director = directorArray[i].Trim();

                        if (ModelState.IsValid)
                        {
                            //Add the new MovieDirector to the table 
                            _context.MovieDirector.Add(newEntry);
                            _context.SaveChanges();
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddToMovieYear(string imdbId, string Year)
        {
            try
            {
                MovieYear movieYearExisting = _context.MovieYear.Where(md => md.Imdbid == imdbId).FirstOrDefault();
                if (movieYearExisting == null)
                {
                    // Create a new MovieYear object and fill in the details
                    MovieYear newEntry = new MovieYear();
                    newEntry.Imdbid = imdbId;
                    newEntry.Year = Year;

                    if (ModelState.IsValid)
                    {
                        //Add the new MovieYear to the table 
                        _context.MovieYear.Add(newEntry);
                        _context.SaveChanges();
                    }                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string[] SplitString(string strInput)
        {
            string[] splitArray = strInput.Split(",");
            return splitArray;
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
                // Get a list of all users in this application
                List<AspNetUsers> allUsers = _context.AspNetUsers.ToList();

                // Create an object of the VM
                UserMovieAllUsersVM userMovieAllUsersVM = new UserMovieAllUsersVM();
                userMovieAllUsersVM.userMovie = movie;
                userMovieAllUsersVM.aspnetUsers = allUsers;
                userMovieAllUsersVM.WatchedWithUserNames = movie.WatchedTogetherId;
                

                //return View(movie);
                return View(userMovieAllUsersVM);
            }
        }
        //update movie in list
        public IActionResult UpdateWatchedList(UserMovie userMovieInfo, List<string> WatchedTogetherUserNames)
        {
            try
            {
                UserMovie userMovieToUpdate = _context.UserMovie.Find(userMovieInfo.Id);
                userMovieToUpdate.Watched = userMovieInfo.Watched;
                userMovieToUpdate.WatchLocation = userMovieInfo.WatchLocation;
                userMovieToUpdate.WatchYear = userMovieInfo.WatchYear;
                userMovieToUpdate.UserRating = userMovieInfo.UserRating;
                userMovieToUpdate.UserReview = userMovieInfo.UserReview;

                // build all the watched together ids as a string
                if(WatchedTogetherUserNames.Count > 0)
                {
                    string userNames = "";
                    foreach(string userName in WatchedTogetherUserNames)
                    {
                        if(string.IsNullOrEmpty(userNames))
                        {
                            userNames = userName;
                        }
                        else
                        {
                            userNames = userNames + "," + userName;
                        }
                    }
                    userMovieToUpdate.WatchedTogetherId = userNames;
                }
                
                if (ModelState.IsValid)
                {
                    _context.Entry(userMovieToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.UserMovie.Update(userMovieToUpdate);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {

            }

            return RedirectToAction("DisplayList");
        }

        // Returns a list of unique Genres stored in the db
        public List<string> GetDistinctGenres()
        {
            List<string> distinctGenres = new List<string>();            
            distinctGenres = _context.Genre.ToList().Select(p => p.Genre1).Distinct().ToList();

            return distinctGenres;
        }

        public IActionResult UpdateUserRating(int id, int userRating)
        {
            try
            {
                UserMovie userMovieToUpdate = _context.UserMovie.Find(id);
                userMovieToUpdate.UserRating = userRating;
                if (ModelState.IsValid)
                {
                    _context.Entry(userMovieToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.UserMovie.Update(userMovieToUpdate);
                    _context.SaveChanges();
                }
            }
            catch(Exception)
            {

            }
            return RedirectToAction("DisplayList");
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
