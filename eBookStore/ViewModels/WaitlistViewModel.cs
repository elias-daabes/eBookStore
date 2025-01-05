using eBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.ViewModels
{
    public class WaitlistViewModel
    {
        public List<Waitlist> waitlists { get; set; }
        public List<Book> waitlistsBooks { get; set; }
    }
}