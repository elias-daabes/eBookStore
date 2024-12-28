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
    public class AccountController : Controller
    {
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

    }
}