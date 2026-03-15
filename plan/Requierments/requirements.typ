#set page(
  paper: "a4",
  margin: (top: 14mm, bottom: 14mm, x: 14mm),
  numbering: "1",
)

#set document(title: "Movie Events Application Requirements")
#set text(font: "Liberation Serif", size: 10pt)
#set par(justify: true, leading: 0.62em)
#set heading(numbering: "1.1.")
#show heading.where(level: 1): set text(size: 18pt, weight: "bold")
#show heading.where(level: 2): set text(size: 13pt, weight: "bold")
#show heading.where(level: 3): set text(size: 11pt, weight: "semibold")

#let req-table(columnspec, rows) = {
  table(
    columns: columnspec,
    stroke: (x, y) => if y == 1 { 0.8pt } else { 0.45pt },
    inset: 4.5pt,
    align: left,
    table.header(..rows.at(0)),
    ..rows.slice(1).flatten(),
  )
}

#let spacer() = v(0.55em)

#title()

= Overview

The application allows a single dummy authenticated test user to browse, search, sort, filter, view, join, create, update, and delete movie events. It also includes supporting features for favorites, favorite-event notifications, price watching, best seat guidance, referral codes, movie marathons, a movie slot machine, and a movie trivia wheel.


There are no login, registration, or profile screens/views. Authentication is represented by a dummy test user that already exists in the application context.


= User and Actor Model

#req-table((22%, 78%), (
  ([Actor], [Description]),
  ([Dummy Authenticated User], [The application shall operate for a single authenticated dummy test user.])
))
#spacer()

The application shall operate for a single authenticated dummy test user.


= Architectural and Consistency Constraints

== Separation of concerns

#req-table((16%, 84%), (
  ([ID], [Requirement]),
  ([ARCH-1], [Domain models shall define application data and relationships.]),
  ([ARCH-2], [Repositories shall handle data access and persistence operations.]),
  ([ARCH-3], [Services shall implement business rules and feature workflows.]),
  ([ARCH-4], [Views/ViewModels or equivalent presentation components shall handle UI behavior and user interaction.])
))
#spacer()

== Consistency

#req-table((16%, 84%), (
  ([ID], [Requirement]),
  ([CONS-1], [The same event data definitions shall be reused across browsing, details, management, favorites, marathons, and game features where applicable.]),
  ([CONS-2], [Search, sort, and filter behavior shall be consistent across all relevant event lists.])
))
#spacer()

= Domain and Data Requirements

The following domain entities are required to support the application.


== User

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-1-1], [A dummy authenticated test user shall exist in persistent storage.], [Rares]),
  ([DR-1-2], [The user shall be identifiable by a unique user identifier.], [Rares]),
  ([DR-1-3], [The user shall own personal data such as joined events, created events, favorites, notifications, rewards, referral code, spin state, and marathon progress.], [Tudor])
))
#spacer()

#pagebreak()

== Event

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-2-1], [An Event entity shall exist for every movie event shown in the application.], [Rares]),
  ([DR-2-2], [An event shall support at least title, description, poster, date/time, location reference, ticket price, historical event rating, event type, seat capacity information, and creator reference.], [Rares]),
  ([DR-2-3], [An event shall support participation records for joined users.], [Emanuel]),
  ([DR-2-4], [An event shall support one or more movie screenings.], [Emanuel])
))
#spacer()

== Location

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-3-1], [A Location entity shall exist for venues where events are held.], [Emanuel]),
  ([DR-3-2], [A location shall support enough information to identify and display the venue in event lists and event details.], [Emanuel]),
  ([DR-3-3], [A location may have one or more hall layouts.], [Emanuel])
))
#spacer()

== Hall Layout

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-4-1], [A HallLayout entity shall exist to support the Best Seat Map feature as a proper seat-based feature.], [Emanuel]),
  ([DR-4-2], [A hall layout shall support rows, columns, seat identifiers, and seat availability state.], [Emanuel]),
  ([DR-4-3], [A hall layout shall be associated with a location or specific hall within a location.], [Emanuel])
))
#spacer()

== Participation

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-5-1], [A Participation entity or equivalent relation shall link a user to an event they joined or purchased.], [Tudor]),
  ([DR-5-2], [Participation shall prevent the same user from joining the same event more than once.], [Tudor])
))
#spacer()

== Favorite Event

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-6-1], [A FavoriteEvent entity or equivalent relation shall link a user to an event saved as favorite.], [Filip]),
  ([DR-6-2], [Favorite events shall be stored independently for each user.], [Filip])
))
#spacer()

== Notification

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-7-1], [A Notification entity shall support favorite-event notifications.], [Filip]),
  ([DR-7-2], [A notification shall store at least user reference, event reference, notification type, short message, creation date, and read/view state if implemented.], [Filip]),
  ([DR-7-3], [Notification data for favorites shall be persisted.], [Filip])
))
#spacer()

== Price Watch Entry

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-8-1], [A PriceWatchEntry structure shall exist in local persistent client storage.], [Emanuel]),
  ([DR-8-2], [A price watch entry shall store at least user context, event reference, target price, and current watch state.], [Emanuel])
))
#spacer()

== Referral Code

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-9-1], [A ReferralCode entity or equivalent user-linked structure shall store a unique permanent code for each user.], [Tudor]),
  ([DR-9-2], [The code shall support tracking of successful uses per event.], [Tudor])
))
#spacer()

== Referral Interaction

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-10-1], [A ReferralInteraction entity shall log each successful referral code use.], [Tudor]),
  ([DR-10-2], [It shall store at least ambassador user reference, referred user reference, event reference, and timestamp.], [Tudor])
))
#spacer()

== Reward

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-11-1], [A Reward entity shall represent earned benefits such as free enrollment, free movie ticket, and slot-machine discount.], [Tudor]),
  ([DR-11-2], [A reward shall store at least reward type, owner user reference, redemption status, applicability scope, and any relevant event or movie linkage.], [Tudor]),
  ([DR-11-3], [Rewards may stack, but the final ticket price shall never become negative.], [Tudor])
))
#spacer()

== Movie

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-12-1], [A Movie entity shall exist for movie-related features.], [Andrei]),
  ([DR-12-2], [A movie shall support enough metadata to power event displays and movie-related features, including at least title, genre, actor relationships, director relationship, and movie rating where applicable.], [Andrei])
))
#spacer()

== Genre

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-13-1], [A Genre entity shall exist for movie categorization and slot machine selection.], [Andrei])
))
#spacer()

== Actor

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-14-1], [An Actor entity shall exist for slot machine selection.], [Andrei])
))
#spacer()

== Director

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-15-1], [A Director entity shall exist for slot machine selection.], [Andrei])
))
#spacer()

== Trivia Question

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-16-1], [A TriviaQuestion entity shall exist for trivia wheel and marathon verification.], [Andreea]),
  ([DR-16-2], [A trivia question shall store question text, category, answer options, correct answer identifier, and movie linkage where required.], [Andreea])
))
#spacer()

== Marathon

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-17-1], [A Marathon entity shall exist for the Movie Marathons feature area.], [Maria]),
  ([DR-17-2], [A marathon shall support week scoping, theme, challenge tier, prerequisite relation if applicable, and leaderboard context.], [Maria])
))
#spacer()

== Marathon Progress

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-18-1], [A MarathonProgress entity shall track a user’s verified movie progress inside a marathon.], [Maria]),
  ([DR-18-2], [It shall support completion count, trivia accuracy, and completion timestamping needed for ranking.], [Maria])
))
#spacer()

== Spin Tracking

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([DR-19-1], [A user-linked spin tracking structure shall exist for Movie Slot Machine and Movie Trivia Wheel.], [Andreea]),
  ([DR-19-2], [It shall store remaining spins or last spin timestamps and any streak-related state required by each feature.], [Andreea])
))
#spacer()

= Functional Requirements

== Home list and sections

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-EB-1], [The system shall display a list of movie events when the user opens the home screen.], [Filip]),
  ([FR-EB-2], [The system shall group events into sections displayed in horizontally scrollable rows.], [Filip]),
  ([FR-EB-3], [The system shall allow the user to select a section by clicking its title.], [Filip]),
  ([FR-EB-4], [When a section is selected, the system shall display all events belonging to that section.], [Filip]),
  ([FR-EB-5], [The system shall display each event card with at least poster, date, location, and price.], [Filip]),
  ([FR-EB-6], [The system shall allow the user to navigate to the My Events section.], [Filip]),
  ([FR-EB-7], [The system shall allow the user to navigate to the Event Management table view.], [Filip])
))
#spacer()

== Search

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SEARCH-1], [The system shall allow the user to search for events by title.], [Rares]),
  ([FR-SEARCH-2], [The system shall display events whose title or type matches the search query.], [Rares]),
  ([FR-SEARCH-3], [The system shall combine the search query with the currently active sort and filters.], [Rares]),
  ([FR-SEARCH-4], [The system shall provide search on every event list where it is relevant, including the home event list, section event list, My Events lists, My Favorites, and the Event Management table view.], [Rares]),
  ([FR-SEARCH-5], [Search results shall update the currently displayed list without affecting unrelated event lists elsewhere in the application.], [Rares])
))
#spacer()

#pagebreak()

== Sorting

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SORT-1], [The system shall allow the user to sort events by price.], [Rares]),
  ([FR-SORT-2], [The system shall allow the user to sort events by historical event rating.], [Rares]),
  ([FR-SORT-3], [The system shall apply the selected sorting option to the currently displayed list.], [Rares]),
  ([FR-SORT-4], [The system shall allow only one sorting option to be active at a time per displayed event list.], [Rares]),
  ([FR-SORT-5], [The system shall provide sorting on every event list where it is relevant, including the home event list, section event list, My Events lists, My Favorites, and the Event Management table view.], [Rares])
))
#spacer()

== Filtering

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-FILTER-1], [The system shall allow the user to filter events by price range.], [Rares]),
  ([FR-FILTER-2], [The system shall allow the user to filter events by type.], [Rares]),
  ([FR-FILTER-3], [The system shall allow the user to filter events by location.], [Rares]),
  ([FR-FILTER-4], [The system shall allow the user to filter events by minimum historical event rating.], [Rares]),
  ([FR-FILTER-5], [The system shall combine all active filters when displaying results.], [Rares]),
  ([FR-FILTER-6], [The system shall allow the user to clear all active filters.], [Rares]),
  ([FR-FILTER-7], [The system shall provide filtering on every event list where it is relevant, including the home event list, section event list, My Events lists, My Favorites, and the Event Management table view.], [Rares])
))
#spacer()

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-DETAIL-1], [The system shall allow the user to select an event from an event list by clicking its card.], [Tudor]),
  ([FR-DETAIL-2], [When an event is selected, the system shall display the event details page.], [Tudor]),
  ([FR-DETAIL-3], [The event details page shall display title, description, historical event rating, location, date, price, and poster.], [Tudor]),
  ([FR-DETAIL-4], [The event details page shall display seat availability information if the event uses seat-based capacity tracking.], [Emanuel]),
  ([FR-DETAIL-5], [The system shall allow the user to mark an event as “Will attend” if the event is free.], [Tudor]),
  ([FR-DETAIL-6], [The system shall allow the user to purchase a ticket if the event is paid.], [Tudor]),
  ([FR-DETAIL-7], [The purchasing action shall redirect the user to the Buy/Sell view.], [Tudor]),
  ([FR-DETAIL-8], [The system shall record the user’s participation after a successful join or purchase.], [Tudor]),
  ([FR-DETAIL-9], [The system shall prevent the same user from joining the same event more than once.], [Tudor]),
  ([FR-DETAIL-10], [The system shall allow reward-based discounts and free enrollments to be applied during paid-event checkout where applicable.], [Tudor]),
  ([FR-DETAIL-11], [When multiple rewards or discounts stack, the final calculated ticket price shall not be less than zero.], [Tudor])
))
#spacer()

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-ME-1], [The system shall display a list of events created by the authenticated user.], [Filip]),
  ([FR-ME-2], [The system shall display a list of events joined by the authenticated user.], [Filip]),
  ([FR-ME-3], [The system shall allow the user to create an event.], [Maria]),
  ([FR-ME-4], [The system shall allow the creator of an event to edit that event.], [Maria]),
  ([FR-ME-5], [The system shall allow the creator of an event to delete that event.], [Maria]),
  ([FR-ME-6], [The system shall update the displayed lists after create, edit, delete, join, or cancel participation actions.], [Filip]),
  ([FR-ME-7], [The system shall allow the user to cancel participation in an event they have joined.], [Maria])
))
#spacer()

#pagebreak()

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-CRUD-1], [The system shall provide a paginated table containing minimal information about events.], [Maria]),
  ([FR-CRUD-2], [The system shall allow the user to select an event row from the table.], [Maria]),
  ([FR-CRUD-3], [The system shall display detailed information for the selected event in a separate view or page.], [Maria]),
  ([FR-CRUD-4], [The system shall support create, update, and delete operations for events from this management area.], [Maria])
))
#spacer()

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-VP-1], [The system shall validate required fields when creating or editing an event.], [Maria]),
  ([FR-VP-2], [The system shall reject invalid event data such as empty title, empty location, invalid date, or negative price.], [Maria]),
  ([FR-VP-3], [The system shall store users, events, and participation data in long-term storage.], [Rares]),
  ([FR-VP-4], [The system shall load persisted data when the application starts.], [Rares]),
  ([FR-VP-5], [The system shall persist favorites, favorite-event notifications, referral data, rewards, seat layouts, seat availability, movie data, trivia questions, marathon definitions, and marathon progress in long-term storage.], [Emanuel]),
  ([FR-VP-6], [The system shall use local persistent client storage for price watcher data.], [Emanuel])
))
#spacer()

== Favorite navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-FAV-1], [The system shall provide a Favorite button on the page of every event.], [Filip]),
  ([FR-FAV-2], [When the user selects the Favorite button, the system shall store the selected event in the user’s favorites list in persistent storage.], [Filip]),
  ([FR-FAV-3], [The system shall provide a dedicated My Favorites page where the user can view all events they have marked as favorites.], [Filip]),
  ([FR-FAV-4], [The My Favorites page shall display at least the event title, date, location, and current price for each saved event.], [Filip]),
  ([FR-FAV-5], [The system shall allow the user to remove an event from the favorites list.], [Filip])
))
#spacer()

== Favorite management logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-FAV-6], [The system shall ensure that the same event cannot be added twice to the favorites list for the same user.], [Filip]),
  ([FR-FAV-7], [The system shall store favorite events separately for each user.], [Filip]),
  ([FR-FAV-8], [The system shall only allow events that already exist in persistent storage to be added to the favorites list.], [Filip]),
  ([FR-FAV-9], [The system shall allow the user to view all favorite events at any time through the My Favorites page.], [Filip])
))
#spacer()

#pagebreak()

== Favorite notification logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-FAV-10], [The system shall monitor changes related to the events saved in the user’s favorites list.], [Filip]),
  ([FR-FAV-11], [The system shall notify the user when the price of a favorite event decreases.], [Filip]),
  ([FR-FAV-12], [The system shall notify the user when new seats become available for a favorite event that was previously full.], [Filip]),
  ([FR-FAV-13], [A relevant seat-availability change shall be defined as available seats changing from 0 to a value greater than 0.], [Filip]),
  ([FR-FAV-14], [The system shall create at most one notification for each relevant change, per user and per event state.], [Filip]),
  ([FR-FAV-15], [Each favorite-event notification shall include at least the event title, change type, a short message, and creation date.], [Filip]),
  ([FR-FAV-16], [Favorite-event notifications shall be displayed inside the application interface and shall be stored in persistent storage for later viewing.], [Filip]),
  ([FR-FAV-17], [Favorite-event notifications shall remain separate from Price Watcher alerts.], [Filip])
))
#spacer()

== Navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-PW-1], [Each event card displayed in the main list shall include a bell icon to allow users to toggle price monitoring.], [Emanuel]),
  ([FR-PW-2], [The My Events section shall include a Price Watchlist tab or category.], [Emanuel]),
  ([FR-PW-3], [Upon activating the watcher, the system shall prompt the user to define a target price.], [Emanuel])
))
#spacer()

== Price comparison logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-PW-4], [The system shall compare the current event price with the user-defined target price.], [Emanuel]),
  ([FR-PW-5], [A synchronization check shall occur automatically whenever the application initializes, performing a full refresh of the Price Watchlist by querying persisted event data.], [Emanuel]),
  ([FR-PW-6], [If the current event price is less than or equal to the target price, the system shall apply a PRICE DROP DETECTED banner to the event card.], [Emanuel]),
  ([FR-PW-7], [Price Watcher alerts shall refer only to price changes and shall not replace favorite-event notifications.], [Emanuel])
))
#spacer()

== Constraints

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-PW-8], [The system shall store Price Watcher preferences in local persistent client storage.], [Emanuel]),
  ([FR-PW-9], [The system shall limit the user to a maximum of 3 active price watchers at any given time.], [Emanuel])
))
#spacer()

#pagebreak()

== Navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-BS-1], [The event detail page shall include a Visual Seat Guide button positioned near the event’s location and price information.], [Emanuel]),
  ([FR-BS-2], [Clicking the Visual Seat Guide button shall open a modal interface displaying an interactive seat map for the selected event location or hall.], [Emanuel]),
  ([FR-BS-3], [The seat guide interface shall include a graphical representation of the screen at the top of the grid.], [Emanuel]),
  ([FR-BS-4], [The seat guide interface shall include a seat grid color-coded by seat quality.], [Emanuel]),
  ([FR-BS-5], [The seat guide interface shall include a distinct Sweet Spot marker highlighting the most recommended seats.], [Emanuel])
))
#spacer()

== Seat and layout data

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-BS-6], [The system shall store hall layouts so the Best Seat Map operates as a proper seat-based feature rather than only a generic visualization.], [Emanuel]),
  ([FR-BS-7], [Each hall layout shall define rows, columns, and uniquely identifiable seats.], [Emanuel]),
  ([FR-BS-8], [The system shall associate seat availability with the corresponding event or event screening.], [Emanuel])
))
#spacer()

== Recommendation logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-BS-9], [The system shall evaluate seat quality using hall geometry and screen-relative position.], [Emanuel]),
  ([FR-BS-10], [The system shall identify a Sweet Spot region centered around the optimal viewing and audio area of the hall.], [Emanuel]),
  ([FR-BS-11], [The system shall visually distinguish poor seats, standard seats, and optimal seats.], [Emanuel]),
  ([FR-BS-12], [The system shall visually indicate unavailable seats separately from available seats.], [Emanuel])
))
#spacer()

== General adaptation for this project

Because the application has no profile screen, referral-code information shall be exposed through a dedicated in-application area that is accessible to the dummy authenticated user.

== Navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-REF-1], [The system shall display the user’s unique Ambassador Code in a dedicated in-application referral area.], [Tudor]),
  ([FR-REF-2], [The system shall provide a Referral History dashboard showing who used the code and for which events.], [Tudor]),
  ([FR-REF-3], [The enrollment form shall include an optional text field for entering a Referral Code.], [Tudor])
))
#spacer()

== Referral generation and validation

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-REF-4], [The system shall generate a unique permanent alphanumeric referral code for the user.], [Tudor]),
  ([FR-REF-5], [When a Referral Code is entered during enrollment, the system shall validate that the code exists and belongs to a valid user.], [Tudor]),
  ([FR-REF-6], [The system shall reject referral-code usage if the user entering the code is also the code owner.], [Tudor])
))
#spacer()

== Referral logging and rewards

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-REF-7], [Each successful use of a referral code shall be logged as a ReferralInteraction linked to a specific EventID.], [Tudor]),
  ([FR-REF-8], [Once the system detects 3 successful usages of a code for the same event, the ambassador shall be granted a free enrollment reward for that same event only.], [Tudor]),
  ([FR-REF-9], [The referral reward shall be stored as a redeemable reward and shall be redeemable once.], [Tudor])
))
#spacer()

== Navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SM-1], [The system shall provide a Movie Slot Machine button on the home screen.], [Andrei]),
  ([FR-SM-2], [When the user selects the Movie Slot Machine button, the system shall navigate the user to the Movie Slot Machine page.], [Andrei]),
  ([FR-SM-3], [The Movie Slot Machine page shall contain a slot-machine style interface with three vertical reels.], [Andrei]),
  ([FR-SM-4], [The reels shall represent the categories Genre, Actor, and Director.], [Andrei]),
  ([FR-SM-5], [Each reel shall display one value at a time.], [Andrei]),
  ([FR-SM-6], [Reel values shall be selected from the corresponding persisted movie-related data.], [Andrei]),
  ([FR-SM-7], [The Movie Slot Machine page shall contain a Spin button.], [Andrei]),
  ([FR-SM-8], [Pressing Spin shall initiate a slot-machine spin animation.], [Andrei]),
  ([FR-SM-9], [During the spin animation, each reel shall display multiple values from its category sequentially.], [Andrei]),
  ([FR-SM-10], [Each reel shall stop spinning independently.], [Andrei]),
  ([FR-SM-11], [The reels shall stop in the order Genre, Actor, Director.], [Andrei]),
  ([FR-SM-12], [The final reel values shall represent the generated roulette combination.], [Andrei]),
  ([FR-SM-13], [The system shall allow users to trigger the spin using keyboard input.], [Andrei])
))
#spacer()

== Event matching logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SM-14], [The system shall select reel values using random selection from the corresponding categories.], [Andrei]),
  ([FR-SM-15], [The system shall only select values that are associated with at least one currently available event screening.], [Andrei]),
  ([FR-SM-16], [The system shall ensure that the generated combination produces at least one matching movie event.], [Andrei]),
  ([FR-SM-17], [The system shall retrieve events containing screenings of matching movies.], [Andrei]),
  ([FR-SM-18], [The system shall retrieve only future or active screenings.], [Andrei]),
  ([FR-SM-19], [For each matching event, the system shall retrieve at least movie title, event name, event location, ticket price, and movie rating.], [Andrei]),
  ([FR-SM-20], [After the reels stop, the system shall display a results section below the slot machine.], [Andrei]),
  ([FR-SM-21], [The results section shall display all events that match the generated combination.], [Andrei]),
  ([FR-SM-22], [Each result card shall contain a View Event button.], [Andrei]),
  ([FR-SM-23], [When the user selects View Event, the system shall navigate to the event details page.], [Andrei])
))
#spacer()

#pagebreak()

== Spin economy

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SM-24], [The system shall provide each user with one free Movie Slot Machine spin per day.], [Andrei]),
  ([FR-SM-25], [The system shall reset the user’s daily free spin at 00:00 server time each day.], [Andrei]),
  ([FR-SM-26], [The system shall prevent the user from initiating a spin when the user has zero available spins.], [Andrei]),
  ([FR-SM-27], [The system shall display the number of available spins on the Movie Slot Machine page.], [Andrei]),
  ([FR-SM-28], [The system shall grant the user one additional spin when the user joins an event.], [Andrei]),
  ([FR-SM-29], [The system shall limit bonus spins earned from event participation to a maximum of two spins per user per day.], [Andrei]),
  ([FR-SM-30], [The system shall immediately update the user’s available spin count after a bonus spin is awarded.], [Andrei]),
  ([FR-SM-31], [The system shall track the number of consecutive days the user opens the application.], [Andrei]),
  ([FR-SM-32], [The system shall grant the user one additional spin when the user reaches a three-day consecutive login/open streak.], [Andrei]),
  ([FR-SM-33], [After awarding the streak spin, the system shall reset the streak counter.], [Andrei])
))
#spacer()

== Jackpot and reward logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-SM-34], [The system shall determine whether the generated Genre, Actor, and Director combination corresponds to an existing movie in persistent storage.], [Andrei]),
  ([FR-SM-35], [If a movie exists that matches the generated combination, the system shall trigger a Movie Slot Machine Jackpot.], [Andrei]),
  ([FR-SM-36], [When a jackpot occurs, the system shall grant the user a discount reward applicable to all events containing a screening of the matching movie.], [Andrei]),
  ([FR-SM-37], [The chance of winning a jackpot shall be intentionally small because the jackpot reward is high-value.], [Andrei]),
  ([FR-SM-38], [The jackpot discount percentage shall be configurable.], [Andrei]),
  ([FR-SM-39], [The results section shall highlight events containing the matching movie when a jackpot occurs.], [Andrei]),
  ([FR-SM-40], [Highlighted events shall visually indicate that a discount has been awarded.], [Andrei]),
  ([FR-SM-41], [The discount shall be applied when the user joins or purchases a qualifying matching-movie event.], [Andrei])
))
#spacer()

== Navigation and interface

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-1], [The home page shall contain a Movie Trivia Wheel section with a short description and a Spin the Wheel button.], [Andreea]),
  ([FR-MW-2], [Pressing the Spin button shall open the Trivia Wheel interface.], [Andreea]),
  ([FR-MW-3], [The wheel shall contain at least the categories Actors, Directors, Movie Quotes, Oscars and Awards, and General Movie Trivia.], [Andreea]),
  ([FR-MW-4], [The interface shall include the wheel visualization, a Spin button, the number of remaining spins available, and a short explanation of the reward rules.], [Andreea]),
  ([FR-MW-5], [Pressing the Spin button shall trigger a wheel-spinning animation, after which one category shall be randomly selected and displayed.], [Andreea]),
  ([FR-MW-6], [After a category is selected, the interface shall transition to the Trivia Question screen.], [Andreea])
))
#spacer()

== Trivia flow

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-7], [Each trivia session shall consist of 20 multiple-choice questions belonging to the selected category.], [Andreea]),
  ([FR-MW-8], [Questions shall be displayed one at a time and shall include the question text, four possible answers, and exactly one correct answer.], [Andreea]),
  ([FR-MW-9], [The user shall select one answer before moving to the next question.], [Andreea]),
  ([FR-MW-10], [Immediately after answer submission, the system shall evaluate the answer and update the displayed score counter.], [Andreea]),
  ([FR-MW-11], [Questions shall not be skippable and shall not be revisitable after submission.], [Andreea]),
  ([FR-MW-12], [The trivia session shall end when all questions have been answered or when the user exits the session.], [Andreea]),
  ([FR-MW-13], [At the end of the session, a results screen shall display the total number of correct answers and whether a reward was earned.], [Andreea])
))
#spacer()

== Trivia selection logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-14], [Trivia questions shall be stored in persistent storage together with question text, category, four answer options, and correct answer identifier.], [Andreea]),
  ([FR-MW-15], [When a trivia session starts, the application shall randomly select 20 questions belonging to the chosen category.], [Andreea]),
  ([FR-MW-16], [The same question shall not appear more than once within the same trivia session.], [Andreea]),
  ([FR-MW-17], [The selected questions shall also be randomized in presentation order.], [Andreea])
))
#spacer()

== Hint system

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-18], [Each trivia session shall provide the user with one hint token.], [Andreea]),
  ([FR-MW-19], [The hint shall be activatable during any question using a Hint button near the answer options.], [Andreea]),
  ([FR-MW-20], [When the hint is activated, the application shall remove two incorrect answers from the available options.], [Andreea]),
  ([FR-MW-21], [After the hint is used, the Hint button shall become disabled for the remainder of the session.], [Andreea]),
  ([FR-MW-22], [The interface shall clearly indicate whether the hint has already been used.], [Andreea])
))
#spacer()

== Reward logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-23], [A user shall receive a free movie event ticket reward only if all 20 trivia questions are answered correctly.], [Andreea]),
  ([FR-MW-24], [When a reward is generated, it shall be stored together with reward type and redemption status.], [Andreea]),
  ([FR-MW-25], [The reward shall be applicable to any movie event.], [Andreea]),
  ([FR-MW-26], [The reward shall never expire until redeemed.], [Andreea]),
  ([FR-MW-27], [The reward shall be applicable during checkout when the user selects a movie event.], [Andreea]),
  ([FR-MW-28], [After the reward is used, the system shall mark it as redeemed to prevent reuse.], [Andreea]),
  ([FR-MW-29], [A trivia-wheel reward shall be redeemable once only.], [Andreea])
))
#spacer()

== Spin limitations

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MW-30], [Each user shall be allowed one trivia wheel spin per day.], [Andreea]),
  ([FR-MW-31], [The application shall store the timestamp of the user’s most recent trivia wheel spin.], [Andreea]),
  ([FR-MW-32], [If the user attempts to spin the wheel again before the next daily reset, the spin action shall be blocked.], [Andreea]),
  ([FR-MW-33], [In this case, the interface shall display a message informing the user when the next spin will become available.], [Andreea]),
  ([FR-MW-34], [The daily spin counter shall reset automatically at 00:00 server time.], [Andreea])
))
#spacer()

== Feature area

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MM-1], [The system shall provide Movie Marathons as a separate feature area from normal event browsing.], [Maria]),
  ([FR-MM-2], [The Movie Marathons feature area shall display the currently active weekly marathons.], [Maria])
))
#spacer()

== Weekly sprint model

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MM-3], [The feature shall use a weekly sprint model that resets completely every week.], [Maria]),
  ([FR-MM-4], [Every Monday at 00:00, the previous week’s marathons shall end and a new set of themed marathons shall be released.], [Maria]),
  ([FR-MM-5], [Each week shall contain themed marathon definitions.], [Maria]),
  ([FR-MM-6], [Users shall have until Sunday night to log progress and verify movie views for the active week.], [Maria])
))
#spacer()

== Verification logic

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MM-7], [Clicking Log for a movie shall trigger a Rapid-Fire Trivia Check.], [Maria]),
  ([FR-MM-8], [The system shall pull three multiple-choice questions at random from a movie-specific trivia pool.], [Maria]),
  ([FR-MM-9], [A user shall answer all 3 out of 3 questions correctly for that movie to count toward marathon progress.], [Maria]),
  ([FR-MM-10], [The verification process shall prevent unverified progress from counting toward rankings.], [Maria])
))
#spacer()

== Tiered progression

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MM-11], [Some marathons shall be designated as Elite.], [Maria]),
  ([FR-MM-12], [Elite marathons shall remain locked until the prerequisite Standard marathon is completed within the same week.], [Maria]),
  ([FR-MM-13], [Prerequisite logic shall be configurable per marathon definition.], [Maria])
))
#spacer()

#pagebreak()

== Marathon leaderboards

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([FR-MM-14], [Every marathon shall have its own live leaderboard.], [Maria]),
  ([FR-MM-15], [Rankings shall be isolated to the specific marathon and shall not carry over to other marathons or future weeks.], [Maria]),
  ([FR-MM-16], [Participants shall be ranked first by total number of successfully verified movies within that marathon.], [Maria]),
  ([FR-MM-17], [If completion counts are tied, rank shall be determined by trivia accuracy.], [Maria]),
  ([FR-MM-18], [If completion and accuracy are tied, rank shall be determined by who finished the marathon fastest.], [Maria])
))
#spacer()

= Business Rules

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([BR-1], [The application shall operate with a dummy authenticated test user and no login/registration/profile screens.], [Tudor]),
  ([BR-2], [All features shall be available to the same user role.], [Tudor]),
  ([BR-3], [The same user shall not join the same event more than once.], [Tudor]),
  ([BR-4], [Favorite events shall not contain duplicates for the same user.], [Filip]),
  ([BR-5], [Favorite-event notifications shall be created only for relevant changes and not duplicated for the same user and event state.], [Filip]),
  ([BR-6], [Price Watcher alerts shall track only price thresholds and shall remain separate from favorite-event notifications.], [Emanuel]),
  ([BR-7], [Price Watcher entries shall be limited to 3 active watches at a time.], [Emanuel]),
  ([BR-8], [Referral rewards shall be granted only after 3 successful code uses for the same event.], [Tudor]),
  ([BR-9], [Referral rewards shall apply only to that same event.], [Tudor]),
  ([BR-10], [Slot machine jackpot rewards shall apply to all events containing a screening of the matching movie.], [Andrei]),
  ([BR-11], [Slot machine jackpot win probability shall remain small relative to the reward value.], [Andrei]),
  ([BR-12], [Trivia wheel rewards shall apply to any movie event, never expire before use, and be redeemable only once.], [Andreea]),
  ([BR-13], [Rewards may stack, including combinations that reduce an event price to zero.], [Tudor]),
  ([BR-14], [Stacked rewards shall never reduce a ticket price below zero.], [Tudor]),
  ([BR-15], [Marathon rankings shall reset with each weekly cycle.], [Maria])
))
#spacer()

= Non-Functional Requirements

== Persistence and startup

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([NFR-1], [Persisted data shall be loaded at application startup before dependent features are used.], [Andreea]),
  ([NFR-2], [Persistent data and local client data shall remain consistent enough that the user sees current event state after startup refresh.], [Emanuel])
))
#spacer()

#pagebreak()

== Performance and responsiveness

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([NFR-3], [Event search, sort, and filter operations shall respond fast enough to support normal interactive use on the supported application environment.], [Filip]),
  ([NFR-4], [Application startup refresh shall update dependent features such as favorites and price watcher without causing duplicate state creation.], [Emanuel]),
  ([NFR-5], [Modal and game interfaces shall remain responsive during animations and state updates.], [Maria])
))
#spacer()

== Data integrity

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([NFR-6], [The system shall preserve referential integrity between users, events, participations, favorites, notifications, rewards, and referrals.], [Tudor]),
  ([NFR-7], [Reward redemption state shall be persisted correctly to prevent duplicate use.], [Tudor]),
  ([NFR-8], [Event and seat availability changes shall not produce contradictory UI state across event details, favorites, and best seat map views.], [Emanuel])
))
#spacer()

== Maintainability

#req-table((15%, 69%, 16%), (
  ([ID], [Requirement], [Owner]),
  ([NFR-9], [Domain models, repositories, services, and UI logic shall be implemented with clear separation of concerns.], [Rares]),
  ([NFR-10], [Feature-specific logic shall be encapsulated so new features can be added without tightly coupling unrelated modules.], [Maria]),
  ([NFR-11], [Shared event-list behavior such as search, sort, and filter shall be reusable across relevant views.], [Rares])
))
#spacer()

= Implementation Notes

#req-table((16%, 84%), (
  ([ID], [Constraint]),
  ([NOTE-1], [References in imported requirements to account creation or profile visibility are adapted to this project by exposing the same data through dedicated in-application areas for the dummy authenticated user.]),
  ([NOTE-2], [Best Seat Map shall be implemented as a proper hall-layout and seat-based feature, not only as a virtual approximation.]),
  ([NOTE-3], [Price Watcher shall use implementation-safe local persistent client storage rather than requiring database schema changes.]),
  ([NOTE-4], [Historical event rating is the normalized wording replacing prior “past rating” terminology.]),
  ([NOTE-5], [Search, sort, and filter requirements apply everywhere they are relevant, not only on the home list.]),
  ([NOTE-6], [Movie Marathons are a separate feature area and not just a property of normal events.]),
  ([NOTE-7], [Reward stacking is allowed, but negative final prices are forbidden.])
))
#spacer()

#pagebreak()

= Requirement Coverage Summary by Owner

#req-table((22%, 78%), (
  ([Owner], [Coverage]),
  ([Rares], [Project base setup and core architectural foundation #linebreak() Core user and event model foundation #linebreak() Search requirements #linebreak() Sorting requirements #linebreak() Filtering requirements #linebreak() Persistence startup foundation #linebreak() Shared event-list reuse requirement]),
  ([Emanuel], [Best Seat Map #linebreak() Price Watcher #linebreak() Location and hall-related event support #linebreak() Event persistence consistency related to venue/seat data]),
  ([Filip], [Favorite Events #linebreak() Favorite-event notifications #linebreak() Home browsing flow and event-list presentation #linebreak() My Events list presentation updates]),
  ([Maria], [Movie Marathons #linebreak() Event management table and CRUD area #linebreak() Validation flow for create/update operations]),
  ([Andrei], [Movie Slot Machine #linebreak() Movie domain model]),
  ([Andreea], [Movie Trivia Wheel #linebreak() Shared spin tracking structures #linebreak() Startup loading dependency for spin-based features]),
  ([Tudor], [Referral Code System #linebreak() Reward model and reward business rules #linebreak() Event details/join/purchase flow #linebreak() Participation domain and related integrity rules])
))
#spacer()
