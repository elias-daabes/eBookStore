using eBookStore.Models;
using eBookStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

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
                    return View("SignUp",account);
                }
                SaveAccountToDB(account);
                TempData["SignUpSuccessMessage"] = "You have successfully signed up!";
                Session["AccountId"] = account.Id;
                Session["FirstName"] = account.FirstName;
                Session["LastName"] = account.LastName;

                return RedirectToAction("HomePage","Home");
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

                string sqlQuery = "INSERT INTO Accounts (FirstName, LastName, Email, Password, IsAdmin) VALUES " +
                                  "(@FirstName, @LastName, @Email, @Password, @IsAdmin)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", account.FirstName);
                    command.Parameters.AddWithValue("@LastName", account.LastName);
                    command.Parameters.AddWithValue("@Email", account.Email);
                    command.Parameters.AddWithValue("@Password", account.Password);
                    command.Parameters.AddWithValue("@IsAdmin", 0);

                    command.ExecuteNonQuery();
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

        private List<Account> getAccountsList() { 
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
                                pdfPath = reader["pdfPath"].ToString()
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
                    deleteOverDuedBooks(account.Id);
                    Session["AccountId"] = account.Id;
                    Session["FirstName"] = account.FirstName;
                    Session["LastName"] = account.LastName;
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
            for(int i = 0; i < library.books.Count; i++)
            {
                if(library.BorrowingDates[i] != null && library.BorrowingDates[i] < DateTime.Today)
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
                string sqlQuery = "UPDATE books SET quantityInStock = quantityInStock + 1 WHERE id = @id";

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
            return RedirectToAction("HomePage","Home");
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
        public ActionResult DeleteBookFromLibrary(int accountId, int bookId)
        {
            DeleteBookFromLibraryFunction(accountId, bookId);
            //BookViewModel bookViewModel = new BookViewModel
            //{
            //    book = new Book(),
            //    booksList = getBooksList(null)
            //};
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
    }
}