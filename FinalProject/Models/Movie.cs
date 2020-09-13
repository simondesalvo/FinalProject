using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Movie
    {
        public Movie()
        {
            Genre = new HashSet<Genre>();
            MovieActor = new HashSet<MovieActor>();
            UserMovie = new HashSet<UserMovie>();
        }

        public string Imdbid { get; set; }
        public string DirectorId { get; set; }
        public byte? CriticScore { get; set; }
        public string Plot { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<Genre> Genre { get; set; }
        public virtual ICollection<MovieActor> MovieActor { get; set; }
        public virtual ICollection<UserMovie> UserMovie { get; set; }
    }
}
