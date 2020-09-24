using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class DictionaryVM
    {
        public Dictionary<UserMovie,double> MoviesWithScore { get; set; }        
        public string Actor { get; set; }

        public DictionaryVM(Dictionary<UserMovie, double> dict, string actor)
        {
            MoviesWithScore = dict;
            Actor = actor;
        }
        public DictionaryVM() { }
    }
    
}
