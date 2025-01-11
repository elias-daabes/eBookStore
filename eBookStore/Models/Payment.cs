using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace eBookStore.Models
{
    public class Payment : IValidatableObject
    {

        [Required(ErrorMessage = "Payment amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(16, ErrorMessage = "Card number must be 16 digits.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Card number must contain only digits.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Cardholder name must not exceed 50 characters.")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        public string ExpiryDate { get; set; }


        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string CVV { get; set; }




        // Validation logic for ExpiryDate without yield
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (!string.IsNullOrWhiteSpace(ExpiryDate))
            {
                // Validate format: MM/YY
                if (!Regex.IsMatch(ExpiryDate, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    validationResults.Add(new ValidationResult("Invalid date format.", new[] { nameof(ExpiryDate) }));
                }
                else
                {
                    try
                    {
                        var dateParts = ExpiryDate.Split('/');
                        int month = int.Parse(dateParts[0]);
                        int year = int.Parse(dateParts[1]) + 2000; // Convert YY to YYYY

                        // Create a DateTime object for the last day of the expiration month
                        DateTime expiry = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                        // Check if the expiration date is in the future
                        if (expiry < DateTime.Now)
                        {
                            validationResults.Add(new ValidationResult("Expiration date cannot be in the past.", new[] { nameof(ExpiryDate) }));
                        }
                    }
                    catch
                    {
                        validationResults.Add(new ValidationResult("Invalid expiration date.", new[] { nameof(ExpiryDate) }));
                    }
                }
            }

            return validationResults;
        }
    }
}