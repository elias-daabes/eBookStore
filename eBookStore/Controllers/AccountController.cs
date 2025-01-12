using eBookStore.Models;
using eBookStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmailService _emailService;
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
        public AccountController()
        {
            _emailService = new EmailService();
        }
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult SignUp()
        {
            return View(new Account());
        }

        [HttpPost]
        public ActionResult SignUpAccount(Account account)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                bool emailExists = CheckIfEmailExists(account.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View("SignUp", account);
                }
                SaveAccountToDB(account);          
                TempData["SignUpSuccessMessage"] = "You have successfully signed up!";
                Session["AccountId"] = account.Id;
                Session["FirstName"] = account.FirstName;
                Session["LastName"] = account.LastName;

                return RedirectToAction("HomePage", "Home");
            }
            // Return the view with validation errors
            return View(account);
        }

        private bool CheckIfEmailExists(string email)
        {

            AccountViewModel accountViewModel = new AccountViewModel
            {
                account = new Account(),
                accountsList = getAccountsList()
            };

            for (int i = 0; i < accountViewModel.accountsList.Count; i++)
            {
                Account account = accountViewModel.accountsList[i];
                if (account.Email.Equals(email))
                {
                    return true;
                }
            }

            return false;
        }


        private void SaveAccountToDB(Account account)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "INSERT INTO Accounts (FirstName, LastName, Email, Password, IsAdmin) " +
                                  "OUTPUT INSERTED.Id " +
                                  "VALUES (@FirstName, @LastName, @Email, @Password, @IsAdmin)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", account.FirstName);
                    command.Parameters.AddWithValue("@LastName", account.LastName);
                    command.Parameters.AddWithValue("@Email", account.Email);
                    command.Parameters.AddWithValue("@Password", account.Password);
                    command.Parameters.AddWithValue("@IsAdmin", 0);

                    account.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public ActionResult ManageAccounts()
        {
            AccountViewModel accountViewModel = new AccountViewModel
            {
                account = new Account(),
                accountsList = getAccountsList()
            };
            return View(accountViewModel);
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


        public ActionResult MyLibrary()
        {
            if (Session["AccountId"] != null)
            {
                int accountId = (int)Session["AccountId"];
                Library library = getLibraryBooks(accountId);
                return View(library);
            }
            //Session["DeniedAccessToLibrary"] = "Please log in in order to access your library.";
            TempData["DeniedAccessToLibrary"] = "Please log in in order to access your library.";
            return RedirectToAction("HomePage", "Home");

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


        public ActionResult DownloadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return HttpNotFound();
            }

            string path = Server.MapPath(filePath);
            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", filePath);
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
                string sqlQuery = "SELECT * FROM books b LEFT JOIN book_files f ON b.id = f.bookId";

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
                }
            }

            return booksList;

        }


        [HttpPost]
        public ActionResult DeleteAccount(int accountId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM Accounts WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", accountId);
                    command.ExecuteNonQuery();
                    TempData["SuccessMessage"] = "Account deleted successfully.";

                }
            }
            return RedirectToAction("ManageAccounts");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel login)
        {


            if (ModelState.IsValid)
            {

                // Replace this with your actual logic to validate the user
                Account account = ValidateUser(login.Email, login.Password);

                if (account != null)
                {
                    //deleteOverDuedBooks(account.Id);
                    Session["AccountId"] = account.Id;
                    Session["FirstName"] = account.FirstName;
                    Session["LastName"] = account.LastName;
                    Session["Email"] = account.Email;
                    return RedirectToAction("HomePage", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password");
                }
            }

            return View(login);
        }

        private void deleteOverDuedBooks(int id)
        {
            Library library = getLibraryBooks(id);
            for (int i = 0; i < library.books.Count; i++)
            {
                if (library.BorrowingDates[i] != null && library.BorrowingDates[i] < DateTime.Today)
                {
                    DeleteBookFromLibraryFunction(id, library.books[i].id);
                    // quentity of book should be increased by 1
                    ReturnBookToStore(library.books[i].id);
                    //send notification
                }
            }
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

        private void BuyBookFromStore(int id)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE books SET quantityInStock = quantityInStock - 1 WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {

                    command.Parameters.AddWithValue("@id", id);

                    // Execute query
                    command.ExecuteNonQuery();
                }
            }
        } 
        
        private void BorrowBookFromStore(int id)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE books SET borrowingCopies = borrowingCopies - 1 WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {

                    command.Parameters.AddWithValue("@id", id);

                    // Execute query
                    command.ExecuteNonQuery();
                }
            }
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("HomePage", "Home");
        }

        private Account ValidateUser(string email, string password)
        {

            AccountViewModel accountViewModel = new AccountViewModel
            {
                account = new Account(),
                accountsList = getAccountsList()
            };

            for (int i = 0; i < accountViewModel.accountsList.Count; i++)
            {
                Account account = accountViewModel.accountsList[i];
                if (account.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    account.Password == password)
                {
                    return account;
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult DeleteBookFromLibrary(int accountId, int bookId, bool isBorrow)
        {
            DeleteBookFromLibraryFunction(accountId, bookId);
            if (isBorrow)
            {
                ReturnBookToStore(bookId);
            }
            return RedirectToAction("MyLibrary");
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
        public ActionResult PaymentPage()
        {
            return View(new Payment());
        }

        [HttpPost]
        public ActionResult SubmitPayment(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                sendFailedPaymentCompletion();
                return View("PaymentPage", payment);
            }
            int accountId = (int)Session["accountId"];
            int bookId;
            string status = TempData["status"].ToString();
            switch (status)
            {
                case "BorrowingBook":
                    bookId = (int)TempData["BookIdToAquire"];
                    DeleteBookFromWaitlist(bookId);
                    BorrowBookFromStore(bookId);
                    addBookToLibrary(accountId, bookId, true);
                    TempData["ActionSuccess"] = "Book '" + getBookByid(bookId).title + "' has been borrowed successfully. Enter the library to view formats.";
                    sendSuccessfullBorrowingPayment();
                    break;

                case "BuyingBook":
                    bookId = (int)TempData["BookIdToAquire"];
                    BuyBookFromStore(bookId);
                    addBookToLibrary(accountId, bookId, false);
                    TempData["ActionSuccess"] = "Book '" + getBookByid(bookId).title + "' has been purchased successfully. Enter the library to view formats.";
                    sendSuccessfullBuyingPayment();
                    break;

                case "BuyingCart":
                    Cart cart = (Cart)Session["Cart"];

                    foreach (Book book in cart.BooksList)
                    {
                        AcquireBookFromStore(book.id);
                        addBookToLibrary(accountId, book.id, false);

                    }
                    TempData["ActionSuccess"] = "Books have been purchased successfully. Enter the library to view the formats.";
                    sendSuccessfullCartPayment();

                    break;
                default:
                    break;
            }
            return RedirectToAction("HomePage", "Home");
        }

        private void sendSuccessfullCartPayment()
        {
            string subject = $"Successfully purchased";
            string body = $"<p>Dear User,</p>" +
                            $"<p>Your payment has been completed successfully. You can read the books in your library.\nAmount paid: {(decimal)Session["Amount"]}</p>";

            _emailService.SendEmailAsync((string)Session["Email"], subject, body); 
        }

        private void sendSuccessfullBuyingPayment()
        {
            int bookId = (int)Session["BookId"];
            Book book = getBookByid(bookId);
            string subject = $"Successfully purchased the book '{book.title}' ";
            string body = $"<p>Dear User,</p>" +
                            $"<p>Book <strong>{book.title}</strong> has been purchased successfully. You can read it in your library.\nAmount paid: {(decimal)Session["Amount"]}</p>";

            _emailService.SendEmailAsync((string)Session["Email"], subject, body); 
        }

        private void sendFailedPaymentCompletion()
        {
            string subject = $"Payment Failur";
            string body = $"<p>Dear User,</p>" +
                            $"<p>Unfortunatelly, your payment did not complete successfully. please try again.</p>";

            _emailService.SendEmailAsync((string)Session["Email"], subject, body);
        }

        private void sendSuccessfullBorrowingPayment()
        {
            int bookId = (int)Session["BookId"];
            Book book = getBookByid(bookId);
            string subject = $"Successfully borrowing the book '{book.title}' ";
            string body = $"<p>Dear User,</p>" +
                            $"<p>Book <strong>{book.title}</strong> has been borrowed successfully. You can read it in your library.\nAmount paid: {(decimal)Session["Amount"]}</p>";

            _emailService.SendEmailAsync((string)Session["Email"], subject, body); 
        }

        private void AcquireBookFromStore(int id)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE books SET quantityInStock = quantityInStock - 1 WHERE id = @id";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {

                    command.Parameters.AddWithValue("@id", id);

                    // Execute query
                    command.ExecuteNonQuery();
                }
            }
        }

        // GET: Payment/PaymentSuccess
        public ActionResult PaymentSuccess()
        {
            // Return a success page or view after the payment is successfully processed
            return View();
        }

        public ActionResult BorrowBook(int bookId, bool isHomePage)
        {
            if (Session["accountId"] == null)
            {
                TempData["ActionError"] = "Please log in to complete borrowing process.";
                return isHomePage ? RedirectToAction("HomePage", "Home"): RedirectToAction("ViewBook", "Book", new { id = bookId });
            }
            int accountId = (int)Session["accountId"];
            Library library = getLibraryBooks(accountId);

            foreach( Book book in library.books)
                if (book.id == bookId)
                {
                    TempData["ActionError"] = "You already have this book.";
                    return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
                }

            if (library.BorrowingDates.Count(date => date != null) == 3)
            {
                TempData["ActionError"] = "You cannot borrow more than 3 books.";
                return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
            }
            Book book1 = getBookByid(bookId);



            if (book1.borrowingCopies == 0)
            {
                Session["BookId"] = book1.id;
                TempData["WaitlistRequest"] = $"Borrowing copies are not available currently for '{book1.title}' book. {getWaitlistsListByBookId(book1.id).Count} readers are in the waiting list. it could be available at {getNextAvailableDate(book1.id).ToString("dd/MM/yyyy")}. Do you want to be added to wait list?";
                return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
            }

            bool allowdToBorrow = IsAllowdToBorrow(bookId, accountId, book1.borrowingCopies);

            if (!allowdToBorrow)
            {
                Session["BookId"] = book1.id;
                TempData["WaitlistRequest"] = $"Borrowing copies are not available currently for '{book1.title}' book. {getWaitlistsListByBookId(book1.id).Count} readers are in the waiting list. it could be available at {getNextAvailableDate(book1.id).ToString("dd/MM/yyyy")}. Do you want to be added to wait list?";
                return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
            }
            TempData["BookIdToAquire"] = bookId;
            Session["Amount"] = book1.dateSale.HasValue ? book1.priceSaleForBorrowing : book1.priceForBorrowing;
            TempData["status"] = "BorrowingBook"; 
            return RedirectToAction("PaymentPage");

        }

        private bool IsAllowdToBorrow(int bookId, int accountId, int borrowingCopies)
        {
            List<Waitlist> waitlists = getWaitlistsListByBookId(bookId);
            // if the available copies are greater than waitlist size, then it's ok to borrow
            if (waitlists.Count < borrowingCopies)
                return true;
            if (waitlists.Count > 0)
            {
                for (int i = 0; i < borrowingCopies; i++)
                {
                    if (waitlists[i].accountId == accountId)
                        return true;
                }
            }
            return false;
        }

        public List<Waitlist> getWaitlistsListByBookId(int bookId)
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
                                notified = Convert.ToBoolean(reader["notified"])
                            };

                            waitlists.Add(waitlist);
                        }
                    }
                }
            }
            return waitlists;
        }

        private DateTime getNextAvailableDate(int bookId)
        {
            List<DateTime> copyAvailability = new List<DateTime>();

            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Step 1: Get the return dates for the 3 copies from Libraries table
                string queryLibraries = @"
                SELECT BorrowingDate
                FROM Libraries 
                WHERE BookId = @BookId 
                ORDER BY BorrowingDate ASC";

                using (SqlCommand command = new SqlCommand(queryLibraries, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime borrowingDate = Convert.ToDateTime(reader["BorrowingDate"]);
                            copyAvailability.Add(borrowingDate);
                        }
                    }
                }

                // Step 2: Get the available_at dates from Waitlist table
                string queryWaitlist = @"
                SELECT available_At 
                FROM Waitlist 
                WHERE BookId = @BookId 
                ORDER BY available_At ASC";

                List<DateTime> waitlistAvailability = new List<DateTime>();

                using (SqlCommand command = new SqlCommand(queryWaitlist, connection))
                {
                    command.Parameters.AddWithValue("@BookId", bookId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime availableAt = Convert.ToDateTime(reader["available_At"]);
                            waitlistAvailability.Add(availableAt);
                        }
                    }
                }

                int index = waitlistAvailability.Count;
                if (index < copyAvailability.Count)
                    return copyAvailability[index];
                return waitlistAvailability[index - copyAvailability.Count].AddDays(30);
            }
        }


        public ActionResult BuyBook(int bookId, bool isHomePage)
        {
            if (Session["accountId"] == null)
            {
                TempData["ActionError"] = "Please log in to complete this purchasing process.";
                return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
            }
            int accountId = (int)Session["accountId"];
            Library library = getLibraryBooks(accountId);

            foreach (Book book in library.books)
                if (book.id == bookId)
                {
                    TempData["ActionError"] = "You already have this book.";
                    return isHomePage ? RedirectToAction("HomePage", "Home") : RedirectToAction("ViewBook", "Book", new { id = bookId });
                }

            Book wantedBook = getBookByid(bookId);
            if(wantedBook.quantityInStock == 0)
            {
                TempData["ActionError"] = "Book out of stock.";
                return isHomePage? RedirectToAction("HomePage", "Home"): RedirectToAction("ViewBook", "Book", new { id = bookId });
            }
            TempData["BookIdToAquire"] = bookId;
            Session["Amount"] = wantedBook.dateSale.HasValue? wantedBook.priceSaleForBuying : wantedBook.priceForBuying;
            Session["BookId"] = bookId;
            TempData["status"] = "BuyingBook";
            return RedirectToAction("PaymentPage");


        }

        private void addBookToLibrary(int accountId, int bookId, bool isBorrow)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO Libraries (AccountId, BookId, BorrowingDate, AcquisitionDate) VALUES " +
                                  "(@AccountId, @BookId, @BorrowingDate, @AcquisitionDate)";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);
                    command.Parameters.AddWithValue("@BookId", bookId);
                    if(isBorrow)
                        command.Parameters.Add("@BorrowingDate", SqlDbType.DateTime).Value = DateTime.Today.AddDays(30);
                    else
                        command.Parameters.Add("@BorrowingDate", SqlDbType.DateTime).Value = DBNull.Value;

                    command.Parameters.Add("@AcquisitionDate", SqlDbType.DateTime).Value = DateTime.Today;

                    command.ExecuteNonQuery();
                }
            }

        }

        public ActionResult DeleteBookFromWaitlist(int bookId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "DELETE FROM Waitlist WHERE bookId = @bookId AND accountId = @accoutnId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@bookId", bookId);
                    command.Parameters.AddWithValue("@accoutnId", (int)Session["AccountId"]);
                    command.ExecuteNonQuery();
                    TempData["SuccessMessage"] = "Book deleted successfully.";

                }
            }
            return RedirectToAction("MyWaitlist");
        }
    }
}