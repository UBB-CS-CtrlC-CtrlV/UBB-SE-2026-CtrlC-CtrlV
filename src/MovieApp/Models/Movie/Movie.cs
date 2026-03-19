using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace MovieApp.Models.Movie.Movie
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }

        public List<Genre> Genres { get; set; } = new();
        public List<Actor> Actors { get; set; } = new();
        public List<Director> Directors { get; set; } = new();

        public double Rating { get; set; }
    }
}
