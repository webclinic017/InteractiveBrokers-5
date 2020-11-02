using System.Collections;
using System.Collections.Generic;

namespace InteractiveBrokers.Models
{
	public class Lots
	{
		public Lots()
		{
			this.Lot = new HashSet<Lot>();
		}
		public IEnumerable<Lot> Lot { get; set; }
	}
}
