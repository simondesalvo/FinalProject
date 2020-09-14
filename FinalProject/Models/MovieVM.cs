using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class MovieVM
    {
        public MovieSearch movieSearch { get; set; }
        public PopcornMovie popcornMovie { get; set; }
        public Genre genre { get; set; }
        public MovieActor movieActor { get; set; }
        public Movie movie { get; set; }

    }
}
