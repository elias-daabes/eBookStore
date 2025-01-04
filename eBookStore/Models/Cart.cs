using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class Cart
    {

        public List<Book> BooksList { get; set; }

    }
}