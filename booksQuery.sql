USE tempdb
GO

drop table books;
CREATE TABLE books (
    id INT  PRIMARY KEY,       -- Unique identifier for the book
    title VARCHAR(150) NOT NULL,             -- Title of the book
    publisher VARCHAR(100) NOT NULL,         -- Publisher's name
    priceForBorrowing DECIMAL(10, 2) NOT NULL, -- Price for borrowing
    priceForBuying DECIMAL(10, 2) NOT NULL,  -- Price for buying
	priceSaleForBorrowing DECIMAL(10, 2),    -- Sale Price for borrowing
    priceSaleForBuying DECIMAL(10, 2),		 -- Sale Price for buying
    yearOfPublishing INT NOT NULL ,			 -- Valid year range
    coverImagePath VARCHAR(255) NOT NULL,    -- Path/URL to the cover image
    ageLimitation VARCHAR(255) NOT NULL,      -- Age limitation (e.g., 18+, 8+)
    quantityInStock INT DEFAULT 0,           -- Number of books available (default is 0)
    popularity INT NOT NULL,
	dateSale DATE,
	genre VARCHAR(255) NOT NULL
);

drop table IF EXISTS authors;
CREATE TABLE authors (
    authorName VARCHAR(100) NOT NULL,              -- Author's name
	bookId INT NOT NULL, 
	PRIMARY KEY(authorName,bookId)
);

-- Insert data into EBooks table
INSERT INTO books (id, title, publisher, priceForBorrowing, priceForBuying, priceSaleForBorrowing, priceSaleForBuying, yearOfPublishing, coverImagePath, ageLimitation, quantityInStock, popularity, dateSale, genre)
VALUES 
(1, 'The Great Gatsby', 'Scribner', 2.99, 9.99, NULL, NULL, 1925, '/Images/gatsby.jpg', '18+', 10, 9, NULL, 'Classic'),
(2, 'To Kill a Mockingbird', 'J.B. Lippincott & Co.', 1.99, 7.99, NULL, NULL, 1960, '/Images/mockingbird.jpg', '12+', 8, 10, NULL, 'Classic'),
(3, '1984', 'Secker & Warburg', 3.49, 12.99, NULL, NULL, 1949, '/Images/1984.jpg', '18+', 5, 8, NULL, 'Dystopian'),
(4, 'Harry Potter and the Sorcerers Stone', 'Bloomsbury', 2.50, 10.00, NULL, NULL, 1997, '/Images/harrypotter1.jpg', '8+', 20, 10, NULL, 'Fantasy'),
(5, 'The Hobbit', 'George Allen & Unwin', 2.75, 8.50, NULL, NULL, 1937, '/Images/hobbit.jpg', '12+', 15, 9, NULL, 'Fantasy'),
(6, 'Pride and Prejudice', 'T. Egerton', 1.99, 6.99, NULL, NULL, 1813, '/Images/pride.jpg', '12+', 12, 8, NULL, 'Romance'),
(7, 'The Catcher in the Rye', 'Little, Brown and Company', 2.20, 8.20, NULL, NULL, 1951, '/Images/catcher.jpg', '18+', 7, 7, NULL, 'Classic'),
(8, 'The Lord of the Rings', 'George Allen & Unwin', 3.00, 12.00, NULL, NULL, 1954, '/Images/lotr.jpg', '12+', 14, 10, NULL, 'Fantasy'),
(9, 'Animal Farm', 'Secker & Warburg', 1.50, 5.99, NULL, NULL, 1945, '/Images/animalfarm.jpg', '12+', 10, 8, NULL, 'Dystopian'),
(10, 'The Chronicles of Narnia', 'Geoffrey Bles', 2.75, 9.50, NULL, NULL, 1950, '/Images/narnia.jpg', '8+', 20, 9, NULL, 'Fantasy'),
(11, 'The Alchemist', 'HarperOne', 2.50, 8.50, NULL, NULL, 1988, '/Images/alchemist.jpg', '12+', 18, 9, NULL, 'Fiction'),
(12, 'Jane Eyre', 'Smith, Elder & Co.', 1.99, 7.99, NULL, NULL, 1847, '/Images/janeeyre.jpg', '12+', 10, 7, NULL, 'Romance'),
(13, 'Wuthering Heights', 'Thomas Cautley Newby', 2.10, 8.10, NULL, NULL, 1847, '/Images/wuthering.jpg', '18+', 8, 8, NULL, 'Romance'),
(14, 'Brave New World', 'Chatto & Windus', 2.50, 9.50, NULL, NULL, 1932, '/Images/bravenewworld.jpg', '18+', 9, 8, NULL, 'Dystopian'),
(15, 'The Fault in Our Stars', 'Dutton Books', 2.20, 7.20, NULL, NULL, 2012, '/Images/fault.jpg', '12+', 25, 10, NULL, 'Romance'),
(16, 'Gone with the Wind', 'Macmillan', 3.00, 10.99, NULL, NULL, 1936, '/Images/gonewiththewind.jpg', '18+', 12, 9, NULL, 'Historical Fiction'),
(17, 'The Hunger Games', 'Scholastic Press', 2.50, 8.99, NULL, NULL, 2008, '/Images/hungergames.jpg', '12+', 30, 10, NULL, 'Dystopian'),
(18, 'Divergent', 'Katherine Tegen Books', 2.75, 9.99, NULL, NULL, 2011, '/Images/divergent.jpg', '12+', 25, 9, NULL, 'Dystopian'),
(19, 'The Maze Runner', 'Delacorte Press', 2.20, 8.50, NULL, NULL, 2009, '/Images/mazerunner.jpg', '12+', 20, 8, NULL, 'Dystopian'),
(20, 'The Girl with the Dragon Tattoo', 'Norstedts Förlag', 3.50, 12.99, NULL, NULL, 2005, '/Images/dragontattoo.jpg', '18+', 10, 9, NULL, 'Mystery'),
(21, 'Fahrenheit 451', 'Ballantine Books', 2.00, 7.99, NULL, NULL, 1953, '/Images/fahrenheit451.jpg', '12+', 15, 9, NULL, 'Dystopian'),
(22, 'The Book Thief', 'Knopf', 2.80, 9.50, NULL, NULL, 2005, '/Images/bookthief.jpg', '12+', 18, 10, NULL, 'Historical Fiction'),
(23, 'Percy Jackson & the Olympians: The Lightning Thief', 'Disney Hyperion', 2.60, 8.50, NULL, NULL, 2005, '/Images/percyjackson.jpg', '8+', 20, 9, NULL, 'Fantasy'),
(24, 'Little Women', 'Roberts Brothers', 2.40, 7.99, NULL, NULL, 1868, '/Images/littlewomen.jpg', '12+', 15, 8, NULL, 'Classic'),
(25, 'A Game of Thrones', 'Bantam Books', 3.50, 13.99, NULL, NULL, 1996, '/Images/gameofthrones.jpg', '18+', 10, 9, NULL, 'Fantasy'),
(26, 'The Shining', 'Doubleday', 3.20, 11.99, NULL, NULL, 1977, '/Images/theshining.jpg', '18+', 8, 9, NULL, 'Horror'),
(27, 'It', 'Viking', 3.80, 13.50, NULL, NULL, 1986, '/Images/it.jpg', '18+', 6, 8, NULL, 'Horror'),
(28, 'The Road', 'Alfred A. Knopf', 2.50, 9.50, NULL, NULL, 2006, '/Images/theroad.jpg', '18+', 12, 9, NULL, 'Post-Apocalyptic'),
(29, 'Life of Pi', 'Knopf Canada', 2.60, 8.99, NULL, NULL, 2001, '/Images/lifeofpi.jpg', '12+', 15, 10, NULL, 'Adventure'),
(30, 'Slaughterhouse-Five', 'Delacorte Press', 2.20, 7.99, NULL, NULL, 1969, '/Images/slaughterhousefive.jpg', '18+', 10, 8, NULL, 'Science Fiction');




-- Insert data into Authors table
INSERT INTO authors (authorName, bookId)
VALUES 
('F. Scott Fitzgerald', 1),  -- The Great Gatsby
('Harper Lee', 2),           -- To Kill a Mockingbird
('George Orwell', 3),        -- 1984
('J.K. Rowling', 4),         -- Harry Potter and the Sorcerer's Stone
('J.R.R. Tolkien', 5),       -- The Hobbit
('Jane Austen', 6),          -- Pride and Prejudice
('J.D. Salinger', 7),        -- The Catcher in the Rye
('J.R.R. Tolkien', 8),       -- The Lord of the Rings
('George Orwell', 9),        -- Animal Farm
( 'C.S. Lewis', 10),         -- The Chronicles of Narnia
( 'Paulo Coelho', 11),       -- The Alchemist
( 'Charlotte Brontë', 12),   -- Jane Eyre
( 'Emily Brontë', 13),       -- Wuthering Heights
( 'Aldous Huxley', 14),      -- Brave New World
( 'John Green', 15),         -- The Fault in Our Stars
('Margaret Mitchell', 16), -- Gone with the Wind
('Suzanne Collins', 17),   -- The Hunger Games
('Veronica Roth', 18),     -- Divergent
('James Dashner', 19),     -- The Maze Runner
('Stieg Larsson', 20),     -- The Girl with the Dragon Tattoo
('Ray Bradbury', 21),      -- Fahrenheit 451
('Markus Zusak', 22),      -- The Book Thief
('Rick Riordan', 23),      -- Percy Jackson & the Olympians: The Lightning Thief
('Louisa May Alcott', 24), -- Little Women
('George R.R. Martin', 25), -- A Game of Thrones
('Stephen King', 26),     -- The Shining
('Stephen King', 27),     -- It
('Cormac McCarthy', 28),  -- The Road
('Yann Martel', 29),      -- Life of Pi
('Kurt Vonnegut', 30);    -- Slaughterhouse-Five