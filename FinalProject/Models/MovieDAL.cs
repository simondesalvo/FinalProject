using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class MovieDAL
    {
        private readonly string _apikey;

        public MovieDAL()
        {
        }

        public MovieDAL(string apiKey)
        {
            _apikey = apiKey;            
        }


        //first API, APIMovie or MovieSearch objs
        public HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://imdb-internet-movie-database-unofficial.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _apikey);
            return client;
        }


        //second API, PopcornMovie obj
        public HttpClient GetSecondClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://movie-database-imdb-alternative.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _apikey);
            return client;
        }

        //calls first API to get list of movies for search results
        public async Task<List<MovieSearch>> GetMovies(string title)
        {
            var client = GetClient();
            //data from the API based off of a certain endpoint.
            var response = await client.GetAsync($"/search/{title}");

            string jasonData = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(jasonData);
            List<JToken> data = json["titles"].ToList();

            MovieSearch movie = new MovieSearch();
            List<MovieSearch> moviesList = new List<MovieSearch>();
            for (int i = 0; i < data.Count; i++)
            {
                movie = JsonConvert.DeserializeObject<MovieSearch>(data[i].ToString());
                moviesList.Add(movie);
            }

            return moviesList;
        }
        
        //method for first API, used to get trailer link
        public async Task<APIMovie> GetMovieInfo(string imdb)
        {
            var client = GetClient();
            var response = await client.GetAsync($"/film/{imdb}");
            string jasonData = await response.Content.ReadAsStringAsync();
            APIMovie movie = JsonConvert.DeserializeObject<APIMovie>(jasonData);
            return movie;
        }


        public async Task<PopcornMovie> SecondGetMovieInfo(string movieId)
        {
            //calls the method that gives the API the general information needed to 
            //receive data from the API 
            var client = GetSecondClient();

            //uses the client (HTTPClient) to receive 
            //data from the API based off of a certain endpoint.
            var response = await client.GetAsync($"/?i={movieId}&r=json");

            string jasonData = await response.Content.ReadAsStringAsync();
            //install-package Microsoft.AspNet.WebAPI.Client

            JObject json = JObject.Parse(jasonData);
            PopcornMovie movie = JsonConvert.DeserializeObject<PopcornMovie>(json.ToString());

            return movie;
        }
    }
}
