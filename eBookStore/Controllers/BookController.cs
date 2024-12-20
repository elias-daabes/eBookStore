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
                string sqlQuery = "UPDATE books SET title = @title, publisher = @publisher, priceForBorrowing = @priceForBorrowing, SalepriceForBorrowing = @SalepriceForBorrowing, Megapixels = @Megapixels, yearOfPublishing = @yearOfPublishing, ageLimitation = @ageLimitation, IsAvailable = @IsAvailable, IsOnSale = IsOnSale, coverImagePath = @coverImagePath, popularity = @popularity, quantityInStock = @quantityInStock WHERE id = @id";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", book.id);
                    command.Parameters.AddWithValue("@title", book.title);
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
                                //authors = new List<string>(),
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
                string sqlQuery = "INSERT INTO books (id, title, publisher, priceForBorrowing, priceForBuying, priceSaleForBorrowing, priceSaleForBuying, yearOfPublishing, coverImagePath, ageLimitation, quantityInStock, popularity, dateSale) VALUES " +
                                  "(@id, @title, @publisher, @priceForBorrowing, @priceForBuying, @priceSaleForBorrowing, @priceSaleForBuying, @yearOfPublishing, @coverImagePath, @ageLimitation, @quantityInStock, @popularity, @dateSale)";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", book.id);
                    command.Parameters.AddWithValue("@title", book.title);
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

    }
}