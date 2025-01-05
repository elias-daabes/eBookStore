using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class Notification
    {
        public int accountId { get; set; }
        public string context { get; set; }
        public DateTime notified_At { get; set; }
    }
}