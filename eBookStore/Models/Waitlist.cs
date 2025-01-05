using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class Waitlist
    {
        public int bookId { get; set; }
        public int accountId { get; set; }
        public DateTime added_At { get; set; }
        public DateTime available_At { get; set; }
        public bool notified { get; set; }
    }
}