USE [MovieApp];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TriviaQuestions)
BEGIN
    INSERT INTO dbo.TriviaQuestions (QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption)
    VALUES

    -- Actors
    ('Which actor played Iron Man in the Marvel Cinematic Universe?',
     'Actors', 'Chris Evans', 'Robert Downey Jr.', 'Mark Ruffalo', 'Chris Hemsworth', 'B'),

    ('Who played the Joker in the 2019 film Joker?',
     'Actors', 'Heath Ledger', 'Jack Nicholson', 'Joaquin Phoenix', 'Jared Leto', 'C'),

    ('Which actress starred as Katniss Everdeen in The Hunger Games?',
     'Actors', 'Emma Watson', 'Jennifer Lawrence', 'Shailene Woodley', 'Kristen Stewart', 'B'),

    ('Who played Forrest Gump in the 1994 film?',
     'Actors', 'Tom Hanks', 'Tom Cruise', 'Kevin Costner', 'Denzel Washington', 'A'),

    ('Which actor portrayed Jack Sparrow in Pirates of the Caribbean?',
     'Actors', 'Orlando Bloom', 'Johnny Depp', 'Geoffrey Rush', 'Javier Bardem', 'B'),

    ('Who played the lead role in The Revenant (2015)?',
     'Actors', 'Matt Damon', 'Brad Pitt', 'Leonardo DiCaprio', 'Christian Bale', 'C'),

    ('Which actress won an Oscar for her role in La La Land?',
     'Actors', 'Emma Stone', 'Natalie Portman', 'Amy Adams', 'Cate Blanchett', 'A'),

    ('Who played Wolverine in the X-Men film series?',
     'Actors', 'Liam Neeson', 'Hugh Jackman', 'Russell Crowe', 'Gerard Butler', 'B'),

    ('Which actor starred as Neo in The Matrix?',
     'Actors', 'Will Smith', 'Keanu Reeves', 'Laurence Fishburne', 'Hugo Weaving', 'B'),

    ('Who played Hermione Granger in the Harry Potter series?',
     'Actors', 'Emma Watson', 'Helena Bonham Carter', 'Evanna Lynch', 'Bonnie Wright', 'A'),

    ('Which actor played T Challa in Black Panther?',
     'Actors', 'Idris Elba', 'Michael B. Jordan', 'Chadwick Boseman', 'Lupita Nyongo', 'C'),

    ('Who starred as Maximus in Gladiator (2000)?',
     'Actors', 'Russell Crowe', 'Mel Gibson', 'Brad Pitt', 'Antonio Banderas', 'A'),

    ('Which actress played Clarice Starling in The Silence of the Lambs?',
     'Actors', 'Sigourney Weaver', 'Jodie Foster', 'Meryl Streep', 'Susan Sarandon', 'B'),

    ('Who played the Terminator in the original 1984 film?',
     'Actors', 'Sylvester Stallone', 'Jean-Claude Van Damme', 'Arnold Schwarzenegger', 'Dolph Lundgren', 'C'),

    ('Which actor played Gandalf in The Lord of the Rings trilogy?',
     'Actors', 'Patrick Stewart', 'Ian McKellen', 'Christopher Lee', 'Anthony Hopkins', 'B'),

    ('Who portrayed Steve Rogers in the Marvel Cinematic Universe?',
     'Actors', 'Chris Hemsworth', 'Chris Pratt', 'Chris Evans', 'Jeremy Renner', 'C'),

    ('Which actress played Mia in La La Land?',
     'Actors', 'Natalie Portman', 'Emma Stone', 'Anne Hathaway', 'Scarlett Johansson', 'B'),

    ('Who played Han Solo in the original Star Wars trilogy?',
     'Actors', 'Mark Hamill', 'Harrison Ford', 'Carrie Fisher', 'Billy Dee Williams', 'B'),

    ('Which actor played Tyler Durden in Fight Club?',
     'Actors', 'Edward Norton', 'Brad Pitt', 'Jared Leto', 'Matt Damon', 'B'),

    ('Who starred as Dom Toretto in the Fast and Furious franchise?',
     'Actors', 'Dwayne Johnson', 'Paul Walker', 'Vin Diesel', 'Tyrese Gibson', 'C'),

    -- Directors
    ('Who directed Inception (2010)?',
     'Directors', 'Steven Spielberg', 'Christopher Nolan', 'James Cameron', 'Ridley Scott', 'B'),

    ('Which director is known for the films Pulp Fiction and Kill Bill?',
     'Directors', 'Martin Scorsese', 'Quentin Tarantino', 'David Fincher', 'Coen Brothers', 'B'),

    ('Who directed Titanic (1997)?',
     'Directors', 'Steven Spielberg', 'Ridley Scott', 'James Cameron', 'Ron Howard', 'C'),

    ('Which director made Schindlers List?',
     'Directors', 'Francis Ford Coppola', 'Martin Scorsese', 'Steven Spielberg', 'Stanley Kubrick', 'C'),

    ('Who directed The Godfather (1972)?',
     'Directors', 'Martin Scorsese', 'Francis Ford Coppola', 'Brian De Palma', 'Sidney Lumet', 'B'),

    ('Which director is behind the Toy Story franchise?',
     'Directors', 'Brad Bird', 'Andrew Stanton', 'John Lasseter', 'Pete Docter', 'C'),

    ('Who directed The Dark Knight (2008)?',
     'Directors', 'Zack Snyder', 'Tim Burton', 'Christopher Nolan', 'Joel Schumacher', 'C'),

    ('Which director made Get Out (2017)?',
     'Directors', 'Spike Lee', 'Jordan Peele', 'Ryan Coogler', 'Ava DuVernay', 'B'),

    ('Who directed Parasite (2019)?',
     'Directors', 'Park Chan-wook', 'Wong Kar-wai', 'Bong Joon-ho', 'Kim Jee-woon', 'C'),

    ('Which director is known for the Harry Potter film series (first film)?',
     'Directors', 'Alfonso Cuaron', 'Mike Newell', 'David Yates', 'Chris Columbus', 'D'),

    ('Who directed Avatar (2009)?',
     'Directors', 'Steven Spielberg', 'Peter Jackson', 'James Cameron', 'Michael Bay', 'C'),

    ('Which director made The Silence of the Lambs?',
     'Directors', 'David Fincher', 'Jonathan Demme', 'Ridley Scott', 'Brian De Palma', 'B'),

    ('Who directed Interstellar (2014)?',
     'Directors', 'Denis Villeneuve', 'Christopher Nolan', 'Alfonso Cuaron', 'Ridley Scott', 'B'),

    ('Which director is known for making Psycho (1960)?',
     'Directors', 'Stanley Kubrick', 'Orson Welles', 'Alfred Hitchcock', 'Billy Wilder', 'C'),

    ('Who directed The Lord of the Rings trilogy?',
     'Directors', 'Guillermo del Toro', 'Ridley Scott', 'Peter Jackson', 'Sam Raimi', 'C'),

    ('Which director made Whiplash (2014)?',
     'Directors', 'Damien Chazelle', 'Derek Cianfrance', 'Bennett Miller', 'Tom McCarthy', 'A'),

    ('Who directed Blade Runner (1982)?',
     'Directors', 'James Cameron', 'Ridley Scott', 'Steven Spielberg', 'Terry Gilliam', 'B'),

    ('Which director made The Grand Budapest Hotel?',
     'Directors', 'Michel Gondry', 'Spike Jonze', 'Wes Anderson', 'Sofia Coppola', 'C'),

    ('Who directed Goodfellas (1990)?',
     'Directors', 'Francis Ford Coppola', 'Brian De Palma', 'Martin Scorsese', 'Michael Mann', 'C'),

    ('Which director made Mad Max: Fury Road (2015)?',
     'Directors', 'Ridley Scott', 'George Miller', 'James Cameron', 'Zack Snyder', 'B'),

    -- Movie Quotes
    ('Which film contains the quote: "Here is looking at you, kid"?',
     'Movie Quotes', 'Gone with the Wind', 'Casablanca', 'Sunset Boulevard', 'Rebecca', 'B'),

    ('In which movie does a character say "I am your father"?',
     'Movie Quotes', 'Star Wars: A New Hope', 'Star Wars: Return of the Jedi', 'Star Wars: The Empire Strikes Back', 'Star Wars: Revenge of the Sith', 'C'),

    ('Which film features the line: "You cannot handle the truth"?',
     'Movie Quotes', 'The Firm', 'A Few Good Men', 'Philadelphia', 'JFK', 'B'),

    ('In which movie does someone say "Why so serious?"?',
     'Movie Quotes', 'Batman Begins', 'Batman v Superman', 'The Dark Knight', 'Joker', 'C'),

    ('Which film contains the quote: "To infinity and beyond"?',
     'Movie Quotes', 'Interstellar', 'Toy Story', 'The Hitchhikers Guide to the Galaxy', 'Wall-E', 'B'),

    ('In which movie is the line "Life is like a box of chocolates" spoken?',
     'Movie Quotes', 'Big', 'Cast Away', 'Forrest Gump', 'Philadelphia', 'C'),

    ('Which film features the quote: "I see dead people"?',
     'Movie Quotes', 'Poltergeist', 'The Others', 'The Sixth Sense', 'Insidious', 'C'),

    ('In which movie does a character say "Just keep swimming"?',
     'Movie Quotes', 'Shark Tale', 'Finding Nemo', 'The Little Mermaid', 'Moana', 'B'),

    ('Which film contains the line: "You is kind, you is smart, you is important"?',
     'Movie Quotes', 'Precious', 'Selma', 'The Help', 'Hidden Figures', 'C'),

    ('In which movie is the quote "Why so serious?" first said?',
     'Movie Quotes', 'Batman Forever', 'Batman and Robin', 'The Dark Knight', 'Batman Begins', 'C'),

    ('Which film features: "Get busy living or get busy dying"?',
     'Movie Quotes', 'The Green Mile', 'The Shawshank Redemption', 'Cool Hand Luke', 'Papillon', 'B'),

    ('In which movie does someone say "I feel the need, the need for speed"?',
     'Movie Quotes', 'Days of Thunder', 'Top Gun', 'Talladega Nights', 'Rush', 'B'),

    ('Which film contains: "E.T. phone home"?',
     'Movie Quotes', 'Close Encounters of the Third Kind', 'Contact', 'E.T. the Extra-Terrestrial', 'Alien', 'C'),

    ('In which movie is "You talking to me?" famously said?',
     'Movie Quotes', 'Raging Bull', 'Mean Streets', 'Taxi Driver', 'GoodFellas', 'C'),

    ('Which film features the quote: "Hasta la vista, baby"?',
     'Movie Quotes', 'The Terminator', 'Predator', 'Terminator 2: Judgment Day', 'Total Recall', 'C'),

    ('In which movie does a character say "My precious"?',
     'Movie Quotes', 'The Hobbit', 'The Lord of the Rings: The Fellowship of the Ring', 'The Lord of the Rings: The Two Towers', 'The Lord of the Rings: The Return of the King', 'C'),

    ('Which film contains: "With great power comes great responsibility"?',
     'Movie Quotes', 'Batman Begins', 'Spider-Man', 'Iron Man', 'Superman', 'B'),

    ('In which movie is "I am Groot" said?',
     'Movie Quotes', 'Thor', 'Avengers: Infinity War', 'Guardians of the Galaxy', 'Avengers: Endgame', 'C'),

    ('Which film features: "You had me at hello"?',
     'Movie Quotes', 'Pretty Woman', 'Notting Hill', 'Jerry Maguire', 'Sleepless in Seattle', 'C'),

    ('In which movie does someone say "They may take our lives, but they will never take our freedom"?',
     'Movie Quotes', 'Rob Roy', 'The Patriot', 'Braveheart', 'Gladiator', 'C'),

    -- Oscars and Awards
    ('Which film won the first Academy Award for Best Picture?',
     'Oscars and Awards', 'It Happened One Night', 'Wings', 'All Quiet on the Western Front', 'Cimarron', 'B'),

    ('How many Oscars did Titanic (1997) win?',
     'Oscars and Awards', '9', '11', '14', '7', 'B'),

    ('Which actress has won the most Academy Awards for acting?',
     'Oscars and Awards', 'Meryl Streep', 'Katharine Hepburn', 'Bette Davis', 'Ingrid Bergman', 'B'),

    ('Which film won Best Picture at the 2020 Academy Awards?',
     'Oscars and Awards', 'Once Upon a Time in Hollywood', '1917', 'Joker', 'Parasite', 'D'),

    ('Who was the first African American to win the Best Actor Oscar?',
     'Oscars and Awards', 'Denzel Washington', 'Sidney Poitier', 'Jamie Foxx', 'Forest Whitaker', 'B'),

    ('Which film holds the record for most Oscar wins?',
     'Oscars and Awards', 'Titanic', 'Ben-Hur', 'Lord of the Rings: Return of the King', 'All three are tied', 'D'),

    ('Who won Best Director for The Silence of the Lambs?',
     'Oscars and Awards', 'David Fincher', 'Jonathan Demme', 'Ridley Scott', 'Martin Scorsese', 'B'),

    ('Which animated film won the first Oscar for Best Animated Feature?',
     'Oscars and Awards', 'Monsters Inc', 'Shrek', 'Ice Age', 'Jimmy Neutron', 'B'),

    ('Who won Best Actress for her role in Monster (2003)?',
     'Oscars and Awards', 'Naomi Watts', 'Nicole Kidman', 'Charlize Theron', 'Hilary Swank', 'C'),

    ('Which film won Best Picture at the 2017 Academy Awards after a mix-up?',
     'Oscars and Awards', 'La La Land', 'Moonlight', 'Manchester by the Sea', 'Hidden Figures', 'B'),

    ('How many times has Meryl Streep been nominated for an Academy Award?',
     'Oscars and Awards', '17', '21', '25', '14', 'B'),

    ('Which director won their first Oscar for The Departed (2006)?',
     'Oscars and Awards', 'Francis Ford Coppola', 'Martin Scorsese', 'Clint Eastwood', 'Ridley Scott', 'B'),

    ('Which film won Best Picture at the 92nd Academy Awards?',
     'Oscars and Awards', '1917', 'Ford v Ferrari', 'Parasite', 'Joker', 'C'),

    ('Who won Best Actor for playing Ray Charles in Ray (2004)?',
     'Oscars and Awards', 'Will Smith', 'Denzel Washington', 'Jamie Foxx', 'Don Cheadle', 'C'),

    ('Which country does Parasite represent?',
     'Oscars and Awards', 'Japan', 'China', 'South Korea', 'Taiwan', 'C'),

    ('Who won Best Supporting Actor for Pulp Fiction?',
     'Oscars and Awards', 'John Travolta', 'Samuel L. Jackson', 'Martin Landau', 'Ed Wood', 'C'),

    ('Which film won Best Picture in 2010?',
     'Oscars and Awards', 'Avatar', 'Inglourious Basterds', 'The Hurt Locker', 'Up in the Air', 'C'),

    ('Who won Best Actress for La La Land?',
     'Oscars and Awards', 'Natalie Portman', 'Cate Blanchett', 'Emma Stone', 'Meryl Streep', 'C'),

    ('Which actor won Best Actor for The Revenant (2015)?',
     'Oscars and Awards', 'Matt Damon', 'Michael Fassbender', 'Leonardo DiCaprio', 'Bryan Cranston', 'C'),

    ('How many Oscar categories did The Lord of the Rings: The Return of the King win?',
     'Oscars and Awards', '9', '11', '13', '7', 'B'),

    -- General Movie Trivia
    ('What year was the first Star Wars film released?',
     'General Movie Trivia', '1975', '1977', '1979', '1980', 'B'),

    ('Which studio produced the Lion King (1994)?',
     'General Movie Trivia', 'Pixar', 'DreamWorks', 'Walt Disney Animation', 'Universal', 'C'),

    ('What is the highest-grossing film of all time (not adjusted for inflation)?',
     'General Movie Trivia', 'Titanic', 'Avengers: Endgame', 'Avatar', 'Star Wars: The Force Awakens', 'C'),

    ('In what city is the film La La Land primarily set?',
     'General Movie Trivia', 'New York', 'Chicago', 'Los Angeles', 'San Francisco', 'C'),

    ('Which film franchise features the character James Bond?',
     'General Movie Trivia', 'Mission Impossible', 'The Bourne Identity', 'Spy', '007', 'D'),

    ('What does CGI stand for in filmmaking?',
     'General Movie Trivia', 'Computer Generated Images', 'Computer Graphic Imagery', 'Computer Generated Imagery', 'Creative Graphic Integration', 'C'),

    ('Which film was the first to use CGI for a main character?',
     'General Movie Trivia', 'The Abyss', 'Terminator 2', 'Jurassic Park', 'The Matrix', 'B'),

    ('What is the name of the fictional African country in Black Panther?',
     'General Movie Trivia', 'Zamunda', 'Genosha', 'Wakanda', 'Narobia', 'C'),

    ('How long is the runtime of Avengers: Endgame?',
     'General Movie Trivia', '2h 40min', '3h 2min', '2h 52min', '3h 15min', 'C'),

    ('Which film features a shark terrorizing a beach town?',
     'General Movie Trivia', 'The Deep', 'Jaws', 'Open Water', 'Deep Blue Sea', 'B'),

    ('In which decade was the Golden Age of Hollywood?',
     'General Movie Trivia', '1920s', '1930s and 1940s', '1950s', '1960s', 'B'),

    ('What is the name of the technology used to make actors look younger in films?',
     'General Movie Trivia', 'Face replacement', 'De-aging', 'Digital rejuvenation', 'Youth rendering', 'B'),

    ('Which film popularized the term "blockbuster"?',
     'General Movie Trivia', 'Star Wars', 'Jaws', 'The Godfather', 'Superman', 'B'),

    ('What was the first feature-length animated film?',
     'General Movie Trivia', 'Bambi', 'Pinocchio', 'Snow White and the Seven Dwarfs', 'Fantasia', 'C'),

    ('Which streaming platform produced Roma (2018)?',
     'General Movie Trivia', 'Amazon Prime', 'Hulu', 'Netflix', 'Apple TV+', 'C'),

    ('What film genre features singing and dancing as part of the story?',
     'General Movie Trivia', 'Opera', 'Musical', 'Fantasy', 'Drama', 'B'),

    ('Which country has the largest film industry by number of films produced?',
     'General Movie Trivia', 'United States', 'China', 'India', 'Japan', 'C'),

    ('What is the term for the list of credits at the end of a film?',
     'General Movie Trivia', 'Epilogue', 'End titles', 'Outro', 'Roll call', 'B'),

    ('Which film features the fictional Overlook Hotel?',
     'General Movie Trivia', 'Psycho', 'The Shining', 'Misery', 'Doctor Sleep', 'B'),

    ('What does the term "mise-en-scene" refer to in film?',
     'General Movie Trivia', 'The film score', 'Everything visible on screen', 'The editing style', 'The screenplay structure', 'B');
END;
GO
