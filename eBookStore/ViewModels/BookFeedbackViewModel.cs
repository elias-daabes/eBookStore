using eBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.ViewModels
{
    public class BookFeedbackViewModel
    {
        public Book book { get; set; }
        public BookFeedback bookFeedback { get; set; }
        public List<BookFeedback> bookFeedbacksList {get; set;}
    }
}