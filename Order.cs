using System;

namespace ATDOrderSystem
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string SpecialInstructions { get; set; }
        public bool GiftWrap { get; set; }
        public bool Newsletter { get; set; }

        public decimal TotalPrice
        {
            get { return Quantity * UnitPrice; }
        }

        public override string ToString()
        {
            return $@"
=== ORDER DETAILS ===
Order Number: {OrderNumber}
Order Date: {OrderDate:yyyy-MM-dd}

=== CUSTOMER INFORMATION ===
Name: {CustomerName}
Email: {CustomerEmail}
Phone: {CustomerPhone}

=== SHIPPING ADDRESS ===
Address: {ShippingAddress}
City: {City}
State: {State}
Zip Code: {ZipCode}

=== ORDER ITEMS ===
Product: {ProductName}
Quantity: {Quantity}
Unit Price: ${UnitPrice:F2}
Total Price: ${TotalPrice:F2}

=== SHIPPING & PAYMENT ===
Shipping Method: {ShippingMethod}
Payment Method: {PaymentMethod}

=== ADDITIONAL OPTIONS ===
Gift Wrap: {(GiftWrap ? "Yes" : "No")}
Newsletter Subscription: {(Newsletter ? "Yes" : "No")}

=== SPECIAL INSTRUCTIONS ===
{(string.IsNullOrWhiteSpace(SpecialInstructions) ? "None" : SpecialInstructions)}
";
        }
    }
}
