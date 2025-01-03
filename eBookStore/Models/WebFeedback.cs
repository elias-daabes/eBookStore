using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    public class WebFeedback
    {
        public int AccountId { get; set; }
        public string Name { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Feedback text is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; } 

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created_At { get; set; } 
    }
}