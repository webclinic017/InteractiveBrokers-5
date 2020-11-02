namespace InteractiveBrokers.Models
{
	public class Product
	{

		public string Symbol { get; set; }
		public SecurityType SecurityType { get; set; }
		public CallPut CallPut { get; set; }
		public int ExpiryYear { get; set; }
		public int ExpiryMonth { get; set; }
		public int ExpiryDay { get; set; }
		public decimal StrikePrice { get; set; }
		public string ExpiryType { get; set; }

	}
}
