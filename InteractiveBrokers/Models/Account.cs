using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveBrokers.Models
{
	public class Account
	{

		public int InstNo { get; set; }
		public string AccountId { get; set; }
		public string AccountIdKey { get; set; }
		public AccountMode AccountMode { get; set; }
		public string AccountDesc { get; set; }
		public string AccountName { get; set; }
		public AccountType AccountType { get; set; }
		public InstitutionType InstitutionType { get; set; }
		public AccountStatus AccountStatus { get; set; }
		public long ClosedDate { get; set; }

	}
}
