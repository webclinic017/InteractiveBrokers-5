using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
    public class Order
    {
		[JsonPropertyName("CustomerOrderId")]
		public string CustomerOrderId { get; set; }
		[JsonPropertyName("Price")]
		public decimal Price { get; set; }
		[JsonPropertyName("Quantity")]
		public int Quantity { get; set; }
		[JsonPropertyName("ListingExchange")]
		public string ListingExchange { get; set; }
		[JsonPropertyName("TimeInForce")]
		public string TimeInForce { get; set; }
		[JsonPropertyName("Side")]
		public string Side { get; set; }

		[JsonPropertyName("OrderType")]
		public string OrderType { get; set; }
		[JsonPropertyName("Ticker")]
		public string Ticker { get; set; }
	}
}
