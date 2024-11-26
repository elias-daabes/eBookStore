using eBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.ViewModels
{
    public class BookViewModel
    {
        public Book book { get; set; }
        public List<Book> booksList { get; set; }
    }
}