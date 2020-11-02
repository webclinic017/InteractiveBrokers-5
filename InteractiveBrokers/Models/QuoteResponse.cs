using System.Collections;
using System.Collections.Generic;

namespace InteractiveBrokers.Models
{
	public class QuoteResponse
	{
		public QuoteResponse()
		{
		}
		public IEnumerable<QuoteData> QuoteData { get; set; }
		public IEnumerable<Messages> Messages { get; set; }
	}
}
