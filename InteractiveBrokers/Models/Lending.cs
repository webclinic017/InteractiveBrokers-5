namespace InteractiveBrokers.Models
{
	public class Lending
	{

		public decimal? CurrentBalance { get; set; }
		public decimal? CreditLine { get; set; }
		public decimal? OutstandingBalance { get; set; }
		public decimal? MinPaymentDue { get; set; }
		public decimal? AmountPastDue { get; set; }
		public decimal? AvailableCredit { get; set; }
		public decimal? YtdInterestPaid { get; set; }
		public decimal? LastYtdInterestPaid { get; set; }
		public long PaymentDueDate { get; set; }
		public long LastPaymentReceivedDate { get; set; }
		public decimal? PaymentReceivedMtd { get; set; }


	}
}
