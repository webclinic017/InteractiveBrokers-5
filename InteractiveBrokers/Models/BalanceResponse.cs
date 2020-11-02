using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
	public class BalanceResponse
	{
		public BalanceResponse()
		{
			this.OpenCalls = new HashSet<OpenCall>();
		}

		public string AccountId { get; set; }
		public InstitutionType InstitutionType { get; set; }
		public long AsOfDate { get; set; }
		public AccountType AccountType { get; set; }
		public OptionLevel OptionLevel { get; set; }
		public string AccountDescription { get; set; }
		public int QuoteMode { get; set; }
		public string DayTraderStatus { get; set; }
		public string AccountMode { get; set; }
		public string AccountDesc { get; set; }
		public IEnumerable<OpenCall> OpenCalls { get; set; }
		public Cash Cash { get; set; }
		public Margin Margin { get; set; }
		public Lending Lending { get; set; }
		public ComputedBalance ComputedBalance { get; set; }
	}
}
