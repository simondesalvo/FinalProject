using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class MovieActor
    {
        public string Actor { get; set; }
        public string Imdbid { get; set; }
        public int Id { get; set; }

        public virtual Movie Imdb { get; set; }
    }
}
