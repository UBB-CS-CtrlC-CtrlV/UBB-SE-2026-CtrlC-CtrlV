using System;
using System.Collections.Generic;
using System.Text;

namespace MovieApp.Models
{
    internal class Participation
    {
        public required int Id { get; init; }
        public required int UserId { get; init; }
        public required int EventId { get; init; }
        public required string Status { get; set; }
        public DateTime JoinedAt { get; init; }=DateTime.UtcNow;

        // Unique key helper (similar to your User's StableId)
        public string ParticipationKey => $"U{UserId}:E{EventId}";
    }
}
