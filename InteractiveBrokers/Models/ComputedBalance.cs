namespace InteractiveBrokers.Models
{
	public class ComputedBalance
	{
		public decimal? CashAvailableForInvestment { get; set; }
		public decimal? CashAvailableForWithdrawal { get; set; }
		public decimal? TotalAvailableForWithdrawal { get; set; }
		public decimal? NetCash { get; set; }
		public decimal? CashBalance { get; set; }
		public decimal? SettledCashForInvestment { get; set; }
		public decimal? UnSettledCashForInvestment { get; set; }
		public decimal? FundsWithheldFromPurchasePower { get; set; }
		public decimal? FundsWithheldFromWithdrawal { get; set; }
		public decimal? MarginBuyingPower { get; set; }
		public decimal? CashBuyingPower { get; set; }
		public decimal? DtMarginBuyingPower { get; set; }
		public decimal? DtCashBuyingPower { get; set; }
		public decimal? MarginBalance { get; set; }
		public decimal? ShortAdjustBalance { get; set; }
		public decimal? RegtEquity { get; set; }
		public decimal? RegtEquityPercent { get; set; }
		public decimal? AccountBalance { get; set; }
		public OpenCall OpenCalls { get; set; }
		public RealTimeValues RealTimeValues { get; set; }
		public PortfolioMargin PortfolioMargin { get; set; }

	}
}
