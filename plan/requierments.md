1. Event browsing

* 1.1 The system shall display a list of movie events when the user opens the home screen.
* 1.2 The system shall group events into sections displayed in horizontally scrollable rows.
* 1.3 The system shall allow the user to select a section by clicking its title.
* 1.4 When a section is selected, the system shall display all events belonging to that section.
* 1.5 The system shall display each event card with at least poster, date, location, and price.
* 1.6 The system shall allow the user to navigate to the My Events section.
* 1.7 The system shall allow the user to navigate to the Event Management table view.

2. Sorting

* 2.1 The system shall allow the user to sort events by price.
* 2.2 The system shall allow the user to sort events by past rating. (CU# Team)
* 2.3 The system shall apply the selected sorting option to the currently displayed list.
* 2.4 The system shall allow only one sorting option to be active at a time.

3. Filtering

* 3.1 The system shall allow the user to filter events by price range.
* 3.2 The system shall allow the user to filter events by type.
* 3.3 The system shall allow the user to filter events by location.
* 3.4 The system shall allow the user to filter events by minimum past rating. (CU# Team)
* 3.5 The system shall combine all active filters when displaying results.
* 3.6 The system shall allow the user to clear all active filters.

4. Searching

* 4.1 The system shall allow the user to search for events by title.
* 4.2 The system shall display events whose title or type matches the search query.
* 4.3 The system shall combine the search query with the currently active sort and filters.

5. Event details

* 5.1 The system shall allow the user to select an event from the event list by clicking its card.
* 5.2 When an event is selected, the system shall display the event details page.
* 5.3 The event details page shall display title, description, past rating, location, date, price, and poster.
* 5.4 The system shall allow the user to mark an event as “Will attend” if the event is free.
* 5.5 The system shall allow the user to purchase a ticket if the event is paid.
* 5.6 The purchasing action shall redirect the user to the Buy/Sell view (VibeCoders Team).
* 5.7 The system shall record the user’s participation after a successful join or purchase.
* 5.8 The system shall prevent the same user from joining the same event more than once.

6. My Events

* 6.1 The system shall display a list of events created by the authenticated user.
* 6.2 The system shall display a list of events joined by the authenticated user.
* 6.3 The system shall allow the user to create an event.
* 6.4 The system shall allow the creator of an event to edit that event.
* 6.5 The system shall allow the creator of an event to delete that event.
* 6.6 The system shall update the displayed lists after create, edit, delete, join, or cancel participation actions.
* 6.7 The system shall allow the user to cancel participation in an event they have joined.

7. Event management (CRUD view)

* 7.1 The system shall provide a paginated table containing minimal information about events.
* 7.2 The system shall allow the user to select an event row from the table.
* 7.3 The system shall display detailed information for the selected event in a separate view or page.
* 7.4 The system shall support create, update, and delete operations for events from this management area.

8. Validation and persistence

* 8.1 The system shall validate required fields when creating or editing an event.
* 8.2 The system shall reject invalid event data such as empty title, empty location, invalid date, or negative price.
* 8.3 The system shall store users, events, and participation data in long-term storage.
* 8.4 The system shall load persisted data when the application starts.
