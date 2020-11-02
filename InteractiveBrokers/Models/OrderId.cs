using System.Text.Json.Serialization;

namespace InteractiveBrokers.Models
{
	public class OrderId
	{
		[JsonPropertyName("orderId")]
		public long Id { get; set; }
		public string CashMargin { get; set; }
	}
}
