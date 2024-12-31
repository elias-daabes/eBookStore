using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class Library
    {
        public int AccountId { get; set; }
        public List<Book> books { get; set; }
        public List<DateTime?> BorrowingDates { get; set; }
        public List<DateTime> AcquisitionDate { get; set; }
    }
}