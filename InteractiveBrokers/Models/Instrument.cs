namespace InteractiveBrokers.Models
{
	public class Instrument
	{
		public Product Product { get; set; }
		public string SymbolDescription { get; set; }
		public OrderAction OrderAction { get; set; }
		public QuantityType QuantityType { get; set; }
		public decimal Quantity { get; set; }
		public decimal CancelQuantity { get; set; }
		public decimal OrderedQuantity { get; set; }
		public decimal FilledQuantity { get; set; }
		public decimal AverageExecutionPrice { get; set; }
		public decimal EstimatedCommission { get; set; }
		public decimal EstimatedFees { get; set; }
		public double Bid { get; set; }
		public double Ask { get; set; }
		public double Lastprice { get; set; }
		public string Currency { get; set; }
		public Lots Lots { get; set; }
		public MfQuantity MfQuantity { get; set; }
		public string OsiKey { get; set; }
		public string MfTransaction { get; set; }
		public bool ReserveOrder { get; set; }
		public decimal ReserveQuantity { get; set; }
	}
}
