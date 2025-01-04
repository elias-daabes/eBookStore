using eBookStore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyCart()
        {
            Cart cart;
            List<string> removedBooks = new List<string>(); // List to track removed books

            if ((Cart)Session["Cart"] == null)
            {
                cart = new Cart();
                cart.BooksList = new List<Book>();
            }
            else
            {
                cart = (Cart)Session["Cart"];
            }
            if (Session["AccountId"] != null) {

                // Fetch the books from the user's library
                int accountId = (int)Session["AccountId"];
                List<Book> accountLibraryBooks = getLibraryBooks(accountId).books;

                // Remove books that the user already owns
                var booksToRemove = accountLibraryBooks.Select(book => book.id).ToList();

                // Track removed books' titles for the message
                foreach (var book in cart.BooksList.ToList())
                {
                    if (booksToRemove.Contains(book.id))
                    {
                        removedBooks.Add(book.title); // Add title of the removed book
                        cart.BooksList.Remove(book); // Remove the book
                    }
                }

                // Pass the removed books to the view
                ViewBag.RemovedBooks = removedBooks;
            }
            return View(cart);
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
        public ActionResult DeleteBookFromCart(int bookId)
        {
            Cart cart = (Cart)Session["Cart"];

            // Safely remove books that match the given bookId
            cart.BooksList.RemoveAll(book => book.id == bookId);

            return RedirectToAction("MyCart");
        }


        public ActionResult AddBookToCart(int bookId, bool isHomePage)
        {

            Cart cart;

            // Check if a cart exists in the session
            if (Session["Cart"] == null)
            {
                cart = new Cart(); // Create a new cart for guest
                cart.BooksList = new List<Book>();
                Session["Cart"] = cart;
            }
            else
            {
                cart = (Cart)Session["Cart"];
            }

            Book book = getBookByid(bookId);
            //Cart cart = getCartFromDB(accountId);
            if (cart.BooksList.Count > 0)
                foreach (Book b in cart.BooksList)
                {
                    if (bookId == b.id)
                    {
                        TempData["ActionError"] = "The book '" + book.title + "' is already in the cart.";
                        return isHomePage? RedirectToAction("HomePage", "Home"): RedirectToAction("ViewBook", "Book",  new { id = bookId});
                    }
                }
            cart.BooksList.Add(book);
            TempData["ActionSuccess"] = "The book '" + book.title + "' added successfully into the cart.";
            return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
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