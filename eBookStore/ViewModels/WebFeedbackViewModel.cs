using eBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBookStore.ViewModels
{
    public class WebFeedbackViewModel
    {
        public WebFeedback webFeedback { get; set; }
        public List<WebFeedback> webFeedbacksList { get; set; }
    }
}