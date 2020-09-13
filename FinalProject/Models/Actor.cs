using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Actor
    {
        public Actor()
        {
            MovieActor = new HashSet<MovieActor>();
        }

        public string ActorId { get; set; }
        public string ActorName { get; set; }

        public virtual ICollection<MovieActor> MovieActor { get; set; }
    }
}
