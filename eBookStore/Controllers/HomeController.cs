using eBookStore.Models;
using eBookStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
        // GET: Home

        public HomeController()
        {
            _emailService = new EmailService();
        }
        public ActionResult HomePage()
        {
            deleteOverDuedBooks();
            sendReminedNotification();
            sendNotificationToAccountsInWaitlist();
            HomePageViewModel homePageViewModel = new HomePageViewModel
            {
                bookViewModel = new BookViewModel
                {
                    book = new Book(),
                    booksList = getBooksList()
                },
                 webFeedbackViewModel = new WebFeedbackViewModel
                 {
                    webFeedback = new WebFeedback(),
                    webFeedbacksList = getWebFeedbacksList()
                }

            };
            TempData["BooksCount"] = (int)homePageViewModel.bookViewModel.booksList.Count;
            return View(homePageViewModel);
        }

        private void sendReminedNotification()
        {
            List<Account> accounts = getAccountsList();
            foreach(Account account in accounts)
            {
                Library libraries = getLibraryBooks(account.Id);
                for(int i = 0; i < libraries.books.Count; i++) { 
                    //List<Waitlist> waitlists = getWaitlistsListByBookId(libraries.books[i].id);
                    if (libraries.BorrowingDates[i] != null && libraries.BorrowingDates[i] <= DateTime.Today.AddDays(5) && libraries.remind_notified[i]==false) { 

                        Book book = getBookByid(libraries.books[i].id);
                        string subject = $"5 days left for borrowing Book '{book.title}' ";
                        string body = $"<p>Dear User,</p>" +
                                        $"<p>We want to inform you that there are 5 days left for borrowing the book <strong>{book.title}</strong>.</p>";

                       _emailService.SendEmailAsync(account.Email, subject, body);
                        UpdateRemindField(book.id, account.Id);                       
                    }
                }
               
            }
        }

        private void sendNotificationToAccountsInWaitlist()
        {
            List<Book> booksList = getBooksList();
            foreach(Book book in booksList)
            {
                if (book.borrowingCopies > 0)
                {
                    List<Waitlist> waitlists = getWaitlistsListByBookId(book.id);
                    for (int i = 0; i < book.borrowingCopies; i++)
                    {
                        if (waitlists.Count > 0) { 
                            Account account = getAccountById(waitlists[i].accountId);
                            if (!waitlists[i].notified)
                            {
                                sendAvailaletyMessage(account.Email, book.title);
                                UpdateNotifiedField(book.id, account.Id);
                            }
                        }
                    }
                }
            }
        }

        private void sendAvailaletyMessage(string email, string title)
        {
            string subject = $"Book '{title}' is available for borrowing";
            string body = $"<p>Dear User,</p>" +
                          $"<p>We want to inform you that the book <strong>{title}</strong> is now available for borrowing in our system.</p>";

            _emailService.SendEmailAsync(email, subject, body);
        }

        private List<Waitlist> getWaitlistsListByBookId(int bookId)
        {
            List<Waitlist> waitlists = new List<Waitlist>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM waitlist WHERE bookId = @bookId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@bookId", bookId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Waitlist waitlist = new Waitlist
                            {
                                bookId = Convert.ToInt32(reader["bookId"]),
                                accountId = Convert.ToInt32(reader["accountId"]),
                                added_At = Convert.ToDateTime(reader["added_At"]),
                                available_At = Convert.ToDateTime(reader["available_At"]),
                                notified = Convert.ToBoolean(reader["notified"]),
                            };

                            waitlists.Add(waitlist);
                        }
                    }
                }
            }
            return waitlists;
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

        [HttpPost]
        public ActionResult AddWebFeedback(HomePageViewModel homePageViewModel)
        {
            WebFeedback webFeedback = homePageViewModel.webFeedbackViewModel.webFeedback;

            HomePageViewModel _homePageViewModel = new HomePageViewModel
            {
                bookViewModel = new BookViewModel
                {
                    book = new Book(),
                    booksList = getBooksList()
                },
                webFeedbackViewModel = new WebFeedbackViewModel
                {
                    webFeedback = new WebFeedback(),
                    webFeedbacksList = getWebFeedbacksList()
                }
            };
            if (Session["AccountId"] == null)
            {
                TempData["ActionError"] = "Please log in to give feedback.";
                return RedirectToAction("HomePage");
            }

            webFeedback.AccountId = (int)Session["AccountId"];
            bool isValidFeedback = isValidWebFeedbackID(webFeedback.AccountId);
            if (!isValidFeedback)
            {
                TempData["ActionError"] = "You can send one feedback only.";
                return RedirectToAction("HomePage");
            }
            if (ModelState.IsValid )
            {
                SaveWebFeedbackToDB(webFeedback);
                _homePageViewModel.webFeedbackViewModel.webFeedbacksList.Add(webFeedback);
                _homePageViewModel.webFeedbackViewModel.webFeedback = new WebFeedback();
                return RedirectToAction("HomePage", _homePageViewModel);
            }



            return RedirectToAction("HomePage", _homePageViewModel);
        }




        private void SaveWebFeedbackToDB(WebFeedback webFeedback)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "INSERT INTO WebFeedbacks (AccountId, Rating, Comment, Created_At) VALUES " +
                                    "(@AccountId, @Rating, @Comment, @Created_At)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", webFeedback.AccountId);
                    command.Parameters.AddWithValue("@Rating", webFeedback.Rating);
                    command.Parameters.AddWithValue("@Created_At", DateTime.Today);
                    command.Parameters.AddWithValue("@Comment", webFeedback.Comment);

                    command.ExecuteNonQuery();
                }
            }
        }

        // every user can give one feedback only!
        private bool isValidWebFeedbackID(int id)
        {
            List<WebFeedback> webFeedbacks = getWebFeedbacksList();
            foreach(WebFeedback wf in webFeedbacks)
            {
                if (wf.AccountId == id)
                    return false;
            }
            return true;
        }

        private List<Book> getBooksList()
        {
            List<Book> booksList = new List<Book>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM books b LEFT JOIN book_files f ON b.id = f.bookId";
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
                                dateSale = reader["dateSale"] != DBNull.Value ? Convert.ToDateTime(reader["dateSale"]) : (DateTime?)null,
                                epubPath = reader["epubPath"].ToString(),
                                fb2Path = reader["fb2Path"].ToString(),
                                mobiPath = reader["mobiPath"].ToString(),
                                pdfPath = reader["pdfPath"].ToString(),
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
                        command.ExecuteNonQuery();
                }
            }
            return booksList;
        }

        public ActionResult SearchBookResult(string searchTerm)
        {
            HomePageViewModel homePageViewModel = new HomePageViewModel { 
                bookViewModel = new BookViewModel
                {
                    book = new Book(),
                    booksList = getBooksList(searchTerm)
                },
                webFeedbackViewModel = new WebFeedbackViewModel
                {
                    webFeedback = new WebFeedback(),
                    webFeedbacksList = getWebFeedbacksList()
                }
            };

            return View("HomePage", homePageViewModel);
        }

       
        private List<Book> getBooksList(string searchTerm)
        {
            List<Book> booksList = new List<Book>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
                                    SELECT b.*, a.authorName 
                                    FROM books b
                                    LEFT JOIN authors a ON b.id = a.bookId";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sqlQuery += " WHERE (b.title LIKE @title OR a.authorName LIKE @author OR b.publisher LIKE @publisher)";
                }

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        command.Parameters.AddWithValue("@title", "%" + searchTerm + "%");
                        command.Parameters.AddWithValue("@author", "%" + searchTerm + "%");
                        command.Parameters.AddWithValue("@publisher", "%" + searchTerm + "%");
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

        public ActionResult Filtering(string sortColumn, string sortOrder, string genre, string priceRange, string byPriceType, bool? onlySalePrices)
        {
            List<Book> booksList = getBooksList();

            // Apply genre filtering
            if (!string.IsNullOrEmpty(genre))
            {
                booksList = booksList.Where(b => b.genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                if (byPriceType == "buyingPrice")
                {
                    switch (priceRange)
                    {
                        case "< 5":
                            booksList = booksList.Where(b => b.priceForBuying < 5).ToList();
                            break;
                        case "5 - 10":
                            booksList = booksList.Where(b => b.priceForBuying < 10 && b.priceForBuying >= 5).ToList();
                            break;
                        case "10 - 15":
                            booksList = booksList.Where(b => b.priceForBuying < 15 && b.priceForBuying >= 10).ToList();
                            break;
                        case "15 - 20":
                            booksList = booksList.Where(b => b.priceForBuying < 20 && b.priceForBuying >= 15).ToList();
                            break;
                        case "20 - 30":
                            booksList = booksList.Where(b => b.priceForBuying < 30 && b.priceForBuying >= 20).ToList();
                            break;
                        case "30+":
                            booksList = booksList.Where(b => b.priceForBuying > 30).ToList();
                            break;

                    }
                }
                else
                {
                    switch (priceRange)
                    {
                        case "< 5":
                            booksList = booksList.Where(b => b.priceForBorrowing < 5).ToList();
                            break;
                        case "5 - 10":
                            booksList = booksList.Where(b => b.priceForBorrowing < 10 && b.priceForBorrowing >= 5).ToList();
                            break;
                        case "10 - 15":
                            booksList = booksList.Where(b => b.priceForBorrowing < 15 && b.priceForBorrowing >= 10).ToList();
                            break;
                        case "15 - 20":
                            booksList = booksList.Where(b => b.priceForBorrowing < 20 && b.priceForBorrowing >= 15).ToList();
                            break;
                        case "20 - 30":
                            booksList = booksList.Where(b => b.priceForBorrowing < 30 && b.priceForBorrowing >= 20).ToList();
                            break;
                        case "30+":
                            booksList = booksList.Where(b => b.priceForBorrowing > 30).ToList();
                            break;

                    }
                }
            }



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
            else if (!string.IsNullOrEmpty(genre))
            {
                ViewBag.Message = $"Filtered by genre: {genre}. No sorting option was chosen; displaying default order.";
            }
            else
            {
                ViewBag.Message = "No filtering or sorting options were chosen; displaying default order.";
            }
            if (onlySalePrices.HasValue && onlySalePrices.Value)
            {
                booksList = booksList.Where(b => b.dateSale >= DateTime.Today).ToList();
            }

            HomePageViewModel homePageViewModel = new HomePageViewModel { 
                bookViewModel = new BookViewModel
                {
                    book = new Book(),
                    booksList = booksList
                },
                webFeedbackViewModel = new WebFeedbackViewModel
                {
                    webFeedback = new WebFeedback(),
                    webFeedbacksList = getWebFeedbacksList()
                }

            };

            return View("HomePage", homePageViewModel);
        }

        private void deleteOverDuedBooks()
        {
            List<Account> accountsList = getAccountsList();
            foreach (Account account in accountsList) { 
                Library library = getLibraryBooks(account.Id);
                for (int i = 0; i < library.books.Count; i++)
                    {
                        if (library.BorrowingDates[i] != null && library.BorrowingDates[i] < DateTime.Today)
                        {
                            DeleteBookFromLibraryFunction(account.Id, library.books[i].id);
                            // quentity of book should be increased by 1
                            ReturnBookToStore(library.books[i].id);
                        string subject = $"Book '{library.books[i].id}' Deleted";
                        string body = $"<p>Dear User,</p>" +
                                      $"<p>We regret to inform you that the book <strong>{library.books[i].title}</strong> has been removed from our library system.</p>" +
                                      $"<p>If you have any questions, feel free to contact us.</p>" +
                                      $"<p>Thank you for understanding.</p>";

                        //_emailService.SendEmailAsync(getAccountById(account.Id).Email, subject, body);
                    }
                    }
            }
        }

        private Account getAccountById(int id)
        {
            List<Account>accounts = getAccountsList();
            foreach(var account in accounts)
            {
                if (account.Id == id) return account;
            }
            return null;
        }

        private List<Account> getAccountsList()
        {
            List<Account> accounts = new List<Account>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT Id, FirstName, LastName, Email, Password, IsAdmin FROM Accounts";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new Account
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                Password = reader.GetString(4),
                                IsAdmin = reader.GetBoolean(5)
                            });
                        }

                    }
                }
            }
            return accounts;
        }


        private void ReturnBookToStore(int id)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE books SET borrowingCopies = borrowingCopies + 1 WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {

                    command.Parameters.AddWithValue("@id", id);

                    // Execute query
                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteBookFromLibraryFunction(int accountId, int bookId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string deleteBookQuery = "DELETE FROM Libraries WHERE BookId = @bookId AND AccountId = @accountId ";

                using (SqlCommand command = new SqlCommand(deleteBookQuery, connection))
                {
                    command.Parameters.AddWithValue("@bookId", bookId);
                    command.Parameters.AddWithValue("@accountId", accountId);

                    command.ExecuteNonQuery();
                }

            }
        }

        private Library getLibraryBooks(int AccountId)
        {
            Library libraryBooks = new Library();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT AccountId, BookId, BorrowingDate, AcquisitionDate, remind_notified FROM Libraries WHERE AccountId = @AccountId";
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
                            AcquisitionDate = new List<DateTime>(),
                            remind_notified = new List<bool>(),
                        };

                        while (reader.Read())
                        {
                            int bookId = Convert.ToInt32(reader["BookId"]);
                            DateTime? borrowingDate = reader["BorrowingDate"] != DBNull.Value ? Convert.ToDateTime(reader["BorrowingDate"]) : (DateTime?)null;
                            DateTime acquisitionDate = Convert.ToDateTime(reader["AcquisitionDate"]);
                            bool remind_notified = Convert.ToBoolean(reader["remind_notified"]);

                            libraryBooks.books.Add(getBookByid(bookId));
                            libraryBooks.BorrowingDates.Add(borrowingDate);
                            libraryBooks.AcquisitionDate.Add(acquisitionDate);
                            libraryBooks.remind_notified.Add(remind_notified);
                        }

                    }
                }
            }
            return libraryBooks;
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

        private void UpdateNotifiedField(int bookId, int accountId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
            UPDATE Waitlist
            SET notified = @notified
            WHERE bookId = @bookId AND accountId = @accountId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@notified", true); // Setting notified field to true
                    command.Parameters.AddWithValue("@bookId", bookId);
                    command.Parameters.AddWithValue("@accountId", accountId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        private void UpdateRemindField(int bookId, int accountId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
            UPDATE Libraries
            SET remind_notified = @remind_notified
            WHERE BookId = @bookId AND AccountId = @accountId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@remind_notified", true); // Setting notified field to true
                    command.Parameters.AddWithValue("@BookId", bookId);
                    command.Parameters.AddWithValue("@AccountId", accountId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


    }
}