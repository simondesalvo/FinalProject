using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class UserMovieAllUsersVM
    {
        public UserMovie userMovie { get; set; }
        public List<AspNetUsers> aspnetUsers { get; set; }

        public string WatchedWithUserNames { get; set; }
    }
}
