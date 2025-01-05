using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBookStore.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Index()
        {
            return View();
        }

        public void AddNotificationDB(int accountId, string message)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
                                INSERT INTO Notifications (accountId, context, notified_At)
                                VALUES (@accountId, @context, @notified_At)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@context", message);
                    command.Parameters.AddWithValue("@notified_At", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}