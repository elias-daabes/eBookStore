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
    public class WaitlistController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

        // GET: Waitlist
        public ActionResult AddToWaitlist(int bookId)
        {
            int accountId = (int)Session["AccountId"];
            List<Waitlist> waitlist = getWaitlistsListByBookId(bookId);
            foreach(Waitlist w in waitlist)
            {
                if(w.accountId == accountId)
                {
                    Book book = getBookByid(bookId);
                    TempData["ActionError"] = $"The book '{book.title}' is already in your waitlist.";
                    return RedirectToAction("HomePage", "Home");
                }
            }
            AddToWaitlistToDB(bookId, accountId);
            TempData["ActionSuccess"] = "You have been added successfully to waitlist. You will be notified when it will be available.";
            return RedirectToAction("HomePage", "Home");
        }


        

        private void AddToWaitlistToDB(int bookId, int accountId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = @"
                                INSERT INTO Waitlist (bookId, accountId, added_At)
                                VALUES (@bookId, @accountId, @added_At)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@bookId", bookId);
                    command.Parameters.AddWithValue("@accountId", accountId);
                    command.Parameters.AddWithValue("@added_At", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
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
                                notified = Convert.ToBoolean(reader["notified"])
                            };

                            waitlists.Add(waitlist);
                        }
                    }
                }
            }
            return waitlists;
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
    }
}