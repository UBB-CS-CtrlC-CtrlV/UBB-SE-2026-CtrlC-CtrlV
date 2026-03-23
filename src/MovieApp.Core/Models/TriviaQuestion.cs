
namespace MovieApp.Core.Models
{
    public sealed class TriviaQuestion
    {
        public required int Id { get; init; }

        public required string QuestionText { get; init; }

        public required string Category { get; init; }

        public required string OptionA { get; init; }

        public required string OptionB { get; init; }

        public required string OptionC { get; init; }

        public required string OptionD { get; init; }

        public required char CorrectOption { get; init; }

        public int? MovieId { get; init; }

    }
}
