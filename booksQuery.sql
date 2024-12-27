USE tempdb
GO

drop table IF EXISTS books;
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
(2, 'To Kill a Mockingbird', 'J.B. Lippincott & Co.', 1.99, 7.99, NULL, NULL, 1960, '/Images/mockingbird.jpg', '12+', 8, 5, NULL, 'Classic'),
(3, '1984', 'Secker & Warburg', 3.49, 12.99, NULL, NULL, 1949, '/Images/1984.jpg', '18+', 5, 8, NULL, 'Dystopian'),
(4, 'Harry Potter and the Sorcerers Stone', 'Bloomsbury', 2.50, 10.00, NULL, NULL, 1997, '/Images/harrypotter1.jpg', '8+', 20, 6, NULL, 'Fantasy'),
(5, 'The Hobbit', 'George Allen & Unwin', 2.75, 8.50, NULL, NULL, 1937, '/Images/hobbit.jpg', '12+', 15, 9, NULL, 'Fantasy'),
(6, 'Pride and Prejudice', 'T. Egerton', 1.99, 6.99, NULL, NULL, 1813, '/Images/pride.jpg', '12+', 12, 4, NULL, 'Romance'),
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

drop table IF EXISTS book_files;
CREATE TABLE book_files (
	bookId INT PRIMARY KEY,
    epubPath VARCHAR(255),                  -- Path to the EPUB file
    fb2Path VARCHAR(255),                   -- Path to the F2B file
    mobiPath VARCHAR(255),                  -- Path to the MOBI file
    pdfPath VARCHAR(255)                    -- Path to the PDF file
);

-- Insert data into book_files table
INSERT INTO book_files (bookId, epubPath, fb2Path, mobiPath, pdfPath)
VALUES 
(1, '/BookFiles/gatsby.epub', '/BookFiles/gatsby.fb2', '/BookFiles/gatsby.mobi', '/BookFiles/gatsby.pdf'),
(2, '/BookFiles/mockingbird.epub', '/BookFiles/mockingbird.fb2', '/BookFiles/mockingbird.mobi', '/BookFiles/mockingbird.pdf'),
(3, '/BookFiles/1984.epub', '/BookFiles/1984.fb2', '/BookFiles/1984.mobi', '/BookFiles/1984.pdf'),
(4, '/BookFiles/harrypotter1.epub', '/BookFiles/harrypotter1.fb2', '/BookFiles/harrypotter1.mobi', '/BookFiles/harrypotter1.pdf'),
(5, '/BookFiles/hobbit.epub', '/BookFiles/hobbit.fb2', '/BookFiles/hobbit.mobi', '/BookFiles/hobbit.pdf'),
(6, '/BookFiles/pride.epub', '/BookFiles/pride.fb2', '/BookFiles/pride.mobi', '/BookFiles/pride.pdf'),
(7, '/BookFiles/catcher.epub', '/BookFiles/catcher.fb2', '/BookFiles/catcher.mobi', '/BookFiles/catcher.pdf'),
(8, '/BookFiles/lotr.epub', '/BookFiles/lotr.fb2', '/BookFiles/lotr.mobi', '/BookFiles/lotr.pdf'),
(9, '/BookFiles/animalfarm.epub', '/BookFiles/animalfarm.fb2', '/BookFiles/animalfarm.mobi', '/BookFiles/animalfarm.pdf'),
(10, '/BookFiles/narnia.epub', '/BookFiles/narnia.fb2', '/BookFiles/narnia.mobi', '/BookFiles/narnia.pdf'),
(11, '/BookFiles/alchemist.epub', '/BookFiles/alchemist.fb2', '/BookFiles/alchemist.mobi', '/BookFiles/alchemist.pdf'),
(12, '/BookFiles/janeeyre.epub', '/BookFiles/janeeyre.fb2', '/BookFiles/janeeyre.mobi', '/BookFiles/janeeyre.pdf'),
(13, '/BookFiles/wuthering.epub', '/BookFiles/wuthering.fb2', '/BookFiles/wuthering.mobi', '/BookFiles/wuthering.pdf'),
(14, '/BookFiles/bravenewworld.epub', '/BookFiles/bravenewworld.fb2', '/BookFiles/bravenewworld.mobi', '/BookFiles/bravenewworld.pdf'),
(15, '/BookFiles/fault.epub', '/BookFiles/fault.fb2', '/BookFiles/fault.mobi', '/BookFiles/fault.pdf'),
(16, '/BookFiles/gonewiththewind.epub', '/BookFiles/gonewiththewind.fb2', '/BookFiles/gonewiththewind.mobi', '/BookFiles/gonewiththewind.pdf'),
(17, '/BookFiles/hungergames.epub', '/BookFiles/hungergames.fb2', '/BookFiles/hungergames.mobi', '/BookFiles/hungergames.pdf'),
(18, '/BookFiles/divergent.epub', '/BookFiles/divergent.fb2', '/BookFiles/divergent.mobi', '/BookFiles/divergent.pdf'),
(19, '/BookFiles/mazerunner.epub', '/BookFiles/mazerunner.fb2', '/BookFiles/mazerunner.mobi', '/BookFiles/mazerunner.pdf'),
(20, '/BookFiles/dragontattoo.epub', '/BookFiles/dragontattoo.fb2', '/BookFiles/dragontattoo.mobi', '/BookFiles/dragontattoo.pdf'),
(21, '/BookFiles/fahrenheit451.epub', '/BookFiles/fahrenheit451.fb2', '/BookFiles/fahrenheit451.mobi', '/BookFiles/fahrenheit451.pdf'),
(22, '/BookFiles/bookthief.epub', '/BookFiles/bookthief.fb2', '/BookFiles/bookthief.mobi', '/BookFiles/bookthief.pdf'),
(23, '/BookFiles/percyjackson.epub', '/BookFiles/percyjackson.fb2', '/BookFiles/percyjackson.mobi', '/BookFiles/percyjackson.pdf'),
(24, '/BookFiles/littlewomen.epub', '/BookFiles/littlewomen.fb2', '/BookFiles/littlewomen.mobi', '/BookFiles/littlewomen.pdf'),
(25, '/BookFiles/gameofthrones.epub', '/BookFiles/gameofthrones.fb2', '/BookFiles/gameofthrones.mobi', '/BookFiles/gameofthrones.pdf'),
(26, '/BookFiles/theshining.epub', '/BookFiles/theshining.fb2', '/BookFiles/theshining.mobi', '/BookFiles/theshining.pdf'),
(27, '/BookFiles/it.epub', '/BookFiles/it.fb2', '/BookFiles/it.mobi', '/BookFiles/it.pdf'),
(28, '/BookFiles/theroad.epub', '/BookFiles/theroad.fb2', '/BookFiles/theroad.mobi', '/BookFiles/theroad.pdf'),
(29, '/BookFiles/lifeofpi.epub', '/BookFiles/lifeofpi.fb2', '/BookFiles/lifeofpi.mobi', '/BookFiles/lifeofpi.pdf'),
(30, '/BookFiles/slaughterhousefive.epub', '/BookFiles/slaughterhousefive.fb2', '/BookFiles/slaughterhousefive.mobi', '/BookFiles/slaughterhousefive.pdf');
