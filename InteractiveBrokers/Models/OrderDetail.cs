using System.Collections;
using System.Collections.Generic;

namespace InteractiveBrokers.Models
{
	public class OrderDetail
	{

		public OrderDetail()
		{
			this.Instrument = new HashSet<Instrument>();
		}
		public int OrderNumber { get; set; }
		public string AccountId { get; set; }
		public long PreviewTime { get; set; }
		public long PlacedTime { get; set; }
		public long ExecutedTime { get; set; }
		public decimal OrderValue { get; set; }
		public OrderStatus? Status { get; set; }
		public OrderType? OrderType { get; set; }
		public OrderTerm? OrderTerm { get; set; }
		public PriceType? PriceType { get; set; }
		public string PriceValue { get; set; }
		public decimal? LimitPrice { get; set; }
		public decimal? StopPrice { get; set; }
		public decimal? StopLimitPrice { get; set; }
		public OffsetType? OffsetType { get; set; }
		public decimal? OffsetValue { get; set; }
		public MarketSession? MarketSession { get; set; }
		public RoutingDestination? RoutingDestination { get; set; }
		public decimal BracketedLimitPrice { get; set; }
		public decimal InitialStopPrice { get; set; }
		public decimal TrailPrice { get; set; }
		public decimal TriggerPrice { get; set; }
		public decimal ConditionPrice { get; set; }
		public string ConditionSymbol { get; set; }
		public decimal ConditionType { get; set; }
		public ConditionFollowPrice? ConditionFollowPrice { get; set; }
		public ConditionSecurityType? ConditionSecurityType { get; set; }
		public int ReplacedByOrderId { get; set; }
		public int ReplacesOrderId { get; set; }
		public bool AllOrNone { get; set; }
		public long PreviewId { get; set; }
		public Messages? Messages { get; set; }
		public string PreClearanceCode { get; set; }
		public int OverrideRestrictedCd { get; set; }
		public double InvestmentAmount { get; set; }
		public PositionQuantity? PositionQuantity { get; set; }
		public bool AipFlag { get; set; }
		public EgQual? EgQual { get; set; }
		public ReInvestOption? ReInvestOption { get; set; }
		public decimal EstimatedCommission { get; set; }
		public decimal EstimatedFees { get; set; }
		public decimal EstimatedTotalAmount { get; set; }
		public decimal NetPrice { get; set; }
		public decimal NetBid { get; set; }
		public decimal NetAsk { get; set; }
		public int Gcd { get; set; }
		public string Ratio { get; set; }
		public string MfpriceType { get; set; }

		public IEnumerable<Instrument> Instrument { get; set; }


	}
}
