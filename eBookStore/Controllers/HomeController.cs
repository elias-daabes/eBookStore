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
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
        // GET: Home
        public ActionResult HomePage()
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList()
            };
            return View(bookViewModel);
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
                        command.ExecuteNonQuery();
                }
            }
            return booksList;
        }

        public ActionResult SearchBookResult(string searchTerm)
        {
            BookViewModel bookViewModel = new BookViewModel
            {
                book = new Book(),
                booksList = getBooksList(searchTerm)
            };

            return View("HomePage", bookViewModel);
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