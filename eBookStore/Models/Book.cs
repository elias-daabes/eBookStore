using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBookStore.Models
{
    
    public class Book: IValidatableObject
    {
        [Key]
        public int id { get; set; } // Unique identifier for the book

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string title { get; set; }

        [Required(ErrorMessage = "At least one author is required.")]
        public List<Author> authors { get; set; } = new List<Author>
        {
            new Author { authorName = "", bookId = -1 }
        };


        [Required(ErrorMessage = "Publisher is required.")]
        [StringLength(100, ErrorMessage = "Publisher name cannot exceed 100 characters.")]
        public string publisher { get; set; }

        [Required(ErrorMessage = "Borrowing price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Borrowing price must be greater than zero.")]
        public decimal priceForBorrowing { get; set; }

        [Required(ErrorMessage = "Buying price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than zero.")]
        public decimal priceForBuying { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Borrowing sale price must be greater than zero.")]
        public decimal? priceSaleForBorrowing { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Buying sale price must be greater than zero.")]
        public decimal? priceSaleForBuying { get; set; }

        [Required(ErrorMessage = "Year of publishing is required.")]
        [Range(1500, 2024, ErrorMessage = "Year of publishing must be valid.")]
        public int yearOfPublishing { get; set; }

        //[Required(ErrorMessage = "Cover image path is required.")]
        public string coverImagePath { get; set; } // Path or URL to the cover image

        [Required(ErrorMessage = "Age limitation is required.")]
        public string ageLimitation { get; set; } // Example: "18+", "8+", etc.

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int quantityInStock { get; set; } // Number of books available

        [Range(0, 10, ErrorMessage = "Popularity rating must be between 0 and 10.")]
        public int popularity { get; set; } // A rating for the book's popularity

        [DataType(DataType.Date)]
        [CustomValidation(typeof(Book), nameof(ValidateDateOfSale))]
        public DateTime? dateSale { get; set; } // Date of sale


        // Custom validation for SaleDate
        public static ValidationResult ValidateDateOfSale(DateTime? dateSale, ValidationContext context)
        {
            if(dateSale == null) return ValidationResult.Success;
            var today = DateTime.Now.Date;
            var oneWeekFromToday = today.AddDays(7);



            if (dateSale < today || dateSale > oneWeekFromToday)
            {
                return new ValidationResult($"Sale date must be within one week from today ({today:yyyy-MM-dd} to {oneWeekFromToday:yyyy-MM-dd}).");
            }

            return ValidationResult.Success;
        }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check if dateSale is provided
            if (dateSale.HasValue)
            {
                // If dateSale is provided, ensure both sale prices are provided
                if (!priceSaleForBorrowing.HasValue)
                {
                    yield return new ValidationResult(
                        "Borrowing sale price is required when a sale date is provided.",
                        new[] { nameof(priceSaleForBorrowing) }
                    );
                }
                if (!priceSaleForBuying.HasValue)
                {
                    yield return new ValidationResult(
                        "Buying sale price is required when a sale date is provided.",
                        new[] { nameof(priceSaleForBuying) }
                    );
                }
            }

            // Existing validation for priceSaleForBorrowing
            if (priceSaleForBorrowing >= priceForBorrowing)
            {
                yield return new ValidationResult(
                    "Sale price for borrowing should be lower than the borrowing price.",
                    new[] { nameof(priceSaleForBorrowing) }
                );
            }
            // Existing validation for priceSaleForBuying
            if (priceSaleForBuying >= priceForBuying)
            {
                yield return new ValidationResult(
                    "Sale price for buying should be lower than the buying price.",
                    new[] { nameof(priceSaleForBuying) }
                );
            }
        }



    }
}

