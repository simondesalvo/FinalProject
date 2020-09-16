using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class UserMovie
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string MovieId { get; set; }
        public bool? Watched { get; set; }
        public string WatchLocation { get; set; }
        public DateTime? WatchYear { get; set; }
<<<<<<< HEAD
        public int? UserRating { get; set; }
=======

        public int UserRating { get; set; }

>>>>>>> 909cb54bc83a7f9a833edf054b38776e7d20fc1b
        public string UserReview { get; set; }
        public int? WatchedTogetherId { get; set; }
        public string Title { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
