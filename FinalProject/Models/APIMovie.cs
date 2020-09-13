using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class APIMovie
    {
        public string id { get; set; }
        public string title { get; set; }
        public string year { get; set; }
        public string length { get; set; }
        public string rating { get; set; }
        public string rating_votes { get; set; }
        public string poster { get; set; }
        public string plot { get; set; }
        public Trailer trailer { get; set; }
        public Cast[] cast { get; set; }
        public string[][] technical_specs { get; set; }
    }

    public class Trailer
    {
        public string id { get; set; }
        public string link { get; set; }
    }

    public class Cast
    {
        public string actor { get; set; }
        public string actor_id { get; set; }
        public string character { get; set; }
    }
}
