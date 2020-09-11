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

        public HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://imdb-internet-movie-database-unofficial.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _apikey);
            return client;
        }

        public HttpClient GetDetailClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://movie-database-imdb-alternative.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _apikey);
            return client;
        }

        public async Task<List<MovieSearch>> GetMovie(string title)
        {
            //calls the method that gives the API the general information needed to 
            //receive data from the API 
            var client = GetClient();

            //uses the client (HTTPClient) to receive 
            //data from the API based off of a certain endpoint.
            var response = await client.GetAsync($"/search/{title}"); 
            
            string jasonData = await response.Content.ReadAsStringAsync();
            //install-package Microsoft.AspNet.WebAPI.Client

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

        public async Task<PopcornMovie> GetMovieInfo(string movieId)
        {
            //calls the method that gives the API the general information needed to 
            //receive data from the API 
            var client = GetDetailClient();

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
