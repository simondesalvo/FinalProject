using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Movie
    {
        public Movie()
        {
            MovieActor = new HashSet<MovieActor>();
        }

        public string Imdbid { get; set; }
        public string DirectorId { get; set; }
        public byte? CriticScore { get; set; }
        public string Plot { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<MovieActor> MovieActor { get; set; }
    }
}
