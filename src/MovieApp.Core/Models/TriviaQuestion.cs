
namespace MovieApp.Core.Models
{
    /// <summary>
    /// Represents a persisted trivia question together with its answer choices.
    /// </summary>
    public sealed class TriviaQuestion
    {
        /// <summary>
        /// Gets the persistent question identifier.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// Gets the prompt shown to the user.
        /// </summary>
        public required string QuestionText { get; init; }

        /// <summary>
        /// Gets the trivia category the question belongs to.
        /// </summary>
        public required string Category { get; init; }

        /// <summary>
        /// Gets the first answer choice.
        /// </summary>
        public required string OptionA { get; init; }

        /// <summary>
        /// Gets the second answer choice.
        /// </summary>
        public required string OptionB { get; init; }

        /// <summary>
        /// Gets the third answer choice.
        /// </summary>
        public required string OptionC { get; init; }

        /// <summary>
        /// Gets the fourth answer choice.
        /// </summary>
        public required string OptionD { get; init; }

        /// <summary>
        /// Gets the option key of the correct answer.
        /// </summary>
        public required char CorrectOption { get; init; }

        /// <summary>
        /// Gets the related movie identifier when the question belongs to a movie-specific pool.
        /// </summary>
        public int? MovieId { get; init; }

    }
}
