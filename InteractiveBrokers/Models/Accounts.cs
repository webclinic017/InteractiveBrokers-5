using System.Collections;
using System.Collections.Generic;

namespace InteractiveBrokers.Models
{
	public class Accounts
	{
		public Accounts()
		{
			this.Account = new HashSet<Account>();
		}
		public IEnumerable<Account> Account { get; set; }
	}
}
