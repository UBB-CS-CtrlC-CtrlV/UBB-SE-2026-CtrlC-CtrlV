using MovieApp.Models;

namespace MovieApp.ViewModels.EventLists;

public static class EventListTransformer
{
    public static IReadOnlyList<Event> Apply(IEnumerable<Event> events, EventListState state)
    {
        // TODO: Run the shared transformation pipeline in this order:
        // 1. Apply filters
        // 2. Apply search
        // 3. Apply sorting
        throw new NotImplementedException();
    }

    public static IEnumerable<Event> ApplyFilters(IEnumerable<Event> events, EventFilterState filters)
    {
        // TODO: Filter by event type, location, ticket price range, and availability.
        throw new NotImplementedException();
    }

    public static IEnumerable<Event> ApplySearch(IEnumerable<Event> events, string searchText)
    {
        // TODO: Perform a case-insensitive search over event title, description, location, and type.
        throw new NotImplementedException();
    }

    public static IOrderedEnumerable<Event> ApplySorting(IEnumerable<Event> events, EventSortOption sortOption)
    {
        // TODO: Apply the selected sort option and add a stable secondary sort such as Id.
        throw new NotImplementedException();
    }
}
