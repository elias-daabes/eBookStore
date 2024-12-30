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

        [HttpGet]
        public ActionResult SignUp()
        {
            return View(new Account()); 
        }
        public ActionResult SignUp(Account account)
        {
            if (ModelState.IsValid)
            {
                SaveAccountToDB(account);
                TempData["SignUpSuccessMessage"] = "You have successfully signed up!";

                return RedirectToAction("HomePage","Home");
                //return RedirectToAction("Success", "AnotherController", new { id = user.Id });
            }
            // Return the view with validation errors
            return View(account);
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


    }
}