namespace InteractiveBrokers.Models
{
	public class OpenCall
	{

		public decimal? MinEquityCall { get; set; }
		public decimal? FedCall { get; set; }
		public decimal? CashCall { get; set; }
		public decimal? HouseCall { get; set; }
	}
}
