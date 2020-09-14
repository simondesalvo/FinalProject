using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{

    public class RootObject
    {
        //public Title[] titles { get; set; }
        public object[] names { get; set; }
        public object[] companies { get; set; }
    }

    public class MovieSearch
    {
        public string id { get; set; }
        public string title { get; set; }
        public string image { get; set; }
    }


}
