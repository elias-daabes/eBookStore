using eBookStore.Models;
using eBookStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
        // GET: Book
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Submit(Book book, HttpPostedFileBase imgFile)
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };
            //book.authors = new List<string>();
            bool isValidBook = isValidBookID(book);
            if (ModelState.IsValid && isValidBook && imgFile != null)
            {
                handleImageFile(book, imgFile);
                SaveBookToDB(book);
                SaveAuthorsToDB(book);
                bookViewModel.booksList.Add(book);
                bookViewModel.book = new Book();
                return View("Enter", bookViewModel);
            }
            if (!isValidBook)
            {
                ModelState.AddModelError("book.id", "Invalid Book id");
            }
            bookViewModel.book = book;
            return View("Enter", bookViewModel);
        }

        [HttpPost]
        public ActionResult EditBook(Book book)
        {
            return View();
        }

        public ActionResult ViewBook(int id)
        {
            BookFeedbackViewModel bookFeedbackViewModel = new BookFeedbackViewModel
            {
                book = getBookByid(id),
                bookFeedback = new BookFeedback(),
                bookFeedbacksList = getBookFeedbacksListById(id, -1)
            };
            return View(bookFeedbackViewModel);
        }


        [HttpPost]
        public ActionResult SaveEdits(Book newBook)
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };
            //handleImageFile(newBook);

            if (ModelState.IsValid)
            {

                SaveChangesOnDB(newBook);
                bookViewModel.booksList.Remove(getBookByid(newBook.id));
                bookViewModel.booksList.Add(newBook);
                bookViewModel.book = new Book();
                return View("Enter", bookViewModel);
            }

            bookViewModel.book = newBook;
            return View("EditBook", newBook);
        }

        private void SaveChangesOnDB(Book book)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE books SET title = @title, publisher = @publisher, priceForBorrowing = @priceForBorrowing, SalepriceForBorrowing = @SalepriceForBorrowing, Megapixels = @Megapixels, yearOfPublishing = @yearOfPublishing, ageLimitation = @ageLimitation, IsAvailable = @IsAvailable, IsOnSale = IsOnSale, coverImagePath = @coverImagePath, popularity = @popularity, quantityInStock = @quantityInStock, genre = @genre, borrowingCopies = @borrowingCopies WHERE id = @id";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", book.id);
                    command.Parameters.AddWithValue("@title", book.title);
                    command.Parameters.AddWithValue("@genre", book.genre);
                    command.Parameters.AddWithValue("@publisher", book.publisher);
                    command.Parameters.AddWithValue("@priceForBorrowing", book.priceForBorrowing);
                    command.Parameters.AddWithValue("@priceForBuying", book.priceForBuying);
                    // Check if SalepriceForBorrowing is null or has a value, and use DBNull.Value if null
                    if (book.priceSaleForBorrowing.HasValue)
                        command.Parameters.AddWithValue("@priceSaleForBorrowing", book.priceSaleForBorrowing.Value);
                    else
                        command.Parameters.AddWithValue("@SalepriceForBorrowing", DBNull.Value); // Use DBNull for null values
                    if (book.priceSaleForBuying.HasValue)
                        command.Parameters.AddWithValue("@priceSaleForBuying", book.priceSaleForBuying.Value);
                    else
                        command.Parameters.AddWithValue("@priceSaleForBuying", DBNull.Value); // Use DBNull for null values
                    command.Parameters.AddWithValue("@yearOfPublishing", book.yearOfPublishing);
                    command.Parameters.AddWithValue("@coverImagePath", book.coverImagePath);
                    command.Parameters.AddWithValue("@ageLimitation", book.ageLimitation);
                    command.Parameters.AddWithValue("@quantityInStock", book.quantityInStock);
                    command.Parameters.AddWithValue("@popularity", book.popularity);
                    command.Parameters.AddWithValue("@dateSale", book.dateSale);
                    command.Parameters.AddWithValue("@borrowingCopies", book.borrowingCopies);

                    command.ExecuteNonQuery();
                }
            }
        }
        private void handleImageFile(Book book, HttpPostedFileBase imgFile)
        {
            // Handle file upload
            //if (book.ImageFile != null && book.ImageFile.ContentLength > 0)
            //{
            //    string fileName = book.publisher + "_" + book.title + "_" + book.id + ".png";
            //    string path = Path.Combine(Server.MapPath("~/images/"), fileName);
            //    book.ImageFile.SaveAs(path);

            //    // Save the file path to the league object
            //    book.coverImagePath = "/images/" + fileName; // Store relative path
            //}
            string path = "";
            if (imgFile.FileName.Length > 0)
            {
                path = "/images/" + Path.GetFileName(imgFile.FileName);
                imgFile.SaveAs(Server.MapPath(path));
            }
            book.coverImagePath = path;
        }



        private List<Book> getBooksList()
        {
            List<Book> booksList = new List<Book>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM books";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Book book = new Book
                            {
                                id = Convert.ToInt32(reader["id"]),
                                title = reader["title"].ToString(),
                                genre = reader["genre"].ToString(),
                                publisher = reader["publisher"].ToString(),
                                priceForBorrowing = Convert.ToDecimal(reader["priceForBorrowing"]),
                                priceForBuying = Convert.ToDecimal(reader["priceForBuying"]),
                                priceSaleForBorrowing = reader["priceSaleForBorrowing"] != DBNull.Value ? Convert.ToDecimal(reader["priceSaleForBorrowing"]) : (decimal?)null,
                                priceSaleForBuying = reader["priceSaleForBuying"] != DBNull.Value ? Convert.ToDecimal(reader["priceSaleForBuying"]) : (decimal?)null,
                                yearOfPublishing = Convert.ToInt32(reader["yearOfPublishing"]),
                                coverImagePath = reader["coverImagePath"].ToString(),
                                ageLimitation = reader["ageLimitation"].ToString(),
                                quantityInStock = Convert.ToInt32(reader["quantityInStock"]),
                                popularity = Convert.ToInt32(reader["popularity"]),
                                borrowingCopies = Convert.ToInt32(reader["borrowingCopies"]),
                                dateSale = reader["dateSale"] != DBNull.Value ? Convert.ToDateTime(reader["dateSale"]) : (DateTime?)null                            
                            };
                            string sqlQuery2 = "SELECT * FROM authors WHERE bookId = @bookId";
                            using (SqlCommand command2 = new SqlCommand(sqlQuery2, connection))
                            {
                                command2.Parameters.AddWithValue("@bookId", book.id);
                                using (SqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        Author author2 = new Author
                                        {
                                            authorName = reader2["authorName"].ToString(),
                                            bookId = Convert.ToInt32(reader2["bookId"])
                                        };
                                        book.authors.Add(author2);
                                    }
                                }
                            }

                            booksList.Add(book);
                        }
                    }
                }
            }
            return booksList;
        }

        public ActionResult Enter()
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };
            return View(bookViewModel);
        }

        public ActionResult Enter2(string sortColumn, string sortOrder)
        {
            List<Book> booksList = getBooksList();

            // Apply sorting based on selected options
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                switch (sortColumn)
                {
                    case "BorrowingPrice":
                        booksList = sortOrder == "asc"
                            ? booksList.OrderBy(c => c.dateSale.HasValue ? c.priceSaleForBorrowing : c.priceForBorrowing).ToList()
                            : booksList.OrderByDescending(c => c.dateSale.HasValue ? c.priceSaleForBorrowing : c.priceForBorrowing).ToList();
                        break;
                    case "BuyingPrice":
                        booksList = sortOrder == "asc"
                            ? booksList.OrderBy(c => c.dateSale.HasValue ? c.priceSaleForBuying : c.priceForBuying).ToList()
                            : booksList.OrderByDescending(c => c.dateSale.HasValue ? c.priceSaleForBuying : c.priceForBuying).ToList();
                        break;
                    case "yearOfPublishing":
                        booksList = sortOrder == "asc"
                            ? booksList.OrderBy(c => c.yearOfPublishing).ToList()
                            : booksList.OrderByDescending(c => c.yearOfPublishing).ToList();
                        break;
                    case "popularity":
                        booksList = sortOrder == "asc"
                            ? booksList.OrderBy(c => c.popularity).ToList()
                            : booksList.OrderByDescending(c => c.popularity).ToList();
                        break;
                    case "title":
                        booksList = sortOrder == "asc"
                            ? booksList.OrderBy(c => c.title).ToList()
                            : booksList.OrderByDescending(c => c.title).ToList();
                        break;
                }
                ViewBag.Message = $"You chose to sort by {sortColumn} in {sortOrder} order.";
            }
            else
            {
                ViewBag.Message = "No sorting option was chosen; displaying default order.";
            }

            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = booksList
            };

            return View("Enter", bookViewModel);
        }

        public ActionResult EditBook(int id)
        {

            Book book = getBookByid(id);
            return View(book);
        }

        public ActionResult AddBookFiles(int id)
        {
            Book book = getBookByid(id);
            return View(book);
        }

        [HttpPost]
        public ActionResult AddFormats(Book book, HttpPostedFileBase epubFile, HttpPostedFileBase fb2File, HttpPostedFileBase mobiFile, HttpPostedFileBase pdfFile)
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };

            List<string> missingFiles = new List<string>();

            if (epubFile == null) missingFiles.Add("EPUB");
            if (fb2File == null) missingFiles.Add("FB2");
            if (mobiFile == null) missingFiles.Add("MOBI");
            if (pdfFile == null) missingFiles.Add("PDF");

            if (missingFiles.Count > 0)
            {
                ViewBag.Message = "The following file(s) have not been uploaded: " + string.Join(", ", missingFiles) + ". Please upload all the files";
                return View("AddBookFiles", getBookByid(book.id)); // Show the form again with a message
            }
            else
            {

                string epubFilePath = "/BookFiles/" + Path.GetFileName(epubFile.FileName);
                string fb2FilePath = "/BookFiles/" + Path.GetFileName(fb2File.FileName);
                string mobiFilePath = "/BookFiles/" + Path.GetFileName(mobiFile.FileName);
                string pdfFilePath = "/BookFiles/" + Path.GetFileName(pdfFile.FileName);
                epubFile.SaveAs(Server.MapPath(epubFilePath));
                fb2File.SaveAs(Server.MapPath(fb2FilePath));
                mobiFile.SaveAs(Server.MapPath(mobiFilePath));
                pdfFile.SaveAs(Server.MapPath(pdfFilePath));
                book.epubPath = epubFilePath;
                book.fb2Path = fb2FilePath;
                book.mobiPath = mobiFilePath;
                book.pdfPath = pdfFilePath;

                string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "INSERT INTO book_files (bookId, epubPath, fb2Path, mobiPath, pdfPath) VALUES " +
                                      "(@id, @epubPath, @fb2Path, @mobiPath, @pdfPath)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", book.id);
                        command.Parameters.AddWithValue("@epubPath", book.epubPath);
                        command.Parameters.AddWithValue("@fb2Path", book.fb2Path);
                        command.Parameters.AddWithValue("@mobiPath", book.mobiPath);
                        command.Parameters.AddWithValue("@pdfPath", book.pdfPath);
                       
                        command.ExecuteNonQuery();
                    }
                }
            }
            ViewBag.Message = "Files uploaded and saved successfully!";
            return View("Enter", bookViewModel); 



        }

        [HttpPost]
        public ActionResult UpdateBook(Book model, HttpPostedFileBase imgFile)
        {

            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };

            if (ModelState.IsValid )
            {
                if(imgFile != null)
                    handleImageFile(model, imgFile);
                else
                {
                    model.coverImagePath = getBookByid(model.id).coverImagePath;
                }


                // Prepare SQL for updating the book in the database
                string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "UPDATE books SET title = @title, genre = @genre, publisher = @publisher, priceForBorrowing = @priceForBorrowing, priceForBuying = @priceForBuying, priceSaleForBorrowing = @priceSaleForBorrowing, priceSaleForBuying = @priceSaleForBuying, yearOfPublishing = @yearOfPublishing, coverImagePath = @coverImagePath, ageLimitation = @ageLimitation, quantityInStock = @quantityInStock, popularity = @popularity, dateSale = @dateSale, borrowingCopies = @borrowingCopies WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@id", model.id);
                        command.Parameters.AddWithValue("@title", model.title);
                        command.Parameters.AddWithValue("@genre", model.genre);
                        command.Parameters.AddWithValue("@publisher", model.publisher);
                        command.Parameters.AddWithValue("@priceForBorrowing", model.priceForBorrowing);
                        command.Parameters.AddWithValue("@priceForBuying", model.priceForBuying);

                        // Handle nullable fields explicitly
                        command.Parameters.Add("@priceSaleForBorrowing", SqlDbType.Decimal).Value =
                            model.priceSaleForBorrowing.HasValue ? (object)model.priceSaleForBorrowing.Value : DBNull.Value;
                        command.Parameters.Add("@priceSaleForBuying", SqlDbType.Decimal).Value =
                            model.priceSaleForBuying.HasValue ? (object)model.priceSaleForBuying.Value : DBNull.Value;

                        command.Parameters.AddWithValue("@yearOfPublishing", model.yearOfPublishing);
                        command.Parameters.AddWithValue("@coverImagePath", model.coverImagePath);
                        command.Parameters.AddWithValue("@ageLimitation", model.ageLimitation);
                        command.Parameters.AddWithValue("@quantityInStock", model.quantityInStock);
                        command.Parameters.AddWithValue("@popularity", model.popularity);
                        command.Parameters.AddWithValue("@borrowingCopies", model.borrowingCopies);

                        // Handle nullable dateSale
                        command.Parameters.Add("@dateSale", SqlDbType.DateTime).Value =
                            model.dateSale.HasValue ? (object)model.dateSale.Value : DBNull.Value;

                        // Execute query
                        command.ExecuteNonQuery();
                    }
                }
                bookViewModel.booksList.Remove(getBookByid(model.id));
                bookViewModel.booksList.Add(model);
                bookViewModel.book = new Book();
                // Redirect to a success or details page
                return RedirectToAction("Enter");
            }

            // If ModelState is not valid, return the same view with validation errors
            return View("EditBook", model);
        }

        private Book getBookByid(int id)
        {
            List<Book> books = getBooksList(null);
            foreach (Book book in books)
            {
                if (book.id == id)
                    return book;
            }
            return null;
        }

        private void SaveBookToDB(Book book)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO books (id, title, genre, publisher, priceForBorrowing, priceForBuying, priceSaleForBorrowing, priceSaleForBuying, yearOfPublishing, coverImagePath, ageLimitation, quantityInStock, popularity, dateSale, borrowingCopies) VALUES " +
                                  "(@id, @title, @genre, @publisher, @priceForBorrowing, @priceForBuying, @priceSaleForBorrowing, @priceSaleForBuying, @yearOfPublishing, @coverImagePath, @ageLimitation, @quantityInStock, @popularity, @dateSale, @borrowingCopies)";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", book.id);
                    command.Parameters.AddWithValue("@title", book.title);
                    command.Parameters.AddWithValue("@genre", book.genre);
                    command.Parameters.AddWithValue("@publisher", book.publisher);
                    command.Parameters.AddWithValue("@priceForBorrowing", book.priceForBorrowing);
                    command.Parameters.AddWithValue("@priceForBuying", book.priceForBuying);

                    // Handle nullable parameters explicitly
                    command.Parameters.Add("@priceSaleForBorrowing", SqlDbType.Decimal).Value = book.priceSaleForBorrowing.HasValue ? (object)book.priceSaleForBorrowing.Value : DBNull.Value;
                    command.Parameters.Add("@priceSaleForBuying", SqlDbType.Decimal).Value = book.priceSaleForBuying.HasValue ? (object)book.priceSaleForBuying.Value : DBNull.Value;

                    command.Parameters.AddWithValue("@yearOfPublishing", book.yearOfPublishing);
                    command.Parameters.AddWithValue("@coverImagePath", book.coverImagePath);
                    command.Parameters.AddWithValue("@ageLimitation", book.ageLimitation);
                    command.Parameters.AddWithValue("@quantityInStock", book.quantityInStock);
                    command.Parameters.AddWithValue("@popularity", book.popularity);
                    command.Parameters.AddWithValue("@borrowingCopies", book.borrowingCopies);

                    // Handle dateSale as nullable
                    command.Parameters.Add("@dateSale", SqlDbType.DateTime).Value = book.dateSale.HasValue ? (object)book.dateSale.Value : DBNull.Value;

                    command.ExecuteNonQuery();
                }
            }
        }

        private void SaveAuthorsToDB(Book book)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                for (int i = 0; i < book.authors.Count; i++)
                {
                    string sqlQuery = "INSERT INTO authors (authorName, bookId) VALUES " + "(@authorName, @bookId)";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {                    
                        command.Parameters.AddWithValue("@authorName", book.authors[i].authorName);
                        command.Parameters.AddWithValue("@bookId", book.id);
                        command.ExecuteNonQuery();
                    } 
                }
            }
        }

        private bool isValidBookID(Book book)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT COUNT(*) FROM books WHERE id = @id";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", book.id);

                    int count = (int)command.ExecuteScalar();

                    return count == 0;
                }
            }
        }

        public ActionResult Search()
        {
            string searchTerm = null;
            if (Request.Form["searchTerm"] != null)
            {
                searchTerm = Request.Form["searchTerm"].ToString();
            }
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList(searchTerm)
            };
            return View(bookViewModel);
        }

        private List<Book> getBooksList(string searchTerm)
        {
            List<Book> booksList = new List<Book>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM books";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sqlQuery += " WHERE title LIKE @title";
                }

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        command.Parameters.AddWithValue("@title", "%" + searchTerm + "%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Book book = new Book
                            {
                                id = Convert.ToInt32(reader["id"]),
                                title = reader["title"].ToString(),
                                genre = reader["genre"].ToString(),
                                authors = new List<Author>(),
                                publisher = reader["publisher"].ToString(),
                                priceForBorrowing = Convert.ToDecimal(reader["priceForBorrowing"]),
                                priceForBuying = Convert.ToDecimal(reader["priceForBuying"]),
                                priceSaleForBorrowing = reader["priceSaleForBorrowing"] != DBNull.Value ? Convert.ToDecimal(reader["priceSaleForBorrowing"]) : (decimal?)null,
                                priceSaleForBuying = reader["priceSaleForBuying"] != DBNull.Value ? Convert.ToDecimal(reader["priceSaleForBuying"]) : (decimal?)null,
                                yearOfPublishing = Convert.ToInt32(reader["yearOfPublishing"]),
                                coverImagePath = reader["coverImagePath"].ToString(),
                                ageLimitation = reader["ageLimitation"].ToString(),
                                quantityInStock = Convert.ToInt32(reader["quantityInStock"]),
                                popularity = Convert.ToInt32(reader["popularity"]),
                                dateSale = reader["dateSale"] != DBNull.Value ? (DateTime?)reader["dateSale"] : null, // This handles nullable DateTime
                                borrowingCopies = Convert.ToInt32(reader["borrowingCopies"])

                            };

                            string sqlQuery2 = "SELECT * FROM authors WHERE bookId = @bookId";
                            using (SqlCommand command2 = new SqlCommand(sqlQuery2, connection))
                            {
                                command2.Parameters.AddWithValue("@bookId", book.id);
                                using (SqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        Author author2 = new Author
                                        {
                                            authorName = reader2["authorName"].ToString(),
                                            bookId = Convert.ToInt32(reader2["bookId"])
                                        };
                                        book.authors.Add(author2);
                                    }
                                }
                            }

                            booksList.Add(book);
                        }
                    }
                }
            }

            return booksList;

        }

        [HttpPost]
        public ActionResult DeleteBook(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string deleteBookQuery = "DELETE FROM books WHERE id = @id";

                using (SqlCommand command = new SqlCommand(deleteBookQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    string deleteAuthorsQuery = "DELETE FROM authors WHERE bookId = @bookId";

                    using (SqlCommand command2 = new SqlCommand(deleteAuthorsQuery, connection))
                    {
                        command2.Parameters.AddWithValue("@bookId", id);
                        command2.ExecuteNonQuery();
                    }


                        command.ExecuteNonQuery();
                }

            }
            //BookViewModel bookViewModel = new BookViewModel
            //{
            //    book = new Book(),
            //    booksList = getBooksList(null)
            //};
            return RedirectToAction("Enter");
        }

        public ActionResult SearchBookResult(string searchTerm)
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList(searchTerm)
            };

            return View("Enter", bookViewModel);
        }

        public ActionResult FilterBooks(string brandFilter, string categoryFilter, bool? isAvailableFilter)
        {
            List<Book> filteredList = getBooksList();

            if (!string.IsNullOrEmpty(brandFilter) && brandFilter != "All titles")
            {
                filteredList = filteredList.Where(c => c.title == brandFilter).ToList();
            }

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
            {
                filteredList = filteredList.Where(c => c.ageLimitation == categoryFilter).ToList();
            }

            if (isAvailableFilter.HasValue && isAvailableFilter.Value)
            {
                filteredList = filteredList.Where(c => c.quantityInStock > 0).ToList();
            }

            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = filteredList
            };

            return View("Enter", bookViewModel);
        }

        [HttpPost]
        public ActionResult AddBookFeedback(BookFeedbackViewModel bookFeedbackViewModel)
        {
            int currentBookId = bookFeedbackViewModel.bookFeedback.BookId;
            if (Session["AccountId"] == null)
            {
                TempData["ActionError"] = "Please log in to give a feedback.";
                return RedirectToAction("ViewBook", new { id = currentBookId });
            }
            BookFeedback bookFeedback = bookFeedbackViewModel.bookFeedback;
            int activeUserId = (int)Session["AccountId"];
            bookFeedback.AccountId = activeUserId;
            List<BookFeedback> _bookFeedbacksList = getBookFeedbacksListById(currentBookId, activeUserId);

            BookFeedbackViewModel _bookFeedbackViewModel = new BookFeedbackViewModel
            {
                book = new Book(),
                bookFeedback = new BookFeedback(),
                bookFeedbacksList = getBookFeedbacksListById(currentBookId,-1)
            };
            bool inLibrary = isInAccountLibrary(activeUserId, currentBookId);
            if (!inLibrary)
            {
                TempData["ActionError"] = "You do not have this book.";
                return RedirectToAction("ViewBook", new { id = currentBookId });
            }

            if (_bookFeedbacksList.Count > 0)
            {
                TempData["ActionError"] = "You can send one feedback only.";
                return RedirectToAction("ViewBook", new { id = currentBookId });
            }
            if (ModelState.IsValid)
            {
                SaveBookFeedbackToDB(bookFeedback);
                _bookFeedbackViewModel.bookFeedbacksList.Add(bookFeedback);
                _bookFeedbackViewModel.bookFeedback = new BookFeedback();
                return RedirectToAction("ViewBook", new { id = currentBookId });
            }



            return RedirectToAction("ViewBook", new { id = bookFeedbackViewModel.bookFeedback.BookId });
        }

        private bool isInAccountLibrary(int activeUserId, int bookId)
        {
            Library library = getLibraryBooks(activeUserId);
            foreach(Book book in library.books)
            {
                if (book.id == bookId)
                    return true;
            }
            return false;
        }

        private List<BookFeedback> getBookFeedbacksListById(int bookId, int accountId)
        {

            List<BookFeedback> bookFeedbackList = new List<BookFeedback>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery;
                if (accountId == -1)
                {
                    sqlQuery = "SELECT * FROM BookFeedbacks, Accounts WHERE BookFeedbacks.AccountId = Accounts.Id AND bookId = @bookId";
                }
                else
                {
                    sqlQuery = "SELECT * FROM BookFeedbacks, Accounts WHERE BookFeedbacks.AccountId = Accounts.Id AND bookId = @bookId AND Accounts.Id = @feedbackerId";
                }
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@bookId", bookId);
                    if (accountId != -1)
                        command.Parameters.AddWithValue("@feedbackerId", accountId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BookFeedback bookFeedback = new BookFeedback
                            {
                                AccountId = Convert.ToInt32(reader["AccountId"]),
                                BookId = Convert.ToInt32(reader["BookId"]),
                                Rating = Convert.ToInt32(reader["Rating"]),
                                Comment = reader["Comment"].ToString(),
                                Created_At = Convert.ToDateTime(reader["Created_At"]),
                                Name = reader["FirstName"].ToString() + " " + reader["LastName"].ToString()

                            };

                            bookFeedbackList.Add(bookFeedback);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
            return bookFeedbackList;
        }


        private void SaveBookFeedbackToDB(BookFeedback bookFeedback)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "INSERT INTO BookFeedbacks (AccountId, BookId, Rating, Comment, Created_At) VALUES " +
                                    "(@AccountId, @BookId, @Rating, @Comment, @Created_At)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", bookFeedback.AccountId);
                    command.Parameters.AddWithValue("@BookId", bookFeedback.BookId);
                    command.Parameters.AddWithValue("@Rating", bookFeedback.Rating);
                    command.Parameters.AddWithValue("@Created_At", DateTime.Today);
                    command.Parameters.AddWithValue("@Comment", bookFeedback.Comment);

                    command.ExecuteNonQuery();
                }
            }
        }

        // every user can give one feedback only!
        private bool isValidbookFeedbackID(int accountId, int bookId)
        {
            List<BookFeedback> bookFeedbacks = getBookFeedbacksListById(bookId, accountId);
            foreach (BookFeedback bf in bookFeedbacks)
            {
                if (bf.AccountId == accountId && bf.BookId == bookId)
                    return false;
            }
            return true;
        }



        private List<WebFeedback> getWebFeedbacksList()
        {

            List<WebFeedback> webFeedbackList = new List<WebFeedback>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM WebFeedbacks, Accounts WHERE WebFeedbacks.AccountId = Accounts.Id";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WebFeedback webFeedback = new WebFeedback
                            {
                                AccountId = Convert.ToInt32(reader["AccountId"]),
                                Rating = Convert.ToInt32(reader["Rating"]),
                                Comment = reader["Comment"].ToString(),
                                Created_At = Convert.ToDateTime(reader["Created_At"]),
                                Name = reader["FirstName"].ToString() + " " + reader["LastName"].ToString()

                            };

                            webFeedbackList.Add(webFeedback);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
            return webFeedbackList;
        }

        private Library getLibraryBooks(int AccountId)
        {
            Library libraryBooks = new Library();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT AccountId, BookId, BorrowingDate, AcquisitionDate FROM Libraries WHERE AccountId = @AccountId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", AccountId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        libraryBooks = new Library
                        {
                            AccountId = AccountId,
                            books = new List<Book>(),
                            BorrowingDates = new List<DateTime?>(),
                            AcquisitionDate = new List<DateTime>()
                        };

                        while (reader.Read())
                        {
                            int bookId = Convert.ToInt32(reader["BookId"]);
                            DateTime? borrowingDate = reader["BorrowingDate"] != DBNull.Value ? Convert.ToDateTime(reader["BorrowingDate"]) : (DateTime?)null;
                            DateTime acquisitionDate = Convert.ToDateTime(reader["AcquisitionDate"]);

                            libraryBooks.books.Add(getBookByid(bookId));
                            libraryBooks.BorrowingDates.Add(borrowingDate);
                            libraryBooks.AcquisitionDate.Add(acquisitionDate);
                        }

                    }
                }
            }
            return libraryBooks;
        }

    }
}