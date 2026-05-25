using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicHub.Application.Features.Payment.DTOs
{
    public class PaymentBillingData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Apartment { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public object? Extra { get; set; }
    }
}
