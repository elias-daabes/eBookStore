using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class Author
    {
        //[Required(ErrorMessage = "Author name can not be empty.")]
        //[StringLength(100, ErrorMessage = "Auther name cannot exceed 100 characters.")]
        public string authorName { get; set; }
        public int bookId { get; set; }
    }
}